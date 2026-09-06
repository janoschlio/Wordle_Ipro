using BlazorServerApp.Data.Models;
using BlazorServerApp.Services.Statistics;

namespace BlazorServerApp.Tests;

/// <summary>
/// Tests fuer die Statistik. Wichtigster Punkt: die Auswertung darf nur die
/// Spiele des jeweiligen Spielers beruecksichtigen.
/// </summary>
public class StatisticsServiceTests : IDisposable
{
    private const string SpielerA = "spieler-a";
    private const string SpielerB = "spieler-b";

    private readonly TestDatabase _datenbank = new();
    private readonly StatisticsService _dienst;

    private DateTime _zeitpunkt = new(2026, 1, 1, 12, 0, 0);

    public StatisticsServiceTests()
    {
        _dienst = new StatisticsService(_datenbank.Context);
    }

    public void Dispose() => _datenbank.Dispose();

    /// <summary>Legt ein Spielergebnis an; jedes weitere liegt eine Stunde spaeter.</summary>
    private async Task SpielAnlegen(string spielerId, bool gewonnen, int versuche)
    {
        _zeitpunkt = _zeitpunkt.AddHours(1);

        await _dienst.SaveGameResultAsync(new GameResult
        {
            PlayerId = spielerId,
            TargetWord = "SPIEL",
            IsWon = gewonnen,
            GuessCount = gewonnen ? versuche : 0,
            PlayedAt = _zeitpunkt
        });
    }

    [Fact]
    public async Task Leere_Datenbank_ergibt_Nullwerte()
    {
        // Arrange: die Datenbank ist frisch und enthaelt keine Spiele

        // Act
        var statistik = await _dienst.GetStatisticsAsync(SpielerA);

        // Assert
        Assert.Equal(0, statistik.GamesPlayed);
        Assert.Equal(0, statistik.AverageGuesses);
        Assert.Equal(0, statistik.CurrentStreak);
        Assert.Empty(statistik.RecentResults);
    }

    [Fact]
    public async Task Statistik_zaehlt_nur_die_Spiele_des_eigenen_Spielers()
    {
        // Arrange
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerB, gewonnen: true, versuche: 4);
        await SpielAnlegen(SpielerB, gewonnen: true, versuche: 5);

        // Act
        var statistikA = await _dienst.GetStatisticsAsync(SpielerA);
        var statistikB = await _dienst.GetStatisticsAsync(SpielerB);

        // Assert
        Assert.Equal(1, statistikA.GamesPlayed);
        Assert.Equal(2, statistikB.GamesPlayed);
    }

    [Fact]
    public async Task Durchschnitt_beruecksichtigt_nur_gewonnene_Spiele()
    {
        // Arrange: zwei Siege mit 2 und 4 Versuchen, dazu eine Niederlage
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 2);
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 4);
        await SpielAnlegen(SpielerA, gewonnen: false, versuche: 0);

        // Act
        var statistik = await _dienst.GetStatisticsAsync(SpielerA);

        // Assert: die Niederlage zaehlt bei den Runden mit, nicht beim Durchschnitt
        Assert.Equal(3, statistik.GamesPlayed);
        Assert.Equal(3.0, statistik.AverageGuesses);
    }

    [Fact]
    public async Task Verteilung_zaehlt_Siege_nach_Anzahl_Versuchen()
    {
        // Arrange
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 5);

        // Act
        var statistik = await _dienst.GetStatisticsAsync(SpielerA);
        var verteilung = statistik.GuessDistribution;

        // Assert
        Assert.Equal(2, verteilung[3]);
        Assert.Equal(1, verteilung[5]);
        Assert.Equal(0, verteilung[1]);
    }

    [Fact]
    public async Task Verteilung_enthaelt_immer_die_Werte_eins_bis_sechs()
    {
        // Arrange: bewusst ohne Spiele, die Verteilung muss trotzdem vollstaendig sein

        // Act
        var statistik = await _dienst.GetStatisticsAsync(SpielerA);
        var verteilung = statistik.GuessDistribution;

        // Assert
        Assert.Equal(6, verteilung.Count);
        Assert.All(Enumerable.Range(1, 6), i => Assert.True(verteilung.ContainsKey(i)));
    }

    [Fact]
    public async Task Niederlage_beendet_die_aktuelle_Serie()
    {
        // Arrange: zwei Siege, danach eine Niederlage
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerA, gewonnen: false, versuche: 0);

        // Act
        var statistik = await _dienst.GetStatisticsAsync(SpielerA);

        // Assert: die laufende Serie ist gebrochen, die beste bleibt erhalten
        Assert.Equal(0, statistik.CurrentStreak);
        Assert.Equal(2, statistik.MaxStreak);
    }

    [Fact]
    public async Task Aktuelle_Serie_zaehlt_die_Siege_seit_der_letzten_Niederlage()
    {
        // Arrange
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerA, gewonnen: false, versuche: 0);
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 4);

        // Act
        var statistik = await _dienst.GetStatisticsAsync(SpielerA);

        // Assert
        Assert.Equal(2, statistik.CurrentStreak);
        Assert.Equal(2, statistik.MaxStreak);
    }

    [Fact]
    public async Task Letzte_Runden_kommen_neueste_zuerst_und_nur_vom_eigenen_Spieler()
    {
        // Arrange: dazwischen ein Spiel von Spieler B, das nicht auftauchen darf
        await SpielAnlegen(SpielerA, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerB, gewonnen: true, versuche: 3);
        await SpielAnlegen(SpielerA, gewonnen: false, versuche: 0);

        // Act
        var statistik = await _dienst.GetStatisticsAsync(SpielerA);
        var letzte = statistik.RecentResults;

        // Assert
        Assert.Equal(2, letzte.Count);
        Assert.False(letzte[0].IsWon);   // die Niederlage war zuletzt
        Assert.True(letzte[1].IsWon);
    }
}
