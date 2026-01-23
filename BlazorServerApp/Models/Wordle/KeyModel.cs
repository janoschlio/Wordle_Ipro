namespace BlazorServerApp.Models.Wordle;

public sealed class KeyModel
{
    public string Label { get; init; } = "";
    public KeyState State { get; set; } = KeyState.Neutral;
    public bool IsWide { get; init; }
    public string? Icon { get; init; }
}
