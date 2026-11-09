#dtsm

# JSON – JavaScript Object Notation

## Was ist JSON?

JSON steht für **JavaScript Object Notation**. Es ist ein leichtgewichtiges, textbasiertes Datenformat zum Speichern und Austauschen strukturierter Daten. Obwohl der Name auf JavaScript verweist, ist JSON unabhängig von einer bestimmten Programmiersprache. Fast jede moderne Programmiersprache besitzt Bibliotheken, mit denen JSON gelesen und erzeugt werden kann.

JSON beschreibt Daten und deren Struktur, aber keine Darstellung auf dem Bildschirm. Ein Programm kann zum Beispiel Filmdaten als JSON empfangen und daraus eine Liste, eine Detailseite oder eine Watchlist anzeigen.

Ein einfaches Beispiel:

```json
{
  "titel": "Die Reise",
  "erscheinungsjahr": 2026,
  "verfuegbar": true
}
```

Das Beispiel enthält drei Eigenschaften: einen Text, eine Zahl und einen Wahrheitswert.

## Grundstruktur und Syntax

JSON besteht nur aus wenigen Bausteinen. Dadurch ist es kompakter als viele andere Formate, zum Beispiel XML.

### Objekte

Ein Objekt steht in geschweiften Klammern `{}` und enthält Schlüssel-Wert-Paare. Der Schlüssel muss immer in doppelten Anführungszeichen stehen:

```json
{
  "titel": "Die Reise",
  "laufzeit": 120
}
```

Zwischen Schlüssel und Wert steht ein Doppelpunkt. Mehrere Paare werden durch Kommas getrennt. Nach dem letzten Paar darf kein zusätzliches Komma stehen.

### Arrays

Ein Array steht in eckigen Klammern `[]` und enthält eine geordnete Liste von Werten:

```json
{
  "genres": ["Abenteuer", "Drama"],
  "bewertungen": [4, 5, 3]
}
```

Die Elemente eines Arrays müssen nicht zwingend denselben Datentyp besitzen. Für verständliche und gut wartbare Daten ist es aber meistens sinnvoll, eine einheitliche Struktur zu verwenden.

### Verschachtelung

Objekte und Arrays können ineinander verschachtelt werden:

```json
{
  "id": 42,
  "titel": "Die Reise",
  "regisseur": {
    "vorname": "Mia",
    "nachname": "Huber"
  },
  "genres": [
    {
      "id": 1,
      "name": "Abenteuer"
    },
    {
      "id": 2,
      "name": "Drama"
    }
  ]
}
```

Die Verschachtelung bildet Beziehungen ab. Sie sollte jedoch nicht unnötig tief werden, weil dadurch die Verarbeitung und Fehlersuche schwieriger werden.

## Die sechs JSON-Datentypen

JSON kennt genau sechs Datentypen:

| Datentyp | Beispiel | Bedeutung |
|---|---|---|
| String | `"StreamingHeld"` | Text in doppelten Anführungszeichen |
| Number | `120` oder `4.5` | Ganzzahl oder Zahl mit Dezimalstellen |
| Boolean | `true` oder `false` | Wahrheitswert |
| Object | `{ "id": 42 }` | Sammlung von Schlüssel-Wert-Paaren |
| Array | `["Film", "Serie"]` | Geordnete Liste von Werten |
| Null | `null` | Kein Wert beziehungsweise unbekannter Wert |

JSON besitzt keinen eigenen Datentyp für Datum, Uhrzeit, Währung oder Binärdateien. Ein Datum wird normalerweise als standardisierter String übertragen:

```json
{
  "erscheinungsdatum": "2026-05-12T18:30:00+02:00",
  "preis": 9.99,
  "waehrung": "EUR"
}
```

Die Anwendung muss festlegen und dokumentieren, welches Datumsformat und welche Zeitzone verwendet werden. Zahlen sollten nicht für Geldbeträge verwendet werden, wenn dadurch Rundungsfehler entstehen können. Häufig wird der Betrag in Cent als Ganzzahl gespeichert oder zusätzlich ein exaktes Dezimalformat vereinbart.

## Wichtige Syntaxregeln

- Eigenschaftsnamen stehen in doppelten Anführungszeichen.
- Strings werden ebenfalls mit doppelten Anführungszeichen geschrieben; einfache Anführungszeichen sind kein gültiges JSON.
- Schlüssel und Wert werden durch `:` getrennt.
- Mehrere Elemente werden durch Kommas getrennt.
- Nach dem letzten Element steht kein Komma.
- Kommentare sind in JSON nicht erlaubt.
- Ein Dokument hat normalerweise genau ein oberstes Objekt oder Array.
- JSON unterscheidet zwischen Groß- und Kleinschreibung: `titel` und `Titel` sind verschiedene Schlüssel.
- Die Standardkodierung ist UTF-8.

Gültig:

```json
{
  "titel": "Die Reise",
  "genres": ["Drama", "Abenteuer"]
}
```

Ungültig sind zum Beispiel einfache Anführungszeichen, ein fehlendes Komma oder ein Komma am Ende:

```text
{'titel': 'Die Reise',}
```


## JSON in Web-APIs

JSON wird besonders häufig bei REST-APIs und AJAX-/Fetch-Anfragen verwendet. Der Client sendet eine Anfrage an einen Server. Der Server verarbeitet sie und antwortet mit JSON. Dadurch kann eine Webseite Daten aktualisieren, ohne die gesamte Seite neu zu laden.

Eine typische Antwort könnte so aussehen:

```json
{
  "status": "success",
  "data": {
    "id": 42,
    "titel": "Die Reise"
  }
}
```

Der HTTP-Header sollte den Inhalt korrekt angeben:

```text
Content-Type: application/json; charset=utf-8
```

Bei einer Anfrage mit JSON wird häufig zusätzlich `Accept: application/json` verwendet. Ein API-Fehler sollte ebenfalls strukturiert zurückgegeben werden:

```json
{
  "status": "error",
  "code": "VALIDATION_ERROR",
  "message": "Der Titel darf nicht leer sein.",
  "fields": {
    "titel": "Pflichtfeld"
  }
}
```

## JSON Schema und Validierung

JSON selbst legt nur die grundlegende Syntax fest. Es sagt nicht automatisch, welche Eigenschaften Pflichtfelder sind, welche Datentypen erwartet werden oder welche Werte erlaubt sind. Dafür kann **JSON Schema** verwendet werden.

Ein vereinfachtes Schema für einen Film:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "type": "object",
  "required": ["id", "titel", "erscheinungsjahr"],
  "properties": {
    "id": { "type": "integer", "minimum": 1 },
    "titel": { "type": "string", "minLength": 1 },
    "erscheinungsjahr": { "type": "integer", "minimum": 1888 },
    "altersfreigabe": { "type": "integer", "enum": [0, 6, 12, 16, 18] }
  },
  "additionalProperties": false
}
```

Die Syntaxprüfung beantwortet die Frage: „Ist der JSON-Text korrekt aufgebaut?“ Die Schema-Validierung beantwortet zusätzlich: „Passt der Inhalt zu den vereinbarten Regeln?“ Beides ersetzt keine fachliche Prüfung und keine Berechtigungsprüfung.

## Typische Anwendungen

JSON wird unter anderem verwendet für:

- REST- und GraphQL-Schnittstellen,
- Kommunikation zwischen Browser und Server,
- Konfigurationsdateien und Einstellungen,
- Datenexport und Datenimport,
- Nachrichten zwischen Microservices,
- lokale Speicherung im Browser, zum Beispiel in `localStorage`,
- Protokolle und Telemetriedaten,
- Build- und Paketdateien wie `package.json`,
- strukturierte Antworten von Such- und Datenbankdiensten.

JSON ist besonders geeignet, wenn Daten zwischen unterschiedlichen Programmen übertragen werden und die Struktur aus Objekten, Listen und einfachen Werten besteht.

## Vorteile von JSON

- **Einfach:** Die Syntax ist klein und schnell erlernbar.
- **Lesbar:** Menschen können JSON-Dateien meistens direkt verstehen.
- **Kompakt:** JSON benötigt oft weniger Zeichen als XML.
- **Plattformunabhängig:** JSON funktioniert mit praktisch jeder modernen Programmiersprache.
- **Webfreundlich:** Browser und Web-APIs können JSON sehr einfach verarbeiten.
- **Flexibel:** Objekte, Arrays und Verschachtelungen bilden viele Datenmodelle ab.
- **Schnell:** Parser sind weit verbreitet und meist effizient.
- **Gut für Fetch und REST:** JSON passt direkt zu typischen Webanwendungen.

## Nachteile und Grenzen

- **Keine Kommentare:** Hinweise oder Erklärungen können nicht direkt im Standardformat gespeichert werden.
- **Keine eingebauten Datentypen für Datum und Dezimalzahlen:** Diese müssen als Konvention festgelegt werden.
- **Wenige Validierungsregeln:** Für Pflichtfelder und Wertebereiche ist ein JSON Schema oder zusätzliche Programmlogik nötig.
- **Datenmodellierung:** Komplexe Beziehungen und Referenzen müssen selbst vereinbart werden.
- **Redundanz:** Bei wiederholten Objekten können dieselben Daten mehrfach übertragen werden.
- **Zahlenpräzision:** Unterschiedliche Programmiersprachen können Zahlen unterschiedlich genau verarbeiten.
- **Keine Binärdaten:** Bilder, Videos oder Dateien werden normalerweise separat übertragen oder als Base64 eingebettet, was die Daten vergrößern kann.
- **Sicherheitsrisiken bei unkritischer Verarbeitung:** JSON muss validiert und mit sicheren Parsern verarbeitet werden. Eingelesene Werte dürfen nicht ungeprüft als HTML, SQL oder Programmcode verwendet werden.

## JSON im Vergleich zu XML

JSON und XML können beide strukturierte Daten speichern und austauschen. JSON ist in vielen modernen Web-APIs kürzer und bequemer. XML bietet dafür zusätzliche Möglichkeiten wie Namespaces, Attribute, gemischte Inhalte und ausgereifte Validierung mit XSD.

Für einen einfachen Film sieht JSON so aus:

```json
{
  "titel": "Die Reise",
  "laufzeit": 120
}
```

Dieselbe Information in XML:

```xml
<film>
  <titel>Die Reise</titel>
  <laufzeit>120</laufzeit>
</film>
```

Für eine Fetch-Schnittstelle zwischen Browser und Server ist JSON im Allgemeinen die praktischere Wahl. XML kann sinnvoll bleiben, wenn ein externer Anbieter dieses Format verlangt oder wenn ein vorhandenes System auf XML und XSD basiert.

## Bezug zum ITL-Streaming-Portal

Im ITL-Streaming-Portal werden unter anderem Filme, Serien, Genres, Personen, Streaming-Dienste, Bewertungen und persönliche Watchlists verwaltet. JSON eignet sich besonders für den laufenden Datenaustausch zwischen Browser und Backend, zum Beispiel bei Suchfiltern, Login, Bewertungen oder der Watchlist.

### Film aus dem Streaming-Portal

Eine API könnte einen Titel etwa so liefern:

```json
{
  "id": 42,
  "typ": "film",
  "titel": "Die Reise",
  "beschreibung": "Ein Abenteuerfilm.",
  "erscheinungsdatum": "2026-05-12",
  "laufzeitMinuten": 120,
  "altersfreigabe": 12,
  "genres": [
    { "id": 1, "name": "Abenteuer" },
    { "id": 2, "name": "Drama" }
  ],
  "streamingDienste": [
    { "id": 3, "name": "StreamingHeld", "zugriffsart": "Abo" }
  ]
}
```

Das Objekt bildet die fachliche Struktur des Portals ab: Ein Titel kann mehreren Genres und mehreren Streaming-Diensten zugeordnet sein. In der relationalen Datenbank werden solche n:m-Beziehungen über Verbindungstabellen wie `TITEL_GENRE` und `TITEL_DIENST` gespeichert. In JSON können die Beziehungen dagegen bequem als verschachtelte Arrays dargestellt werden.

### Suchfilter und Bewertungen

Auch komplexere Filter können strukturiert an das Backend gesendet werden:

```json
{
  "suchbegriff": "reise",
  "genres": ["Abenteuer", "Drama"],
  "minBewertung": 4,
  "maxLaufzeit": 140,
  "seite": 1,
  "proSeite": 20
}
```

Eine Bewertung könnte so übertragen werden:

```json
{
  "titelId": 42,
  "sterne": 5,
  "kommentar": "Spannender Film."
}
```

Das Backend muss Wertebereiche, Pflichtfelder und die Identität des Benutzers prüfen. Danach werden die Daten mit parametrisierten SQL-Befehlen gespeichert. JSON darf niemals direkt zu einer SQL-Anweisung zusammengesetzt werden, weil sonst SQL-Injection möglich wäre.

## Gute Praxis im Streaming-Portal

- Eine einheitliche Schreibweise für Schlüssel verwenden, zum Beispiel `camelCase`.
- Feldnamen, Datentypen und Pflichtfelder als API-Vertrag dokumentieren.
- Für Listen konsistente Objekte liefern statt bei jedem Eintrag eine andere Struktur zu verwenden.
- Datumswerte in einem eindeutig dokumentierten Format und mit Zeitzone übertragen.
- Keine Passwörter, Session-Geheimnisse oder unnötigen personenbezogenen Daten im JSON zurückgeben.
- Eingaben mit JSON Schema und zusätzlicher Geschäftslogik validieren.
- Berechtigungen serverseitig prüfen; der Client darf nicht als vertrauenswürdig gelten.
- Fehler über strukturierte Statuscodes und verständliche JSON-Fehlerobjekte melden.
- Große Kataloge mit Suche, Filterung, Pagination oder begrenzten Feldern übertragen.
- Versionen von APIs berücksichtigen, wenn sich die Struktur später ändern kann.
- SQL-Abfragen parametrisieren und Ausgaben kontextabhängig escapen.

## Merksätze

- JSON ist ein kompaktes, textbasiertes Format für strukturierte Daten.
- Die wichtigsten Bausteine sind Objekte, Arrays, Strings, Zahlen, Boolean und `null`.
- JSON ist nicht dasselbe wie ein JavaScript-Objekt und erlaubt keine Kommentare.
- JSON Schema ergänzt die reine Syntaxprüfung um fachliche Regeln.
- Web-APIs verwenden JSON häufig für Anfragen und Antworten ohne vollständiges Neuladen der Seite.
- Im ITL-Streaming-Portal eignet sich JSON besonders für Filme, Suchfilter, Bewertungen und Watchlists.
- Die Datenbankbeziehungen des Portals werden in JSON häufig als verschachtelte Objekte und Arrays dargestellt.
- Validierung, Berechtigungsprüfung und parametrisierte SQL-Befehle bleiben trotz JSON unbedingt notwendig.
