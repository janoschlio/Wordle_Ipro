using BlazorServerApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        modelBuilder.Entity<Word>().SeedData();

        modelBuilder.Entity<GameResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetWord).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PlayedAt).IsRequired();
            entity.HasIndex(e => e.PlayedAt);
        });
    }
}

public static class DataBuilderExtension
{
    public static EntityTypeBuilder<Word> SeedData(this EntityTypeBuilder<Word> builder)
    {
        builder.HasData(
            new Word { Id = 1, Value = "APFEL" },
            new Word { Id = 2, Value = "BANANE" },
            new Word { Id = 3, Value = "KIWI" },
            new Word { Id = 4, Value = "ORANGE" },
            new Word { Id = 5, Value = "WASSERMELONE" },
            new Word { Id = 6, Value = "TRAUBE" },
            new Word { Id = 7, Value = "STRAUBE" }
        );
        return builder;
    }
}