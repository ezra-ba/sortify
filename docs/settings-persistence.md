# Lokale Speicherung von Einstellungen und Regeln

Stand: 23. Oktober 2026. Speicherung aus #28 und #37, erweitert im Arbeitsstand für #46.

## Umfang und Format

Ein Überwachungsordner, ein Archiv-Stammordner, `IncludeSubdirectories` und die vollständige Regelliste werden gemeinsam als JSON gespeichert. `SettingsDocument` schreibt `Version = 3` und `Settings` mit den Feldern von `AppSettings`. Zusätzlich werden `TargetFolders` und `DefaultTargetFolderId` gespeichert. Alle Regelfelder einschließlich ID, Priorität und Aktivierungszustand bleiben erhalten. Ordnerpfade dürfen vor der Einrichtung `null` sein; eine leere Regelliste ist erlaubt. Profile und Import/Export sind nicht Bestandteil dieses Issues.

Versionen 1, 2 und 3 werden gelesen; andere Formatversionen werden abgelehnt. In Version-1-Dateien fehlende Target-Eigenschaften erhalten eine leere Liste und kein Standardziel. Laden verändert die vorhandene Datei nicht. Erst das nächste erfolgreiche Speichern schreibt Version 3; bisherige Ordnerangaben und Regelfelder bleiben erhalten. Ältere App-Stände können Version 3 nicht laden. Version 3 ergänzt die optionale TargetFolderId einer Regel. Fehlende oder unbekannte Regelzielreferenzen werden als Warnungen gemeldet und ohne automatische Zuordnung erhalten; Details beschreibt [target-rule-references.md](target-rule-references.md). Fehlende Konstruktorfelder und ungültige JSON-Daten führen zu einem Ladefehler. Explizite Nullwerte werden zusätzlich fachlich validiert.

## Speicherort

`SettingsStoragePath.GetDefaultFilePath()` verwendet `Environment.SpecialFolder.LocalApplicationData` und hängt `Sortify/settings.json` mit `Path.Combine` an. Der tatsächliche Benutzerpfad wird vom Betriebssystem bestimmt. Die Datei liegt außerhalb des Repositorys und des Überwachungsordners. Eine vollständige Einstellungsdatei entsteht erst beim erfolgreichen Speichern; das reine Laden legt keine Konfiguration an.

## Zuständigkeiten

- Core: `AppSettings`, Lade- und Speicherergebnisse sowie `ISettingsStore` und `ISettingsService`.
- Infrastructure: `JsonSettingsStore`, `SettingsDocument`, `SettingsValidator`, `SettingsStoragePath` und `SettingsService`.
- App: `App.axaml.cs` verbindet die Komponenten und startet die Initialisierung beim Öffnen des Hauptfensters. `MainViewModel` zeigt Ordner und Ladezustand an.

Der Store verarbeitet Dateien. Der Dienst hält den aktuellen Stand unter `Current` bereit und serialisiert Initialisierung und Speicheraufrufe innerhalb einer Instanz. `UpdateAsync` berechnet eine Änderung aus dem aktuellen Stand innerhalb derselben Sperre und speichert sie anschließend. Änderungsfunktionen dürfen den übergebenen Stand nicht mutieren. `SaveAsync` ersetzt dagegen weiterhin die vollständige Konfiguration und ist nicht zum Zusammenführen paralleler Änderungen vorgesehen. Für alle Verbraucher soll dieselbe Dienstinstanz verwendet werden. Eine Koordination mehrerer laufender App-Prozesse ist nicht implementiert.

## Laden und Fehlerbehandlung

- `Loaded`: Version und Daten sind gültig; der Dienst stellt die Einstellungen bereit.
- `NotFound`: Datei oder Speicherverzeichnis fehlt. Der Dienst stellt eine leere Ausgangskonfiguration für die Ersteinrichtung bereit.
- `Failed`: Beschädigte, ungültige, nicht unterstützte oder nicht lesbare Daten. Eine verständliche Meldung wird zurückgegeben und im Hauptfenster angezeigt. Die Datei wird beim Laden nicht verändert.

`SettingsValidator` prüft gesetzte Ordnerpfade auf vollständige absolute Pfade für die aktuelle Plattform, Regel-IDs auf Eindeutigkeit und alle Regeln über `IRuleValidator`. Ungültige Regeln führen derzeit zur Ablehnung der gesamten Konfiguration, auch wenn sie deaktiviert sind. Target-IDs müssen eindeutig und nicht leer sein, Target-Pfade absolut und Spitznamen nicht leer. `DefaultTargetFolderId` darf fehlen oder muss auf einen gespeicherten Target-Eintrag verweisen. Existenz, Erreichbarkeit, Rechte, Überschneidungen der Ordner und symbolische Links werden hier noch nicht geprüft; diese Prüfungen gehören zur Einrichtung beziehungsweise zum Start der Verarbeitung.

Nach einem Ladefehler bleibt `Current` leer und Speichern über den Dienst ist gesperrt. Die Datei wird nicht automatisch zurückgesetzt. Nach einer Reparatur ist eine neue Dienstinstanz beziehungsweise ein Neustart erforderlich. Die Initialisierung wird pro Instanz einmal ausgeführt; spätere Aufrufe liefern das ursprüngliche Initialisierungsergebnis. Den aktuellen Stand liest man über `Current`.

## Speichern

Vor dem Schreiben werden die Einstellungen validiert. Der Store schreibt eine eindeutige temporäre Datei im selben Verzeichnis, schließt sie und ersetzt anschließend die Einstellungsdatei. Temporäre Dateien werden bei normalem Abschluss, Fehlern und Abbruch nach Möglichkeit entfernt. Die Verzeichnisse werden bei Bedarf angelegt. Es gibt keine Garantie gegen Stromausfälle oder einen abrupten Prozessabbruch.

Nur erfolgreiches Speichern aktualisiert `SettingsService.Current`. Ein angeforderter Abbruch wird als `OperationCanceledException` weitergegeben. Erwartbare Zugriffs- und E/A-Fehler werden als fehlgeschlagenes Ergebnis mit verständlicher Meldung zurückgegeben. Das Ersetzen betrifft ausschließlich die eigene Konfigurationsdatei; Benutzerdateien dürfen weiterhin nicht überschrieben werden.

## Schnittstellen für die Oberfläche und Regelauswertung

`MainViewModel.SaveWatchedFolderAsync(path)` speichert die Ordnerauswahl und erhält die übrigen Einstellungen. Der angezeigte Ordner ändert sich erst nach erfolgreichem Speichern.

`MainViewModel.SaveRulesAsync(updatedRules)` nimmt die vollständige aktualisierte Regelliste entgegen und erhält Ordnerpfade und Rekursionseinstellung. Die Regelverwaltung aus Issue #27 nutzt zum einzelnen Hinzufügen `MainViewModel.AddRuleAsync(rule)`. Dieser Aufruf validiert Regel, Target-ID und eindeutige Regel-ID innerhalb der Dienstsperre und ergänzt die aktuelle Liste, ohne bestehende Regeln zu ersetzen. Target-Ordner sind Teil von `AppSettings` und werden wie im folgenden Abschnitt beschrieben gespeichert.

Nach erfolgreicher Initialisierung stehen Regeln über `ISettingsService.Current.Rules` bereit. Der tatsächliche Übergabenachweis an die Engine aus Issue #26 ist noch offen. Eine Ordnerüberwachung oder Dateioperation wird durch das Laden allein nicht gestartet.

## Frei wählbare Target-Ordner (Issue #37)

`TargetFolder` enthält eine stabile `Id`, einen absoluten `Path` und einen `Nickname`. Die Listenreihenfolge in `AppSettings.TargetFolders` bestimmt die Kartenreihenfolge. Ein optionaler Verweis `DefaultTargetFolderId` erlaubt höchstens ein Standardziel. Erneutes Betätigen des Standard-Buttons am bestehenden Standard hebt diesen auf. Das Entfernen des Standardziels setzt den Verweis ebenfalls auf `null`.

Beim App-Start werden Karten aus den gespeicherten Daten erzeugt. Neue Einträge erhalten neue IDs; geladene Einträge behalten ihre IDs. Es werden keine Beispielziele automatisch angelegt. Auswahl, Namensentwurf, Bearbeitungszustand und Drag-Zustand werden nicht gespeichert.

Hinzufügen, Entfernen, Standardwechsel, Spitznamen und Umsortieren verwenden `UpdateAsync`. Spitznamen werden getrennt als `DraftNickname` bearbeitet und erst nach erfolgreichem Speichern übernommen. Abbrechen verändert den gespeicherten Namen nicht. Andere Kartenänderungen werden ebenfalls erst nach erfolgreichem Speichern übernommen. Beim Drag-and-drop wird die Reihenfolge vorübergehend angezeigt und beim Loslassen gespeichert; ein Fehler stellt die gespeicherte Reihenfolge wieder her. Entfernen löscht nur den Konfigurationseintrag, niemals den tatsächlichen Ordner oder Dateien.

`IsSavingSettings` deaktiviert den Fensterinhalt während einer UI-Speicheraktion bis zur anschließenden Kartenaktualisierung. Die Drag-Handler prüfen diesen Zustand zusätzlich. Weitere Aufrufe der Task-Wrapper werden währenddessen übersprungen; `SaveRulesAsync` gibt stattdessen ein fehlgeschlagenes Ergebnis zurück. `finally` gibt die Sperre auch bei Ausnahmen frei. Unerwartete Programmierfehler werden dadurch nicht automatisch zu UI-Fehlermeldungen.

Exakt gleiche Pfadzeichenketten werden beim Hinzufügen als bereits vorhanden erkannt. Eine Gleichsetzung unterschiedlicher Schreibweisen anhand des tatsächlichen Dateisystems oder symbolischer Links ist noch nicht implementiert. Ein zeitweise nicht erreichbarer Target-Ordner wird nicht automatisch entfernt. Die Standardmarkierung allein definiert noch kein Fallback-Verhalten der Engine.

Die bestehende `ArchiveRootPath`-Eigenschaft bleibt unverändert erhalten und wird nicht automatisch zu einem Target konvertiert. Die Target-ID-Verknüpfung und Popup-Anbindung liegen im Arbeitsstand für #46. Die Auswahlmaske ist auf dem darauf aufbauenden #27-Branch implementiert. Die bisherige Archiv-/Kategoriehierarchie ist keine Voraussetzung des künftig vorgesehenen Zielmodells.

## Verifikation und verbleibende Abnahme

Infrastructure-Tests prüfen Validierung, fehlende und beschädigte Dateien, Formatversionen, vollständige Regelfelder, neue Store- und Dienstinstanzen, Ersteinrichtung, Speicherfehler, vorab angeforderte Abbrüche und Bereinigung temporärer Dateien. Die Abbruchtests beweisen keinen Abbruch während eines bereits laufenden Schreibvorgangs.

App-Tests prüfen fehlgeschlagenes Hinzufügen, Entfernen, Standardwechsel und Aufheben, Spitznamen und Umsortieren. Ein kontrolliert wartender Speicher prüft die aktive UI-Sperre und ihre Freigabe nach Erfolg, Speicherfehler und Ausnahme. Infrastructure-Tests prüfen zusätzlich Target-Validierung, Reihenfolge, Standardverweis, Version-1-Kompatibilität und überlappende Dienständerungen.

Manuelle Neustarts für Ordnerauswahl sowie Target-Aktionen wurden durchgeführt. Die Auswertung neu geladener Target-ID-Regeln wird durch Integrationstests nachgewiesen. Ein Headless-UI-Test für #27 prüft Regelanlage über gebundene Eingaben, Validierung, Zielreparatur und einen Vorschlag nach erneutem Laden. Der Benutzer bestätigte am 27. Oktober 2026 auch den manuellen Regelablauf unter Linux mit Neustart, laufender Dateierkennung, passendem Vorschlag und bestätigter Verschiebung als erfolgreich. Die Tests der ViewModels ersetzen keine vollständige Oberflächenprüfung auf allen Zielplattformen.
