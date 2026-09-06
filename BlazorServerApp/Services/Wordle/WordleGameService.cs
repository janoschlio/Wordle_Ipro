using BlazorServerApp.Data.Models;
using BlazorServerApp.Models.Wordle;
using BlazorServerApp.Services.Player;
using BlazorServerApp.Services.Statistics;

namespace BlazorServerApp.Services.Wordle;

public class WordleGameService
{
    private const int Rows = 6;
    private const int Cols = WordListService.WordLength;
    
    private const string FallbackWord = "SPIEL";

    private readonly StatisticsService? _statisticsService;
    private readonly WordListService? _wordListService;
    private readonly PlayerService? _playerService;

    public string TargetWord { get; private set; } = FallbackWord;

    public TileModel[][] Tiles { get; private set; } = default!;
    public List<KeyModel> Keys { get; private set; } = default!;

    private int _currentRow;
    private int _currentCol;
    private DateTime _gameStartTime;

    public bool IsGameWon { get; private set; }
    public bool IsGameLost { get; private set; }
    public bool IsGuessInvalid { get; private set; }
    
    public event Action? OnStateChanged;

    public WordleGameService(
        StatisticsService? statisticsService = null,
        WordListService? wordListService = null,
        PlayerService? playerService = null)
    {
        _statisticsService = statisticsService;
        _wordListService = wordListService;
        _playerService = playerService;
        ResetBoard();
    }
    
    public async Task ResetGameAsync()
    {
        ResetBoard();
        TargetWord = await PickTargetWordAsync();

        NotifyStateChanged();
    }
    
    private void ResetBoard()
    {
        IsGameWon = false;
        IsGameLost = false;
        IsGuessInvalid = false;

        _currentRow = 0;
        _currentCol = 0;
        _gameStartTime = DateTime.Now;

        Tiles = Enumerable.Range(0, Rows)
            .Select(_ => Enumerable.Range(0, Cols).Select(_ => new TileModel()).ToArray())
            .ToArray();

        Keys = BuildKeyboard().ToList();

        NotifyStateChanged();
    }
    
    private async Task<string> PickTargetWordAsync()
    {
        if (_wordListService is null)
            return FallbackWord;

        var word = await _wordListService.GetRandomWordAsync();

        return string.IsNullOrEmpty(word) ? FallbackWord : word;
    }
    
    private async Task<bool> IsAcceptedWordAsync(string guess)
    {
        if (_wordListService is null)
            return true;

        return await _wordListService.ExistsAsync(guess);
    }

    public bool IsBlocked() => IsGameWon || IsGameLost;

    public void AddLetter(char letter)
    {
        if (IsBlocked() || _currentCol >= Cols)
            return;

        // Verhindere Eingabe von ausgegrauten Buchstaben
        var key = Keys.FirstOrDefault(k => k.Label.Length == 1 && k.Label[0] == letter);
        if (KeyState.Absent == key?.State)
            return;

        IsGuessInvalid = false;

        Tiles[_currentRow][_currentCol].Letter = letter;
        Tiles[_currentRow][_currentCol].State = TileState.Filled;
        _currentCol++;

        NotifyStateChanged();
    }


    public void Backspace()
    {
        if (IsBlocked() || _currentCol <= 0)
            return;

        IsGuessInvalid = false;

        _currentCol--;
        Tiles[_currentRow][_currentCol].Letter = null;
        Tiles[_currentRow][_currentCol].State = TileState.Empty;

        NotifyStateChanged();
    }

    public async Task SubmitGuessAsync()
    {
        if (IsBlocked() || _currentCol < Cols)
            return; // nur wenn 5 Buchstaben
        
        var guess = new string(Tiles[_currentRow].Select(t => t.Letter ?? ' ').ToArray()).ToUpperInvariant();

        if (!await IsAcceptedWordAsync(guess))
        {
            IsGuessInvalid = true;
            NotifyStateChanged();
            return;
        }

        EvaluateRow(_currentRow, guess, TargetWord);

        if (guess == TargetWord)
        {
            IsGameWon = true;
            await SaveGameResultAsync();
            NotifyStateChanged();
            return;
        }

        _currentRow++;
        _currentCol = 0;

        if (_currentRow >= Rows)
        {
            IsGameLost = true;
            await SaveGameResultAsync();
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Bewertet einen Rateversuch gegen das gesuchte Wort.
    /// </summary>
    /// <remarks>
    /// Bewusst eine reine Funktion ohne Zugriff auf den Spielzustand: die
    /// Bewertung ist der kniffligste Teil des Spiels (mehrfach vorkommende
    /// Buchstaben) und laesst sich so isoliert testen.
    /// </remarks>
    public static TileState[] EvaluateGuess(string guess, string target)
    {
        var result = new TileState[Cols];
        var remaining = new Dictionary<char, int>();

        // Buchstaben des gesuchten Wortes zaehlen
        foreach (var ch in target)
        {
            remaining[ch] = remaining.TryGetValue(ch, out var c) ? c + 1 : 1;
        }

        // 1) Zuerst die exakten Treffer, sie haben Vorrang auf das Kontingent
        for (int i = 0; i < Cols; i++)
        {
            if (guess[i] == target[i])
            {
                result[i] = TileState.Correct;
                remaining[guess[i]]--;
            }
        }

        // 2) Danach der Rest aus dem verbliebenen Kontingent
        for (int i = 0; i < Cols; i++)
        {
            if (result[i] == TileState.Correct) continue;

            var ch = guess[i];
            if (remaining.TryGetValue(ch, out var left) && left > 0)
            {
                result[i] = TileState.Present;
                remaining[ch]--;
            }
            else
            {
                result[i] = TileState.Absent;
            }
        }

        return result;
    }

    private void EvaluateRow(int row, string guess, string target)
    {
        var result = EvaluateGuess(guess, target);

        for (int i = 0; i < Cols; i++)
        {
            Tiles[row][i].State = result[i];
            UpdateKeyState(guess[i], result[i]);
        }
    }

    private void UpdateKeyState(char letter, TileState tileState)
    {
        var key = Keys.FirstOrDefault(k => k.Label.Length == 1 && k.Label[0] == letter);
        if (key is null) return;

        // Priorität: Correct > Present > Absent > Neutral
        var newState = tileState switch
        {
            TileState.Correct => KeyState.Correct,
            TileState.Present => KeyState.Present,
            TileState.Absent => KeyState.Absent,
            _ => key.State
        };

        key.State = MaxKeyState(key.State, newState);
    }

    private static KeyState MaxKeyState(KeyState a, KeyState b)
    {
        int Rank(KeyState s) => s switch
        {
            KeyState.Neutral => 0,
            KeyState.Absent => 1,
            KeyState.Present => 2,
            KeyState.Correct => 3,
            _ => 0
        };

        return Rank(b) > Rank(a) ? b : a;
    }

    private static IReadOnlyList<KeyModel> BuildKeyboard()
    {
        var keys = new List<KeyModel>();

        keys.AddRange("QWERTYUIOP".Select(c => new KeyModel { Label = c.ToString() }));
        keys.AddRange("ASDFGHJKL".Select(c => new KeyModel { Label = c.ToString() }));
        keys.Add(new KeyModel { Label = "Enter", IsWide = true });
        keys.AddRange("ZXCVBNM".Select(c => new KeyModel { Label = c.ToString() }));
        keys.Add(new KeyModel { Label = "Back", IsWide = true, Icon = "backspace" });

        return keys;
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
    
    private async Task SaveGameResultAsync()
    {
        // Ohne Spieler-Kennung liesse sich das Ergebnis keiner Statistik
        // zuordnen, dann wird es gar nicht erst gespeichert.
        if (_statisticsService is null || _playerService is null)
            return;

        var gameResult = new GameResult
        {
            PlayerId = await _playerService.EnsurePlayerIdAsync(),
            TargetWord = TargetWord,
            GuessCount = IsGameWon ? _currentRow + 1 : 0,
            IsWon = IsGameWon,
            PlayedAt = _gameStartTime
        };

        await _statisticsService.SaveGameResultAsync(gameResult);
    }
}
