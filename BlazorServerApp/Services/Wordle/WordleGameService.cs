using BlazorServerApp.Data.Models;
using BlazorServerApp.Models.Wordle;
using BlazorServerApp.Services.Player;
using BlazorServerApp.Services.Statistics;

namespace BlazorServerApp.Services.Wordle;

public class WordleGameService
{
    private const int RowCount = WordleRules.MaxAttempts;
    private const int ColumnCount = WordleRules.WordLength;
    
    private const string FallbackWord = "SPIEL";

    private readonly StatisticsService? _statisticsService;
    private readonly WordListService? _wordListService;
    private readonly PlayerService? _playerService;

    public string TargetWord { get; private set; } = FallbackWord;

    public TileModel[][] Tiles { get; private set; } = default!;

    private readonly KeyboardState _keyboard = new(); 
    public IReadOnlyList<KeyModel> Keys => _keyboard.Keys;
    
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

        Tiles = new TileModel[RowCount][];

        for (var row = 0; row < RowCount; row++)
        {
            Tiles[row] = new TileModel[ColumnCount];

            for (var column = 0; column < ColumnCount; column++)
            {
                Tiles[row][column] = new TileModel();
            }
        }
        
        _keyboard.Reset();
    }
    
    private async Task<string> PickTargetWordAsync()
    {
        if (_wordListService is null)
            return FallbackWord;

        var word = await _wordListService.GetRandomWordAsync();

        return string.IsNullOrEmpty(word) ? FallbackWord : word;
    }
    
    private string ReadCurrentRow()
    {
        var letters = new char[ColumnCount];

        for (var column = 0; column < ColumnCount; column++)
        {
            letters[column] = Tiles[_currentRow][column].Letter ?? ' ';
        }

        return new string(letters).ToUpperInvariant();
    }

    private async Task<bool> IsAcceptedWordAsync(string guess)
    {
        // Ohne Wortliste laesst sich nichts pruefen. Dann wird jeder Versuch
        // angenommen, sonst waere das Spiel in diesem Fall unspielbar.
        if (_wordListService is null)
        {
            return true;
        }

        return await _wordListService.ExistsAsync(guess);
    }
    
    public bool IsGameOver() => IsGameWon || IsGameLost;

    public void AddLetter(char letter)
    {
        if (IsGameOver() || _currentCol >= ColumnCount)
            return;
        
        // Bewusste Abweichung vom Original: Buchstaben, die bereits als nicht
        // enthalten bekannt sind, lassen sich gar nicht erst eingeben. Das
        // erspart Versuche, die ohnehin nicht aufgehen koennen.
        if (_keyboard.IsLetterRuledOut(letter))
        {
            return;
        }

        IsGuessInvalid = false;

        Tiles[_currentRow][_currentCol].Letter = letter;
        Tiles[_currentRow][_currentCol].State = TileState.Filled;
        _currentCol++;

        NotifyStateChanged();
    }


    public void Backspace()
    {
        if (IsGameOver() || _currentCol <= 0)
            return;

        IsGuessInvalid = false;

        _currentCol--;
        Tiles[_currentRow][_currentCol].Letter = null;
        Tiles[_currentRow][_currentCol].State = TileState.Empty;

        NotifyStateChanged();
    }

    public async Task SubmitGuessAsync()
    {
        if (IsGameOver() || _currentCol < ColumnCount)
            return; // nur wenn 5 Buchstaben
        
        var guess = ReadCurrentRow();

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

        if (_currentRow >= RowCount)
        {
            IsGameLost = true;
            await SaveGameResultAsync();
        }

        NotifyStateChanged();
    }
    
    public static TileState[] EvaluateGuess(string guess, string target)
    {
        var result = new TileState[ColumnCount];
        var remaining = new Dictionary<char, int>();

        // Kontingent aufbauen: wie oft darf jeder Buchstabe eingefaerbt werden?
        foreach (var letter in target)
        {
            if (remaining.ContainsKey(letter))
            {
                remaining[letter]++;
            }
            else
            {
                remaining[letter] = 1;
            }
        }

        // 1) Zuerst die exakten Treffer, sie haben Vorrang auf das Kontingent
        for (int i = 0; i < ColumnCount; i++)
        {
            if (guess[i] == target[i])
            {
                result[i] = TileState.Correct;

                // Der Schluessel existiert sicher: guess[i] == target[i], und
                // jeder Buchstabe des Zielworts steht im Kontingent.
                remaining[guess[i]]--;
            }
        }

        // 2) Danach der Rest aus dem verbliebenen Kontingent
        for (int i = 0; i < ColumnCount; i++)
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

        for (int i = 0; i < ColumnCount; i++)
        {
            Tiles[row][i].State = result[i];
            _keyboard.UpdateKeyState(guess[i], result[i]);
        }
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
    
    private async Task SaveGameResultAsync()
    {
        // Ohne Spieler-Kennung liesse sich das Ergebnis keiner Statistik
        // zuordnen, dann wird es gar nicht erst gespeichert.
        if (_statisticsService is null || _playerService is null)
            return;
        
        var usedAttempts = IsGameWon ? _currentRow + 1 : 0;

        var gameResult = new GameResult
        {
            PlayerId = await _playerService.EnsurePlayerIdAsync(),
            TargetWord = TargetWord,
            GuessCount = usedAttempts,
            IsWon = IsGameWon,
            PlayedAt = _gameStartTime
        };

        await _statisticsService.SaveGameResultAsync(gameResult);
    }
}
