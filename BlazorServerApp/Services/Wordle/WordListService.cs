using BlazorServerApp.Data;
using BlazorServerApp.Data.DTOs;
using BlazorServerApp.Data.Models;
using BlazorServerApp.Models.Wordle;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerApp.Services.Wordle;

public class WordListService
{
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

    public async Task<bool> ExistsAsync(string word)
    {
        return await _context.Words.AnyAsync(w => w.Value == word);
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

        var known = new HashSet<string>();

        // Nach dem Leeren ist die Tabelle garantiert leer -- dann sparen wir uns
        // die Abfrage des bisherigen Bestands.
        if (!replaceExisting)
        {
            var alreadyStored = await _context.Words.Select(word => word.Value).ToListAsync();

            foreach (var word in alreadyStored)
            {
                known.Add(word);
            }
        }

        var newWords = new List<Word>();

        foreach (var word in valid)
        {
            var isNew = !known.Contains(word);

            if (isNew)
            {
                known.Add(word);
                newWords.Add(new Word { Value = word });
                result.Added++;
            }
            else
            {
                result.Duplicates++;
            }
        }

        _context.Words.AddRange(newWords);
        await _context.SaveChangesAsync();

        // known enthaelt den vorherigen Bestand plus alle neu eingefuegten
        // Woerter und entspricht damit dem Stand nach dem Import.
        result.TotalAfterImport = known.Count;

        return result;
    }
    
    private static (List<string> Valid, List<string> Rejected) Parse(string rawText)
    {
        var valid = new List<string>();
        var rejected = new List<string>();

        if (string.IsNullOrWhiteSpace(rawText))
            return (valid, rejected);
        
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
        return word.Length == WordleRules.WordLength && word.All(c => c is >= 'A' and <= 'Z');
    }
}
