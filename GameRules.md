# 🎮 Wordle - Spielregeln

## Spielziel

Das Ziel von Wordle ist es, ein verstecktes Wort innerhalb von **maximal 6 Versuchen** zu erraten. Nach jedem Versuch erhältst du farbcodiertes Feedback, das dir hilft, das richtige Wort zu finden.

## 📖 Grundregeln

### 1. Wortlänge
- Das zu erratende Wort besteht aus **5 Buchstaben**
- Alle Wörter sind gültige deutsche Wörter

### 2. Anzahl der Versuche
- Du hast **maximal 6 Versuche**, um das Wort zu erraten
- Jeder Versuch muss ein gültiges 5-Buchstaben-Wort sein
- Ungültige Wörter werden nicht akzeptiert

### 3. Feedback-System

Nach jedem Versuch wird jeder Buchstabe farblich markiert:

#### 🟩 Grün - Richtige Position
- Der Buchstabe ist **richtig** und steht an der **richtigen Position** im Wort
- Beispiel: Wenn das gesuchte Wort "HAUS" ist und du "HALS" eingibst, werden **H**, **A** und **S** grün markiert

#### 🟨 Gelb - Falsche Position
- Der Buchstabe kommt im Wort **vor**, steht aber an der **falschen Position**
- Beispiel: Wenn das gesuchte Wort "HAUS" ist und du "SAFT" eingibst, wird **S** und **A** gelb markiert

#### ⬜ Grau - Nicht vorhanden
- Der Buchstabe kommt **nicht** im gesuchten Wort vor
- Du kannst diesen Buchstaben in zukünftigen Versuchen ausschließen

## 🎯 Spielablauf

1. **Start**: Ein zufälliges 5-Buchstaben-Wort wird vom System ausgewählt
2. **Eingabe**: Gib dein erstes Ratewort ein (5 Buchstaben)
3. **Feedback**: Betrachte die Farbmarkierungen deiner Buchstaben
4. **Strategie**: Nutze die gewonnenen Informationen für den nächsten Versuch
5. **Wiederholung**: Wiederhole Schritte 2-4 bis zu 6 Mal
6. **Ende**: 
   - ✅ **Gewonnen**: Du hast das Wort innerhalb von 6 Versuchen erraten
   - ❌ **Verloren**: Nach 6 Versuchen wurde das Wort nicht erraten

## 💡 Tipps & Strategien

### Für Anfänger
1. **Starte mit einem Wort mit vielen Vokalen** (z.B. "AUDIO", "REISE")
2. **Nutze häufige Konsonanten** in den ersten Versuchen (z.B. N, R, S, T)
3. **Eliminiere Buchstaben**: Graue Buchstaben kannst du ausschließen
4. **Positioniere gelbe Buchstaben um**: Sie sind im Wort, aber an falscher Stelle

### Fortgeschrittene Strategien
1. **Verwende unterschiedliche Buchstaben** in den ersten Versuchen
2. **Achte auf Buchstabenhäufigkeit** im Deutschen
3. **Denke an Wortstämme und Endungen** (z.B. -EN, -ER, -EL)
4. **Nutze gelbe Hinweise effektiv**: Teste verschiedene Positionen systematisch

## 📊 Punktesystem (optional)

Falls implementiert, könnte das Spiel folgende Bewertung nutzen:

- **1. Versuch**: 🏆 Perfekt! (6 Punkte)
- **2. Versuch**: 🌟 Ausgezeichnet! (5 Punkte)
- **3. Versuch**: 👍 Sehr gut! (4 Punkte)
- **4. Versuch**: ✅ Gut! (3 Punkte)
- **5. Versuch**: 👌 In Ordnung (2 Punkte)
- **6. Versuch**: 💪 Geschafft! (1 Punkt)
- **Nicht erraten**: ❌ Nächstes Mal! (0 Punkte)

## 🎲 Beispiel-Runde

**Gesuchtes Wort**: STEIN (dem Spieler unbekannt)

| Versuch | Eingabe | Feedback | Erklärung |
|---------|---------|----------|-----------|
| 1 | AUDIO | ⬜⬜⬜🟨⬜ | Nur das I kommt vor, aber nicht an Position 4 |
| 2 | LISTE | ⬜🟨🟩🟨🟩 | I an falscher Stelle, S und E richtig positioniert, T kommt vor |
| 3 | STEIN | 🟩🟩🟩🟩🟩 | ✅ Gewonnen! |

## ❗ Wichtige Hinweise

- **Nur gültige Wörter**: Das System akzeptiert nur Wörter aus dem deutschen Wörterbuch
- **Groß-/Kleinschreibung**: Spielt keine Rolle - du kannst in Groß- oder Kleinbuchstaben eingeben
- **Ein Wort pro Tag**: Im klassischen Wordle-Modus gibt es ein neues Wort pro Tag
- **Keine Umlaute**: Je nach Implementierung werden ä, ö, ü zu ae, oe, ue

## 🏁 Spielende

Das Spiel endet, wenn:
1. Du das richtige Wort errätst ✅
2. Du alle 6 Versuche aufgebraucht hast ❌

Nach Spielende wird das korrekte Wort angezeigt und du kannst:
- Deine Statistik einsehen
- Eine neue Runde starten
- Deine Ergebnisse teilen (falls implementiert)

---

**Viel Erfolg beim Rätseln! 🎉**
