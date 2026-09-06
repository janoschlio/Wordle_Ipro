namespace BlazorServerApp.Models.Wordle;

public static class WordleRules
{
    /// <summary>Laenge des gesuchten Wortes und damit die Breite des Spielfelds.</summary>
    public const int WordLength = 5;

    /// <summary>Anzahl Rateversuche und damit die Hoehe des Spielfelds.</summary>
    public const int MaxAttempts = 6;
}
