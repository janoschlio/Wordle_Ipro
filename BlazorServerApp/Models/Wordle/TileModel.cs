namespace BlazorServerApp.Models.Wordle;

public sealed class TileModel
{
    public char? Letter { get; set; }
    public TileState State { get; set; } = TileState.Empty;
}
