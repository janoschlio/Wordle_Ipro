namespace BlazorServerApp.Models.Wordle;

public sealed class KeyboardState
{
    private List<KeyModel> _keys = BuildKeyboard();
    public IReadOnlyList<KeyModel> Keys => _keys;
    public void Reset() => _keys = BuildKeyboard();
    public bool IsLetterRuledOut(char letter)
    {
        var key = Find(letter);

        // Enter und Backspace haben keinen Buchstaben und sind nie ausgeschlossen.
        if (key is null)
        {
            return false;
        }

        return key.State == KeyState.Absent;
    }
    
    public void UpdateKeyState(char letter, TileState tileState)
    {
        var key = Find(letter);

        if (key is null)
        {
            return;
        }

        var newState = tileState switch
        {
            TileState.Correct => KeyState.Correct,
            TileState.Present => KeyState.Present,
            TileState.Absent => KeyState.Absent,
            _ => key.State
        };

        key.State = KeepBetterState(key.State, newState);
    }
    
    private KeyModel? Find(char letter)
    {
        return _keys.FirstOrDefault(key => key.Label.Length == 1 && key.Label[0] == letter);
    }

    private static KeyState KeepBetterState(KeyState current, KeyState candidate)
    {
        if (RankOf(candidate) > RankOf(current))
        {
            return candidate;
        }

        return current;
    }
    
    private static int RankOf(KeyState state)
    {
        return state switch
        {
            KeyState.Correct => 3,
            KeyState.Present => 2,
            KeyState.Absent => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Baut die Tastatur reihenweise auf. Die Reihenzugehoerigkeit wird am
    /// KeyModel mitgegeben, damit sie nur hier festgelegt ist.
    /// </summary>
    private static List<KeyModel> BuildKeyboard()
    {
        var keys = new List<KeyModel>();

        keys.AddRange("QWERTYUIOP".Select(c => new KeyModel { Label = c.ToString(), Row = 0 }));
        keys.AddRange("ASDFGHJKL".Select(c => new KeyModel { Label = c.ToString(), Row = 1 }));

        keys.Add(new KeyModel { Label = "Enter", IsWide = true, Row = 2 });
        keys.AddRange("ZXCVBNM".Select(c => new KeyModel { Label = c.ToString(), Row = 2 }));
        keys.Add(new KeyModel { Label = "Back", IsWide = true, Icon = "backspace", Row = 2 });

        return keys;
    }
}
