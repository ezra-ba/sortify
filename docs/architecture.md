# Sortify Architecture

## Ziele

Die Struktur trennt Benutzeroberfläche, fachliche Regeln und technische Zugriffe. Dadurch können die Sortierregeln unabhängig von Avalonia und vom echten Dateisystem getestet werden.

## Projekte

```text
Sortify.sln
├── src/
│   ├── Sortify.App/             Avalonia UI, Views, ViewModels, App-Start
│   ├── Sortify.Core/            Modelle, Regeln und Schnittstellen
│   └── Sortify.Infrastructure/  Dateisystem, Überwachung und Speicherung
├── tests/
│   ├── Sortify.Core.Tests/      Tests der fachlichen Logik
│   ├── Sortify.Infrastructure.Tests/ Speicherung und technische Dienste
│   └── Sortify.App.Tests/       ViewModels und Headless-Popup-Integration
├── assets/branding/             Zentrale Logo- und Icon-Dateien
└── docs/                        Architektur und technische Entscheidungen
```

## Abhängigkeiten

```text
Sortify.App ──────────────> Sortify.Core
     │
     └────> Sortify.Infrastructure ─────> Sortify.Core

Sortify.Core.Tests ───────> Sortify.Core
```

`Sortify.Core` kennt weder Avalonia noch konkrete Dateisystem- oder Speicherklassen. `Sortify.App` verbindet beim Programmstart die fachlichen Schnittstellen mit den Implementierungen aus `Sortify.Infrastructure`.

## Geplante Ordner

| Projekt | Ordner | Inhalt |
|---|---|---|
| `Sortify.App` | `Views`, `ViewModels` | Fenster, Dialoge und UI-Zustand |
| `Sortify.Core` | `Models`, `Services`, `Rules` | Fachliche Daten und Sortierlogik |
| `Sortify.Infrastructure` | `Monitoring`, `FileSystem`, `Persistence` | Ordnerüberwachung, Dateioperationen und lokale Speicherung |
| `Sortify.Core.Tests` | nach Fachbereich | Tests für Regeln, Vorschläge und Konfliktfälle |

## Branding

- `sortify-icon.png`: Fenster- und Anwendungsicon
- `sortify-logo.png`: App-Header und Repository-README
- `syd-firmen-logo.png`: Firmenkennzeichnung in README und Projektdokumentation

Die Originaldateien liegen nur unter `assets/branding`. Das Avalonia-Projekt bindet benötigte Dateien als verlinkte Ressourcen ein, damit keine doppelten Versionen entstehen.

## Lokale Speicherung

Die Speicherung aus Issue #28 speichert Konfiguration und Regeln gemeinsam in einer versionierten JSON-Datei. Der Arbeitsstand für Issue #37 ergänzt frei wählbare Target-Ordner, Listenreihenfolge und ein optionales Standardziel in Formatversion 2. Komponenten, Speicherort, Fehlerverhalten und offene Integrationsnachweise beschreibt [settings-persistence.md](settings-persistence.md).

`TargetFolder` ist ein Core-Modell mit stabiler ID, Pfad und Spitznamen. `TargetFolderViewModel` enthält zusätzlich ausschließlich UI-Zustände. Die Regelzuordnung über Target-IDs ist eine separate Erweiterung; die bisherigen relativen Regelziele werden durch #37 noch nicht umgedeutet.

## Dateierkennung und Popup

Die erste Umsetzung zu Issue #29 verbindet einen `IFileMonitor` mit einem austauschbaren `IFileSuggestionProvider` und einem `IDownloadPresenter`. Der Ablauf und die Grenzen sind in [download-detection.md](download-detection.md) beschrieben. Dateisystemzugriffe und die Verarbeitung liegen in Infrastructure; der Avalonia-Presenter zeigt Vorschläge auf dem UI-Thread. Eine spätere Regelengine- oder KI-Anbindung ersetzt den Vorschlagsanbieter, ohne die Ordnerüberwachung ändern zu müssen.
