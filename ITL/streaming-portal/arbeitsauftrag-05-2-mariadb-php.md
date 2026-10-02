#itl

**ARBEITSAUFTRAG 05_2 · STREAMING-PORTAL**

MariaDB und PHP: Daten aus der Datenbank

Suchfilter, Suchergebnisse, Detailseite und Startseite mit echten Datensätzen befüllen

|   |   |
|---|---|
|**Lehrstoffgebiete**|Datenbanken, PHP, PDO, objektorientierte Programmierung (Vererbung), Formularverarbeitung|
|**Aufbauend auf**|Arbeitsauftrag 05_1 (Infrastruktur) und dem gemeinsam ausgewählten ERM|
|**Klasse / Name**||
|**Abgabetermin**||
|**Punkte**|/ 80|

## Ausgangssituation

In Arbeitsauftrag 05_1 haben Sie die Infrastruktur für das Streaming-Portal aufgesetzt. Die Seiten zeigen derzeit noch Dummy-Daten. In diesem Auftrag verbinden Sie das Portal mit der MariaDB-Datenbank, die aus den ERM-Entwürfen der Klasse ausgewählt wurde: Suchfilter, Suchergebnisse, Detailseite und Startseite werden mit echten Datensätzen befüllt.

## Lernziele

Nach diesem Auftrag können Sie

- mit **PDO** eine Verbindung zu MariaDB herstellen und diese in einer eigenen Klasse kapseln,
- Datenbankzugriffe über **Vererbung** in Model-Klassen strukturieren,
- Formulardaten (POST) auswerten und daraus **Prepared Statements** mit Platzhaltern erzeugen,
- ein Resultset (Array) mit PHP durchlaufen und in Bootstrap-Markup ausgeben,
- Daten per **GET-Parameter** an eine Detailseite übergeben und dort sicher verarbeiten.

## Rahmenbedingungen

- Entwicklungsumgebung: XAMPP (Apache, MariaDB, PHP 8) wie in Auftrag 05_1.
- Datenbank: die gemeinsam ausgewählte Streaming-Datenbank. Tabellen- und Spaltennamen im Auftrag sind Beispiele; verwenden Sie die Namen aus dem ERM.
- Datenbankzugriff ausschließlich mit **PDO** und **Prepared Statements**. SQL-Befehle, die Benutzereingaben per String-Verkettung enthalten, gelten als nicht erfüllt.

|   |
|---|
|**Wichtige Regel: Kein SQL außerhalb der Models**<br><br>Jede Datenbankabfrage steht im Source-Code des zugehörigen **Models**, also in einer Methode der jeweiligen Model-Klasse. Die PHP-Seiten (index.php, search_results.php, detail.php …) binden das Model ein und rufen nur dessen Methoden auf.<br><br>Beispiel: $movies->getNewest(10) statt eines SELECT direkt in der Seite.|

### Vorgeschlagene Ordnerstruktur

|   |
|---|
|streaming-portal/<br><br>├── models/<br><br>│ ├── dbConn.class.php ← Verbindungsklasse (Aufgabe 1a)<br><br>│ ├── movies.class.php ← Model für Filme/Serien<br><br>│ └── genres.class.php ← Model für Genres (Aufgabe 2)<br><br>├── includes/<br><br>│ ├── search_filter.php ← Suchformular (Aufgabe 2)<br><br>│ ├── search_handler.php ← POST-Verarbeitung (Aufgabe 3, optional)<br><br>│ └── search_results.php ← Ergebnisliste (Aufgabe 1b, 4)<br><br>├── index.php ← Startseite (Aufgabe 6)<br><br>├── search.php ← Suchseite<br><br>└── detail.php ← Detailansicht (Aufgabe 5)|

|   |   |
|---|---|
|**Aufgabe 1 · Datenbankklasse, Models und Ergebnisliste**|**20 Punkte**|

### a) Klasse dbConn und abgeleitete Models (10 Punkte)

Erstellen Sie eine eigene Klasse dbConn, die die Verbindung zur Datenbank herstellt. Für jede benötigte Tabelle erstellen Sie ein Model, das von dbConn erbt, zum Beispiel für Filme class movies extends dbConn. Alle Models inklusive dbConn werden im Projektordner models gespeichert.

- Zugangsdaten (Host, Datenbankname, Benutzer, Passwort) stehen nur an einer Stelle.
- Die Verbindung wird mit charset=utf8mb4 aufgebaut, damit Umlaute korrekt dargestellt werden.
- Fehler werden als Exception gemeldet (PDO::ERRMODE_EXCEPTION), Ergebnisse als assoziative Arrays geliefert (PDO::FETCH_ASSOC).

|   |
|---|
|**Hilfestellung: Grundgerüst**<br><br>\|   \|<br>\|---\|<br>\|<?php<br><br>class dbConn {<br><br>protected PDO $pdo;<br><br>public function __construct() {<br><br>$dsn = 'mysql:host=localhost;dbname=streaming;charset=utf8mb4';<br><br>$this->pdo = new PDO($dsn, 'root', '', [<br><br>PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,<br><br>PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,<br><br>]);<br><br>}<br><br>}\|<br><br>Das Model erbt die Verbindung über $this->pdo und ergänzt eigene Methoden für die Abfragen, zum Beispiel getById(int $id) oder getNewest(int $limit).|

### b) Ergebnisliste auslagern (10 Punkte)

Erstellen Sie die Datei search_results.php für die Darstellung der gefundenen Datensätze. Sie enthält den Bootstrap-Code und den PHP-Code zum Durchlaufen des Resultsets. Vorerst werden hier noch die Dummy-Daten angezeigt; in Aufgabe 4 ersetzen Sie diese durch Datenbankinhalte.

Binden Sie search_results.php mit include auf Ihrer Suchseite ein.

|   |   |
|---|---|
|**Aufgabe 2 · Suchfilter aus der Datenbank befüllen**|**10 Punkte**|

Überarbeiten Sie search_filter.php: Befüllen Sie die enthaltenen Auswahlfelder (Drop-downs, Checkboxen o. Ä.) mit Werten aus der Datenbank, zum Beispiel alle Genres oder alle Streaming-Anbieter.

- Die Abfragen stehen als Methoden im passenden Model (z. B. genres->getAll()) und werden direkt vor dem Bootstrap-Code aufgerufen.
- Als value jeder Option wird die **ID** des Datensatzes verwendet, als angezeigter Text der Name.
- Jedes Auswahlfeld hat eine Option „alle“ mit leerem Wert, damit ohne Einschränkung gesucht werden kann.

|   |   |
|---|---|
|**Aufgabe 3 · Suchanfrage (POST) verarbeiten**|**20 Punkte**|

Werten Sie die POST-Daten des Suchformulars aus und erzeugen Sie daraus das passende SELECT-Statement für die Streaming-Datenbank. Die Abfrage steht im Model der Filme/Serien, zum Beispiel in einer Methode search(array $filter).

- Das Formular sendet mit method="post"; prüfen Sie mit $_SERVER['REQUEST_METHOD'] === 'POST', ob eine Suche abgeschickt wurde.
- Nur ausgefüllte Filter schränken die Suche ein. Bauen Sie die WHERE-Bedingung schrittweise auf und sammeln Sie die Werte in einem Parameter-Array.
- Alle Benutzereingaben werden über **Platzhalter** übergeben, nie direkt in den SQL-String eingefügt.
- Für eine Textsuche im Titel verwenden Sie LIKE mit Platzhalter; die %-Zeichen kommen in den Wert, nicht ins SQL.

|   |
|---|
|**Hinweis: Verarbeitung auslagern**<br><br>Die Verarbeitung des POST-Requests kann in eine eigene Datei (z. B. search_handler.php) ausgelagert werden, die **vor** search_results.php eingebunden wird. Das Resultset steht dann als Array (z. B. $results) für die Ausgabe zur Verfügung.<br><br>Ausblick: Die fixen Filterwerte werden in einem späteren Auftrag in der **Session** gespeichert, damit die Suche beim Seitenwechsel erhalten bleibt.|

|   |   |
|---|---|
|**Aufgabe 4 · Suchergebnisse ausgeben**|**10 Punkte**|

Durchlaufen Sie das Resultset in search_results.php und listen Sie die gefundenen Datensätze auf. Ersetzen Sie dabei die Dummy-Daten vollständig durch die Inhalte aus der Datenbank.

- Alle Werte aus der Datenbank werden bei der Ausgabe mit htmlspecialchars() geschützt.
- Liefert die Suche keine Treffer, erscheint eine verständliche Meldung (z. B. Bootstrap-Alert) statt einer leeren Seite.

|   |   |
|---|---|
|**Aufgabe 5 · Detailseite**|**10 Punkte**|

Verlinken Sie jeden gelisteten Datensatz auf die Seite detail.php. Übergeben Sie im Link die ID des Datensatzes als GET-Parameter, zum Beispiel detail.php?id=42.

- detail.php liest die ID aus $_GET, prüft sie auf eine gültige Ganzzahl und lädt den Datensatz über eine Model-Methode (z. B. getById()).
- Die Detailseite zeigt die wichtigsten Informationen zum Film bzw. zur Serie in einem Bootstrap-Layout an.
- Bei fehlender, ungültiger oder nicht vorhandener ID erscheint eine Fehlermeldung mit Link zurück zur Suche.

|   |   |
|---|---|
|**Aufgabe 6 · Die 10 neuesten Filme auf der Startseite**|**10 Punkte**|

Erstellen Sie ein Frontend-Element Ihrer Wahl (z. B. Karussell, Kartenreihe, Liste), das die **10 neuesten Filme** darstellt. Binden Sie dieses Element auf der Startseite ein und befüllen Sie es mit Datensätzen aus der Datenbank.

- Legen Sie fest, was „neu“ bedeutet (z. B. Erscheinungsdatum oder Datum der Aufnahme ins Portal), und sortieren Sie entsprechend absteigend.
- Die Abfrage steht als Methode im Movies-Model (z. B. getNewest(int $limit)) und begrenzt das Ergebnis mit LIMIT.
- Jeder Eintrag verlinkt ebenfalls auf detail.php.

## Bewertungsübersicht

|**Nr.**|**Aufgabe**|**Punkte**|**Erreicht**|
|---|---|---|---|
|1a|Klasse dbConn und abgeleitete Models|10||
|1b|Ergebnisliste in search_results.php ausgelagert|10||
|2|Suchfilter aus der Datenbank befüllt|10||
|3|POST-Daten verarbeitet, Prepared Statement im Model|20||
|4|Suchergebnisse ausgegeben|10||
|5|Detailseite mit GET-Parameter|10||
|6|Die 10 neuesten Filme auf der Startseite|10||
||**Summe**|**80**||

## Checkliste vor der Abgabe

- Kein SQL-Befehl steht außerhalb eines Models.
- Alle Abfragen mit Benutzereingaben verwenden Prepared Statements mit Platzhaltern.
- Alle Ausgaben von Datenbankwerten sind mit htmlspecialchars() geschützt.
- Die Suche funktioniert mit einem, mehreren und keinem ausgefüllten Filter.
- Umlaute werden in Filtern, Ergebnissen und auf der Detailseite korrekt angezeigt.
- detail.php?id=abc und detail.php?id=999999 liefern eine Fehlermeldung statt einer PHP-Warnung.
- Auf der Startseite erscheinen genau die 10 neuesten Filme.
- Es werden keine Dummy-Daten mehr angezeigt.
