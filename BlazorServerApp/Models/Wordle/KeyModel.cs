namespace BlazorServerApp.Models.Wordle;

public sealed class KeyModel
{
    public string Label { get; init; } = "";
    public KeyState State { get; set; } = KeyState.Neutral;
    public bool IsWide { get; init; }
    public string? Icon { get; init; }

    /// <summary>
    /// Nullbasierte Reihe auf der Bildschirmtastatur. Wird beim Aufbau gesetzt,
    /// damit die Anzeige die Aufteilung nicht aus den Beschriftungen erraten muss.
    /// </summary>
    public int Row { get; init; }
}
