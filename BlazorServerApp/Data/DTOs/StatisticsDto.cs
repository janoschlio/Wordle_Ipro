namespace BlazorServerApp.Data.DTOs;

/// <summary>
/// Data Transfer Object for aggregated statistics
/// </summary>
public class StatisticsDto
{
    /// <summary>
    /// Total number of games played
    /// </summary>
    public int GamesPlayed { get; set; }

    /// <summary>
    /// Average number of guesses for won games
    /// </summary>
    public double AverageGuesses { get; set; }

    /// <summary>
    /// Current win streak
    /// </summary>
    public int CurrentStreak { get; set; }

    /// <summary>
    /// Maximum win streak achieved
    /// </summary>
    public int MaxStreak { get; set; }

    /// <summary>
    /// Distribution of wins by guess count (1-6)
    /// Key: number of guesses, Value: count of wins
    /// </summary>
    public Dictionary<int, int> GuessDistribution { get; set; } = new();

    /// <summary>
    /// Recent game results (last N games)
    /// </summary>
    public List<GameResultDto> RecentResults { get; set; } = new();
}
