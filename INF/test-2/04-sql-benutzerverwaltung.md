#flashcards/2_INF_Test_Deck/sql-benutzerverwaltung

**Was ändert der Host-Teil eines DB-Kontos?**
?
`'portal_user'@'localhost'` ist für lokale Verbindungen gedacht. `'%'` kann Verbindungen von beliebigen Hosts zulassen, die den DB-Server erreichen.
<!--SR:!2026-11-13,3,250-->

**Was kann eine SQL-Injection mit einem reinen SELECT-Konto anrichten?**
?
Sie kann Daten im erlaubten Lesebereich auslesen, aber keine Tabellen löschen oder Daten ändern. Ein weitreichendes Konto kann auch DDL oder Schreiboperationen erlauben. Prepared Statements verhindern die Injection bei gebundenen Werten.
<!--SR:!2026-11-13,3,250-->

**Braucht jede registrierte Person ein MariaDB-Konto?**
?
Die Anwendung nutzt wenige Konten passend zu ihren Rollen. Einzelne Portalnutzer werden über ihre Session identifiziert. Die App filtert private Daten nach der Benutzer-ID, etwa mit `WHERE user_id = ?`.
<!--SR:!2026-11-13,3,250-->

**Wann ist ALL PRIVILEGES sinnvoll?**
?
Für Verwaltungskonten im notwendigen Geltungsbereich, etwa `*.*` für die Administration oder `wer_streamt_was.*` für ein separates Entwicklungskonto. App-Konten bekommen nur die tatsächlich benötigten Rechte.
<!--SR:!2026-11-13,3,250-->

**Was bedeutet GRANT USAGE ON *.*?**
?
Dieser Grant vergibt keine Objektrechte. Mit gültigen Zugangsdaten kann sich das Konto anmelden. Weitere Rechte kommen durch eigene Grants oder Rollen hinzu.
<!--SR:!2026-11-13,3,250-->

**Wie schützt eine VIEW unveröffentlichte Filme und interne Felder?**
?
Die Sicht wählt nur öffentliche Spalten und Zeilen, etwa mit `WHERE veroeffentlicht = 1`. Das Gastkonto erhält `SELECT` auf die View. Die Basistabelle bleibt dem Gastkonto unzugänglich.
<!--SR:!2026-11-12,3,250-->

**Wie lässt du einen Redakteur Filme lesen und anlegen, aber nicht löschen?**
?
`GRANT SELECT, INSERT ON wer_streamt_was.filme TO 'redakteur'@'localhost';` `DELETE` darf auch über keine andere Rolle oder Freigabe vorliegen. Gemeinsame Rechte lassen sich in einer Rolle bündeln.
<!--SR:!2026-11-13,3,250-->

**Was ist Privilege Creep und welche Befehle helfen dagegen?**
?
Alte, ungenutzte Rechte sammeln sich an. Mit `REVOKE INSERT ON wer_streamt_was.filme FROM 'redakteur'@'localhost';` entziehen, mit `SHOW GRANTS FOR 'redakteur'@'localhost';` prüfen und überflüssige Konten per `DROP USER` entfernen.
<!--SR:!2026-11-12,3,250-->

**Wo gehören DB-Zugangsdaten hin?**
?
In eine serverseitig lesbare, geschützte Konfiguration außerhalb des Webroots. Dateien wie `config.bak` könnten im Webroot per URL erreichbar sein. Lokale Zugangsdaten per `.gitignore` vom Repository fernhalten.
<!--SR:!2026-11-13,3,250-->

**Wie darf jemand nur die eigene E-Mail-Adresse ändern?**
?
Die DB vergibt `UPDATE (email)` auf `users`. Die App bindet E-Mail und Session-Benutzer-ID an `UPDATE users SET email = ? WHERE user_id = ?`. Die ID kommt aus der Session. Eine Prozedur kann die Update-Logik kapseln.
<!--SR:!2026-11-13,3,250-->
