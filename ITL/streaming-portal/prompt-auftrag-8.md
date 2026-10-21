# Arbeitsauftrag 08 – Streaming-Portal

## Transaktionen im Streaming-Portal

### Abhängige Datensätze ganz oder gar nicht speichern

## Ausgangssituation

Dein Streaming-Portal basiert auf **.NET 10**, **ASP.NET Core MVC**, **Entity Framework Core** und **Pomelo.EntityFrameworkCore.MySql** für MySQL/MariaDB. Filme werden im `StreamingDbContext` als `Movie` gespeichert und über EF-Core-Beziehungen mit Genres und Streaming-Anbietern verknüpft. Beim Anlegen, Ändern und Löschen eines Films werden deshalb mehrere Tabellen bzw. Join-Tabellen verändert.

Wenn einer dieser Schritte fehlschlägt, darf kein halbfertiger Datenstand zurückbleiben. Beispielsweise darf kein Film ohne die vorgesehenen Zuordnungen oder ohne seinen Eintrag in `film` gespeichert werden. In diesem Auftrag sicherst du diese Schreibvorgänge mit EF-Core-Transaktionen ab und weist nach, dass ein Fehler einen vollständigen Rollback auslöst.

## Lernziele

Nach diesem Auftrag kannst du:

- erklären, was `COMMIT` und `ROLLBACK` in einer Datenbanktransaktion bewirken,
- in einem ASP.NET-Core-Projekt abhängige Schreibvorgänge erkennen,
- mit `StreamingDbContext.Database.BeginTransactionAsync()` eine Transaktion starten,
- Transaktionen mit `CommitAsync()` und `RollbackAsync()` bzw. `await using` sicher abschließen,
- Fehler in Controller-, Service- und EF-Core-Code bis zur Benutzeroberfläche behandeln,
- Cache-Invalidierung und Weiterleitung erst nach einem erfolgreichen Commit ausführen.

## Technischer Rahmen

- Zielplattform ist das vorhandene Projekt **.NET 10 / ASP.NET Core MVC**.
- Verwende den bereits registrierten `StreamingDbContext` und die Dependency Injection.
- Der Datenbankzugriff erfolgt ausschließlich über **Entity Framework Core** und LINQ; schreibe für diese Aufgabe keine PDO- oder rohen PHP-Lösungen.
- Der MySQL-/MariaDB-Zugriff wird über **Pomelo.EntityFrameworkCore.MySql** hergestellt.
- Die fachlichen Schreibvorgänge gehören in den vorhandenen Service `Services/MovieCatalog.cs` bzw. in eine von ihm verwendete Service-Schicht, nicht in die Razor Views.
- Die bestehenden Entitäten und Beziehungen sind zu berücksichtigen: `Movie`, `Genre`, `StreamingProvider`, `FilmType`, `SeriesType`, `Rating` sowie die Join-Tabellen `titel_genre` und `titel_dienst`.
- Die SQL-Tabellen müssen eine transaktionsfähige Storage-Engine wie **InnoDB** verwenden. Prüfe dies bei Bedarf mit `SHOW TABLE STATUS`.
- EF Core wirft bei Datenbankfehlern Exceptions. Behandle diese im Service bzw. Controller so, dass das Formular mit einer verständlichen Fehlermeldung und den eingegebenen Werten erneut angezeigt wird.
- Die vorhandene Datei-Cache-Implementierung (`IFileCache`/`MovieFileCache`) darf erst nach erfolgreichem Commit invalidiert werden.

## Aufgaben

### Aufgabe 1 · Übung: Commit und Rollback

Erstelle eine kurze technische Übung in `sql/transaktionen.sql` oder in einer passenden Projektdokumentation. Verwende dabei die tatsächlichen Tabellen- und Spaltennamen des Projekts, insbesondere `titel`, `genre`, `titel_genre` und – falls für den Test erforderlich – `titel_dienst`.

Führe die Testschritte in derselben Datenbankverbindung aus, zum Beispiel über eine MySQL-/MariaDB-Konsole oder ein geeignetes Datenbankwerkzeug. Dokumentiere:

1. einen Insert innerhalb einer Transaktion und die Abfrage vor dem Rollback,
2. die Abfrage nach `ROLLBACK` und warum der Testdatensatz nicht mehr vorhanden ist,
3. einen Insert innerhalb einer Transaktion und die Abfrage nach `COMMIT`,
4. den Unterschied zwischen expliziter Transaktion und Autocommit,
5. das anschließende Aufräumen des Testdatensatzes.

Beziehe die Ergebnisse auf das Verhalten des EF-Core-Codes: `SaveChangesAsync()` schreibt innerhalb der offenen Transaktion, dauerhaft bestätigt wird der Vorgang erst durch `CommitAsync()`.

### Aufgabe 2 · Stellen mit abhängigen Datensätzen finden

Untersuche `Services/MovieCatalog.cs`, die zugehörigen Controller sowie den `StreamingDbContext`. Liste alle Funktionen auf, in denen mehrere voneinander abhängige Datensätze gemeinsam geschrieben, geändert oder gelöscht werden.

Dokumentiere die Liste als Tabelle in `sql/transaktionen.sql` oder in einer `README.md` mit mindestens diesen Spalten:

| Funktion | Betroffene Entitäten/Tabellen | Was könnte ohne Transaktion schiefgehen? |
|---|---|---|
| Film anlegen | `Movie`, `FilmType`, `titel_genre`, `titel_dienst` | Film oder einzelne Zuordnungen bleiben ohne vollständigen Datensatz zurück |
| Film ändern | `Movie`, Genres, Streaming-Anbieter | alte Zuordnungen sind gelöscht, bevor neue vollständig gespeichert wurden |
| Film löschen | Zuordnungen, `FilmType`, `SeriesType`, `Rating`, `Movie` | abhängige Datensätze und Titel werden nur teilweise gelöscht |

Ergänze weitere Fundstellen, falls dein Projektstand zusätzliche abhängige Schreibvorgänge enthält.

### Aufgabe 3 · Transaktionen anwenden

Implementiere für die drei folgenden Vorgänge eine Transaktion im Service `MovieCatalog`. Verwende das Muster:

```csharp
await using var transaction =
    await db.Database.BeginTransactionAsync(cancellationToken);

try
{
    // zusammengehörige Änderungen und SaveChangesAsync-Aufrufe
    await transaction.CommitAsync(cancellationToken);
}
catch
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```

Wenn `await using` und die verwendete EF-Core-Version die Transaktion beim Verlassen des Scopes zuverlässig verwirft, darf die Fehlerbehandlung entsprechend vereinfacht werden. Ein erfolgreicher Commit muss trotzdem eindeutig erkennbar sein.

#### a) Film mit Genres und Anbietern anlegen

- Lade die ausgewählten Genre- und Anbieter-IDs serverseitig aus der Datenbank.
- Lege den `Movie` an und speichere ihn, damit die neue ID verfügbar ist.
- Speichere die Filmklassifizierung über `FilmType`, wie es das bestehende Modell verlangt.
- Speichere die m:n-Beziehungen für Genres und Anbieter über die EF-Core-Navigationen und die Join-Tabellen.
- Führe alle zusammengehörigen `SaveChangesAsync()`-Aufrufe innerhalb derselben Transaktion aus.
- Bei einem Fehler darf weder der Film noch eine einzelne Zuordnung oder Klassifizierung bestehen bleiben.
- Zeige die Fehlermeldung wieder im Formular an; die eingegebenen Werte sollen erhalten bleiben.

#### b) Film ändern

- Lade den Film inklusive `Genres` und `StreamingProviders`.
- Aktualisiere die Stammdaten.
- Ersetze die ausgewählten Genre- und Anbieter-Zuordnungen innerhalb derselben Transaktion.
- Speichere alle Änderungen und bestätige sie erst mit `CommitAsync()`.
- Wenn ein Schritt fehlschlägt, müssen auch bereits ausgeführte Änderungen zurückgerollt werden.

#### c) Film löschen

- Berücksichtige die vorhandenen Beziehungen zu Genres, Anbietern, `FilmType`, `SeriesType` und `Rating`.
- Lösche beziehungsweise entkopple abhängige Datensätze in einer Transaktion und entferne anschließend den `Movie`.
- Falls Fremdschlüssel mit `ON DELETE CASCADE` verwendet werden, dokumentiere, welche Löschvorgänge die Datenbank übernimmt.
- Begründe trotzdem, warum der gesamte fachliche Löschvorgang als eine atomare Operation behandelt werden muss.

### Aufgabe 4 · Nachweis: Rollback im Portal

Weise im laufenden ASP.NET-Core-Portal nach, dass die Transaktion wirkt:

1. Öffne das Formular zum Anlegen eines Films.
2. Provoziere einen Fehler in einem abhängigen Schritt. Du kannst zum Beispiel im Browser-Entwicklertool eine Genre- oder Anbieter-ID manipulieren oder testweise eine ungültige ID an den POST-Endpunkt senden.
3. Der Service muss die ungültige ID erkennen oder die Datenbank muss den Fremdschlüssel-Fehler auslösen.
4. Das Formular wird mit einer verständlichen Fehlermeldung erneut angezeigt.
5. Prüfe anschließend in der Datenbank, dass weder der neue Film noch Teilzuordnungen oder der zugehörige `FilmType` gespeichert wurden.

Dokumentiere Vorgehen und Ergebnis kurz in `sql/transaktionen.sql`, einer README oder einem Testprotokoll. Entferne nach dem Test alle bewusst erzeugten Testdaten.

## Abgabe und Nachweis

Die Abgabe enthält:

- die geänderte Implementierung im Service,
- die erforderlichen Anpassungen an Controller und Fehleranzeige,
- `sql/transaktionen.sql` oder eine gleichwertige Dokumentation der SQL-Übung,
- die vollständige Liste der abhängigen Schreibvorgänge,
- einen kurzen Rollback-Nachweis mit Datenbankprüfung.

## Checkliste vor der Abgabe

- [ ] `BeginTransactionAsync()`, `CommitAsync()` und eine geeignete Rollback-Behandlung sind im Service umgesetzt.
- [ ] Anlegen, Ändern und Löschen eines Films sind atomar.
- [ ] `Movie`, `FilmType` und die Genre-/Anbieter-Zuordnungen werden gemeinsam behandelt.
- [ ] Ungültige Genre- oder Anbieter-IDs werden serverseitig abgelehnt.
- [ ] Ein absichtlich provozierter Fehler hinterlässt keinen neuen Film und keine Teilzuordnungen.
- [ ] Das Formular zeigt die Fehlermeldung und die eingegebenen Werte erneut an.
- [ ] Weiterleitung und `IFileCache.ClearNewestAsync()` erfolgen erst nach erfolgreichem Commit.
- [ ] Die Datenbanktabellen verwenden InnoDB oder eine gleichwertige transaktionsfähige Engine.
- [ ] Die Änderung wurde mit `dotnet build` und einem manuellen oder automatisierten Rollback-Test geprüft.


> **Wichtig:** In die Transaktion gehören nur die fachlich zusammengehörigen Datenbankänderungen. Weiterleitung, Benutzeroberflächen-Ausgaben und das Leeren des Datei-Caches erfolgen erst nach einem erfolgreichen Commit.
