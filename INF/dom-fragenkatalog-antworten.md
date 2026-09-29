#inf

# Fragenkatalog zum Document Object Model

## Was ist das DOM?

Das DOM (Document Object Model) ist eine Programmierschnittstelle (API), die ein HTML- oder XML-Dokument im Arbeitsspeicher als Baum aus Objekten abbildet. JavaScript kann diesen Baum auslesen und verändern, zum Beispiel Elemente suchen, Inhalte ändern oder neue Elemente einfügen.

## Was ist das hierarchiehöchste Objekt im DOM? Nennen Sie drei Unterobjekte.

Das hierarchiehöchste Objekt im DOM ist `document`. Drei Unterobjekte beziehungsweise wichtige Bestandteile sind:

- `html`
- `head`
- `body`

Im Browser gehören außerdem `window`, `location` und `history` zum übergeordneten Browsermodell (BOM). Das Objekt `document` bildet dabei die Brücke zum DOM.

## Wie stellt sich die Struktur eines Dokumentes im Browser dar?

Ein Dokument wird als hierarchischer Baum dargestellt. Das Wurzelelement ist `document`. Darunter liegt das Element `html`, das in `head` und `body` aufgeteilt ist. Die Elemente können weitere Elemente und Text als Kinder enthalten.

Beispiel:

```text
document
└── html
    ├── head
    └── body
        └── h1
            └── Text
```

## Nennen Sie drei wichtige Knotentypen bei Webseiten.

Drei wichtige Knotentypen sind:

- Dokumentknoten, zum Beispiel `document`
- Elementknoten, zum Beispiel `<div>`, `<h1>` oder `<li>`
- Textknoten, zum Beispiel der Text innerhalb eines Elements

Weitere Knotentypen sind Attributknoten und Kommentarknoten.

## Was bewirkt die DOM-Methode `getElementById()`?

`getElementById()` sucht ein Element anhand seines eindeutigen `id`-Attributs und gibt dieses Element zurück. Wird kein passendes Element gefunden, liefert die Methode `null`.

```javascript
const liste = document.getElementById("liste");
```

## Was bewirkt die DOM-Methode `getElementsByTagName()`?

`getElementsByTagName()` sucht alle Elemente mit einem bestimmten HTML-Tag, zum Beispiel alle `li`-Elemente. Das Ergebnis ist eine live aktualisierte `HTMLCollection`. Änderungen am DOM können die Sammlung daher automatisch verändern.

```javascript
const eintraege = document.getElementsByTagName("li");
```

## Welche drei wesentlichen Kategorien zum Zugriff auf Elemente einer Webseite gibt es?

Elemente können vor allem auf drei Arten ausgewählt werden:

- über die ID mit `getElementById()`
- über den Tag-Namen mit `getElementsByTagName()`
- über die CSS-Klasse mit `getElementsByClassName()`

Zusätzlich gibt es die modernen und vielseitigeren Methoden `querySelector()` und `querySelectorAll()`, die CSS-Selektoren verwenden.

## Wie kann auf Formularinhalte zugegriffen werden?

Formularinhalte können über das jeweilige Formularelement ausgewählt und anschließend über die Eigenschaft `value` ausgelesen werden.

```javascript
const eingabe = document.getElementById("name");
const inhalt = eingabe.value;
```

Bei Formularfeldern wie `input`, `textarea` und `select` enthält `value` den aktuellen eingegebenen beziehungsweise ausgewählten Wert.
