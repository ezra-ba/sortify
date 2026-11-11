#dtsm

# Gemessener Größenvergleich: JSON und XML

## Datengrundlage

Verglichen werden die Dateien `filmvergleich.json` und `filmvergleich.xml`. Beide enthalten dieselben fünf realen Filme und dieselben Felder: ID, Titel, Erscheinungsjahr, Regie, Laufzeit und zwei Genres.

## Messung

Die Dateigröße wurde als Anzahl der Bytes der UTF-8-kodierten Dateien gemessen. Leerzeichen und Zeilenumbrüche wurden dabei mitgezählt, weil sie tatsächlich Teil der Dateien sind.

| Datei | Größe |
|---|---:|
| `filmvergleich.json` | 775 Byte |
| `filmvergleich.xml` | 1.156 Byte |

Damit ist die XML-Datei in diesem konkreten Beispiel 381 Byte beziehungsweise rund 49,16 % größer als die JSON-Datei.

## Interpretation

XML ist bei diesem Beispiel voraussichtlich größer, weil die Feldnamen in jedem Start- und End-Tag wiederholt werden. JSON verwendet dagegen Schlüssel-Wert-Paare und ist dadurch kompakter.

Der Vergleich ist ein konkretes Beispiel, aber kein allgemeingültiger Faktor: Die Dateigröße hängt unter anderem von der Anzahl der Datensätze, der Feldnamen, der Verschachtelung, der Einrückung und der verwendeten Kodierung ab.
