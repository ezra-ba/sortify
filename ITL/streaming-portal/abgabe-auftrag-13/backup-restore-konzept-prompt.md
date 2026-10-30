#itl

# Backup Restore Konzept und KI Prompt

## Konzept

Die Datenbank `streamingheld` enthält unter anderem Benutzerkonten, Bewertungen und Watchlists. Diese Daten sind besonders wertvoll, weil sie individuell entstehen und nicht aus einer externen Quelle rekonstruiert werden können. Weitere wichtige Daten sind die Beziehungen zwischen Titeln, Filmen, Serien, Staffeln, Episoden, Genres, Personen und Streamingdiensten.

Risiken sind Hardware- oder Festplattenausfall, versehentliches Löschen, fehlerhafte Änderungen, Schadsoftware oder Ransomware sowie Diebstahl oder Brand. Auch eine unbemerkt beschädigte Sicherung ist ein Risiko.

Gesichert wird täglich um 02:00 Uhr mit `mysqldump` und den Optionen `--single-transaction`, `--routines`, `--events`, `--triggers` und `--hex-blob`. Die Backups werden mit Zeitstempel in monatlichen Ordnern gespeichert. Eine zusätzliche Kopie liegt auf einem getrennten Medium oder Netzlaufwerk, um die 3-2-1-Regel anzustreben. Tägliche Backups werden 14 Tage, wöchentliche Stände 8 Wochen aufbewahrt. Passwörter stehen nicht im Skript. Die Dateien werden mit SHA-256 geprüft und die ausgelagerte Kopie verschlüsselt.

Beim Restore wird zunächst die Prüfsumme geprüft. Die Sicherung wird ausschließlich in eine getrennte Testdatenbank `streamingheld_test` eingespielt. Danach werden Tabellenanzahl, Datensätze und Stichproben mit der Originaldatenbank verglichen. Erst nach erfolgreicher Prüfung wäre ein Restore der Produktionsdatenbank zulässig.

Als Alternativen wurden VM-Snapshots, Replikation und Hoster-Backups betrachtet. `mysqldump` wurde gewählt, weil die Methode portabel, transparent und für die Größe des Schulprojekts ausreichend ist.

## KI Prompt

Werkzeug: ChatGPT/Codex

> Erstelle zwei sichere PowerShell-Skripte für die MariaDB-Datenbank `streamingheld` eines Streaming-Portals. Verwende `mysqldump` mit `--single-transaction`, `--routines`, `--events`, `--triggers` und `--hex-blob`. Erzeuge zeitgestempelte SQL-Dateien in einer monatlichen Ordnerstruktur, speichere SHA-256-Prüfsummen und verwende keine Klartextpasswörter. Das Restore muss eine separat benannte Test-Datenbank anlegen, die Prüfsumme prüfen und bei Fehlern abbrechen. Sieh Parameter für Pfade, Host, Port, Benutzer und Datenbank vor.

## Änderungen am KI Ergebnis

- Den XAMPP-Pfad zu `mysqldump.exe` und `mysql.exe` ergänzt.
- Die Ziel-Datenbank des Restore-Skripts auf `streamingheld_test` festgelegt.
- Prüfsummenprüfung und Abbruch bei Fehlern ergänzt.
- Datenbanknamen im Restore-Skript auf sichere Zeichen eingeschränkt.
- Keine Zugangsdaten oder Passwörter im Klartext aufgenommen.
