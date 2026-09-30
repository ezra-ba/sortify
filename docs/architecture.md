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
│   └── Sortify.Core.Tests/      Tests der fachlichen Logik
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
