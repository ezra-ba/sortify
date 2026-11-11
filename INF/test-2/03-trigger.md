#flashcards/2_INF_Test_Deck/trigger

**Was unterscheidet einen Trigger von einer Stored Procedure?**
?
Die Datenbank startet den Trigger automatisch bei einem Tabellenereignis. Eine Prozedur wird per `CALL` aufgerufen. Trigger haben keine Aufrufparameter und lesen Zeilenwerte über `NEW` und `OLD`.
<!--SR:!2026-11-13,3,250-->

**Welche drei Bestandteile hat ein Trigger?**
?
Name, Auslösebedingung (Zeitpunkt, Ereignis und Tabelle) und Ausführungsteil mit SQL bzw. Ablaufsteuerung.
<!--SR:!2026-11-12,3,250-->

**Wofür setzt man Trigger ein? Nenne vier Beispiele.**
?
Integrität prüfen (Bewertung 1–5), abgeleitete Werte aktualisieren (Durchschnitt), fehlende Werte ergänzen und Änderungen protokollieren.
<!--SR:!2026-11-13,3,250-->

**Wann passt BEFORE und wann AFTER?**
?
`BEFORE` prüft oder ändert neue Werte vor dem Schreiben und kann das Schreiben abbrechen. `AFTER` passt nach dem Schreiben z. B. den Durchschnitt an, weil die neue Bewertung bereits vorliegt.
<!--SR:!2026-11-13,3,250-->

**Was bedeutet FOR EACH ROW?**
?
MariaDB führt den Trigger für jede betroffene Zeile aus. Ein Update von fünf Zeilen löst ihn fünfmal aus.
<!--SR:!2026-11-13,3,250-->

**Wann stehen NEW und OLD zur Verfügung?**
?
`INSERT`: nur `NEW`. `UPDATE`: beide. `DELETE`: nur `OLD`. `SET NEW.spalte = …` ist bei passenden BEFORE-Triggern möglich.
<!--SR:!2026-11-13,3,250-->

**Wie löst man dieselbe Trigger-Logik für zwei Tabellen aus?**
?
Je Tabelle einen Trigger anlegen. Beide können dieselbe geeignete Prozedur aufrufen. Mehrere Trigger für dasselbe Ereignis einer Tabelle sind seit MariaDB 10.2 möglich.
<!--SR:!2026-11-13,3,250-->

**Warum beschleunigen vorberechnete Lagersummen Abfragen und welche Nachteile hat die naive Lösung?**
?
Die Abfrage liest eine fertige Summe. Nur auf INSERT zu reagieren lässt UPDATE und DELETE unberücksichtigt. Eine volle Neuberechnung bei jedem Insert ist teuer. Änderungen lassen sich inkrementell einrechnen.
<!--SR:!2026-11-13,3,250-->

**Wie weist ein Trigger Bewertung 7 ab?**
?
Ein `BEFORE INSERT` prüft `NEW.rating`. Bei einem Wert außerhalb 1–5 löst `SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Bewertung muss zwischen 1 und 5 liegen';` einen Fehler aus. Das Insert scheitert und die Anwendung behandelt den Fehler.
<!--SR:!2026-11-13,3,250-->

**Wie findest und änderst du Trigger bei einem unerwarteten Update?**
?
`SHOW TRIGGERS;` listet sie, `SHOW CREATE TRIGGER name;` zeigt den Code. In MariaDB per `CREATE OR REPLACE TRIGGER` oder `DROP TRIGGER IF EXISTS name;` und Neuanlegen ändern. Trigger-Logik dokumentieren.
<!--SR:!2026-11-13,3,250-->
