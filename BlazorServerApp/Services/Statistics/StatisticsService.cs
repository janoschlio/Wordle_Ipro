using BlazorServerApp.Data;
using BlazorServerApp.Data.DTOs;
using BlazorServerApp.Data.Models;
using BlazorServerApp.Models.Wordle;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerApp.Services.Statistics;

/// <summary>
/// Wertet die gespeicherten Spielergebnisse eines Spielers aus.
/// </summary>
public class StatisticsService
{
    private const int RecentRoundsShown = 7;

    private readonly WordleDbContext _context;

    public StatisticsService(WordleDbContext context)
    {
        _context = context;
    }

    public async Task SaveGameResultAsync(GameResult gameResult)
    {
        _context.GameResults.Add(gameResult);
        await _context.SaveChangesAsync();
    }
    
    public async Task<StatisticsDto> GetStatisticsAsync(string playerId)
    {
        // Aufsteigend nach Zeit: die Serienberechnung setzt diese Reihenfolge voraus.
        var allGames = await _context.GameResults
            .Where(game => game.PlayerId == playerId)
            .OrderBy(game => game.PlayedAt)
            .Select(game => new GameResultDto
            {
                IsWon = game.IsWon,
                GuessCount = game.GuessCount,
                PlayedAt = game.PlayedAt
            })
            .ToListAsync();

        var wonGames = allGames.Where(game => game.IsWon).ToList();

        var averageGuesses = CalculateAverageGuesses(wonGames);
        var (currentStreak, maxStreak) = CalculateStreaks(allGames);

        return new StatisticsDto
        {
            GamesPlayed = allGames.Count,
            AverageGuesses = averageGuesses,
            GuessDistribution = CalculateGuessDistribution(wonGames),
            RecentResults = GetRecentGames(allGames),
            CurrentStreak = currentStreak,
            MaxStreak = maxStreak
        };
    }
    
    private static double CalculateAverageGuesses(List<GameResultDto> wonGames)
    {
        if (wonGames.Count == 0)
        {
            return 0;
        }

        return wonGames.Average(game => game.GuessCount);
    }
    
    private static List<GameResultDto> GetRecentGames(List<GameResultDto> allGames)
    {
        var newestFirst = new List<GameResultDto>();

        // Von hinten nach vorne: das zuletzt gespielte Spiel steht am Listenende.
        for (var i = allGames.Count - 1; i >= 0; i--)
        {
            if (newestFirst.Count == RecentRoundsShown)
            {
                break;
            }

            newestFirst.Add(allGames[i]);
        }

        return newestFirst;
    }
    
    private static Dictionary<int, int> CalculateGuessDistribution(List<GameResultDto> wonGames)
    {
        var distribution = new Dictionary<int, int>();

        for (var attempts = 1; attempts <= WordleRules.MaxAttempts; attempts++)
        {
            distribution[attempts] = wonGames.Count(game => game.GuessCount == attempts);
        }

        return distribution;
    }
    
    private static (int Current, int Max) CalculateStreaks(List<GameResultDto> games)
    {
        var streak = 0;
        var maxStreak = 0;

        foreach (var game in games)
        {
            if (game.IsWon)
            {
                streak++;
            }
            else
            {
                streak = 0;
            }

            maxStreak = Math.Max(maxStreak, streak);
        }

        // Die laufende Serie zählt nur, solange die letzte Runde gewonnen wurde.
        // Nach einer Niederlage steht streak ohnehin auf 0, aber der ausdrückliche
        // Fall macht die Regel beim Lesen sichtbar.
        var currentStreak = 0;

        if (games.Count > 0)
        {
            var lastGame = games[games.Count - 1];

            if (lastGame.IsWon)
            {
                currentStreak = streak;
            }
        }

        return (currentStreak, maxStreak);
    }
}
