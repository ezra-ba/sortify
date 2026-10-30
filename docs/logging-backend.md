# Dateiaktionsprotokoll (#88)

## Umfang

Das Aktivitätsprotokoll enthält ausschließlich Dateiaktionen:

- Erfolgreiche Verschiebung: Quelldateiname, tatsächlicher Zielordner und Zieldateiname (auch bei Umbenennung).
- Fehlgeschlagene/nicht bestätigte/abgebrochene Verschiebung: betroffene Datei und kurzer Grund.
- Schließen eines Dateivorschlags ohne vorherigen Verschiebeversuch: betroffene Datei und „Vorschlag abgebrochen“.

Keine neuen Einträge für Programmstart, Laden/Speichern von Einstellungen, Regeln, Überwachungszustand, Dateierkennung oder Bereitschaftsprüfung. Bereits vorhandene technische Logeinträge werden weder gelöscht noch nachträglich verändert und bleiben bis zur regulären Rotation lesbar.

## Beispiele

```text
Datei „rechnung.pdf“ nach „/Ziel/Rechnungen“ verschoben (Zieldatei: „rechnung-2026.pdf“).
Datei „bericht.docx“ nicht verschoben: Vorschlag abgebrochen.
Datei „foto.jpg“ nicht verschoben: Zieldatei existiert bereits.
```

Es werden nun ausdrücklich Dateinamen und Zielordner gespeichert. Dateiinhalte, Regeldefinitionen, Einstellungen und Exception-Details werden nicht gespeichert. Steuerzeichen in Anzeigenamen werden durch Leerzeichen ersetzt; sehr lange Namen/Pfade werden gekürzt, damit der gesamte Eintrag innerhalb des Größenlimits bleibt.

## Technische Anbindung

`IActivityLog` und die lokale JSONL-Ablage bleiben unverändert. Eine gemeinsame Instanz wird in der App-Komposition erstellt und an Anzeige sowie `DesktopSession` übergeben. Die Sitzung reicht sie an `DownloadMonitorController` weiter.

Der Controller erstellt für jeden echten Dateivorschlag eine eigene `LoggedFileMoveService`-Instanz um den eigentlichen `IFileMoveService`. Sie schreibt das Ergebnis jedes Verschiebeversuchs; `InspectSourceAsync` ist still. Das Popup-Closed-Ereignis ruft `RecordDismissal(sourcePath)` auf. Die Methode schreibt höchstens einmal und nur, wenn noch kein Verschiebeversuch erfasst wurde. Schließen nach Erfolg, Fehler oder bereits protokolliertem Abbruch erzeugt damit keinen zusätzlichen Abschluss. Ein erneuter echter Versuch, etwa nach Fehlerkorrektur, erhält sein eigenes Ergebnis.

Auch ein wegen Ordnerwechsel, Pause oder App-Beenden verworfener offener Vorschlag gilt als abgebrochen, sofern zuvor kein Verschiebeversuch protokolliert wurde. Ein laufender Verschiebevorgang blockiert das Popup-Schließen weiterhin wie zuvor.

Wichtig für weitere Verbraucher: Den Rohdienst in die Sitzung injizieren, nicht zusätzlich global mit einem zweiten Logger dekorieren. Sonst könnten doppelte Einträge entstehen. Die aktuelle App-Komposition verwendet einen Rohdienst und dekoriert ausschließlich pro Vorschlag. Vorschauen ohne echte Datei erhalten keinen Dateiaktionslogger.

`LoggedSettingsStore`, `LoggedRuleEngine` und der optionale Loggerparameter von `FolderMonitor` bleiben für bestehende Aufrufer kompatibel, erzeugen aber keinerlei Logeinträge mehr. Die App verwendet für Einstellungen/Regeln direkt die ursprünglichen Dienste. Views, Layout und Log-Anzeige wurden nicht geändert.

## Speicherung und Anzeige

`JsonActivityLog.GetDefaultDirectory()` liefert den Ordner `logs` neben `settings.json`, auf Basis von `LocalApplicationData`. Dateien: `activity.jsonl` sowie `activity.1.jsonl` bis `activity.3.jsonl`. Standard: 1 MiB je Datei, drei Archive; älteste Archive werden bei Rotation ersetzt. UTF-8-JSONL, UTC-Zeit, Information/Warning/Error, Ereigniskennung und Nachricht.

`ReadRecent(limit)` liest standardmäßig die letzten 100, maximal 1000 Einträge, neueste Schreibreihenfolge zuerst. Lesen erstellt keine Dateien. Beschädigte Zeilen werden übersprungen und in `SkippedEntries` gezählt. Lesefehler stehen in `ErrorMessage`. Die vorhandene Anzeige verwendet weiterhin denselben Vertrag; Lesevorgänge außerhalb des UI-Threads ausführen.

Synchronisierung gilt innerhalb eines Prozesses. Mehrere unabhängige Prozesse sollen nicht denselben Logordner beschreiben. Kurze synchrone Schreibzugriffe können auf langsamen Datenträgern verzögern. Logging-Ausfälle verändern weiterhin keine Dateioperation und werden nicht rekursiv protokolliert.

## Verifikation

Release-Build ohne Warnungen/Fehler; 430 Tests erfolgreich (240 Core, 114 Infrastructure, 76 App). Angepasste Tests prüfen Dateiname/Ziel, stille Bereitschaftsprüfung, keine technischen Einträge auch bei Fehlern, Abbruch ohne Versuch, unterdrückte doppelte Abschlüsse und tatsächliches Popup-Schließen. Bestehende Tests für Rotation, Parallelität und Fehlerisolation bleiben erhalten. `git diff --check` ist sauber.

## Manuell prüfen

1. Neue App-Version starten. Bereits vorhandene technische Einträge anhand des Zeitstempels vom neuen Test unterscheiden.
2. Einstellungen ändern und Testdatei im Überwachungsordner ablegen: noch kein neuer Logeintrag.
3. Vorschlag abbrechen: genau ein Eintrag mit dem betroffenen Dateinamen.
4. Andere Testdatei bestätigen: ein Erfolgseintrag mit Datei, Zielordner und Zieldateiname.
5. Bereits vorhandene Zieldatei als Konflikt herstellen und bestätigen: ein „nicht verschoben“-Eintrag. Anschließendes Schließen erzeugt keinen zweiten Abschluss; Quelldatei bleibt erhalten.
