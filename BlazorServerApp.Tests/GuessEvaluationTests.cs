using BlazorServerApp.Models.Wordle;
using BlazorServerApp.Services.Wordle;

namespace BlazorServerApp.Tests;

/// <summary>
/// Tests fuer die Bewertung eines Rateversuchs. Das ist der Kern des Spiels und
/// die einzige Stelle mit wirklich kniffliger Logik, weil mehrfach vorkommende
/// Buchstaben ein Kontingent verbrauchen.
/// </summary>
public class GuessEvaluationTests
{
    /// <summary>Verdichtet das Ergebnis zu einem lesbaren Muster: G gruen, Y gelb, - grau.</summary>
    private static string Muster(TileState[] zustaende) =>
        string.Concat(zustaende.Select(z => z switch
        {
            TileState.Correct => 'G',
            TileState.Present => 'Y',
            TileState.Absent => '-',
            _ => '?'
        }));

    [Fact]
    public void Richtiges_Wort_ergibt_durchgehend_gruen()
    {
        // Arrange
        const string ziel = "SPIEL";
        const string versuch = "SPIEL";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert
        Assert.Equal("GGGGG", Muster(ergebnis));
    }

    [Fact]
    public void Kein_gemeinsamer_Buchstabe_ergibt_durchgehend_grau()
    {
        // Arrange
        const string ziel = "SPIEL";
        const string versuch = "MOTOR";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert
        Assert.Equal("-----", Muster(ergebnis));
    }

    [Fact]
    public void Buchstaben_an_falscher_Stelle_werden_gelb()
    {
        // Arrange: P, I, L und E kommen in SPIEL vor, aber an anderer Stelle
        const string ziel = "SPIEL";
        const string versuch = "PILZE";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert
        Assert.Equal("YYY-Y", Muster(ergebnis));
    }

    [Fact]
    public void Zu_oft_geratener_Buchstabe_wird_nur_so_oft_eingefaerbt_wie_vorhanden()
    {
        // Arrange: STERN enthaelt ein E, EBENE raet drei
        const string ziel = "STERN";
        const string versuch = "EBENE";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert: nur das korrekt platzierte E wird gruen, die anderen bleiben grau
        Assert.Equal("--GY-", Muster(ergebnis));
    }

    [Fact]
    public void Exakter_Treffer_hat_Vorrang_vor_frueherem_Vorkommen()
    {
        // Arrange: das erste E steht vor dem korrekt platzierten
        const string ziel = "STERN";
        const string versuch = "EBENE";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert: das Kontingent geht an den exakten Treffer, nicht an das erste E
        Assert.Equal(TileState.Absent, ergebnis[0]);
        Assert.Equal(TileState.Correct, ergebnis[2]);
    }

    [Fact]
    public void Doppelter_Buchstabe_im_Zielwort_wird_zweimal_eingefaerbt()
    {
        // Arrange: WOLLE enthaelt zwei L, LILIE raet zwei
        const string ziel = "WOLLE";
        const string versuch = "LILIE";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert: eines richtig platziert, eines an falscher Stelle
        Assert.Equal("Y-G-G", Muster(ergebnis));
    }

    [Fact]
    public void Beide_Vorkommen_gelb_wenn_keines_richtig_steht()
    {
        // Arrange: HECKE enthaelt zwei E, EIMER raet zwei, keines an richtiger Stelle
        const string ziel = "HECKE";
        const string versuch = "EIMER";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert
        Assert.Equal("Y--Y-", Muster(ergebnis));
    }

    [Theory]
    [InlineData("SPIEL", "SPIEL", "GGGGG")]
    [InlineData("MOTOR", "SPIEL", "-----")]
    [InlineData("LEIPS", "SPIEL", "YYGYY")]  // das I steht zufaellig richtig
    [InlineData("SPEIL", "SPIEL", "GGYYG")]
    public void Bewertung_liefert_erwartetes_Muster(string versuch, string ziel, string erwartet)
    {
        // Arrange: Eingaben kommen aus den InlineData-Faellen

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert
        Assert.Equal(erwartet, Muster(ergebnis));
    }

    [Fact]
    public void Bewertung_hat_immer_so_viele_Felder_wie_das_Wort_Buchstaben()
    {
        // Arrange
        const string ziel = "SPIEL";
        const string versuch = "SPIEL";

        // Act
        var ergebnis = WordleGameService.EvaluateGuess(guess: versuch, target: ziel);

        // Assert
        Assert.Equal(WordleRules.WordLength, ergebnis.Length);
    }
}
