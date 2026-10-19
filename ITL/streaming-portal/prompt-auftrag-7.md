# Login und Zugriffsrechte

Zwei Ebenen von Benutzern

Anwendungsebene: die Personen, die sich im Portal anmelden. Sie stehen in den Tabellen users und usergroups Ihrer Datenbank.

Datenbankebene: die MariaDB-Benutzer webuser und redakteur, mit denen sich bei der Datenbank anmeldet. Sie werden mit CREATE USER und GRANT angelegt.

Meldet sich eine Person als Redakteur im Portal an, verbindet sich die Anwendung mit dem DB-Benutzer redakteur. Sonst verwendet sie webuser. 

## Aufgabe A1 · SQL-Script für die Benutzerverwaltung
Speichere alle SQL-Befehle dieser Aufgabe in einem SQL-Script innerhalb des PHP-Projekts, z. B. sql/benutzerverwaltung.sql. Das Script muss sich auf einer frischen Kopie der Datenbank fehlerfrei ausführen lassen.

### a) Tabellen für die Anwendung

Tabelle usergroups mit den Gruppen redakteur und webuser.

Tabelle users mit Benutzername (eindeutig), Passwort-Hash und group_id als Fremdschlüssel auf usergroups.

Mindestens ein Testbenutzer pro Gruppe. Im Feld für das Passwort steht nur der Hash, niemals das Klartext-Passwort. Die Spalte ist dafür VARCHAR(255).

### b) Benutzer und Rechte in MariaDB

Lege für die Datenbank wer_streamt_was die MariaDB-Benutzer webuser und redakteur mit eigenem Passwort an.

webuser darf alle Inhalte lesen (SELECT), aber keine Filme, Serien, Genres oder Anbieter anlegen, ändern oder löschen. Er muss außerdem die Tabelle users lesen können, damit der Login funktioniert.

redakteur darf die Inhaltstabellen zusätzlich mit INSERT, UPDATE und DELETE bearbeiten.

Vergebe nur die Rechte, die wirklich gebraucht werden. GRANT ALL und Rechte auf andere Datenbanken sind nicht erlaubt.

#### Überlegung: Wie ist mit der Watchlist umzugehen?

Endbenutzer sollen Filme auf ihre Watchlist setzen und wieder entfernen können, also schreibend auf eine Tabelle zugreifen, obwohl sie nur webuser sind. Welche Rechte braucht webuser dafür, und auf welche Tabelle beschränken wir sie? Beantworte die Frage in einem Kommentar im SQL-Script und setze die Antwort in den GRANT-Befehlen um. Falls die Datenbank noch keine Watchlist enthält, beschreibe, welche Rechte du vergeben würdest.

#### Hilfestellung: Passwort-Hash erzeugen

## Aufgabe A2 · Standardverbindung mit webuserPflicht

Ab jetzt ist webuser der Standardbenutzer beim Aufbau der Datenbankverbindung, nicht mehr root. Passe die Klasse dbConn entsprechend an. Diese Umstellung ist Voraussetzung für die Aufgaben A3 bis A6 und wird dort mitbewertet.

## Aufgabe A3 · Login-Seite

Über einen neuen Menüpunkt Anmelden wird eine Seite mit einem Login-Formular (Benutzername, Passwort) aufgerufen.

Das Formular sendet per POST und fügt sich in das Template ein.

Ist bereits jemand angemeldet, zeigt das Menü stattdessen den Benutzernamen und einen Eintrag Abmelden.

## Aufgabe A4 · Login prüfen, Session und Logout

Prüfe die eingegebenen Login-Daten gegen die Tabelle users. Bei erfolgreichem Login schreibe die Benutzerdaten in die Session.

Der Benutzer wird über ein Prepared Statement anhand des Benutzernamens geladen, das Passwort mit password_verify() geprüft.

In der Session stehen nur Benutzer-ID, Benutzername und Gruppe, niemals das Passwort.

Nach erfolgreichem Login wird die Session-ID mit session_regenerate_id(true) erneuert.

Bei falschen Daten erscheint eine allgemeine Meldung wie „Benutzername oder Passwort falsch“, ohne zu verraten, welcher Teil falsch war.

Abmelden entfernt nur die Anmeldedaten aus der Session (z. B. unset($_SESSION['user'])) und leitet auf die Startseite weiter. Die gespeicherten Suchfilter aus Aufgabe S1 bleiben erhalten.

## Aufgabe A5 · Seiten für Redakteure schützen

Sorge dafür, dass nur Redakteure die Seiten bzw. Actions zum Anlegen, Ändern und Löschen aufrufen können (z. B. add_movie, upd_movie).

Die Prüfung erfolgt serverseitig am Anfang jeder geschützten Seite.

Schreibe die Prüfung einmal als Funktion oder Methode (z. B. requireRole('redakteur')) und verwenden Sie sie überall wieder.

Nicht angemeldete Besucher werden zur Login-Seite umgeleitet, angemeldete Webuser erhalten eine Meldung, dass ihnen die Berechtigung fehlt.

Links und Buttons zum Anlegen, Ändern und Löschen sind nur für Redakteure sichtbar.

## Aufgabe A6 · Datenbankverbindung je nach Benutzer

Sobald ein Redakteur angemeldet ist, muss die Datenbank mit dem MariaDB-Benutzer redakteur statt mit webuser angesprochen werden. Hier zahlt sich die zentrale Klasse dbConn aus: Nur sie muss angepasst werden.

dbConn liest die Gruppe des angemeldeten Benutzers aus der Session und wählt danach die Zugangsdaten.

Die Zugangsdaten der MariaDB-Benutzer stehen in einer Konfigurationsdatei, nicht in der Session.