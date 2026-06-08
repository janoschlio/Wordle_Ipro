using BlazorServerApp.Data.Models;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetWord).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PlayedAt).IsRequired();
            entity.HasIndex(e => e.PlayedAt);
        });
    }
}