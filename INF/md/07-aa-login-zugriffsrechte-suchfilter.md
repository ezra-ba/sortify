#inf

**ARBEITSAUFTRAG 07 · STREAMING-PORTAL**

Login, Zugriffsrechte und Suchfilter

Nur Redakteure dürfen Filme anlegen, ändern und löschen – und die Suche merkt sich die Filter

|   |   |
|---|---|
|**Lehrstoffgebiete**|Sessions, Authentifizierung, Passwort-Hashing, Benutzer- und Rechteverwaltung in MariaDB (CREATE USER, GRANT), PHP mit PDO|
|**Aufbauend auf**|Arbeitsauftrag 05_3 (CRUD und Caching) und dem Thema Sessions|
|**Klasse / Name**||
|**Abgabetermin**||
|**Gewählter Weg**|☐ Weg A (max. 90 Punkte) ☐ Weg B (max. 70 Punkte)|
|**Punkte**||

## Ausgangssituation

Bisher kann jede Person, die das Streaming-Portal aufruft, Filme anlegen und ändern, und die Anwendung verbindet sich als root mit der Datenbank. Beides ist für eine Anwendung im Internet nicht akzeptabel. Außerdem vergisst die Suche die gewählten Filter, sobald man eine andere Seite öffnet.

In diesem Auftrag führen Sie einen Login ein und trennen zwei Rollen: **Redakteure** pflegen die Inhalte, **Webuser** sind die Endbenutzer im Internet und dürfen das Portal nur benutzen. Sie wählen dazu einen von zwei Lösungswegen. Unabhängig vom gewählten Weg speichern Sie zusätzlich die Suchfilter in der Session.

## Lernziele

Nach diesem Auftrag können Sie

- Passwörter sicher mit password_hash() speichern und mit password_verify() prüfen,
- einen angemeldeten Benutzer in der **Session** verwalten (Login, Logout),
- Benutzereingaben wie **Suchfilter** über mehrere Seiten in der Session merken und im Formular wieder anzeigen,
- Seiten serverseitig vor unberechtigtem Zugriff schützen,
- in MariaDB Benutzer anlegen und mit **minimalen Rechten** ausstatten (Least Privilege),
- ein externes PHP-Paket recherchieren, einrichten und in ein bestehendes Projekt integrieren (Weg B).

## Rahmenbedingungen

- Sie arbeiten im bestehenden Projekt „Wer streamt was“ mit der Datenbank wer_streamt_was.
- Alle Datenbankzugriffe verwenden Prepared Statements, alle Ausgaben von Benutzerdaten htmlspecialchars().

|   |
|---|
|**Wichtig**<br><br>Zugriffsrechte werden immer **serverseitig** geprüft. Ausgeblendete Menüpunkte und Buttons sind kein Schutz, denn jede URL lässt sich direkt eingeben.|

## Zwei Lösungswege und eine gemeinsame Aufgabe

|**Weg**|**Beschreibung**|**Punkte**|
|---|---|---|
|**A**|Login selbst programmieren, inklusive Benutzerverwaltung in der Datenbank und Rechten auf Datenbankebene (Aufgaben A1–A6)|75|
|**B**|Ein fertiges Login-Plugin (externes PHP-Paket) einbinden und konfigurieren (Aufgaben B1–B4)|55|
|**beide**|Suchfilter in der Session merken (Aufgabe S1)|+ 15|

Kreuzen Sie den gewählten Weg im Kopf des Arbeitsauftrags an. Bewertet wird nur ein Weg, die Aufgabe S1 bearbeiten alle. Damit sind in Weg A maximal **90 Punkte**, in Weg B maximal **70 Punkte** erreichbar.

## Weg A · Login selbst programmieren (max. 75 Punkte)

|   |
|---|
|**Zwei Ebenen von Benutzern**<br><br>In Weg A arbeiten Sie mit zwei verschiedenen Arten von Benutzern. Halten Sie diese auseinander:<br><br>- **Anwendungsebene:** die Personen, die sich im Portal anmelden. Sie stehen in den Tabellen users und usergroups Ihrer Datenbank.<br>- **Datenbankebene:** die MariaDB-Benutzer webuser und redakteur, mit denen sich PHP bei der Datenbank anmeldet. Sie werden mit CREATE USER und GRANT angelegt.<br><br>Meldet sich eine Person als Redakteur im Portal an, verbindet sich die Anwendung mit dem MariaDB-Benutzer redakteur. Sonst verwendet sie webuser. Selbst eine Sicherheitslücke im PHP-Code kann so ohne Anmeldung keine Daten verändern.|

**Aufgabe A1 · SQL-Script für die Benutzerverwaltung20 Punkte**

Speichern Sie alle SQL-Befehle dieser Aufgabe in einem SQL-Script innerhalb des PHP-Projekts, z. B. sql/benutzerverwaltung.sql. Das Script muss sich auf einer frischen Kopie der Datenbank fehlerfrei ausführen lassen.

### a) Tabellen für die Anwendung

- Tabelle usergroups mit den Gruppen **redakteur** und **webuser**.
- Tabelle users mit Benutzername (eindeutig), Passwort-Hash und group_id als Fremdschlüssel auf usergroups.
- Mindestens ein Testbenutzer pro Gruppe. Im Feld für das Passwort steht **nur der Hash**, niemals das Klartext-Passwort. Die Spalte ist dafür VARCHAR(255).

### b) Benutzer und Rechte in MariaDB

- Legen Sie für die Datenbank wer_streamt_was die MariaDB-Benutzer webuser und redakteur mit eigenem Passwort an.
- **webuser** darf alle Inhalte lesen (SELECT), aber keine Filme, Serien, Genres oder Anbieter anlegen, ändern oder löschen. Er muss außerdem die Tabelle users lesen können, damit der Login funktioniert.
- **redakteur** darf die Inhaltstabellen zusätzlich mit INSERT, UPDATE und DELETE bearbeiten.
- Vergeben Sie nur die Rechte, die wirklich gebraucht werden. GRANT ALL und Rechte auf andere Datenbanken sind nicht erlaubt.

|   |
|---|
|**Überlegung: Wie ist mit der Watchlist umzugehen?**<br><br>Endbenutzer sollen Filme auf ihre Watchlist setzen und wieder entfernen können, also schreibend auf eine Tabelle zugreifen, obwohl sie nur webuser sind. Welche Rechte braucht webuser dafür, und auf welche Tabelle beschränken Sie sie? Beantworten Sie die Frage in einem Kommentar im SQL-Script und setzen Sie Ihre Antwort in den GRANT-Befehlen um. Falls Ihre Datenbank noch keine Watchlist enthält, beschreiben Sie, welche Rechte Sie vergeben würden.|

|   |
|---|
|**Hilfestellung: Passwort-Hash erzeugen**<br><br>SQL kann keine PHP-Hashes erzeugen. Erzeugen Sie den Hash für Ihre Testbenutzer einmalig mit PHP und kopieren Sie ihn ins Script:<br><br>\|   \|<br>\|---\|<br>\|<?php echo password_hash('MeinTestPasswort', PASSWORD_DEFAULT);\||

**Aufgabe A2 · Standardverbindung mit webuserPflicht**

Ab jetzt ist webuser der Standardbenutzer beim Aufbau der Datenbankverbindung, **nicht mehr** root. Passen Sie Ihre Klasse dbConn entsprechend an. Diese Umstellung ist Voraussetzung für die Aufgaben A3 bis A6 und wird dort mitbewertet.

**Aufgabe A3 · Login-Seite15 Punkte**

Über einen neuen Menüpunkt **Anmelden** wird eine Seite mit einem Login-Formular (Benutzername, Passwort) aufgerufen.

- Das Formular sendet per POST und fügt sich in das Template ein.
- Ist bereits jemand angemeldet, zeigt das Menü stattdessen den Benutzernamen und einen Eintrag **Abmelden**.

**Aufgabe A4 · Login prüfen, Session und Logout15 Punkte**

Prüfen Sie die eingegebenen Login-Daten gegen die Tabelle users. Bei erfolgreichem Login schreiben Sie die Benutzerdaten in die Session und lösen einen **Redirect** auf index.php aus.

- Der Benutzer wird über ein Prepared Statement anhand des Benutzernamens geladen, das Passwort mit password_verify() geprüft.
- In der Session stehen nur Benutzer-ID, Benutzername und Gruppe, **niemals das Passwort**.
- Nach erfolgreichem Login wird die Session-ID mit session_regenerate_id(true) erneuert.
- Bei falschen Daten erscheint eine allgemeine Meldung wie „Benutzername oder Passwort falsch“, ohne zu verraten, welcher Teil falsch war.
- **Abmelden** entfernt nur die Anmeldedaten aus der Session (z. B. unset($_SESSION['user'])) und leitet auf die Startseite weiter. Die gespeicherten Suchfilter aus Aufgabe S1 bleiben erhalten.

**Aufgabe A5 · Seiten für Redakteure schützen15 Punkte**

Sorgen Sie dafür, dass nur Redakteure die Seiten bzw. Actions zum Anlegen, Ändern und Löschen aufrufen können (z. B. add_movie.php, upd_movie.php).

- Die Prüfung erfolgt **serverseitig** am Anfang jeder geschützten Seite.
- Schreiben Sie die Prüfung einmal als Funktion oder Methode (z. B. requireRole('redakteur')) und verwenden Sie sie überall wieder.
- Nicht angemeldete Besucher werden zur Login-Seite umgeleitet, angemeldete Webuser erhalten eine Meldung, dass ihnen die Berechtigung fehlt.
- Links und Buttons zum Anlegen, Ändern und Löschen sind nur für Redakteure sichtbar.

**Aufgabe A6 · Datenbankverbindung je nach Benutzer10 Punkte**

Sobald ein Redakteur angemeldet ist, muss die Datenbank mit dem MariaDB-Benutzer redakteur statt mit webuser angesprochen werden. Hier zahlt sich die zentrale Klasse dbConn aus: Nur sie muss angepasst werden.

- dbConn liest die Gruppe des angemeldeten Benutzers aus der Session und wählt danach die Zugangsdaten.
- Die Zugangsdaten der MariaDB-Benutzer stehen in einer Konfigurationsdatei, **nicht in der Session**.
- Test: Melden Sie sich ab und rufen Sie eine Speicherfunktion direkt auf (z. B. nach vorübergehendem Entfernen der Rollenprüfung). Die Datenbank muss das Speichern ablehnen.

## Weg B · Login-Plugin verwenden (max. 55 Punkte)

Statt den Login selbst zu programmieren, binden Sie ein externes PHP-Paket für Authentifizierung ein. Eingeloggte Benutzer erhalten die Möglichkeit, Filme hinzuzufügen und zu ändern. Setzen Sie sich dazu gründlich mit den Möglichkeiten des Pakets auseinander: Installation, Konfiguration, Funktionen.

- Das Paket muss aktiv gepflegt sein und mit Ihrer PHP-Version funktionieren. Stimmen Sie Ihre Auswahl vor Beginn mit der Lehrkraft ab.
- Die Installation erfolgt nach Möglichkeit über Composer.

**Aufgabe B1 · Auswahl und Einrichtung15 Punkte**

- Kurze Begründung der Auswahl (Funktionsumfang, Wartung, Dokumentation).
- Das Paket ist installiert und konfiguriert, die benötigten Tabellen sind angelegt.
- Passwörter werden vom Paket gehasht gespeichert.

**Aufgabe B2 · Login und Logout integriert15 Punkte**

- Login und Logout sind über das Menü erreichbar und fügen sich in das Template ein.
- Im Menü ist erkennbar, ob und als wer man angemeldet ist.

**Aufgabe B3 · Zugriffsschutz15 Punkte**

- Nur angemeldete Benutzer können Filme anlegen und ändern.
- Die Prüfung erfolgt serverseitig auf jeder geschützten Seite, nicht nur über ausgeblendete Links.

**Aufgabe B4 · Dokumentation10 Punkte**

Beschreiben Sie in einer kurzen Datei im Projekt (z. B. LOGIN.md) Installation, Konfiguration und die verwendeten Funktionen des Pakets so, dass eine Mitschülerin oder ein Mitschüler das Setup nachvollziehen kann.

## Für beide Wege

**Aufgabe S1 · Suchfilter in der Session merken15 Punkte**

In der Suche wählen Besucher Filter, z. B. einen Suchbegriff, ein Genre oder einen Streaming-Anbieter. Bisher gehen diese Werte verloren, sobald man eine Detailseite öffnet und über das Menü zur Suche zurückkehrt. Speichern Sie die gewählten Filter in der Session, sodass sie beim nächsten Aufruf wieder angewendet werden und im Formular ausgewählt bleiben. Damit ersetzen Sie die fixen Suchfilter aus dem JavaScript-Auftrag.

- Beim Absenden des Suchformulars werden alle Filterwerte gemeinsam in $_SESSION['filter'] gespeichert, als assoziatives Array (z. B. suche, genre_id, anbieter_id).
- Wird die Suchseite ohne Formulardaten aufgerufen, gelten die gespeicherten Filter. Gibt es keine, gelten die Standardwerte (alle Genres, alle Anbieter).
- Im Formular bleiben die Werte ausgewählt: Das Textfeld enthält den Suchbegriff (value), in Auswahllisten ist die gespeicherte Option selected, bei Checkboxen checked.
- Eine Schaltfläche **Filter zurücksetzen** entfernt die Filter mit unset($_SESSION['filter']) und zeigt wieder alle Filme.
- Gespeichert werden nur gültige Werte: IDs werden als ganze Zahl geprüft (filter_input(… FILTER_VALIDATE_INT)) und müssen zu einem vorhandenen Genre bzw. Anbieter gehören. Die Filmabfrage verwendet Prepared Statements, die Ausgabe im Formular htmlspecialchars().
- Beim Abmelden bleiben die Filter erhalten. Falls Ihr Login-Paket (Weg B) beim Logout die gesamte Session beendet, halten Sie das in LOGIN.md fest.

|   |
|---|
|**Hinweis: GET und Session ergänzen sich**<br><br>Die Suche sendet weiterhin per **GET**, denn sie verändert keine Daten und soll verlinkbar bleiben. Die Session sorgt dafür, dass die Filter auch dann erhalten bleiben, wenn die Suchseite später ohne Parameter aufgerufen wird, z. B. über das Menü oder den Zurück-Link der Detailseite.|

|   |
|---|
|**Grundgerüst: Filter übernehmen, zurücksetzen, anwenden**<br><br><?php<br><br>session_start(); // falls die Session nicht schon zentral gestartet wird<br><br>// 1) Neue Filter übernehmen, wenn das Formular abgeschickt wurde<br><br>if (isset($_GET['suchen'])) {<br><br>$genreId = filter_input(INPUT_GET, 'genre_id', FILTER_VALIDATE_INT);<br><br>$_SESSION['filter'] = [<br><br>'suche' => trim($_GET['suche'] ?? ''),<br><br>'genre_id' => $genreId ?: null, // ungültig oder leer -> alle Genres<br><br>];<br><br>}<br><br>// 2) Filter zurücksetzen<br><br>if (isset($_GET['reset'])) {<br><br>unset($_SESSION['filter']);<br><br>}<br><br>// 3) Aktive Filter: gespeicherte Werte oder Standardwerte<br><br>$filter = $_SESSION['filter'] ?? ['suche' => '', 'genre_id' => null];|

|   |
|---|
|**Ausschnitt aus der View: gespeichertes Genre bleibt ausgewählt**<br><br><select name="genre_id"><br><br><option value="">alle Genres</option><br><br><?php foreach ($genres as $g): ?><br><br><option value="<?= $g['id'] ?>"<br><br><?= (int) $g['id'] === $filter['genre_id'] ? 'selected' : '' ?>><br><br><?= htmlspecialchars($g['name']) ?><br><br></option><br><br><?php endforeach; ?><br><br></select>|

|   |
|---|
|**Ausblick**<br><br>Im MVC-Projekt aus Auftrag 06 gehören die Schritte 1 bis 3 in den Controller der Suche. Die View erhält nur das fertige Array $filter und markiert damit die ausgewählten Werte.|

## Bewertungsübersicht

|**Nr.**|**Weg A – Login selbst programmieren**|**Punkte**|**Erreicht**|
|---|---|---|---|
|A1|SQL-Script: Tabellen, Testbenutzer, MariaDB-Benutzer und Rechte|20||
|A3|Login-Seite mit Menüpunkt|15||
|A4|Login-Prüfung, Session, Redirect, Logout|15||
|A5|Seiten für Redakteure serverseitig geschützt|15||
|A6|Datenbankverbindung abhängig vom Benutzer|10||
|S1|Suchfilter in der Session merken|15||
||**Summe Weg A**|**90**||

|**Nr.**|**Weg B – Login-Plugin**|**Punkte**|**Erreicht**|
|---|---|---|---|
|B1|Auswahl und Einrichtung|15||
|B2|Login und Logout integriert|15||
|B3|Zugriffsschutz|15||
|B4|Dokumentation|10||
|S1|Suchfilter in der Session merken|15||
||**Summe Weg B**|**70**||

## Checkliste vor der Abgabe

### Weg A

- Das SQL-Script liegt im Projekt und läuft auf einer frischen Datenbank fehlerfrei durch.
- Im PHP-Code kommt root als Datenbankbenutzer nicht mehr vor.
- In der Tabelle users stehen nur Hashes, keine Klartext-Passwörter.
- Die Eingabe von add_movie.php in der Adresszeile ohne Anmeldung führt zur Login-Seite.
- Als Webuser angemeldet: keine Buttons zum Anlegen, Ändern, Löschen und kein Zugriff über die URL.
- Mit dem MariaDB-Benutzer webuser schlägt ein INSERT in die Filmtabelle fehl (z. B. in phpMyAdmin testen).
- Nach dem Abmelden ist kein Zugriff auf geschützte Seiten mehr möglich.
- Die Watchlist-Frage ist im SQL-Script beantwortet.

### Beide Wege – Suchfilter

- Genre wählen, Detailseite öffnen, über das Menü zurück zur Suche: Das Genre ist noch ausgewählt und die Liste gefiltert.
- Der Suchbegriff steht nach dem Seitenwechsel wieder im Textfeld.
- **Filter zurücksetzen** zeigt wieder alle Filme und leert das Formular.
- Eine manipulierte URL wie ?suchen=1&genre_id=abc wird nicht gespeichert und führt zu keinem Fehler.
- Nach dem Abmelden sind die Suchfilter noch vorhanden (Weg B: Verhalten in LOGIN.md beschrieben).
