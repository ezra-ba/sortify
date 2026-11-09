#itl

# Implementierungsplan Watchlist

## Ziel

StreamingHeld erhält eine persönliche Watchlist für eingeloggte Benutzer. Titel können auf der Startseite, in der Suche, auf der Detailseite und auf der Watchlist-Seite per `fetch()` hinzugefügt oder entfernt werden. Die Oberfläche aktualisiert sich ohne vollständigen Seitenreload.

Die Umsetzung baut auf der vorhandenen ASP.NET-Core-MVC-Struktur, Entity Framework Core, MySQL, der bestehenden Session und dem bestehenden `Movie`-Modell auf.

## Bestand und Leitentscheidungen

| Bereich | Vorhandener Stand | Konsequenz |
|---|---|---|
| Framework | ASP.NET Core MVC auf .NET 10 | Controller-Action statt PHP-Skript |
| Datenzugriff | EF Core mit `StreamingDbContext` | parametrisierte SQL-Erzeugung durch EF Core |
| Login | Session-Schlüssel `user:id` | Benutzer-ID ausschließlich aus der Session lesen |
| Benutzer | Tabelle `users`, Schlüssel `id` | Watchlist-Fremdschlüssel auf `users.id` |
| Titel | Entität `Movie`, Tabelle `titel`, Schlüssel `titel_id` | Watchlist-Fremdschlüssel auf `titel.titel_id` |
| Frontend | Razor, Bootstrap, `wwwroot/js/site.js` | gemeinsamer Watchlist-Handler für alle Seiten |
| Datenbankrechte | Webuser darf `watchlist` lesen, einfügen und löschen | für die Toggle-Logik ausreichend |

## Wichtige Vorbedingung Datenbankschema

Vor der Anwendung muss ein einheitliches Watchlist-Schema festgelegt werden. Das bestehende Projekt verwendet im aktuellen EF-Modell `users` und `titel`; der Seed enthält dagegen ältere bzw. abweichende Namen (`benutzer`, `benutzer_id`, `status`).

Für die aktuelle Anwendung wird folgende Struktur empfohlen:

```sql
CREATE TABLE IF NOT EXISTS watchlist (
    watchlist_id INT AUTO_INCREMENT PRIMARY KEY,
    benutzer_id INT NOT NULL,
    titel_id INT NOT NULL,
    hinzugefuegt_am DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_watchlist_user
        FOREIGN KEY (benutzer_id) REFERENCES users(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_watchlist_title
        FOREIGN KEY (titel_id) REFERENCES titel(titel_id)
        ON DELETE CASCADE,
    CONSTRAINT uq_watchlist_user_title UNIQUE (benutzer_id, titel_id)
);
```

Der Name `benutzer_id` ist innerhalb der Watchlist mit dem bestehenden Projekt kompatibel, obwohl die Benutzer-Tabelle `users` heißt. Die Spalten müssen anschließend im EF-Mapping explizit zugeordnet werden. Die im Seed vorhandene Spalte `status` ist für die geforderte Funktion nicht erforderlich und sollte entweder entfernt oder bewusst als spätere Erweiterung behandelt werden.

## Umsetzung in Arbeitspaketen

### 1. Datenbank und EF-Modell

1. Neue Datei `sql/watchlist.sql` mit der finalen Tabelle, Fremdschlüsseln und Unique-Constraint anlegen.
2. `Models/WatchlistEntry.cs` anlegen mit:
   - `WatchlistId`
   - `UserId`
   - `TitleId`
   - `AddedAt`
   - Navigationen zu `PortalUser` und `Movie`
3. In `Data/StreamingDbContext.cs` `DbSet<WatchlistEntry>` ergänzen.
4. In `OnModelCreating` Tabelle, Spalten, Primärschlüssel, Fremdschlüssel, `CURRENT_TIMESTAMP` und den Unique-Index `(UserId, TitleId)` konfigurieren.
5. Beim Löschen eines Titels prüfen, dass Watchlist-Einträge durch `ON DELETE CASCADE` oder durch eine explizite Löschung in `MovieCatalog.DeleteAsync` entfernt werden.
6. `database/seed-streamingheld.sql` und `sql/benutzerverwaltung.sql` auf die gewählte Struktur abgleichen. Insbesondere dürfen Seed-Daten nicht weiterhin auf eine nicht vorhandene Tabelle `benutzer` zeigen.

### 2. Watchlist-Service

Einen getrennten `IWatchlistService` und `WatchlistService` anlegen. Dadurch bleibt die Controller-Action schlank und die Benutzerprüfung wird nicht über mehrere Controller dupliziert.

Empfohlene Methoden:

```csharp
Task<bool> ToggleAsync(int userId, int titleId, CancellationToken cancellationToken);
Task<bool> ContainsAsync(int userId, int titleId, CancellationToken cancellationToken);
Task<IReadOnlyList<WatchlistItemViewModel>> GetForUserAsync(int userId, CancellationToken cancellationToken);
Task<int> CountForUserAsync(int userId, CancellationToken cancellationToken);
```

Die Toggle-Logik:

1. Titel-ID serverseitig auf eine positive Zahl prüfen.
2. Vorhandenen Eintrag anhand von `userId` und `titleId` suchen.
3. Wenn vorhanden: löschen und `false` zurückgeben.
4. Wenn nicht vorhanden: Titel-Existenz prüfen, neuen Eintrag mit UTC-Zeit anlegen und `true` zurückgeben.
5. Bei konkurrierenden Requests den Unique-Index als letzte Datenbankgarantie beibehalten und den Fehler kontrolliert behandeln.

### 3. Controller-Endpunkte

Neuen `WatchlistController` anlegen:

- `POST /Watchlist/Toggle`
  - ganz am Anfang `HttpContext.Session.GetInt32("user:id")` lesen
  - ohne Session-ID `401 Unauthorized` als JSON/Statusantwort senden
  - `titleId` aus Request-Body oder Form-Parameter lesen
  - JSON `{ "inWatchlist": true|false }` zurückgeben
  - `ValidateAntiForgeryToken` verwenden; das Token wird im Razor-Markup bereitgestellt
- `GET /Watchlist/Index`
  - ganz am Anfang prüfen, ob `user:id` vorhanden ist
  - andernfalls mit `RedirectToAction("Login", "Auth", new { returnUrl = ... })` zur Anmeldung weiterleiten
  - Watchlist des Benutzers laden und an die View übergeben

Für nicht vorhandene Titel sollte der Toggle-Endpunkt `404 NotFound` liefern. Serverfehler dürfen nicht als erfolgreicher Toggle erscheinen.

### 4. ViewModels und Status beim Rendern

Anlegen:

- `ViewModels/WatchlistItemViewModel.cs` für Titel, `TitleId` und `AddedAt`
- optional ein kleines `WatchlistButtonViewModel` oder ein Partial mit `titleId`, `inWatchlist` und sichtbarem Buttontext

Damit beim initialen Rendern der korrekte Zustand angezeigt wird, sollte der Watchlist-Status für die aktuell dargestellten Titel in einer einzigen Abfrage geladen werden. Einzelne Datenbankabfragen pro Button sind zu vermeiden.

Praktische erste Ausbaustufe:

1. `IMovieCatalog` um eine Methode für die Watchlist-IDs des aktuellen Benutzers erweitern oder diese Abfrage im `WatchlistService` anbieten.
2. `HomeController` und `SearchController` erhalten die IDs als Teil ihres ViewModels.
3. `TitlesController.Details` setzt für den einzelnen Titel einen booleschen Status.
4. Alternativ kann ein Partial den Status über einen ViewData-Wert erhalten; ein stark typisiertes ViewModel ist langfristig wartbarer.

### 5. Buttons in den vorhandenen Views

Ein gemeinsames Partial `Views/Shared/_WatchlistButton.cshtml` erstellen. Der Button enthält:

```html
<button type="button"
        class="btn btn-watchlist"
        data-media-id="..."
        data-in-watchlist="true|false">
    Zur Watchlist hinzufügen
</button>
```

Der konkrete Text lautet abhängig vom Zustand:

- `Zur Watchlist hinzufügen`
- `Aus der Watchlist entfernen`

Das Partial wird in folgenden Views eingebunden:

- `Views/Home/Index.cshtml` im jeweiligen Filmdatensatz
- `Views/Search/Index.cshtml` im jeweiligen Ergebnis
- `Views/Titles/Details.cshtml` beim Titel
- `Views/Watchlist/Index.cshtml` bei jedem Watchlist-Eintrag

Die Buttons werden nur gerendert, wenn `Context.Session.GetInt32("user:id")` nicht null ist. Die Navigation erhält zusätzlich nur für eingeloggte Benutzer den Link `Meine Watchlist`.

### 6. Fetch-Implementierung

In `wwwroot/js/site.js` einen einzigen delegierten Handler für `.btn-watchlist` ergänzen:

1. Click-Event abfangen.
2. Bei Bedarf den Anti-Forgery-Token aus dem Formular oder einem Meta-Element lesen.
3. `FormData` mit `titleId` senden.
4. `POST` an `@Url.Action("Toggle", "Watchlist")` über ein `data-toggle-url` oder ein Meta-Element konfigurieren, damit keine fragile hardcodierte URL nötig ist.
5. Nicht erfolgreiche Antworten behandeln:
   - `401`: Hinweis anzeigen bzw. Login anbieten
   - `404`: Titel nicht vorhanden
   - sonst: allgemeine Fehlermeldung
6. Bei Erfolg `data-in-watchlist`, Buttontext, `aria-pressed` und optional `aria-label` aktualisieren.
7. Während des Requests den Button deaktivieren und danach immer wieder aktivieren.
8. Auf der Watchlist-Seite bei `inWatchlist: false` den Eintrag aus der Liste entfernen; bei leerer Liste den Hinweis `Deine Watchlist ist leer.` anzeigen.

### 7. Seite Meine Watchlist

`Views/Watchlist/Index.cshtml` als übersichtliche Liste oder Kartenansicht erstellen. Pro Eintrag anzeigen:

- Titel
- hinzugefügt am, lokal formatiert
- Link zur Detailseite
- gemeinsamer Watchlist-Button zum direkten Entfernen

Die Abfrage sortiert nach `AddedAt` absteigend. Bei null Einträgen wird kein leerer Container, sondern ein kurzer Hinweis ausgegeben.

### 8. Navigation und optionaler Bonus

In `Views/Shared/_Layout.cshtml` den Link `Meine Watchlist` neben dem Benutzerbereich anzeigen, sobald `user:id` gesetzt ist.

Der Bonus kann nach der Kernfunktion ergänzt werden:

- `CountForUserAsync` im Watchlist-Service verwenden
- Anzahl in Navigation, Profil oder einem bestehenden Benutzerbereich anzeigen
- die Anzahl nach erfolgreichem Toggle im Browser aktualisieren, wenn sie sichtbar ist

## Empfohlene Reihenfolge

1. Schema und Seed bereinigen.
2. EF-Modell und Mapping ergänzen.
3. Watchlist-Service mit Unit-nahen Tests erstellen.
4. Toggle-Action und Authentifizierungsverhalten implementieren.
5. Watchlist-Index mit Redirect-Schutz implementieren.
6. Statusdaten in Home, Suche und Detailseite integrieren.
7. Gemeinsames Button-Partial und `fetch()`-Handler einbauen.
8. Navigation ergänzen.
9. Titel-Löschung, konkurrierende Requests und Fehlerfälle testen.
10. Bonus erst nach erfolgreicher Kernabnahme umsetzen.

## Test- und Abnahmekriterien

### Datenbank

- Tabelle wird auf einer leeren Datenbank angelegt.
- `(benutzer_id, titel_id)` ist eindeutig.
- Fremdschlüssel verhindern unbekannte Benutzer und Titel.
- `hinzugefuegt_am` wird ohne Client-Wert automatisch gesetzt.
- Seed und Anwendung verwenden dieselben Tabellen und Spalten.

### Zugriffsschutz

- Nicht eingeloggte Benutzer sehen keine Watchlist-Buttons und keinen Navigationslink.
- `POST /Watchlist/Toggle` liefert ohne `user:id` Status `401`.
- `GET /Watchlist/Index` leitet ohne `user:id` zur Login-Seite weiter und führt danach nicht weiter aus.
- Ein Benutzer kann nur eigene Einträge ändern oder löschen.

### Funktion

- Hinzufügen liefert `{ "inWatchlist": true }`.
- Erneutes Klicken entfernt den Eintrag und liefert `{ "inWatchlist": false }`.
- Der Buttontext und sein Zustand ändern sich ohne Seitenreload.
- Die Watchlist ist nach `hinzugefuegt_am DESC` sortiert.
- Leere Watchlist zeigt einen Hinweis.
- Einträge können aus der Watchlist-Seite direkt entfernt werden.
- Fehler bei ungültiger Titel-ID werden sichtbar behandelt.

### Regression

- Login, Logout, Suche, Details und Administration funktionieren unverändert.
- Das Löschen eines Titels hinterlässt keine verwaisten Watchlist-Einträge.
- `dotnet build` läuft ohne Warnungen, die durch die Änderung neu entstehen.
- Manuelle Browserprüfung mit mindestens zwei Benutzern bestätigt die Trennung der Watchlists.

## Offene Entscheidung vor der Implementierung

Die einzige fachliche Entscheidung, die vor dem Coding verbindlich getroffen werden sollte, ist der Umgang mit dem alten Seed-Schema. Der Plan geht davon aus, dass die aktuelle Anwendung maßgeblich ist und `users` als Benutzer-Tabelle verwendet. Wenn die produktive Datenbank stattdessen noch `benutzer` verwendet, muss entweder das Login-Modell angepasst oder eine kontrollierte Migration auf `users` durchgeführt werden. Eine Mischform aus beiden Schemas sollte vermieden werden.
