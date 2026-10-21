# Lokale Speicherung von Einstellungen und Regeln

Stand: 21. Oktober 2026. Arbeitsstand für Issue #28.

## Umfang und Format

Ein Überwachungsordner, ein Archiv-Stammordner, `IncludeSubdirectories` und die vollständige Regelliste werden gemeinsam als JSON gespeichert. `SettingsDocument` enthält `Version = 1` und `Settings` mit den Feldern von `AppSettings`. Alle Regelfelder einschließlich ID, Priorität und Aktivierungszustand bleiben erhalten. Ordnerpfade dürfen vor der Einrichtung `null` sein; eine leere Regelliste ist erlaubt. Profile und Import/Export sind nicht Bestandteil dieses Issues.

Unbekannte Formatversionen werden abgelehnt. Eine automatische Migration ist noch nicht implementiert. Fehlende Konstruktorfelder und ungültige JSON-Daten führen zu einem Ladefehler. Explizite Nullwerte werden zusätzlich fachlich validiert.

## Speicherort

`SettingsStoragePath.GetDefaultFilePath()` verwendet `Environment.SpecialFolder.LocalApplicationData` und hängt `Sortify/settings.json` mit `Path.Combine` an. Der tatsächliche Benutzerpfad wird vom Betriebssystem bestimmt. Die Datei liegt außerhalb des Repositorys und des Überwachungsordners. Eine Datei entsteht erst beim erfolgreichen Speichern; das reine Laden legt keine Konfiguration an.

## Zuständigkeiten

- Core: `AppSettings`, Lade- und Speicherergebnisse sowie `ISettingsStore` und `ISettingsService`.
- Infrastructure: `JsonSettingsStore`, `SettingsDocument`, `SettingsValidator`, `SettingsStoragePath` und `SettingsService`.
- App: `App.axaml.cs` verbindet die Komponenten und startet die Initialisierung beim Öffnen des Hauptfensters. `MainViewModel` zeigt Ordner und Ladezustand an.

Der Store verarbeitet Dateien. Der Dienst hält den aktuellen Stand unter `Current` bereit und serialisiert Initialisierung und Speicheraufrufe innerhalb einer Instanz. Für alle Verbraucher soll dieselbe Dienstinstanz verwendet werden. Eine Koordination mehrerer laufender App-Prozesse ist nicht implementiert.

## Laden und Fehlerbehandlung

- `Loaded`: Version und Daten sind gültig; der Dienst stellt die Einstellungen bereit.
- `NotFound`: Datei oder Speicherverzeichnis fehlt. Der Dienst stellt eine leere Ausgangskonfiguration für die Ersteinrichtung bereit.
- `Failed`: Beschädigte, ungültige, nicht unterstützte oder nicht lesbare Daten. Eine verständliche Meldung wird zurückgegeben und im Hauptfenster angezeigt. Die Datei wird beim Laden nicht verändert.

`SettingsValidator` prüft gesetzte Ordnerpfade auf vollständige absolute Pfade für die aktuelle Plattform, Regel-IDs auf Eindeutigkeit und alle Regeln über `IRuleValidator`. Ungültige Regeln führen derzeit zur Ablehnung der gesamten Konfiguration, auch wenn sie deaktiviert sind. Existenz, Erreichbarkeit, Rechte, Überschneidungen der Ordner und symbolische Links werden hier noch nicht geprüft; diese Prüfungen gehören zur Einrichtung beziehungsweise zum Start der Verarbeitung.

Nach einem Ladefehler bleibt `Current` leer und Speichern über den Dienst ist gesperrt. Die Datei wird nicht automatisch zurückgesetzt. Nach einer Reparatur ist eine neue Dienstinstanz beziehungsweise ein Neustart erforderlich. Die Initialisierung wird pro Instanz einmal ausgeführt; spätere Aufrufe liefern das ursprüngliche Initialisierungsergebnis. Den aktuellen Stand liest man über `Current`.

## Speichern

Vor dem Schreiben werden die Einstellungen validiert. Der Store schreibt eine eindeutige temporäre Datei im selben Verzeichnis, schließt sie und ersetzt anschließend die Einstellungsdatei. Temporäre Dateien werden bei normalem Abschluss, Fehlern und Abbruch nach Möglichkeit entfernt. Die Verzeichnisse werden bei Bedarf angelegt. Es gibt keine Garantie gegen Stromausfälle oder einen abrupten Prozessabbruch.

Nur erfolgreiches Speichern aktualisiert `SettingsService.Current`. Ein angeforderter Abbruch wird als `OperationCanceledException` weitergegeben. Erwartbare Zugriffs- und E/A-Fehler werden als fehlgeschlagenes Ergebnis mit verständlicher Meldung zurückgegeben. Das Ersetzen betrifft ausschließlich die eigene Konfigurationsdatei; Benutzerdateien dürfen weiterhin nicht überschrieben werden.

## Schnittstellen für die Oberfläche und Regelauswertung

`MainViewModel.SaveWatchedFolderAsync(path)` speichert die Ordnerauswahl und erhält die übrigen Einstellungen. Der angezeigte Ordner ändert sich erst nach erfolgreichem Speichern.

`MainViewModel.SaveRulesAsync(updatedRules)` nimmt die vollständige aktualisierte Regelliste entgegen und erhält Ordnerpfade und Rekursionseinstellung. Die Regelverwaltung aus Issue #27 muss diese Methode beziehungsweise denselben Einstellungsdienst noch anbinden. Die vorhandenen Zielordnerkarten und Spitznamen sind nicht Teil von `AppSettings` und werden derzeit nicht gespeichert.

Nach erfolgreicher Initialisierung stehen Regeln über `ISettingsService.Current.Rules` bereit. Der tatsächliche Übergabenachweis an die Engine aus Issue #26 ist noch offen. Eine Ordnerüberwachung oder Dateioperation wird durch das Laden allein nicht gestartet.

## Verifikation und verbleibende Abnahme

Infrastructure-Tests prüfen Validierung, fehlende und beschädigte Dateien, Formatversionen, vollständige Regelfelder, neue Store- und Dienstinstanzen, Ersteinrichtung, Speicherfehler, vorab angeforderte Abbrüche und Bereinigung temporärer Dateien. Die Abbruchtests beweisen keinen Abbruch während eines bereits laufenden Schreibvorgangs.

Der manuelle Neustart mit wiederhergestellter Ordnerauswahl wurde durchgeführt. Für die vollständige Abnahme von #28 bleiben die Anbindung der tatsächlichen Regelanlage, ein Neustart mit einer darüber angelegten Regel und die Übergabe geladener Regeln an die Engine nachzuweisen. Die gemeinsame Integration ist mit #27 und #26 abzustimmen.
