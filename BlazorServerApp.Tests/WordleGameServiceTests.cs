using BlazorServerApp.Models.Wordle;
using BlazorServerApp.Services.Wordle;

namespace BlazorServerApp.Tests;

/// <summary>
/// Tests zum Spielablauf. Ohne Wortliste faellt der Service auf ein festes
/// Zielwort zurueck und akzeptiert jeden Versuch — dadurch laesst sich der
/// Ablauf ohne Datenbank pruefen.
/// </summary>
public class WordleGameServiceTests
{
    private const string Zielwort = "SPIEL";

    /// <summary>Besteht nur aus Buchstaben des Zielworts, ist aber nicht das Zielwort.</summary>
    private const string FalscherVersuch = "LEIPS";

    private static void Tippe(WordleGameService spiel, string wort)
    {
        foreach (var buchstabe in wort)
        {
            spiel.AddLetter(buchstabe);
        }
    }

    private static string Zeile(WordleGameService spiel, int index) =>
        string.Concat(spiel.Tiles[index].Select(t => t.Letter ?? '_'));

    [Fact]
    public void Neues_Spiel_startet_mit_leerem_Brett()
    {
        // Arrange & Act
        var spiel = new WordleGameService();

        // Assert
        Assert.Equal(6, spiel.Tiles.Length);
        Assert.All(spiel.Tiles, zeile => Assert.Equal(5, zeile.Length));
        Assert.All(spiel.Tiles.SelectMany(z => z), kachel => Assert.Null(kachel.Letter));
        Assert.False(spiel.IsGameWon);
        Assert.False(spiel.IsGameLost);
    }

    [Fact]
    public void Ohne_Wortliste_wird_das_Ersatzwort_verwendet()
    {
        // Arrange & Act
        var spiel = new WordleGameService();

        // Assert
        Assert.Equal(Zielwort, spiel.TargetWord);
    }

    [Fact]
    public void AddLetter_fuellt_die_Kacheln_der_Reihe_nach()
    {
        // Arrange
        var spiel = new WordleGameService();

        // Act
        Tippe(spiel, "SPI");

        // Assert
        Assert.Equal("SPI__", Zeile(spiel, 0));
        Assert.Equal(TileState.Filled, spiel.Tiles[0][0].State);
        Assert.Equal(TileState.Empty, spiel.Tiles[0][3].State);
    }

    [Fact]
    public void Mehr_als_fuenf_Buchstaben_werden_ignoriert()
    {
        // Arrange
        var spiel = new WordleGameService();

        // Act
        Tippe(spiel, "SPIELE");

        // Assert
        Assert.Equal("SPIEL", Zeile(spiel, 0));
    }

    [Fact]
    public void Backspace_loescht_den_letzten_Buchstaben()
    {
        // Arrange
        var spiel = new WordleGameService();
        Tippe(spiel, "SPI");

        // Act
        spiel.Backspace();

        // Assert
        Assert.Equal("SP___", Zeile(spiel, 0));
        Assert.Equal(TileState.Empty, spiel.Tiles[0][2].State);
    }

    [Fact]
    public void Backspace_auf_leerer_Zeile_tut_nichts()
    {
        // Arrange
        var spiel = new WordleGameService();

        // Act
        spiel.Backspace();

        // Assert
        Assert.Equal("_____", Zeile(spiel, 0));
    }

    [Fact]
    public async Task Unvollstaendige_Zeile_kann_nicht_abgeschickt_werden()
    {
        // Arrange
        var spiel = new WordleGameService();
        Tippe(spiel, "SPI");

        // Act
        await spiel.SubmitGuessAsync();

        // Assert: die Zeile steht unveraendert, es wurde nicht bewertet
        Assert.Equal("SPI__", Zeile(spiel, 0));
        Assert.Equal(TileState.Filled, spiel.Tiles[0][0].State);
    }

    [Fact]
    public async Task Richtiges_Wort_gewinnt_das_Spiel()
    {
        // Arrange
        var spiel = new WordleGameService();
        Tippe(spiel, Zielwort);

        // Act
        await spiel.SubmitGuessAsync();

        // Assert
        Assert.True(spiel.IsGameWon);
        Assert.False(spiel.IsGameLost);
        Assert.True(spiel.IsBlocked());
    }

    [Fact]
    public async Task Nach_sechs_Fehlversuchen_ist_das_Spiel_verloren()
    {
        // Arrange
        var spiel = new WordleGameService();

        // Act
        for (var runde = 0; runde < 6; runde++)
        {
            Tippe(spiel, FalscherVersuch);
            await spiel.SubmitGuessAsync();
        }

        // Assert
        Assert.True(spiel.IsGameLost);
        Assert.False(spiel.IsGameWon);
        Assert.True(spiel.IsBlocked());
    }

    [Fact]
    public async Task Nach_fuenf_Fehlversuchen_laeuft_das_Spiel_noch()
    {
        // Arrange
        var spiel = new WordleGameService();

        // Act
        for (var runde = 0; runde < 5; runde++)
        {
            Tippe(spiel, FalscherVersuch);
            await spiel.SubmitGuessAsync();
        }

        // Assert
        Assert.False(spiel.IsGameLost);
        Assert.False(spiel.IsBlocked());
    }

    [Fact]
    public async Task Nach_Spielende_wird_keine_Eingabe_mehr_angenommen()
    {
        // Arrange: Spiel durch das richtige Wort beenden
        var spiel = new WordleGameService();
        Tippe(spiel, Zielwort);
        await spiel.SubmitGuessAsync();

        // Act
        Tippe(spiel, "MOTOR");

        // Assert
        Assert.Equal("_____", Zeile(spiel, 1));
    }

    [Fact]
    public async Task Grau_markierte_Buchstaben_lassen_sich_nicht_erneut_eingeben()
    {
        // Arrange: MOTOR teilt keinen Buchstaben mit SPIEL, danach sind alle grau
        var spiel = new WordleGameService();
        Tippe(spiel, "MOTOR");
        await spiel.SubmitGuessAsync();

        // Act
        Tippe(spiel, "MOTOR");

        // Assert
        Assert.Equal("_____", Zeile(spiel, 1));
    }

    [Fact]
    public async Task Tastatur_uebernimmt_den_besten_bekannten_Zustand()
    {
        // Arrange
        var spiel = new WordleGameService();
        Tippe(spiel, "MOTOR");

        // Act
        await spiel.SubmitGuessAsync();

        // Assert
        var m = spiel.Keys.Single(k => k.Label == "M");
        var s = spiel.Keys.Single(k => k.Label == "S");

        Assert.Equal(KeyState.Absent, m.State);
        Assert.Equal(KeyState.Neutral, s.State);
    }

    [Fact]
    public async Task Zustandsaenderungen_loesen_ein_Ereignis_aus()
    {
        // Arrange
        var spiel = new WordleGameService();
        var aufrufe = 0;
        spiel.OnStateChanged += () => aufrufe++;

        // Act
        spiel.AddLetter('S');
        await spiel.SubmitGuessAsync();

        // Assert
        Assert.True(aufrufe > 0);
    }
}
