using BlazorServerApp.Services.Wordle;

namespace BlazorServerApp.Tests;

/// <summary>
/// Tests fuer den Import und die Abfrage der Wortliste. Laeuft gegen eine echte
/// SQLite-Datenbank, damit auch der eindeutige Index geprueft wird.
/// </summary>
public class WordListServiceTests : IDisposable
{
    private readonly TestDatabase _datenbank = new();
    private readonly WordListService _dienst;

    public WordListServiceTests()
    {
        _dienst = new WordListService(_datenbank.Context);
    }

    public void Dispose() => _datenbank.Dispose();

    [Fact]
    public async Task Import_uebernimmt_gueltige_Woerter()
    {
        // Arrange
        const string eingabe = "APFEL\nBLUME\nKATZE";

        // Act
        var ergebnis = await _dienst.ImportAsync(eingabe, replaceExisting: false);

        // Assert
        Assert.Equal(3, ergebnis.Added);
        Assert.Equal(0, ergebnis.Rejected);
        Assert.Equal(3, ergebnis.TotalAfterImport);
    }

    [Fact]
    public async Task Import_wandelt_in_Grossbuchstaben_um()
    {
        // Arrange
        const string eingabe = "apfel";

        // Act
        await _dienst.ImportAsync(eingabe, replaceExisting: false);

        // Assert
        Assert.Equal(["APFEL"], await _dienst.GetAllAsync());
    }

    [Theory]
    [InlineData("APFEL,BLUME,KATZE")]
    [InlineData("APFEL;BLUME;KATZE")]
    [InlineData("APFEL BLUME KATZE")]
    [InlineData("APFEL\nBLUME\r\nKATZE")]
    public async Task Import_akzeptiert_verschiedene_Trennzeichen(string eingabe)
    {
        // Arrange: Eingabe kommt aus den InlineData-Faellen

        // Act
        var ergebnis = await _dienst.ImportAsync(eingabe, replaceExisting: false);

        // Assert
        Assert.Equal(3, ergebnis.Added);
    }

    [Fact]
    public async Task Import_ignoriert_Kommentarzeilen()
    {
        // Arrange
        const string eingabe = "# Kommentar\nAPFEL\n# noch einer";

        // Act
        var ergebnis = await _dienst.ImportAsync(eingabe, replaceExisting: false);

        // Assert
        Assert.Equal(1, ergebnis.Added);
        Assert.Equal(0, ergebnis.Rejected);
    }

    [Theory]
    [InlineData("HAI")]          // zu kurz
    [InlineData("MAULWURF")]     // zu lang
    [InlineData("GRUENE")]       // sechs Buchstaben
    [InlineData("GRÜNE")]        // Umlaut
    [InlineData("AB1EL")]        // Ziffer
    public async Task Import_lehnt_ungueltige_Eintraege_ab(string eingabe)
    {
        // Arrange: Eingabe kommt aus den InlineData-Faellen

        // Act
        var ergebnis = await _dienst.ImportAsync(eingabe, replaceExisting: false);

        // Assert
        Assert.Equal(0, ergebnis.Added);
        Assert.Equal(1, ergebnis.Rejected);
        Assert.Contains(eingabe, ergebnis.RejectedSamples);
    }

    [Fact]
    public async Task Import_zaehlt_Doppelte_innerhalb_derselben_Eingabe_nur_einmal()
    {
        // Arrange
        const string eingabe = "APFEL\nAPFEL\nAPFEL";

        // Act
        var ergebnis = await _dienst.ImportAsync(eingabe, replaceExisting: false);

        // Assert
        Assert.Equal(1, ergebnis.Added);
        Assert.Equal(0, ergebnis.Duplicates);
    }

    [Fact]
    public async Task Bereits_vorhandene_Woerter_werden_als_Duplikat_gemeldet()
    {
        // Arrange
        await _dienst.ImportAsync("APFEL", replaceExisting: false);

        // Act
        var ergebnis = await _dienst.ImportAsync("APFEL\nBLUME", replaceExisting: false);

        // Assert
        Assert.Equal(1, ergebnis.Added);
        Assert.Equal(1, ergebnis.Duplicates);
        Assert.Equal(2, ergebnis.TotalAfterImport);
    }

    [Fact]
    public async Task Ersetzen_leert_die_bestehende_Liste()
    {
        // Arrange
        await _dienst.ImportAsync("APFEL\nBLUME", replaceExisting: false);

        // Act
        var ergebnis = await _dienst.ImportAsync("KATZE", replaceExisting: true);

        // Assert
        Assert.Equal(1, ergebnis.Added);
        Assert.True(ergebnis.Replaced);
        Assert.Equal(["KATZE"], await _dienst.GetAllAsync());
    }

    [Fact]
    public async Task Ergaenzen_behaelt_die_bestehende_Liste()
    {
        // Arrange
        await _dienst.ImportAsync("APFEL", replaceExisting: false);

        // Act
        await _dienst.ImportAsync("BLUME", replaceExisting: false);

        // Assert
        Assert.Equal(["APFEL", "BLUME"], await _dienst.GetAllAsync());
    }

    [Fact]
    public async Task ExistsAsync_erkennt_enthaltene_und_fehlende_Woerter()
    {
        // Arrange
        await _dienst.ImportAsync("APFEL", replaceExisting: false);

        // Act
        var vorhanden = await _dienst.ExistsAsync("APFEL");
        var fehlend = await _dienst.ExistsAsync("BLUME");

        // Assert
        Assert.True(vorhanden);
        Assert.False(fehlend);
    }

    [Fact]
    public async Task Zufallswort_stammt_aus_der_Liste()
    {
        // Arrange
        await _dienst.ImportAsync("APFEL\nBLUME\nKATZE", replaceExisting: false);

        // Act
        var wort = await _dienst.GetRandomWordAsync();

        // Assert
        Assert.Contains(wort, new[] { "APFEL", "BLUME", "KATZE" });
    }

    [Fact]
    public async Task Zufallswort_ist_null_wenn_die_Liste_leer_ist()
    {
        // Arrange: die Datenbank ist frisch und damit leer

        // Act
        var wort = await _dienst.GetRandomWordAsync();

        // Assert
        Assert.Null(wort);
    }

    [Fact]
    public async Task Leere_Eingabe_aendert_nichts()
    {
        // Arrange
        const string eingabe = "   ";

        // Act
        var ergebnis = await _dienst.ImportAsync(eingabe, replaceExisting: false);

        // Assert
        Assert.Equal(0, ergebnis.Added);
        Assert.Equal(0, ergebnis.Rejected);
        Assert.Empty(await _dienst.GetAllAsync());
    }
}
