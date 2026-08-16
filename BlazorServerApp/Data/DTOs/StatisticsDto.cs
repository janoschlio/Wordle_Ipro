namespace BlazorServerApp.Data.DTOs;

public class StatisticsDto
{
    public int GamesPlayed { get; set; }
    public double AverageGuesses { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }
    public Dictionary<int, int> GuessDistribution { get; set; } = new();
    public List<GameResultDto> RecentResults { get; set; } = new();
}
