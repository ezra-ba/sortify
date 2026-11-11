#flashcards/2_INF_Test_Deck/stored-procedures

**Was ist eine Stored Procedure und wie startet man sie?**
?
Ein benanntes Unterprogramm, das auf dem DB-Server gespeichert und ausgeführt wird. `CALL name(...)` startet den Ablauf. Er kann SQL, Ablaufsteuerung, Parameter und Ausgaben enthalten.
<!--SR:!2026-11-13,3,250-->

**Wofür eignen sich Stored Procedures?**
?
Für häufig wiederkehrende, mehrstufige Abläufe, etwa Überweisungen oder das Anlegen und Verknüpfen eines Films und seines Genres.
<!--SR:!2026-11-13,3,250-->

**Welche Vorteile bieten Stored Procedures?**
?
Ein Aufruf kann mehrere Netzwerk-Rundreisen sparen. Die Geschäftslogik ist zentral wartbar. Mit passenden Rechten kann ein Konto die Prozedur ausführen, ohne direkten Tabellenzugriff zu haben.
<!--SR:!2026-11-13,3,250-->

**Wie unterscheiden sich CALL-Prozeduren von selektierbaren Prozeduren?**
?
Eine CALL-Prozedur wird mit `CALL` ausgeführt und kann Daten ändern oder Ergebnisse liefern. Eine selektierbare Prozedur ließe sich wie eine Tabelle in `SELECT` und `JOIN` verwenden. Diese Form unterstützt MariaDB nicht.
<!--SR:!2026-11-13,3,250-->

**Was unterscheidet deklaratives SQL von prozeduralem SQL?**
?
Deklarativ beschreibt das gewünschte Ergebnis. Prozedural beschreibt die Schritte, etwa mit Variablen, `IF`, Schleifen und `DECLARE … HANDLER`. PL/SQL ist der Oracle-Dialekt, PL/pgSQL der PostgreSQL-Dialekt. MariaDB hat eine eigene Syntax.
<!--SR:!2026-11-12,3,250-->

**Warum braucht man im MariaDB-Client DELIMITER für eine mehrteilige Prozedur?**
?
Sonst würde der Client die Definition am ersten inneren Semikolon abschicken. Vorher `DELIMITER //` setzen, mit `END //` abschließen und danach `DELIMITER ;` setzen.
<!--SR:!2026-11-13,3,250-->

**Was bedeuten IN, OUT und INOUT?**
?
`IN` ist die Standardeingabe, z. B. eine Film-ID. `OUT` gibt einen Wert zurück, z. B. eine Anzahl. `INOUT` nimmt einen Startwert entgegen und gibt ihn verändert zurück.
<!--SR:!2026-11-12,3,250-->

**Wie liest man den OUT-Parameter einer Prozedur aus?**
?
Einen Platzhalter als Sitzungsvariable übergeben, z. B. `CALL sp_count_movies(2010, @anzahl);` und danach `SELECT @anzahl;`.
<!--SR:!2026-11-12,3,250-->

**Wo deklariert man lokale Variablen und wie weist man Werte zu?**
?
Am Anfang des `BEGIN … END`-Blocks mit `DECLARE v_name DATENTYP [DEFAULT wert];`. Gültig im eigenen und in inneren Blöcken. Zuweisung mit `SET` oder `SELECT … INTO`.
<!--SR:!2026-11-12,3,250-->

**Warum sind Präfixe wie v_ und p_ nützlich?**
?
Sie unterscheiden lokale Variablen und Parameter von Spalten. Sonst kann eine Bedingung wie `WHERE title = title` beide Seiten gleich auflösen und die Zeilen nicht wie beabsichtigt filtern.
<!--SR:!2026-11-13,3,250-->

**Wie legt eine Prozedur einen Film mit einem eventuell neuen Genre an?**
?
Film einfügen und seine ID merken. Genre mit `SELECT … INTO` suchen, bei Bedarf anlegen und dessen ID merken. Beide IDs in die Zuordnungstabelle schreiben.
<!--SR:!2026-11-13,3,250-->

**Warum LAST_INSERT_ID() statt MAX(id) + 1?**
?
`AUTO_INCREMENT` vergibt IDs. `LAST_INSERT_ID()` liefert die in der eigenen Verbindung erzeugte ID. Bei `MAX(id) + 1` können parallele Sitzungen denselben Wert berechnen.
<!--SR:!2026-11-13,3,250-->

**Wie listet, liest, ändert und löscht man Prozeduren?**
?
`SHOW PROCEDURE STATUS WHERE Db = 'wer_streamt_was';` listet sie. `SHOW CREATE PROCEDURE name;` zeigt den Code. Für neuen Code oder Parameter `CREATE OR REPLACE PROCEDURE` oder löschen und neu anlegen. `ALTER PROCEDURE` ändert Eigenschaften. Löschen mit `DROP PROCEDURE IF EXISTS name;`.
<!--SR:!2026-11-13,3,250-->
