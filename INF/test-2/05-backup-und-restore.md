#flashcards/2_INF_Test_Deck/backup-und-restore

**Wozu dient ein Backup und was bedeutet 3-2-1?**
?
Es ermöglicht Wiederherstellung nach Defekt, Fehlbedienung oder Angriff. Drei Kopien einschließlich Original, zwei verschiedene Speichermedien, eine Kopie außer Haus. Beispiel: Produktivdaten, lokaler Dump und externes NAS an anderem Standort.
<!--SR:!2026-11-13,3,250-->

**Was unterscheidet Offline- und Online-Backup?**
?
Offline ist der Server gestoppt, die Dateien sind konsistent und der Dienst ist vorübergehend nicht verfügbar. Online bleibt der Server erreichbar. Sperren oder ein konsistenter Snapshot schützen vor gemischten Datenständen.
<!--SR:!2026-11-13,3,250-->

**Wie sicherst du InnoDB online trotz laufender Schreibvorgänge?**
?
Ein logischer Dump mit `--single-transaction` liest einen konsistenten InnoDB-Snapshot. Eine Lesesperre ist ein anderer Ansatz und blockiert Schreibvorgänge während des Kopierens.
<!--SR:!2026-11-13,3,250-->

**Wie unterscheiden sich READ, READ LOCAL und WRITE?**
?
`READ`: Lesen erlaubt, Schreiben gesperrt. `READ LOCAL`: zusätzlich konfliktfreie Inserts bei MyISAM. `WRITE`: nur die sperrende Sitzung darf lesen und schreiben.
<!--SR:!2026-11-12,3,250-->

**Wie kombinierst du Flush und Lesesperre?**
?
`FLUSH TABLES movie, genre WITH READ LOCK;` kombiniert beides. Beim Dateibackup sind offene Tabellen und gepufferte Änderungen zu berücksichtigen. Nach der Sicherung `UNLOCK TABLES;` ausführen.
<!--SR:!2026-11-13,3,250-->

**Wie exportierst du Filme als CSV mit Semikolon und doppelten Anführungszeichen?**
?
`SELECT * FROM movie INTO OUTFILE '/var/lib/mysql-files/movie.csv' FIELDS TERMINATED BY ';' ENCLOSED BY '"' LINES TERMINATED BY '\n';`
<!--SR:!2026-11-13,3,250-->

**Welche Voraussetzungen und Grenzen hat SELECT … INTO OUTFILE?**
?
Zieldatei darf nicht existieren. DB-Konto braucht `FILE`, Serverprozess Schreibrechte und der Pfad muss gemäß `secure_file_priv` erlaubt sein. Ausgabe erfolgt auf dem DB-Server und enthält Daten ohne Tabellenstruktur.
<!--SR:!2026-11-13,3,250-->

**Wie importierst du einen OUTFILE-Export und welche Standardoptionen gelten?**
?
Mit `LOAD DATA INFILE` und denselben Formatoptionen. Ohne Optionen gelten Tabulator, keine Textbegrenzung, Backslash zum Maskieren und Zeilenumbruch.
<!--SR:!2026-11-13,3,250-->

**Was ist ein SQL-Dump und warum eignet er sich für einen Umzug?**
?
Eine SQL-Textdatei, typischerweise mit `DROP TABLE IF EXISTS`, `CREATE TABLE` und `INSERT`. Sie baut Struktur und Daten neu auf und ist gut komprimierbar. Ein kompatibler Zielserver kann sie einspielen. Ein Datenverzeichnis hängt stärker von Version, Engine und Umgebung ab.
<!--SR:!2026-11-12,3,250-->

**Wann passen Dateikopie, OUTFILE, Dump oder phpMyAdmin?**
?
Dateikopie für das ganze Datenverzeichnis bei gestopptem Server. OUTFILE für CSV-Datenaustausch. Dump für regelmäßige Sicherungen oder Umzüge. phpMyAdmin für gelegentlichen Export per Oberfläche.
<!--SR:!2026-11-13,3,250-->

**Welche Sicherheitsregeln gelten für Backups?**
?
Zugangsdaten etwa in `~/.my.cnf` mit `chmod 600` schützen. Backup-Konto mit den nötigen Minimalrechten. Dumps außerhalb des Webroots und eine Kopie außer Haus.
<!--SR:!2026-11-13,3,250-->

**Was übernimmt ein automatisches Backup-Skript?**
?
Dump erstellen und komprimieren, datiert benennen, Fehler erkennen und protokollieren sowie alte Sicherungen nach Ablauf der Aufbewahrungsfrist löschen.
<!--SR:!2026-11-13,3,250-->

**Was bedeuten die fünf cron-Felder und wie planst du täglich 02:30 Uhr?**
?
Minute, Stunde, Tag des Monats, Monat, Wochentag. `*` bedeutet jeder Wert. In `crontab -e`: `30 2 * * * /usr/local/bin/backup.sh`.
<!--SR:!2026-11-12,3,250-->

**Warum testet man Restores und wie gehst du im Ernstfall vor?**
?
Ein Backup kann leer, unvollständig oder beschädigt sein. Ursache stoppen, Dump in eine Testdatenbank einspielen, Daten prüfen und gezielt Verlorenes übernehmen. Der Dump liefert den Stand seines Sicherungszeitpunkts.
<!--SR:!2026-11-12,3,250-->
