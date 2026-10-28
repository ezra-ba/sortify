# Lokale Protokollierung – Backend (#78)

## Zuständigkeit und Stand

Tuni implementiert Core-Vertrag und Infrastructure-Dienste. Manuel übernimmt App-Komposition, ViewModels und Anzeige. Ezra betreut die zukünftigen KI-Tools. Dieses Issue ändert keine Dateien unter `Sortify.App` und ergänzt keine UI.

Das Backend ist vorbereitet, aber ohne die nachfolgende Anbindung noch nicht im laufenden Programm aktiv. Kein automatischer Export, kein Netzwerk und keine zusätzliche Bibliothek.

## Vertrag

`IActivityLog.TryWrite(level, eventName, message)` persistiert einen Eintrag und meldet Fehler mit `false`. Ungültige oder überlange Einträge werden abgelehnt (Ereigniskennung maximal 100, Nachricht maximal 2000 Zeichen). `Information`, `Warning` und `Error` werden unterstützt. `LogEntry` enthält `TimestampUtc`, `Level`, `EventName` und `Message`.

`ReadRecent(limit)` liefert 1–1000 Einträge, standardmäßig 100, neuester zuerst. Die Reihenfolge entspricht der Schreibreihenfolge, auch bei korrigierter Systemzeit. Fehlende Dateien ergeben eine leere erfolgreiche Liste, ohne Verzeichnisse anzulegen. `ErrorMessage` meldet Lesefehler; beschädigte Zeilen werden übersprungen und in `SkippedEntries` gezählt. Die UI soll bei solchen Ergebnissen einen Hinweis anzeigen. Keine Exceptions oder Dateiinhalte an die UI weiterreichen.

## Speicherung und Grenzen

`JsonActivityLog.GetDefaultDirectory()` liefert den Ordner `logs` neben der bestehenden `settings.json`, auf Basis von `LocalApplicationData` des Betriebssystems. Unter Linux ist dies üblicherweise `~/.local/share/Sortify/logs`. Keine hartcodierten Benutzerpfade.

JSONL mit UTF-8 und einem JSON-Objekt je Zeile: `activity.jsonl`, danach `activity.1.jsonl` (neuestes Archiv) bis `activity.3.jsonl`. Standard: maximal 1 MiB pro Datei und drei Archive, also höchstens ungefähr 4 MiB. Rotation vor dem Schreiben; älteste Archive werden ersetzt. Es gibt keine zeitbasierte Aufbewahrungsfrist. Größe und Archivanzahl sind Konstruktorparameter (4 KiB–16 MiB, 1–10 Archive); für denselben Ordner einheitlich konfigurieren.

Schreiben und Lesen sind innerhalb eines Prozesses auch über mehrere Logger-Instanzen synchronisiert. Mehrere unabhängige Prozesse dürfen nicht denselben Log-Ordner verwenden; Sortify verwendet bereits eine Einzelinstanz-Sitzung. Symlink-Aliase eines Ordners sind keine zusätzlich unterstützte Synchronisationsgrenze.

Die API verwendet kurze synchrone Dateioperationen, keine Warteschlange und keinen Shutdown-Flush. Bei einer späteren UI-Anbindung `ReadRecent` außerhalb des UI-Threads aufrufen. Sehr langsame Datenträger können die aufrufende Backend-Operation verzögern. Ein Prozessabsturz kann eine unvollständige letzte Zeile hinterlassen; diese wird beim Lesen übersprungen. Extern vergrößerte Dateien oberhalb des Limits werden beim Lesen mit Fehler abgelehnt.

## Anbindung für Manuel

Eine gemeinsame Instanz bei der App-Zusammenstellung erstellen und weitergeben:

```csharp
IActivityLog log = new JsonActivityLog(JsonActivityLog.GetDefaultDirectory());
ISettingsStore store = new LoggedSettingsStore(
    new JsonSettingsStore(settingsPath, settingsValidator), log);
IFileMoveService moves = new LoggedFileMoveService(new FileMoveService(), log);
IRuleEngine engine = new LoggedRuleEngine(
    new RuleEngine(ruleValidator, ruleTargetValidator), log);
var monitor = new FolderMonitor(log);
```

Namespaces: `Sortify.Core.Services`, `Sortify.Infrastructure.Logging` sowie die bestehenden Namespaces der umschlossenen Dienste. `settingsPath` und Validatoren entsprechen der vorhandenen App-Komposition. Die dekorierten Dienste müssen an sämtliche Verbraucher übergeben werden; vorhandene direkte `new FileMoveService()`-Aufrufe im App-Projekt werden durch diesen Backend-Branch nicht umgestellt. Keine neuen Dienste parallel zum vorhandenen Monitor starten.

Für die Anzeige die gleiche `IActivityLog`-Instanz bereitstellen. Eine spätere Aktualisierung erfolgt durch erneutes `ReadRecent`, kein UI-Event und keine Dateisystemabhängigkeit im ViewModel. Kein Ereignis pro Polling-Schritt der Dateibereitschaft; nur das Endergebnis wird protokolliert.

## Ereignisse

| Kennung | Bedeutung |
| --- | --- |
| `FileMove.<Status>` | Ergebnis einschließlich Erfolg, fehlender Bestätigung, geänderter Quelle oder existierendem Ziel |
| `FileMove.Cancelled` / `FileMove.Exception` | Abbruch beziehungsweise unerwartete Exception |
| `FileSource.Ready` / `FileSource.Unavailable` / `FileSource.Exception` | Ergebnis der Bereitschaftsprüfung |
| `Settings.Load.<Status>` / `Settings.Save.Succeeded` / `Settings.Save.Failed` | Laden/Speichern; unerwartete Exceptions mit `.Exception` |
| `Rules.<Status>` / `Rules.Exception` | Ergebnis der Regelauswertung; mehrdeutig = Warnung, fehlgeschlagen = Fehler |
| `Monitor.Started` / `Monitor.Stopped` / `Monitor.StartFailed` / `Monitor.Interrupted` | Überwachungszustand |
| `Monitor.FileDetected` | Erkennung einer relevanten neuen Datei |

Automatische Nachrichten enthalten keine vollständigen Pfade, Dateinamen, Dateiinhalte, Regeltexte, Einstellungswerte oder Exception-Details. Lokale Logs liefern damit technische Ablaufdiagnose, keine vollständige Historie einzelner Dateipfade. Erweiterungen mit sensiblen Informationen müssen bewusst vereinbart werden. Benutzereingaben mit Zeilenumbrüchen werden JSON-escaped; sie erzeugen keine zusätzlichen Log-Zeilen.

Die Dekoratoren geben Originalergebnisse unverändert zurück und reichen ursprüngliche Exceptions/Abbrüche weiter. Logger-Fehler werden isoliert, auch wenn ein anderer `IActivityLog` entgegen dem Vertrag wirft. Es gibt keine rekursive Protokollierung von Log-Fehlern. Die aufrufende Anwendung kann direkte `TryWrite`-Fehler oder `ReadRecent.ErrorMessage` bei Bedarf sichtbar machen.

## Verifikation

Gezielte Tests für paralleles Schreiben, erneutes Lesen, Rotation, beschädigte Zeilen, Fehler/Rückkehr des Speichers, Logger-Ausfälle bei echten Dateioperationen, fehlende Bestätigung, Abbruch, Einstellungen, Überwachung und Regelauswertung. Bestehende Tests sichern die ursprüngliche Dateisicherheit weiterhin ab.

Geprüfter Stand: `dotnet build Sortify.sln --configuration Release --no-restore` ohne Warnungen/Fehler; `dotnet test Sortify.sln --configuration Release --no-build --no-restore`: 411 erfolgreich (237 Core, 107 Infrastructure, 67 App). Neun gezielte Backend-Tests wurden ergänzt. `git diff --check` ist sauber. Die Live-Anbindung und visuelle Abnahme gehören zum Frontend-Folgeauftrag.
