# Wordle - Blazor Server Edition

Ein Wordle-Klon entwickelt mit **Blazor Server** im Rahmen eines IPRO-Projekts.

## 📋 Projektübersicht

Dieses Projekt ist eine Implementierung des beliebten Wortspiels Wordle als Web-Anwendung. Spieler versuchen, ein verstecktes Wort innerhalb von 6 Versuchen zu erraten, wobei sie nach jedem Versuch farbcodiertes Feedback erhalten.

## 🎯 Projekt-Milestones

Das Projekt folgt einem strukturierten Entwicklungsplan mit 6 Milestones: [Milestones.md](./Milestones.md)

## 🛠️ Tech-Stack

- **Framework:** Blazor Server (.NET 10.0)
- **Sprache:** C#
- **Frontend:** Blazor Components, Razor, CSS
- **Backend:** ASP.NET Core
- **ORM:** Entity Framework Core
- **Datenbank:** SQLite (Datei `wordle.db`, wird beim Start automatisch angelegt)
- **Deployment:** Render.com (Docker)
- **Containerisierung:** Docker

## 🛠️ Setup & Installation

### Voraussetzungen
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) oder höher
- Ein Code-Editor (z.B. Visual Studio, VS Code, Rider)
- Optional: [Docker](https://www.docker.com/get-started), um den Container lokal zu testen

### Projekt lokal starten

1. **Repository klonen:**
   ```bash
   git clone <repository-url>
   cd Wordle_Ipro
   ```

2. **In das Blazor-Projekt wechseln:**
   ```bash
   cd BlazorServerApp
   ```

3. **Abhängigkeiten wiederherstellen:**
   ```bash
   dotnet restore
   ```

4. **Anwendung starten:**
   ```bash
   dotnet run
   ```
   Die Datenbank wird beim ersten Start via `EnsureCreatedAsync()` automatisch
   angelegt und mit den Seed-Wörtern befüllt. Es ist kein separater
   Migrations-Schritt nötig.

5. **Im Browser öffnen:**
   - Die Anwendung läuft standardmäßig auf `http://localhost:5066`
   - Die URL wird beim Start in der Konsole angezeigt

### Entwicklungsmodus mit Hot Reload

Für die Entwicklung empfiehlt sich der Watch-Modus:
```bash
dotnet watch
```
Änderungen am Code werden automatisch erkannt und die Anwendung neu geladen.

### Datenbank zurücksetzen

Die SQLite-Datei einfach löschen — sie wird beim nächsten Start neu erzeugt:
```bash
rm BlazorServerApp/wordle.db
```

### Container lokal testen

Das Image entspricht 1:1 dem, was Render baut:
```bash
docker build -t wordle-ipro .
```
```bash
docker run --rm -e PORT=10000 -p 10000:10000 wordle-ipro
```
Danach ist die App unter `http://localhost:10000` erreichbar.

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

## 🐳 Deployment auf Render.com

Die App wird als Docker-Container deployt. Die relevanten Dateien liegen im
Repo-Root: [`Dockerfile`](./Dockerfile), [`.dockerignore`](./.dockerignore) und
[`render.yaml`](./render.yaml).

### Vorgehen

1. Änderungen nach GitHub pushen (Render deployt vom `main`-Branch).
2. Auf [dashboard.render.com](https://dashboard.render.com) → **New** → **Blueprint**
   → das Repo `Wordle_Ipro` auswählen. Render liest `render.yaml` und legt den
   Web-Service an.
3. Nach dem ersten Build ist die App unter
   `https://<service-name>.onrender.com` erreichbar.

Alternativ ohne Blueprint: **New** → **Web Service** → Repo wählen →
Language auf **Docker** stellen → Region `Frankfurt` → Plan `Free`.

### Was im Code dafür nötig war

| Thema | Lösung |
|---|---|
| Port | Render gibt den Port per `PORT`-Env-Variable vor. `Program.cs` liest sie aus und bindet auf `0.0.0.0:$PORT`. |
| HTTPS | Render terminiert TLS in einem Reverse Proxy. Ohne `UseForwardedHeaders()` sähe die App jeden Request als `http` und `UseHttpsRedirection()` würde eine Redirect-Schleife erzeugen. |
| Datenbankpfad | Über die Env-Variable `ConnectionStrings__WordleDb` konfigurierbar (im Container: `/app/data/wordle.db`). |

### ⚠️ Einschränkung des Free-Plans

Der Free-Plan hat **kein persistentes Dateisystem** und fährt den Container nach
15 Minuten ohne Traffic herunter. Beim nächsten Aufruf startet er neu — die
SQLite-Datei und damit alle Statistiken sind dann wieder leer. Ausserdem dauert
der erste Aufruf nach dem Spin-Down ca. 30–60 Sekunden.

Für dauerhafte Persistenz gäbe es zwei Wege:
- eine **Render-Disk** unter `/app/data` mounten (setzt einen bezahlten Plan voraus), oder
- auf **Render Postgres** wechseln und `ConnectionStrings__WordleDb` entsprechend setzen.

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
