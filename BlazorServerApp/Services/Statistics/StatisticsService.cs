using BlazorServerApp.Data;
using BlazorServerApp.Data.DTOs;
using BlazorServerApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerApp.Services.Statistics;

/// <summary>
/// Service for managing game statistics
/// </summary>
public class StatisticsService
{
    private readonly WordleDbContext _context;

    public StatisticsService(WordleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Saves a game result to the database
    /// </summary>
    public async Task SaveGameResultAsync(GameResult gameResult)
    {
        _context.GameResults.Add(gameResult);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets aggregated statistics for all games
    /// </summary>
    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        var allResults = await _context.GameResults
            .OrderBy(g => g.PlayedAt)
            .ToListAsync();

        var wonGames = allResults.Where(g => g.IsWon).ToList();

        var stats = new StatisticsDto
        {
            GamesPlayed = allResults.Count,
            AverageGuesses = wonGames.Any()
                ? wonGames.Average(g => g.GuessCount)
                : 0,
            GuessDistribution = CalculateGuessDistribution(wonGames),
            RecentResults = await GetRecentPerformanceAsync(7)
        };

        // Calculate current and max streaks
        var (currentStreak, maxStreak) = CalculateStreaks(allResults);
        stats.CurrentStreak = currentStreak;
        stats.MaxStreak = maxStreak;

        return stats;
    }

    /// <summary>
    /// Gets the most recent N game results
    /// </summary>
    public async Task<List<GameResultDto>> GetRecentPerformanceAsync(int count)
    {
        return await _context.GameResults
            .OrderByDescending(g => g.PlayedAt)
            .Take(count)
            .Select(g => new GameResultDto
            {
                IsWon = g.IsWon,
                GuessCount = g.GuessCount,
                PlayedAt = g.PlayedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Calculates the distribution of wins by guess count
    /// </summary>
    private Dictionary<int, int> CalculateGuessDistribution(List<GameResult> wonGames)
    {
        var distribution = new Dictionary<int, int>();

        // Initialize all possible guess counts (1-6)
        for (int i = 1; i <= 6; i++)
        {
            distribution[i] = 0;
        }

        // Count wins by guess count
        foreach (var game in wonGames)
        {
            if (game.GuessCount >= 1 && game.GuessCount <= 6)
            {
                distribution[game.GuessCount]++;
            }
        }

        return distribution;
    }

    /// <summary>
    /// Calculates current streak and maximum streak
    /// </summary>
    private (int currentStreak, int maxStreak) CalculateStreaks(List<GameResult> results)
    {
        if (!results.Any())
            return (0, 0);

        int currentStreak = 0;
        int maxStreak = 0;
        int tempStreak = 0;

        // Iterate from oldest to newest
        foreach (var result in results)
        {
            if (result.IsWon)
            {
                tempStreak++;
                maxStreak = Math.Max(maxStreak, tempStreak);
            }
            else
            {
                tempStreak = 0;
            }
        }

        // Current streak is only valid if the last game was won
        if (results.Last().IsWon)
        {
            currentStreak = tempStreak;
        }

        return (currentStreak, maxStreak);
    }
}
