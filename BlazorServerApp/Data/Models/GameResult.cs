namespace BlazorServerApp.Data.Models;

/// <summary>
/// Represents a single game result
/// </summary>
public class GameResult
{
    public int Id { get; set; }

    /// <summary>
    /// Anonyme Kennung des Browsers, der dieses Spiel gespielt hat. Trennt die
    /// Statistiken der Besucher voneinander.
    /// </summary>
    public required string PlayerId { get; set; }

    public required string TargetWord { get; set; }

    /// <summary>Anzahl benoetigter Versuche bei einem Sieg, 0 bei einer Niederlage.</summary>
    public int GuessCount { get; set; }

    public bool IsWon { get; set; }

    public DateTime PlayedAt { get; set; }
}
