#flashcards/2_INF_Test_Deck/transaktionen

**Was ist eine Transaktion?**
?
Eine Gruppe logisch zusammengehöriger DB-Operationen. Sie beginnt mit `START TRANSACTION` und endet mit `COMMIT` oder `ROLLBACK`.


**Welche Fehlerfälle können einen mehrstufigen Schreibvorgang unterbrechen?**
?
Programmfehler, System- oder Hardwareausfälle, Netzwerkabbrüche und SQL-Fehler wie doppelte Schlüssel oder ungültige Fremdschlüssel.


**Was bedeutet ACID?**
?
**A**tomicity: alles oder nichts. **C**onsistency: Integritätsregeln bleiben erfüllt. **I**solation: Sichtbarkeit und Wechselwirkungen zwischen parallelen Transaktionen sind geregelt. **D**urability: Bestätigte Änderungen bleiben nach einem Absturz erhalten.


**Wie speicherst du einen Film und seine Genrezuordnungen atomar?**
?
Transaktion starten, Film einfügen, ID mit `LAST_INSERT_ID()` merken, alle Zuordnungen einfügen. Bei Erfolg `COMMIT`, bei Fehler `ROLLBACK`. Scheitert die zweite Zuordnung, wird auch der Film zurückgenommen.


**Was ist ein Dirty Read?**
?
A liest eine Änderung von B, die B noch nicht bestätigt hat. Wenn B zurückrollt, hat A einen Wert gesehen, der nie bestätigt wurde.


**Was ist nicht wiederholbares Lesen?**
?
A liest dieselbe Zeile zweimal. B ändert und bestätigt sie dazwischen. A kann beim zweiten Lesen einen anderen Wert erhalten.


**Was ist ein Phantom?**
?
Dieselbe Bedingung liefert bei der zweiten Abfrage eine andere Menge an Zeilen. Beispiel: B fügt einen Film des gesuchten Genres ein und bestätigt. A zählt beim nächsten Lesen einen Treffer mehr.


**Welche vier Isolationsebenen gibt es und was verhindern sie mindestens laut SQL-Standard?**
?
`READ UNCOMMITTED`: keines der drei Leseprobleme. `READ COMMITTED`: Dirty Reads. `REPEATABLE READ`: zusätzlich nicht wiederholbares Lesen. `SERIALIZABLE`: zusätzlich Phantome. Standard in MariaDB ist `REPEATABLE READ`.


**Was kostet stärkere Isolation?**
?
Je nach Stufe und Implementierung können mehr Zugriffe blockieren. Das verringert die Parallelität. Sperren und konsistente Lesestände steuern, was parallele Transaktionen sehen.


**Was bewirkt COMMIT und warum beendet man auch Lesetransaktionen?**
?
Es bestätigt die Änderungen dauerhaft und beendet die Transaktion. Auch eine Lesetransaktion sollte abgeschlossen werden, damit etwaige Sperren freigegeben werden.


**Was bewirkt ROLLBACK? Setzt jeder SQL-Fehler die ganze Transaktion zurück?**
?
`ROLLBACK` nimmt die unbestätigten Änderungen zurück und beendet die Transaktion. Ein Fehler kann nur die betroffene Anweisung abbrechen. Die Anwendung muss bei Bedarf ausdrücklich zurückrollen. Nach einem Absturz werden offene InnoDB-Transaktionen zurückgesetzt.


**Wie rollt man nur einen Teil einer Transaktion zurück?**
?
Mit `SAVEPOINT name` eine Marke setzen und später `ROLLBACK TO SAVEPOINT name` ausführen. Die Transaktion bleibt offen.


**Welche Engine unterstützt im Portal Transaktionen und wie prüfst du das?**
?
InnoDB unterstützt Transaktionen, Zeilensperren und Fremdschlüssel. MyISAM-Änderungen sind nicht rückrollbar. Prüfen mit `SHOW ENGINES;` und `SHOW TABLE STATUS FROM wer_streamt_was;`. Eigene Tabellen mit `ALTER TABLE film ENGINE = InnoDB;` umstellen. Systemtabellen auslassen.
