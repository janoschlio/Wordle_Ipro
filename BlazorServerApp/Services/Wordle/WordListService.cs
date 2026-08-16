using BlazorServerApp.Data;
using BlazorServerApp.Data.DTOs;
using BlazorServerApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerApp.Services.Wordle;

public class WordListService
{
    public const int WordLength = 5;
    
    private const string DefaultWordListPath = "Data/words.txt";
    
    private const int MaxRejectedSamples = 5;

    private static readonly char[] Separators = ['\n', '\r', ',', ';', '\t', ' '];

    private readonly WordleDbContext _context;

    public WordListService(WordleDbContext context)
    {
        _context = context;
    }
    
    private async Task<int> GetCountAsync()
    {
        return await _context.Words.CountAsync();
    }
    
    public async Task<List<string>> GetAllAsync()
    {
        return await _context.Words
            .OrderBy(w => w.Value)
            .Select(w => w.Value)
            .ToListAsync();
    }
    
    public async Task<string?> GetRandomWordAsync()
    {
        var count = await GetCountAsync();
        if (count == 0)
            return null;

        var index = Random.Shared.Next(count);

        return await _context.Words
            .OrderBy(w => w.Id)
            .Skip(index)
            .Select(w => w.Value)
            .FirstOrDefaultAsync();
    }
    
    public async Task<int> EnsureSeededAsync(string contentRootPath)
    {
        if (await GetCountAsync() > 0)
            return 0;

        var path = Path.Combine(contentRootPath, DefaultWordListPath);
        if (!File.Exists(path))
            return 0;

        var content = await File.ReadAllTextAsync(path);
        var result = await ImportAsync(content, replaceExisting: false);

        return result.Added;
    }
    
    public async Task<WordImportResultDto> ImportAsync(string rawText, bool replaceExisting)
    {
        var result = new WordImportResultDto { Replaced = replaceExisting };

        var (valid, rejected) = Parse(rawText);

        result.Rejected = rejected.Count;
        result.RejectedSamples = rejected.Take(MaxRejectedSamples).ToList();

        if (replaceExisting)
        {
            await _context.Words.ExecuteDeleteAsync();
        }
        
        var existing = await _context.Words
            .Select(w => w.Value)
            .ToListAsync();

        var known = new HashSet<string>(existing);

        foreach (var word in valid)
        {
            if (known.Add(word))
            {
                _context.Words.Add(new Word { Value = word });
                result.Added++;
            }
            else
            {
                result.Duplicates++;
            }
        }

        await _context.SaveChangesAsync();

        result.TotalAfterImport = known.Count;

        return result;
    }
    
    private static (List<string> Valid, List<string> Rejected) Parse(string rawText)
    {
        var valid = new List<string>();
        var rejected = new List<string>();

        if (string.IsNullOrWhiteSpace(rawText))
            return (valid, rejected);

        // Kommentarzeilen entfernen
        var lines = rawText
            .Split('\n')
            .Where(line => !line.TrimStart().StartsWith('#'));

        var entries = string.Join('\n', lines)
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Innerhalb eines Imports doppelte Eintraege nur einmal zaehlen.
        var seen = new HashSet<string>();

        foreach (var entry in entries)
        {
            var word = entry.ToUpperInvariant();

            if (IsValid(word))
            {
                if (seen.Add(word))
                {
                    valid.Add(word);
                }
            }
            else
            {
                rejected.Add(entry);
            }
        }

        return (valid, rejected);
    }
    
    private static bool IsValid(string word)
    {
        return word.Length == WordLength && word.All(c => c is >= 'A' and <= 'Z');
    }
}
