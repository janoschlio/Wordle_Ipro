using BlazorServerApp.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerApp.Tests;

/// <summary>
/// Legt fuer jeden Test eine frische SQLite-Datenbank im Arbeitsspeicher an.
/// Bewusst SQLite und nicht der InMemory-Provider: so gelten dieselben Regeln
/// wie im Betrieb, insbesondere der eindeutige Index auf Words.Value.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public WordleDbContext Context { get; }

    public TestDatabase()
    {
        // Die Datenbank lebt nur so lange, wie die Verbindung offen ist.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WordleDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new WordleDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
