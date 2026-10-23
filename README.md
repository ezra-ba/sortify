# Sortify

[![CI](https://github.com/ezra-ba/sortify/actions/workflows/ci.yml/badge.svg)](https://github.com/ezra-ba/sortify/actions/workflows/ci.yml)

<p align="center">
  <img src="assets/branding/sortify-logo.png" alt="Sortify" width="520">
</p>

Sortify ist eine geplante Desktop-Anwendung, die neue Dateien erkennt und Benutzer beim einheitlichen Benennen und Einsortieren unterstützt. Statt Dateien automatisch und unbemerkt zu verändern, zeigt Sortify zuerst einen verständlichen Vorschlag an. Erst nach einer Bestätigung wird die Datei umbenannt oder verschoben.

Das Projekt wird von **SYD – Sort Your Data GmbH** im Rahmen des Projektpraktikums entwickelt.

## Projektstatus

Sortify befindet sich in der Entwicklung. Ordnerauswahl und lokale Speicherung sind vorhanden. Neue Dateien im ausgewählten Ordner werden erkannt und mit echten Dateidaten im Popup angezeigt. Eine Prüfung auf vollständig abgeschlossene Downloads, Dateioperationen und die Anbindung der fachlichen Regelauswertung sind noch offen; der Einsortieren-Button ist deaktiviert.

## Problemstellung

Heruntergeladene Dateien landen häufig mit uneinheitlichen oder wenig aussagekräftigen Namen in einem gemeinsamen Ordner. Das manuelle Prüfen, Umbenennen und Verschieben kostet Zeit, wird leicht vergessen und muss für ähnliche Dateien immer wieder durchgeführt werden.

Sortify soll diesen Ablauf vereinfachen, ohne dem Benutzer die Kontrolle über seine Dateien zu nehmen.

## Geplanter Ablauf

1. Der Benutzer wählt einen Ordner aus, der beobachtet werden soll.
2. Sortify erkennt eine neue, vollständig verfügbare Datei.
3. Die Datei wird anhand der gespeicherten Regeln geprüft.
4. Sortify schlägt einen neuen Dateinamen und einen Zielordner vor.
5. Ein Dialog zeigt den aktuellen Namen, den neuen Namen und das Ziel an.
6. Der Benutzer bestätigt oder verwirft den Vorschlag.
7. Nur bestätigte Änderungen werden ausgeführt.

## MVP

Für die erste verwendbare Version sind folgende Kernfunktionen vorgesehen:

- überwachte Ordner hinzufügen und verwalten,
- Regeln für Dateinamen und Zielordner anlegen, bearbeiten und entfernen,
- neue Dateien erkennen und auf vollständige Verfügbarkeit warten,
- regelbasierte Sortiervorschläge erstellen,
- Vorschläge vor der Ausführung anzeigen,
- Vorschläge bestätigen oder ablehnen,
- bestätigte Dateien sicher umbenennen und verschieben,
- Einstellungen lokal speichern,
- Fehler verständlich anzeigen und bestehende Dateien vor unbeabsichtigtem Überschreiben schützen.

## Mögliche Erweiterungen

- Verlauf ausgeführter und abgelehnter Vorschläge,
- Rückgängig-Funktion,
- Profile für Schule, Privat und Arbeit,
- Prioritäten für Regeln,
- Duplikaterkennung,
- Import und Export von Regeln,
- Systembenachrichtigungen,
- automatischer Start mit dem Betriebssystem,
- Probelauf für ganze Ordner,
- Statistiken über sortierte Dateien.

## Technologie

- **C# und .NET** für die Anwendung
- **Avalonia UI** für die plattformübergreifende Desktop-Oberfläche
- **MVVM** zur Trennung von Oberfläche, Anwendungslogik und Daten
- **FileSystemWatcher** zur Erkennung neuer Dateien
- lokale Speicherung für Regeln und Einstellungen
- automatisierte Tests für Regelprüfung und Dateioperationen

Die erste Version ist als Desktop-Anwendung vorgesehen. Durch Avalonia bleibt eine spätere Nutzung auf mehreren Betriebssystemen möglich.

## Projektstruktur

```text
Sortify.sln
├── src/
│   ├── Sortify.App/             Avalonia-Oberfläche und MVVM
│   ├── Sortify.Core/            Modelle, Regeln und Schnittstellen
│   └── Sortify.Infrastructure/  Dateisystem und lokale Speicherung
├── tests/Sortify.Core.Tests/    Tests der fachlichen Logik
├── assets/branding/             Logos und App-Icon
└── docs/architecture.md         Architekturentscheidungen
```

Weitere Details stehen in [`docs/architecture.md`](docs/architecture.md).

## Entwicklung

Voraussetzung ist das .NET 10 SDK.

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\Sortify.App
dotnet test
```

## Hintergrundbetrieb

Sortify zeigt unter Windows ein Symbol im Infobereich und unter macOS in der Menueleiste. Das Menue bietet Oeffnen, Ueberwachung pausieren/fortsetzen und Beenden. Beim Schliessen bleibt die Anwendung standardmaessig im Tray aktiv; diese Option kann im Tray-Menue deaktiviert werden und wird in der lokalen Einstellungsdatei gespeichert. Beenden stoppt die Ueberwachung und schliesst offene Datei-Popups.

Unter Windows kann im Tray-Menue optional der Start bei der Benutzeranmeldung aktiviert werden. Standardmaessig ist kein Autostart eingetragen. Der Eintrag liegt ausschliesslich unter `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\Sortify` und wird beim Deaktivieren entfernt. Hierfuer Sortify zuerst in einen stabilen Ordner veroeffentlichen; den Build-Ordner danach nicht verschieben:

```powershell
dotnet publish .\src\Sortify.App -c Release -r win-x64 --self-contained true -o .\publish
.\publish\Sortify.App.exe
```

Der Autostart startet die App mit `--background`. Einstellungen und Ordnerueberwachung werden ohne sichtbares Hauptfenster geladen. Ohne konfigurierten Ordner oder bei einem Einstellungsfehler wird stattdessen das Hauptfenster angezeigt. Ein erneuter normaler Programmstart aktiviert die bereits laufende Instanz; ein zweiter Hintergrundstart bleibt still. Es wird kein Windows-Dienst eingerichtet, und vor der Anmeldung laeuft Sortify nicht.

Linux verwendet vorerst den normalen Fensterbetrieb, da die Verfuegbarkeit eines Tray-Bereichs von der Desktop-Umgebung abhaengt. Die Autostart-Option ist ausserhalb von Windows deaktiviert. Native Tray-Anzeige und Betriebssystem-Autostart muessen auf dem Zielsystem geprueft werden; Headless-Tests decken Lebenszyklus und Dateierkennung ab.

## Mitarbeit

Jede Änderung beginnt mit einem GitHub-Issue und wird über einen eigenen Branch und Pull Request eingebracht. Feature-Branches verwenden:

```text
features/<github-username>/<issue-number>/<kebab-case-description>
```

Alle Arbeits-, Architektur-, Build- und Review-Konventionen stehen in [`AGENTS.md`](AGENTS.md).

## Geplante Architektur

| Bereich | Verantwortung |
|---|---|
| Benutzeroberfläche | Ordner, Regeln und Vorschläge anzeigen und verwalten |
| Ordnerüberwachung | Neue und vollständig verfügbare Dateien erkennen |
| Regelprüfung | Passende Benennung und Zielordner bestimmen |
| Dateioperationen | Bestätigte Änderungen sicher ausführen |
| Speicherung | Einstellungen, Regeln und später den Verlauf lokal sichern |

## Datenschutz und Sicherheit

Sortify soll vollständig lokal funktionieren. KI-Endpunkte, Cloud-Dienste und externe Schnittstellen sind nicht als Kernbestandteil vorgesehen. Dateien werden nur nach ausdrücklicher Bestätigung verändert. Vorhandene Dateien dürfen nicht unbemerkt überschrieben werden; bei Fehlern muss die ursprüngliche Datei erhalten bleiben.

## Projektteam

| Name | Rolle |
|---|---|
| Ezra Bauchinger | Projektleitung |
| Tunahan Barak | Entwicklung und Logo |
| Manuel Stromberger | Dokumentation und Qualitätssicherung |

## Contributor-Rangliste

Jeder gemergte Team-PR bringt **10 XP**. Beiträge zu Code, Tests, Dokumentation und Organisation zählen gleichermaßen. Wer die meisten XP sammelt, steht auf Platz 1.

| Platz | Teammitglied | Gemergte PRs | XP |
|---|---|---|---|
| ![1. Platz: Gold](https://img.shields.io/badge/1._Platz-Gold-D4AF37?style=flat-square) | [Tunahan · @TuNi02](https://github.com/TuNi02) | [5](https://github.com/ezra-ba/sortify/pulls?q=is%3Apr+is%3Amerged+author%3ATuNi02) | **50 XP** |
| ![2. Platz: Silber](https://img.shields.io/badge/2._Platz-Silber-A7ADB4?style=flat-square) | [Manuel · @smanuel89](https://github.com/smanuel89) | [4](https://github.com/ezra-ba/sortify/pulls?q=is%3Apr+is%3Amerged+author%3Asmanuel89) | **40 XP** |
| ![3. Platz: Bronze](https://img.shields.io/badge/3._Platz-Bronze-CD7F32?style=flat-square) | [Ezra · @ezra-ba](https://github.com/ezra-ba) | [2](https://github.com/ezra-ba/sortify/pulls?q=is%3Apr+is%3Amerged+author%3Aezra-ba) | **20 XP** |

**Stand: 21.10.2026.** Die Tabelle wird manuell aktualisiert. Bots, offene PRs und ohne Merge geschlossene PRs zählen nicht. Bei gleicher Punktzahl teilen sich die Teammitglieder den Platz. Die XP zeigen Projektaktivität, keine Qualitätsbewertung; zusammengehörige Änderungen bleiben in einem PR.

## Nächste Schritte

- Verwaltung eines überwachten Ordners umsetzen,
- Regelmodell und lokale Speicherung definieren,
- ersten Ablauf von Dateierkennung bis Vorschau als Prototyp entwickeln,
- Kernfunktionen mit Tests absichern.

---

<p align="center">
  <strong>Ein Projekt von SYD – Sort Your Data GmbH</strong><br>
  <img src="assets/branding/syd-firmen-logo.png" alt="SYD – Sort Your Data GmbH" width="360">
</p>
