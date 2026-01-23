# KI-Nutzung im Wordle-Projekt

## Transparenzerklärung

Dieses Dokument beschreibt, wie Künstliche Intelligenz während der Entwicklung dieses Wordle-Projekts eingesetzt wurde.

## Rolle der KI & Verantwortung des Entwicklers

Ich nutze KI als **Pair-Programmer**, um die Entwicklung zu beschleunigen und Implementierungsmuster vorzuschlagen. Allerdings evaluiere und überarbeite ich alle KI-generierten Inhalte sorgfältig.

Dieser Workflow stellt einen Paradigmenwechsel dar: von "jede Zeile selbst schreiben" hin zu "High-Level-Architektur orchestrieren und verfeinern". Dies erfordert erheblichen Aufwand für architektonische Entscheidungen, Code-Review, Debugging und Integration verschiedener Komponenten.

## Eingesetzte KI-Tools

- **GPT-4o (5.2 Thinking)**: Für konzeptionelle Unterstützung und Problemlösung
- **Antigravity**: KI-Coding-Assistent für Refactoring und Code-Generierung

## Spezifische Verwendungsbeispiele

### Code-Entwicklung
- **Blazor Components**: Hauptsächlich selbst erstellt, gelegentlich mit Hilfe von GPT-4o bei komplexen Problemen
- **Code-Refactoring**: Mit Antigravity durchgeführt (z.B. Extraktion von Business-Logik in Services)
- Alle KI-Vorschläge wurden kritisch geprüft und an die Projektanforderungen angepasst

### Dokumentation
Alle Markdown-Dateien (README.md, Milestones.md, GameRules.md, AI_USAGE.md) wurden mit KI-Unterstützung erstellt:
- Ich gebe Struktur und Anforderungen vor
- KI erstellt den Entwurf
- Ich überprüfe und passe den Inhalt an

### Architektur & Design
Grundlegende Entscheidungen (Blazor Server, EF Core, Docker) stammen von mir. KI diente als Sparringspartner zur Validierung von Best Practices.

### Datenbank & Deployment
- Datenbankschema und Models selbst konzipiert
- Docker-Konfiguration teilweise KI-unterstützt
- KI-Hilfe bei spezifischen EF Core-Fragen

### Debugging
KI wurde zur Analyse komplexer Fehlermeldungen verwendet. Finale Debugging-Entscheidungen und Fixes wurden von mir implementiert.

## Akademische Integrität

Ich bestätige:
- ✅ Ich habe die Architektur und Struktur des Projekts selbst entworfen
- ✅ Ich verstehe alle KI-generierten Inhalte und kann sie erklären
- ✅ Alle KI-Vorschläge wurden aktiv reviewt, nicht blind übernommen
- ✅ KI wurde als Werkzeug zur Produktivitätssteigerung genutzt, nicht als Ersatz für eigenes Denken

## Fazit

Der Einsatz von KI entspricht modernen Entwicklungspraktiken. Entscheidend ist, dass ich als Entwickler die Kontrolle, das Verständnis und die Verantwortung für den gesamten Code behalte. Die Nutzung von KI ermöglicht es mir, mich auf komplexere Probleme zu konzentrieren und architektonische Kompetenzen zu entwickeln.

---

**Projekt**: Wordle - Blazor Server Edition | **Kontext**: IPRO-Projekt | **Datum**: Januar 2026
