# Wordle - Blazor Server Edition

Ein Wordle-Klon entwickelt mit **Blazor Server** im Rahmen eines IPRO-Projekts.

## 📋 Projektübersicht

Dieses Projekt ist eine Implementierung des beliebten Wortspiels Wordle als Web-Anwendung. Spieler versuchen, ein verstecktes Wort innerhalb von 6 Versuchen zu erraten, wobei sie nach jedem Versuch farbcodiertes Feedback erhalten.

## 🎯 Projekt-Milestones

Das Projekt folgt einem strukturierten Entwicklungsplan mit 6 Milestones:

### ✅ Milestone 1 — Projekt-Setup & Tech-Stack
- **Status:** Abgeschlossen
## 🛠️ Tech-Stack

- **Framework:** Blazor Server (.NET 10.0)
- **Sprache:** C#
- **Frontend:** Blazor Components, Razor, CSS
- **Backend:** ASP.NET Core
- **ORM:** Entity Framework Core
- **Datenbank:** MS SQL Server (Docker Container)
- **Deployment:** Linux Server
- **Containerisierung:** Docker & Docker Compose

### 🔄 Milestone 2 — Funktionalitäten definieren
- **Status:** In Planung
- **Ziele:**
  - Feature-Liste erstellen und priorisieren
  - Spielregeln definieren
  - Wortlänge, Anzahl Versuche, Bewertungslogik festlegen

### 📝 Milestone 3 — MVP: Basic UI + Doku + Testing
- **Ziele:**
  - Grundlegende Benutzeroberfläche implementieren
  - Spielfeld mit Eingabemöglichkeiten
  - Erste Tests einrichten
  - Setup-Dokumentation erweitern

### ⚙️ Milestone 4 — Workflow implementieren
- **Ziele:**
  - Kernspiellogik implementieren
  - Worteingabe und -validierung
  - Feedback-System (grün/gelb/grau)
  - Win/Loss-Bedingungen

### 📊 Milestone 5 — Statistik + Datenbank
- **Ziele:**
  - Spielstatistiken erfassen (Siege, Versuche, Streak)
  - Datenbank-Integration
  - Persistierung von Spielergebnissen

### 🚀 Milestone 6 — Deployment & Präsentation
- **Ziele:**
  - Deployment-Strategie festlegen
  - Anwendung deployen
  - Demo-Material erstellen

## 🛠️ Setup & Installation

### Voraussetzungen
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) oder höher
- [Docker](https://www.docker.com/get-started) und Docker Compose
- Ein Code-Editor (z.B. Visual Studio, VS Code, Rider)

### Projekt lokal starten

1. **Repository klonen:**
   ```bash
   git clone <repository-url>
   cd Wordle_Ipro
   ```

2. **MS SQL Server Container starten:**
   ```bash
   docker-compose up -d
   ```
   Dies startet einen MS SQL Server Container im Hintergrund.

3. **In das Blazor-Projekt wechseln:**
   ```bash
   cd BlazorServerApp
   ```

4. **Abhängigkeiten wiederherstellen:**
   ```bash
   dotnet restore
   ```

5. **Datenbank-Migrationen anwenden:**
   ```bash
   dotnet ef database update
   ```

6. **Anwendung starten:**
   ```bash
   dotnet run
   ```

7. **Im Browser öffnen:**
   - Die Anwendung läuft standardmäßig auf `https://localhost:5001` oder `http://localhost:5000`
   - Die URL wird beim Start in der Konsole angezeigt

### Entwicklungsmodus mit Hot Reload

Für die Entwicklung empfiehlt sich der Watch-Modus:
```bash
dotnet watch
```
Änderungen am Code werden automatisch erkannt und die Anwendung neu geladen.

### Docker Container verwalten

**Container stoppen:**
```bash
docker-compose down
```

**Container-Logs anzeigen:**
```bash
docker-compose logs -f mssql
```

**Datenbank zurücksetzen:**
```bash
docker-compose down -v  # Löscht auch die Volumes
docker-compose up -d
```

## 📁 Projektstruktur

```
Wordle_Ipro/
├── BlazorServerApp/        # Hauptanwendung
│   ├── Components/         # Blazor-Komponenten
│   │   ├── Layout/        # Layout-Komponenten
│   │   └── Pages/         # Seiten/Routen
│   ├── Data/              # Entity Framework DbContext & Models (wird erstellt)
│   ├── Migrations/        # EF Core Migrationen (wird erstellt)
│   ├── wwwroot/           # Statische Dateien (CSS, JS, Bilder)
│   ├── Program.cs         # Einstiegspunkt
│   └── appsettings.json   # Konfiguration & Connection String
├── Milestones.md          # Projekt-Milestones
└── README.md              # Diese Datei
```

## 🎮 Spielregeln

> **Hinweis:** Die finalen Spielregeln werden in Milestone 2 definiert.

Geplante Grundregeln:
- Errate das versteckte Wort in maximal 6 Versuchen
- Jeder Versuch muss ein gültiges Wort sein
- Nach jedem Versuch erhältst du farbcodiertes Feedback:
  - 🟩 **Grün:** Buchstabe ist korrekt und an der richtigen Position
  - 🟨 **Gelb:** Buchstabe ist im Wort, aber an der falschen Position
  - ⬜ **Grau:** Buchstabe kommt nicht im Wort vor

## 🧪 Testing

> **Hinweis:** Test-Strategie wird in Milestone 3 definiert.

Geplante Test-Arten:
- Unit Tests für Spiellogik
- Integration Tests für Komponenten
- End-to-End Tests für User-Flows

## 🤝 Entwicklung

### Branching-Strategie
- `main` - Produktions-Branch
- `develop` - Entwicklungs-Branch
- `feature/*` - Feature-Branches
- `bugfix/*` - Bugfix-Branches

### Commit-Konventionen
Verwende aussagekräftige Commit-Messages:
```
feat: Neue Funktion hinzufügen
fix: Fehler beheben
docs: Dokumentation aktualisieren
style: Code-Formatierung
refactor: Code-Refactoring
test: Tests hinzufügen/ändern
```

## 🐳 Deployment auf Linux Server

### Voraussetzungen auf dem Server
- Docker und Docker Compose installiert
- .NET 10.0 Runtime
- Ports 5000/5001 (Blazor App) und 1433 (MS SQL Server) verfügbar

### Deployment-Schritte

1. **Projekt auf den Server übertragen:**
   ```bash
   git clone <repository-url>
   cd Wordle_Ipro
   ```

2. **Umgebungsvariablen konfigurieren:**
   ```bash
   cp .env.example .env
   # .env bearbeiten und Passwörter setzen
   ```

3. **MS SQL Server Container starten:**
   ```bash
   docker-compose up -d
   ```

4. **Blazor App builden und starten:**
   ```bash
   cd BlazorServerApp
   dotnet publish -c Release -o ./publish
   dotnet ef database update
   dotnet ./publish/BlazorServerApp.dll
   ```

> **Hinweis:** Für Produktions-Deployment sollte ein Reverse Proxy (z.B. Nginx) und ein Process Manager (z.B. systemd) verwendet werden.

## 📚 Weitere Dokumentation

- **Milestones:** Siehe [Milestones.md](Milestones.md) für den detaillierten Projektplan
- [Blazor Dokumentation](https://learn.microsoft.com/de-de/aspnet/core/blazor/)
- [Entity Framework Core Dokumentation](https://learn.microsoft.com/de-de/ef/core/)
- [Docker Dokumentation](https://docs.docker.com/)
- [.NET Dokumentation](https://learn.microsoft.com/de-de/dotnet/)

## 📝 Lizenz

TBD

## 👥 Team

IPRO-Projekt Team

---

**Letztes Update:** Januar 2026
