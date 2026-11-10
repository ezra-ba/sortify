# Bestätigte Dateioperationen – Arbeitsstand #31

## Automatisches Sortieren (#99)

Unter **Einstellungen → Allgemein → Automatisch sortieren** kann der Nutzer das Verschieben ohne Einzelbestätigung einschalten. Die Einstellung wird gespeichert. In bestehenden und neuen Konfigurationen ist sie zunächst ausgeschaltet. Bei ausgeschaltetem Schalter öffnet sich für neu erkannte Dateien das bisherige Vorschlagsfenster.

Bei eingeschaltetem Schalter wartet Sortify auf eine stabil lesbare Datei. Eine eindeutige gültige Regel bestimmt Zielordner und gegebenenfalls den neuen Dateinamen. Gibt es für eine freigegebene Dateiendung keine passende Regel, öffnet Sortify das Popup und wählt den eingerichteten Standardordner vor. Der Nutzer bestätigt das Verschieben im Popup. Ohne Standardordner bleibt die Zielauswahl leer und ein Hinweis erklärt, warum eine manuelle Auswahl nötig ist. Auch mehrdeutige Regeln, Endungen außerhalb der Freigabeliste, Regelprobleme und fehlgeschlagene Verschiebungen führen zur manuellen Prüfung.

Der bestehende Dateidienst prüft den Quellzustand und verhindert Überschreiben. Automatische Verschiebungen in den Überwachungsordner selbst werden zur manuellen Prüfung vorgelegt, damit Umbenennungen keinen weiteren automatischen Vorgang auslösen. Beim Pausieren oder Wechseln des Überwachungsordners werden ausstehende automatische Vorgänge abgebrochen. Verschiebeergebnisse erscheinen im Dateiaktionsprotokoll.


## Systembenachrichtigungen (#106)

Der Schalter **Einstellungen → Allgemein → Automatisch sortieren** aktiviert zugleich die native Erfolgsmeldung. Nach erfolgreichem Auto-Sort übergibt Sortify den tatsächlich verwendeten Zieldateinamen, Zielordner und die lokale Uhrzeit. Manuelle Verschiebungen und fehlgeschlagene automatische Versuche erzeugen keine Erfolgsmeldung.

- Windows verwendet die Benachrichtigung der Windows-Shell über `NotifyIcon` und Windows PowerShell. Die kurze Anzeigedauer wird mit 5000 ms angefragt. [Windows-Benachrichtigungsbereich](https://learn.microsoft.com/en-us/windows/win32/shell/notification-area)
- macOS verwendet `osascript` mit `display notification`. Der Absender kann als Skript-Host erscheinen. Die Anzeige folgt den Mitteilungseinstellungen. [Apple-Dokumentation](https://developer.apple.com/library/archive/documentation/LanguagesUtilities/Conceptual/MacAutomationScriptingGuide/DisplayNotifications.html)
- Linux verwendet `notify-send` in der laufenden Desktop-Sitzung. Dafür müssen das Programm und ein Benachrichtigungsdienst verfügbar sein. Sortify fragt 5000 ms an. [Linux-Desktop-Benachrichtigungen](https://specifications.freedesktop.org/notification/latest-single/)

Die Desktop-Umgebung bestimmt Position und tatsächliche Dauer. Bei deaktivierten oder fehlenden Systembenachrichtigungen bleibt der Verschiebevorgang im Aktivitätsprotokoll nachvollziehbar. Für die native Abnahme auf jedem Betriebssystem Auto-Sort einschalten, eine passende Testdatei ablegen und Anzeige sowie Zeit prüfen. Die automatisierten Tests prüfen die Auslösung und den Inhalt unabhängig von der Desktop-Umgebung.


Stand: 27. Oktober 2026.

## Ablauf im Popup

Das Popup einer erkannten Datei erhält einen IFileMoveService. Beim Öffnen erfasst InspectSourceAsync den Ausgangszustand erst nach einer Stabilitäts- und Lesbarkeitsprüfung. Die manuelle Popup-Vorschau ohne echte Quelle und Dienst kann keine Dateioperation auslösen. Im vorgeschlagenen Zielnamen werden unzulässige Zeichen im Dateinamensstamm durch `-` ersetzt. Die Regelprüfung verwendet weiterhin den ursprünglichen Dateinamen.

Standardwerte im FileMoveService: 2 Sekunden unveränderte Dateigröße und Änderungszeit, Abfrage alle 250 ms, maximal 30 Sekunden Wartezeit. Die Werte sind über den Konstruktor für Tests und spätere Konfiguration anpassbar; sie sind noch keine gespeicherten Benutzereinstellungen. Temporäre E/A- und Freigabefehler setzen das Stabilitätsfenster zurück. Ein Lesezugriff wird versucht, bevor eine Probe als verfügbar gilt. Abschließende Änderungen an Größe oder Zeitstempel starten das Fenster erneut.

Während der Prüfung zeigt das Popup einen Wartehinweis. Bestätigen und Bearbeiten bleiben gesperrt; Abbrechen und Schließen sind möglich und brechen die Prüfung ab. Bei ausbleibender Stabilität bleibt die Datei unverändert, eine Meldung und die Aktion „Datei erneut prüfen“ erscheinen. Ein erneuter Prüfversuch erfasst nach demselben Verfahren einen neuen Snapshot und führt keine Verschiebung aus.

Erst „Datei einsortieren“ erstellt aus diesem Snapshot und dem aktuell angezeigten Zielpfad einen ausdrücklich bestätigten FileMoveRequest. Während der Verschiebeoperation sind Eingaben und Schließen gesperrt. Bei Erfolg schließt das Popup. Bei Kollision oder Fehler bleibt eine Meldung sichtbar. SourceChanged oder SourceNotFound verwerfen den Snapshot. Vor einem erneuten Versuch muss die Datei ausdrücklich neu geprüft und anschließend wieder bestätigt werden.

## Dienstgrenzen und Schutzprüfungen

FileMoveService prüft Bestätigung, absolute Pfade, vorhandene Quelle und Zielordner, identische Pfadzeichenketten, symbolische Links in Quelle und Zielordnerpfad sowie Zielkollisionen. Ein fehlender Zielordner wird nicht automatisch angelegt. Direkt vor File.Move werden Größe und Änderungszeit mit dem bestätigten Snapshot verglichen. File.Move verwendet overwrite: false, um auch eine zwischen Vorschau und Ausführung entstandene Zieldatei nicht zu überschreiben.

Ein Stabilitätsfenster ist ein Indiz, keine Garantie für einen abgeschlossenen Download: Ein fremder Prozess kann nach einer längeren Pause weiter schreiben. Deshalb bleibt die erneute Zustandsprüfung vor dem Verschieben erforderlich. Die derzeitige Metadatenprüfung ist keine Inhaltsprüfung und keine allgemeine Sperre gegen fremde Prozesse. Ein lokaler Integrationstest unter Linux hat Verschiebungen zwischen ext4 (/tmp) und tmpfs (/dev/shm) in beiden Richtungen mit unverändertem Dateiinhalt sowie erhaltenen Dateien bei Zielkollision bestätigt. Dies belegt weder das Verhalten bei unterbrochenem Kopieren noch die Unterstützung aller Laufwerke oder Betriebssysteme. Weitergehende konkurrierende Dateisystemänderungen bleiben gesondert zu prüfen.

Die Target-Auswahl im aktuellen #31-Branch ist manuell beziehungsweise durch die bisherige Popup-Vorbelegung bestimmt. Eine automatische Auswahl über die Target-ID-Engine aus #46 ist eine separate Integration. Die Dateierkennung selbst zeigt neue Dateien weiterhin früh an; die Bestätigung wird erst nach der Readiness-Prüfung freigegeben.

## Regressionstests

FileMoveServiceTests prüfen bestätigte Verschiebung und Umbenennung, fehlende Bestätigung, nachträglich entstandene Zielkollision, geänderte Größe beziehungsweise Änderungszeit, fehlende Quelle, fehlenden Zielordner und einen reproduzierbaren Dateisystemfehler mit erhaltener Quelle.

FileSourceReadinessTests prüfen eine stabile lesbare Datei, eine exklusiv gesperrte Kopie mit finalen Daten und Zeitstempel, begrenztes Warten bei fehlender Datei und Abbruch. FileReadinessViewModelTests prüfen gesperrte Bestätigung während des Wartens, mögliches Abbrechen, Wiederholbarkeit nach Timeout und erneute ausdrückliche Bestätigung nach SourceChanged. Diese Tests ersetzen keine manuelle Prüfung mit echten Downloads auf allen Betriebssystemen.

## Manuelle Prüfung

Am 27. Oktober 2026 hat der Benutzer auf Linux erfolgreich geprüft: bestätigtes Verschieben mit unverändertem Inhalt, Abbrechen ohne Dateiänderung und Zielkollision mit Erhalt beider Dateien. Diese Prüfung fand vor der Integration des Einstellungsfensters aus PR #56 statt. Windows und macOS wurden nicht praktisch geprüft.
