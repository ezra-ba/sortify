# Präsentationsskript: JSON und XML – Seiten 7–10

## Seite 7 – XML beschreibt Daten mit Tags

**Sprechertext:**

Auf dieser Folie sehen wir, wie XML Daten beschreibt. XML verwendet dafür sogenannte Tags. Ein Element beginnt mit einem öffnenden Tag und endet mit einem passenden schließenden Tag. Im Beispiel steht der Film im Wurzelelement `<film>`. Darin befinden sich die Elemente `<titel>`, `<jahr>` und `<verfuegbar>`.

Zusätzliche Informationen können als Attribute im öffnenden Tag angegeben werden, zum Beispiel die Film-ID `id="42"`. Weitere typische Attribute wären `typ`, `status`, `sprache` oder `altersfreigabe`. Solche Attribute eignen sich vor allem für kurze Zusatzinformationen und Metadaten. Größere oder besonders wichtige Inhalte werden meist als eigene Elemente dargestellt.

Wichtig ist außerdem, dass jedes XML-Dokument genau ein Wurzelelement besitzt. Das Wurzelelement ist immer das äußerste Element. Sein Name ist nicht fest vorgeschrieben und hängt vom Datenmodell ab. Es kann zum Beispiel `<film>`, `<filme>` oder `<katalog>` heißen. Mehrere Filme sind möglich, wenn sie gemeinsam innerhalb eines Wurzelelements wie `<filme>` stehen. Die XML-Deklaration am Anfang enthält technische Informationen: `version="1.0"` gibt an, nach welcher XML-Spezifikation das Dokument aufgebaut ist, und `encoding="UTF-8"` legt die Zeichenkodierung fest. Dadurch können auch Umlaute und andere Sonderzeichen korrekt verarbeitet werden.

## Seite 8 – Wohlgeformt oder gültig?

**Sprechertext:**

Hier unterscheiden wir zwischen „wohlgeformt“ und „gültig“. Ein XML-Dokument ist wohlgeformt, wenn die Syntax stimmt: Die Tags sind korrekt geöffnet und geschlossen, richtig verschachtelt und die Attribute stehen in Anführungszeichen.

„Gültig“ bedeutet zusätzlich, dass das Dokument festgelegte fachliche Regeln erfüllt. Diese Regeln können beispielsweise in einer DTD oder einer XSD beschrieben werden. Eine XSD-Datei kann festlegen, welche Elemente vorkommen müssen und welche Datentypen erlaubt sind.

XML bietet außerdem Namespaces. Sie verhindern Namenskonflikte, wenn Daten aus unterschiedlichen XML-Vokabularen kombiniert werden. 

Die Abkürzungen stehen für unterschiedliche Aufgaben: DTD bedeutet „Document Type Definition“ und beschreibt grundlegende XML-Regeln. XSD steht für „XML Schema Definition“ und kann zusätzlich Datentypen, Pflichtfelder und Wertebereiche prüfen. DOM bedeutet „Document Object Model“ und lädt das gesamte XML-Dokument als Baum in den Arbeitsspeicher. SAX steht für „Simple API for XML“ und liest das Dokument schrittweise über Ereignisse ein. Streaming bezeichnet die fortlaufende Verarbeitung eines Datenstroms, ohne das gesamte Dokument zu speichern. DTD und XSD prüfen also die Struktur, während DOM, SAX und Streaming festlegen, wie die Daten eingelesen werden.

## Seite 9 – JSON und XML im direkten Vergleich

**Sprechertext:**

Beide Formate können dieselben Filmdaten darstellen. JSON verwendet dafür kompakte Schlüssel-Wert-Paare und geschweifte Klammern. XML nutzt Tags und ist dadurch ausführlicher.

Bei der Lesbarkeit ist JSON meistens kompakter, während XML mehr Informationen direkt über seine Tags und Attribute ausdrückt. JSON-Dateien sind daher oft kleiner. In der Syntax unterscheiden sie sich deutlich: JSON arbeitet mit Klammern und Paaren, XML mit Tags und Attributen.

Auch bei der Validierung gibt es Unterschiede. Für JSON gibt es JSON Schema, für XML werden häufig DTD oder XSD verwendet. JSON ist bei modernen Web-APIs sehr verbreitet. XML ist besonders dann sinnvoll, wenn Kataloge, Standards, Namespaces oder umfangreiche Integrationen eine Rolle spielen.

## Seite 10 – Anwendung im ITL-Streaming-Portal

**Sprechertext:**

Im ITL-Streaming-Portal ergänzen sich beide Formate. JSON eignet sich besonders für die laufende Kommunikation im Web: zum Beispiel für Suchfilter, Bewertungen und Watchlists. Auch moderne Web-APIs verwenden JSON häufig, weil es kompakt ist und sich im Browser leicht verarbeiten lässt.

XML passt dagegen gut zu Katalogen und zu Daten, die mit XSD-Regeln geprüft werden. Ein externer Anbieter könnte beispielsweise einen Filmkatalog als XML liefern. Das Portal validiert die Datei und importiert anschließend die Filme, Genres und Streaming-Dienste. XML macht hier besonders Sinn, weil ein externer Katalog eine feste Struktur und genaue Regeln einhalten muss. Mit XML und XSD kann geprüft werden, ob Pflichtfelder vorhanden sind, Datentypen stimmen und die Elemente korrekt aufgebaut sind. JSON wäre zwar ebenfalls möglich, XML bietet für solche standardisierten Katalogimporte aber ausführlichere und etablierte Validierungsmöglichkeiten.

Die Entscheidung lautet deshalb nicht „JSON oder XML für alles“, sondern: JSON für kompakte Web-Kommunikation und XML für geprüfte Kataloge, Standards und Integrationen. Beide Formate haben ihren passenden Einsatzbereich.

