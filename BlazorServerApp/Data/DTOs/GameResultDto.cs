/// <summary>
/// Simplified game result for recent performance
/// </summary>
public class GameResultDto
{
    public bool IsWon { get; set; }
    public int GuessCount { get; set; }
    public DateTime PlayedAt { get; set; }
}