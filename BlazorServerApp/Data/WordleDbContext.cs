using BlazorServerApp.Data.Models;
using BlazorServerApp.Services.Wordle;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerApp.Data;

/// <summary>
/// Database context for Wordle game statistics
/// </summary>
public class WordleDbContext : DbContext
{
    public WordleDbContext(DbContextOptions<WordleDbContext> options)
        : base(options)
    {
    }

    public DbSet<GameResult> GameResults => Set<GameResult>();
    public DbSet<Word> Words => Set<Word>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Word>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(WordListService.WordLength);
            entity.HasIndex(e => e.Value).IsUnique();
        });

        modelBuilder.Entity<GameResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetWord).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PlayedAt).IsRequired();
            entity.HasIndex(e => e.PlayedAt);
        });
    }
}
