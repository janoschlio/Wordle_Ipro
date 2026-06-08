namespace BlazorServerApp.Data.Models;

/// <summary>
/// Represents a single game result
/// </summary>
public class GameResult
{
    public int Id { get; set; }

    /// <summary>
    /// The target word that was being guessed
    /// </summary>
    public required string TargetWord { get; set; }

    /// <summary>
    /// Number of guesses used (1-6 for wins, 0 for losses)
    /// </summary>
    public int GuessCount { get; set; }

    /// <summary>
    /// Whether the game was won
    /// </summary>
    public bool IsWon { get; set; }

    /// <summary>
    /// When the game was played
    /// </summary>
    public DateTime PlayedAt { get; set; }
}
