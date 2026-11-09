#dtsm

# XML – Extensible Markup Language

## Was ist XML?

XML steht für **Extensible Markup Language** und ist eine textbasierte Auszeichnungssprache. Sie wurde entwickelt, um strukturierte Daten unabhängig von einem bestimmten Programm, Betriebssystem oder Hersteller zu speichern und zwischen verschiedenen Systemen auszutauschen.

XML beschreibt nicht in erster Linie, wie Daten dargestellt werden, sondern **welche Bedeutung und Struktur** die Daten besitzen. Die verwendeten Elemente dürfen selbst gewählt werden. Dadurch ist XML erweiterbar und kann an unterschiedliche Anwendungsbereiche angepasst werden.

Beispiel:

```xml
<film>
    <titel>Die Reise</titel>
    <erscheinungsdatum>2026-05-12</erscheinungsdatum>
    <laufzeit minuten="120">120</laufzeit>
</film>
```

Der Computer kann aus diesem Beispiel erkennen, dass es einen Film mit einem Titel, einem Erscheinungsdatum und einer Laufzeit gibt. Die Namen `film`, `titel` und `erscheinungsdatum` sind keine fest eingebauten XML-Befehle, sondern fachlich passende Elementnamen.

## Grundaufbau und Syntax

### Elemente und Tags

Ein Element besteht normalerweise aus einem Start-Tag, einem Inhalt und einem End-Tag:

```xml
<titel>Die Reise</titel>
```

Das Start-Tag ist `<titel>`, der Inhalt ist `Die Reise` und das End-Tag ist `</titel>`. Elemente können ineinander verschachtelt werden:

```xml
<film>
    <titel>Die Reise</titel>
    <genre>Abenteuer</genre>
</film>
```

Ein Element ohne Inhalt kann als selbstschließendes Element geschrieben werden:

```xml
<beschreibung />
```

### Wurzelelement

Jedes XML-Dokument muss genau **ein Wurzelelement** besitzen. Alle anderen Elemente liegen innerhalb dieses Elements:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<katalog>
    <film>
        <titel>Die Reise</titel>
    </film>
</katalog>
```

Die XML-Deklaration ist optional, aber empfehlenswert. `UTF-8` stellt sicher, dass auch Umlaute und andere Sonderzeichen korrekt interpretiert werden.

### Attribute

Attribute enthalten zusätzliche Informationen zu einem Element. Sie stehen im Start-Tag und müssen immer in Anführungszeichen stehen:

```xml
<film id="42" altersfreigabe="12">
    <titel>Die Reise</titel>
</film>
```

Attribute eignen sich besonders für kurze Zusatzinformationen, Kennungen oder technische Eigenschaften. Inhalt, der eine eigene Bedeutung oder Struktur besitzt, sollte besser in einem eigenen Element stehen.

### Groß- und Kleinschreibung

XML unterscheidet zwischen Groß- und Kleinschreibung. Die folgenden Namen sind daher verschieden:

```xml
<Titel>...</Titel>
<titel>...</titel>
```

Ein Start- und ein End-Tag müssen exakt gleich geschrieben werden. In der Praxis ist es sinnvoll, eine einheitliche Schreibweise zu verwenden, zum Beispiel nur kleingeschriebene Namen mit Bindestrichen.

### Sonderzeichen und CDATA

Bestimmte Zeichen müssen durch sogenannte Entitäten ersetzt werden:

| Zeichen | XML-Schreibweise |
|---|---|
| `&` | `&amp;` |
| `<` | `&lt;` |
| `>` | `&gt;` |
| `"` | `&quot;` |
| `'` | `&apos;` |

Für größere Textbereiche, in denen viele Sonderzeichen vorkommen, kann ein CDATA-Abschnitt verwendet werden:

```xml
<beschreibung><![CDATA[5 < 10 und 10 > 5]]></beschreibung>
```

## Wohlgeformtheit und Gültigkeit

Ein XML-Dokument ist **wohlgeformt**, wenn es die grundlegenden XML-Syntaxregeln einhält. Dazu gehören:

- genau ein Wurzelelement,
- jedes Element besitzt ein passendes End-Tag,
- Elemente sind korrekt verschachtelt,
- Attributwerte stehen in Anführungszeichen,
- Elementnamen dürfen keine Leerzeichen enthalten,
- XML-Sonderzeichen werden korrekt maskiert.

Beispiel für eine falsche Verschachtelung:

```xml
<film><titel>Die Reise</film></titel>
```

Ein Dokument kann außerdem **gültig** beziehungsweise validierbar sein. Dafür wird eine zusätzliche Regelbeschreibung verwendet, die festlegt, welche Elemente, Attribute, Datentypen und Reihenfolgen erlaubt sind.

### DTD und XML Schema

Eine ältere Möglichkeit zur Beschreibung der Struktur ist die **DTD** (Document Type Definition). Heute wird häufig **XML Schema (XSD)** eingesetzt. XSD kann Datentypen wie `string`, `date`, `integer` oder `boolean` genauer festlegen.

Beispiel einer vereinfachten XSD-Idee:

```xml
<xs:element name="titel" type="xs:string" />
<xs:element name="erscheinungsdatum" type="xs:date" />
```

Die Wohlgeformtheit prüft die XML-Syntax. Eine XSD-Validierung prüft zusätzlich, ob die Daten zur vereinbarten fachlichen Struktur passen.

## Namensräume

Namensräume verhindern Namenskonflikte, wenn XML-Daten aus unterschiedlichen Quellen kombiniert werden. Dafür wird ein Präfix mit einer eindeutigen URI verbunden:

```xml
<portal:film xmlns:portal="https://beispiel.at/streaming">
    <portal:titel>Die Reise</portal:titel>
</portal:film>
```

Die URI muss nicht zwingend eine aufrufbare Website sein. Sie dient vor allem als eindeutige Kennzeichnung des Vokabulars.

## Kommentare und Formatierung

Kommentare beginnen mit `<!--` und enden mit `-->`:

```xml
<!-- Dieser Film ist nur ein Beispieldatensatz. -->
```

Kommentare werden von XML-Parsern normalerweise ignoriert und sollten keine vertraulichen Informationen enthalten. Einrückungen und Zeilenumbrüche sind für XML meist nicht zwingend, verbessern aber die Lesbarkeit deutlich.

## Verarbeitung von XML

Programme verwenden XML-Parser, um die Datei einzulesen. Häufige Verfahren sind:

- **DOM-Parser:** Lädt das gesamte Dokument als Baum in den Speicher. Die Daten können einfach durchsucht und verändert werden, bei großen Dateien ist der Speicherbedarf aber hoch.
- **SAX-Parser:** Liest das Dokument fortlaufend und meldet Ereignisse wie „Element begonnen“ oder „Element beendet“. Das ist speichersparend, aber programmiertechnisch weniger bequem.
- **Streaming-Reader:** Liest XML ähnlich wie SAX schrittweise und eignet sich besonders für große Datenmengen.

Zum Erzeugen oder Umwandeln von XML werden unter anderem XSLT-Transformationsregeln verwendet. Mit XSLT kann XML beispielsweise in HTML oder ein anderes XML-Format umgewandelt werden.

## Typische Anwendungen

XML wird unter anderem eingesetzt für:

- Konfigurationsdateien von Programmen und Frameworks,
- Datenaustausch zwischen unterschiedlichen Systemen,
- Webservices und ältere SOAP-Schnittstellen,
- RSS- und Atom-Feeds,
- Dokumentformate wie SVG und viele Office-Formate,
- Export- und Importdateien in Datenbanken,
- Metadaten, Kataloge und Archivsysteme,
- strukturierte Nachrichten in Unternehmensanwendungen.

JSON ist bei modernen Web-APIs oft kompakter und einfacher zu verarbeiten. XML bleibt jedoch wichtig, wenn eine sehr genaue Validierung, Namensräume, gemischte Inhalte oder etablierte Unternehmensstandards erforderlich sind.

## Vorteile von XML

- **Erweiterbarkeit:** Eigene Element- und Attributnamen können definiert werden.
- **Lesbarkeit:** Die Struktur ist für Menschen relativ gut nachvollziehbar.
- **Plattformunabhängigkeit:** XML ist nicht an eine bestimmte Programmiersprache oder Datenbank gebunden.
- **Strukturierung:** Verschachtelte Daten und Beziehungen lassen sich abbilden.
- **Validierung:** DTD und XSD können Regeln und Datentypen festlegen.
- **Werkzeugvielfalt:** Für praktisch jede Programmiersprache gibt es XML-Bibliotheken.
- **Metadaten:** Daten können direkt mit zusätzlichen Informationen versehen werden.
- **Namensräume:** Vokabulare verschiedener Systeme können sauber kombiniert werden.

## Nachteile und Grenzen

- **Hoher Speicherbedarf:** Start- und End-Tags machen XML-Dateien relativ groß.
- **Mehr Übertragungsaufwand:** Bei vielen Datensätzen ist XML meist größer als JSON oder ein binäres Format.
- **Komplexität:** Namespaces, XSD, XSLT und verschiedene Parser können den Einstieg erschweren.
- **Verarbeitung:** DOM benötigt bei großen Dateien viel Arbeitsspeicher; Streaming-Verfahren sind schwieriger zu programmieren.
- **Fehleranfälligkeit:** Falsch geschriebene Tags, Sonderzeichen oder Namespaces können die Verarbeitung verhindern.
- **Weniger bequem für einfache APIs:** Für einfache Schlüssel-Wert-Daten ist JSON oft kürzer und übersichtlicher.
- **Sicherheitsrisiken:** Unsichere Parser-Konfigurationen können beispielsweise XXE-Angriffe oder übermäßige Ressourcenverwendung ermöglichen.

## Bezug zum ITL-Streaming-Portal

Das ITL-Streaming-Portal verwaltet Filme und Serien mit gemeinsamen Titeldaten. Im ERM-Modell gibt es unter anderem die Entitäten `TITEL`, `FILM`, `SERIE`, `GENRE`, `PERSON`, `STREAMING_DIENST`, `BEWERTUNG` und `WATCHLIST`. XML könnte als Austauschformat für einen Katalogexport oder für den Import von Filmdaten verwendet werden.

Ein möglicher XML-Katalog könnte so aussehen:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<katalog xmlns="https://beispiel.at/streaming">
    <titel id="42" typ="film">
        <name>Die Reise</name>
        <beschreibung>Ein Abenteuerfilm.</beschreibung>
        <erscheinungsdatum>2026-05-12</erscheinungsdatum>
        <altersfreigabe>12</altersfreigabe>
        <laufzeit minuten="120" />
        <genres>
            <genre>Abenteuer</genre>
            <genre>Drama</genre>
        </genres>
        <streaming-dienste>
            <dienst name="StreamingHeld" zugriffsart="Abo" />
        </streaming-dienste>
    </titel>
</katalog>
```

### Mögliche Einsatzszenarien im Portal

1. **Katalogimport:** Ein Redakteur erhält von einem externen Anbieter eine XML-Datei mit neuen Filmen. Ein Importprogramm prüft das XML, validiert es gegen ein XSD und speichert die Daten in den Tabellen des Streaming-Portals.
2. **Katalogexport:** Das Portal exportiert Titel, Genres und Streaming-Dienste als XML, damit ein anderes System den Katalog übernehmen kann.
3. **Datenaustausch zwischen Modulen:** Ein separates Administrations- oder Importmodul kann XML-Nachrichten an das Portal übergeben.
4. **Backup für strukturierte Fachdaten:** XML kann für einen lesbaren Export der fachlichen Daten verwendet werden. Ein vollständiges Datenbank-Backup ersetzt es jedoch nicht.
5. **Validierung:** Ein XSD kann sicherstellen, dass jeder Titel eine ID, einen Namen und ein gültiges Datum besitzt und dass `zugriffsart` nur erlaubte Werte enthält.

Die im ITL-Portal verwendeten Beziehungen müssen beim Import berücksichtigt werden. Ein Titel kann mehreren Genres und mehreren Streaming-Diensten zugeordnet sein. Das entspricht den n:m-Beziehungen über `TITEL_GENRE` und `TITEL_DIENST`. Daher sollte der Import zuerst den Haupttitel anlegen, danach Genres und Dienste auflösen und anschließend die Zuordnungen in den Verbindungstabellen speichern. Für einen solchen mehrstufigen Schreibvorgang ist eine Datenbanktransaktion sinnvoll: Wenn ein Teil fehlschlägt, werden alle Änderungen zurückgerollt.

Die persönliche Watchlist ist dagegen benutzerspezifisch. Ein allgemeiner Katalogexport sollte daher keine privaten Watchlist-Einträge enthalten. Falls Watchlists exportiert werden müssen, sollte der Export nur nach einer eindeutigen Benutzerprüfung und mit möglichst wenigen personenbezogenen Daten erfolgen.

## XML oder JSON im Streaming-Portal?

Für eine moderne JavaScript-Schnittstelle des Portals, zum Beispiel eine `fetch()`-Anfrage für die Watchlist, wäre JSON wahrscheinlich die praktischere Wahl. Eine kleine Antwort könnte so aussehen:

```json
{ "inWatchlist": true }
```

XML wäre sinnvoller, wenn ein externer Kataloganbieter XML mit XSD verlangt, wenn ein bestehender Unternehmensstandard verwendet wird oder wenn umfangreiche Metadaten und Namespaces benötigt werden. Für den laufenden Austausch zwischen Browser und ASP.NET-Core-Portal ist JSON meist kompakter; für den standardisierten Katalogimport oder -export kann XML trotzdem eine gute Ergänzung sein.

## Merksätze

- XML beschreibt strukturierte Daten, nicht deren konkrete Bildschirmdarstellung.
- Ein XML-Dokument braucht genau ein Wurzelelement.
- Wohlgeformt bedeutet syntaktisch korrekt; validiert bedeutet zusätzlich fachlich regelkonform.
- XSD kann Struktur und Datentypen genauer festlegen.
- XML ist ausführlich, robust und gut für standardisierte Datenaustausche geeignet.
- Bei großen Dateien und einfachen Web-APIs sind Streaming-Verfahren oder JSON oft effizienter.
- Im Streaming-Portal eignet sich XML besonders für validierte Katalogimporte und -exporte.
