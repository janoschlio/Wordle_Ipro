using BlazorServerApp.Models.Wordle;

namespace BlazorServerApp.Services.Wordle;

public class WordleGameService
{
    private const int Rows = 6;
    private const int Cols = 5;

    // Demo: fixes Wort
    public string TargetWord { get; private set; } = "SLANG";

    public TileModel[][] Tiles { get; private set; } = default!;
    public List<KeyModel> Keys { get; private set; } = default!;

    private int _currentRow;
    private int _currentCol;

    public bool IsGameWon { get; private set; }
    public bool IsGameLost { get; private set; }

    // Event to notify UI when state changes
    public event Action? OnStateChanged;

    public WordleGameService()
    {
        ResetGame();
    }

    public void ResetGame()
    {
        IsGameWon = false;
        IsGameLost = false;

        _currentRow = 0;
        _currentCol = 0;

        Tiles = Enumerable.Range(0, Rows)
            .Select(_ => Enumerable.Range(0, Cols).Select(__ => new TileModel()).ToArray())
            .ToArray();

        Keys = BuildKeyboard().ToList();

        NotifyStateChanged();
    }

    public bool IsBlocked() => IsGameWon || IsGameLost;

    public void AddLetter(char letter)
    {
        if (IsBlocked() || _currentCol >= Cols) 
            return;

        // Verhindere Eingabe von ausgegrauten Buchstaben
        var key = Keys.FirstOrDefault(k => k.Label.Length == 1 && k.Label[0] == letter);
        if (key?.State == KeyState.Absent)
            return;

        Tiles[_currentRow][_currentCol].Letter = letter;
        Tiles[_currentRow][_currentCol].State = TileState.Filled;
        _currentCol++;

        NotifyStateChanged();
    }


    public void Backspace()
    {
        if (IsBlocked() || _currentCol <= 0) 
            return;

        _currentCol--;
        Tiles[_currentRow][_currentCol].Letter = null;
        Tiles[_currentRow][_currentCol].State = TileState.Empty;

        NotifyStateChanged();
    }

    public void SubmitGuess()
    {
        if (IsBlocked() || _currentCol < Cols) 
            return; // nur wenn 5 Buchstaben

        var guess = new string(Tiles[_currentRow].Select(t => t.Letter ?? ' ').ToArray()).ToUpperInvariant();

        EvaluateRow(_currentRow, guess, TargetWord);

        if (guess == TargetWord)
        {
            IsGameWon = true;
            NotifyStateChanged();
            return;
        }

        _currentRow++;
        _currentCol = 0;

        if (_currentRow >= Rows)
        {
            IsGameLost = true;
        }

        NotifyStateChanged();
    }

    // Wordle Bewertung (mit Duplicate-Letters korrekt)
    private void EvaluateRow(int row, string guess, string target)
    {
        var result = new TileState[Cols];
        var remaining = new Dictionary<char, int>();

        // count letters in target
        foreach (var ch in target)
        {
            remaining[ch] = remaining.TryGetValue(ch, out var c) ? c + 1 : 1;
        }

        // 1) correct pass
        for (int i = 0; i < Cols; i++)
        {
            if (guess[i] == target[i])
            {
                result[i] = TileState.Correct;
                remaining[guess[i]]--;
            }
        }

        // 2) present/absent pass
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

        // apply tile states + update key states
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
}
