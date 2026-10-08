# INF Test 1

#flashcards 
## Web Grundlagen

Nenne drei typische Einsatzmöglichkeiten der Browser-DevTools.::HTML und CSS untersuchen oder live verändern; JavaScript-Fehler in der Konsole prüfen; Netzwerkzugriffe und Ladezeiten analysieren.
<!--SR:!2026-10-09,1,230-->

Was bedeutet clientseitige Programmierung?::Der Code wird im Browser des Benutzers ausgeführt, typischerweise mit HTML, CSS und JavaScript.
<!--SR:!2026-10-09,1,230-->

Was ist eine statische Webseite?::Eine Webseite mit festem Inhalt, der sich ohne Änderung der Dateien nicht automatisch verändert.
<!--SR:!2026-10-09,1,230-->

Was ist eine dynamische Webseite?::Eine Webseite, deren Inhalte sich durch Benutzereingaben, JavaScript, Serverlogik oder Datenbankdaten verändern können.
<!--SR:!2026-10-09,1,230-->

Was ist der Unterschied zwischen clientseitiger und serverseitiger Programmierung?::Clientseitiger Code läuft im Browser; serverseitiger Code läuft auf dem Webserver und verarbeitet Daten oder erzeugt Inhalte vor der Übertragung.
<!--SR:!2026-10-09,1,230-->

Wofür steht CSS und wozu dient es?::CSS steht für Cascading Style Sheets und bestimmt Darstellung, Farben, Schriften, Abstände, Größen und Layout einer Webseite.
<!--SR:!2026-10-09,1,170-->

Wozu wird JavaScript auf Webseiten verwendet?::Für dynamische Inhalte und Interaktionen, etwa Klickreaktionen, Formularprüfung, DOM-Änderungen und das Nachladen von Daten.
<!--SR:!2026-10-09,1,230-->

Worauf bietet die DOM-Schnittstelle Zugriff?::Auf die Dokumentstruktur mit HTML-Elementen, Texten und Attributen, die JavaScript lesen und verändern kann.
<!--SR:!2026-10-09,1,230-->

Welche Aufgabe hat ein Webbrowser?::Er lädt Ressourcen, interpretiert HTML, CSS und JavaScript und stellt die resultierende Webseite dar.
<!--SR:!2026-10-09,1,230-->

Welche drei Technologien sind für clientseitige Webentwicklung wesentlich?::HTML für die Struktur, CSS für die Gestaltung und JavaScript für Verhalten und Interaktion.
<!--SR:!2026-10-12,4,270-->

Welche Gemeinsamkeit haben HTML, CSS und JavaScript?::Alle drei werden vom Browser verarbeitet und bilden gemeinsam die Grundlage moderner Webseiten.
<!--SR:!2026-10-09,1,230-->

## JavaScript

Zu welchem Zweck wurde JavaScript geschaffen?::Um statische Webseiten um dynamische Inhalte und Interaktionen zu erweitern.
<!--SR:!2026-10-09,1,230-->

Was ist JavaScript?::Eine Programmiersprache, die besonders im Browser für Logik, Interaktion und dynamische Webseiten eingesetzt wird.
<!--SR:!2026-10-09,1,230-->

In welcher Reihenfolge werden JavaScript-Anweisungen normalerweise ausgeführt?::Von oben nach unten in der Reihenfolge ihres Auftretens; Funktionscode erst, wenn die Funktion aufgerufen wird.
<!--SR:!2026-10-09,1,210-->

Was ist eine anonyme Funktion?::Eine Funktion ohne eigenen Namen, die direkt als Wert oder Argument verwendet wird.
<!--SR:!2026-10-09,1,230-->

Wo sind anonyme Funktionen besonders sinnvoll?::Zum Beispiel als Eventhandler, Callback oder kurzlebige Funktion, die nicht an anderer Stelle aufgerufen werden muss.
<!--SR:!2026-10-09,1,230-->

Was bewirkt `window.onload = function () { ... }`?::Die Funktion wird ausgeführt, nachdem die gesamte Seite einschließlich ihrer Ressourcen geladen wurde.
<!--SR:!2026-10-09,1,210-->

Wie erhält ein Eventhandler nach dem W3C-Ereignismodell Zugriff auf das Eventobjekt?::Das Eventobjekt wird der Handlerfunktion als erster Parameter übergeben, zum Beispiel `function (event) { ... }`.
<!--SR:!2026-10-09,1,230-->

## Document Object Model

Was ist das DOM?::Eine Objektschnittstelle, die ein HTML-Dokument als veränderbaren Baum aus Objekten und Knoten darstellt.
<!--SR:!2026-10-09,1,210-->

Wer standardisiert das DOM?::Heute vor allem die WHATWG als Living Standard; das W3C hat das DOM historisch standardisiert.
<!--SR:!2026-10-09,1,230-->

Was ist das hierarchisch höchste Browserobjekt und welche Unterobjekte besitzt es?::Das Objekt `window`; wichtige Unterobjekte beziehungsweise Eigenschaften sind `document`, `location` und `history`.
<!--SR:!2026-10-09,1,230-->

Wie stellt das DOM ein HTML-Dokument im Browser dar?::Als Baumstruktur, die der Browser beim Laden des Dokuments im Speicher aufbaut.
<!--SR:!2026-10-09,1,230-->

Nenne vier wichtige DOM-Knotentypen.::Dokumentknoten, Elementknoten, Attributknoten und Textknoten; zusätzlich gibt es Kommentarknoten.
<!--SR:!2026-10-09,1,230-->

Was bewirkt `document.getElementById()`?::Die Methode liefert das Element mit der angegebenen eindeutigen ID oder `null`, wenn kein solches Element existiert.
<!--SR:!2026-10-09,1,230-->

Was liefert `document.getElementsByTagName()`?::Eine live aktualisierte `HTMLCollection` aller Elemente mit dem angegebenen Tag-Namen, keine normale JavaScript-Array-Struktur.
<!--SR:!2026-10-09,1,230-->

Welche drei wichtigen Zugriffskategorien gibt es bei DOM-Elementen?::Zugriff auf Textinhalte, Werte von Formularfeldern und HTML-Attribute beziehungsweise Eigenschaften.
<!--SR:!2026-10-09,1,230-->

Wie liest oder verändert man den Inhalt eines Formulareingabefeldes?::Über die Eigenschaft `value`, zum Beispiel `eingabe.value`.
<!--SR:!2026-10-09,1,230-->

## Bootstrap

Was ist Bootstrap?::Ein Frontend-Framework mit CSS-Klassen, Layoutsystem, fertigen Komponenten und JavaScript-Funktionen für responsive Webseiten.
<!--SR:!2026-10-09,1,230-->

Welche Vorteile bietet Bootstrap?::Schnelle Entwicklung, responsive Standardlösungen, einheitliche Gestaltung, viele fertige Komponenten und gute Dokumentation.
<!--SR:!2026-10-09,1,230-->

Wie funktioniert das Bootstrap-Grid?::Inhalte werden in Container, Zeilen und bis zu zwölf Spalten gegliedert; responsive Klassen bestimmen die Breite je Bildschirmgröße.
<!--SR:!2026-10-09,1,230-->

Nenne typische Bootstrap-Komponenten.::Buttons, Cards, Navbar, Formulare, Alerts, Badges, Dropdowns, Modals, Accordion, Carousel und Pagination.
<!--SR:!2026-10-09,1,230-->

Was sind Sass und Less?::CSS-Präprozessoren mit Variablen, Funktionen, Mixins und Verschachtelung, die zu normalem CSS kompiliert werden.
<!--SR:!2026-10-09,1,230-->

Welchen CSS-Präprozessor verwendet aktuelles Bootstrap?::Sass; Less wurde hauptsächlich in älteren Bootstrap-Versionen verwendet.
<!--SR:!2026-10-09,1,230-->

Wie kann Bootstrap angepasst werden?::Durch eigene CSS-Regeln oder systematisch über Sass-Variablen, eigene Breakpoints und den gezielten Import benötigter Komponenten.
<!--SR:!2026-10-09,1,210-->

Welche Nachteile kann Bootstrap haben?::Webseiten können ähnlich aussehen, ungenutzte Bestandteile erhöhen die Dateigröße und das Projekt wird von Bootstrap-Klassen abhängig.
<!--SR:!2026-10-09,1,230-->

Wie bindet man Bootstrap grundsätzlich in HTML ein?::Bootstrap-CSS wird im `head` per `link` eingebunden; das JavaScript-Bundle wird meist vor `</body>` per `script` geladen.
<!--SR:!2026-10-09,1,210-->

Was bedeutet Responsive Design und wie unterstützt Bootstrap es?::Eine Oberfläche passt sich an verschiedene Bildschirmgrößen an; Bootstrap nutzt dafür Mobile First, Breakpoints, Grid-Klassen und flexible Komponenten.
<!--SR:!2026-10-09,1,230-->

Was ist der Unterschied zwischen dem Bootstrap-Grid und Flexbox?::Flexbox ist eine CSS-Layouttechnik; das Bootstrap-Grid ist ein fertiges responsives Layoutsystem, das intern unter anderem Flexbox verwendet.
<!--SR:!2026-10-09,1,230-->

Nenne Alternativen zu Bootstrap.::Zum Beispiel Tailwind CSS, Foundation, Bulma, Materialize, UIkit oder komponentenbasierte Bibliotheken wie Material UI.
<!--SR:!2026-10-09,1,230-->

## Datenbank Grundlagen und Normalisierung

Was ist ein relationales Datenbankmanagementsystem?::Software, die Daten in Tabellen organisiert und Beziehungen zwischen diesen Tabellen über Schlüssel verwaltet.
<!--SR:!2026-10-09,1,230-->

Was ist ein Primärschlüssel?::Ein Attribut oder eine Attributkombination, die jeden Datensatz einer Tabelle eindeutig identifiziert und weder doppelt noch `NULL` sein darf.
<!--SR:!2026-10-09,1,230-->

Was ist ein Fremdschlüssel?::Ein Attribut, das auf einen Primärschlüssel oder eindeutigen Schlüssel einer anderen oder derselben Tabelle verweist.
<!--SR:!2026-10-09,1,230-->

Was ist der Unterschied zwischen Primär- und Fremdschlüssel?::Der Primärschlüssel identifiziert den eigenen Datensatz; der Fremdschlüssel stellt eine Beziehung zu einem anderen Datensatz her.
<!--SR:!2026-10-09,1,230-->

Wozu dient ein SQL-JOIN?::Er kombiniert zusammengehörige Zeilen aus mehreren Tabellen anhand einer Verknüpfungsbedingung.
<!--SR:!2026-10-09,1,230-->

Was ist das Ziel der Normalisierung?::Redundanzen und Änderungsanomalien zu vermeiden und Daten logisch sowie konsistent auf Tabellen zu verteilen.
<!--SR:!2026-10-11,3,250-->

Welche Bedingung stellt die zweite Normalform sicher?::Die Tabelle ist in 1NF und jedes Nichtschlüsselattribut hängt vollständig vom gesamten Primärschlüssel ab, nicht nur von einem Teil eines zusammengesetzten Schlüssels.
<!--SR:!2026-10-09,1,230-->

Was ist eine transitive Abhängigkeit?::Ein Nichtschlüsselattribut hängt über ein anderes Nichtschlüsselattribut indirekt vom Primärschlüssel ab.
<!--SR:!2026-10-09,1,230-->

Wie wird eine transitive Abhängigkeit für die dritte Normalform beseitigt?::Die indirekt abhängigen Attribute werden in eine eigene Tabelle ausgelagert und über einen Schlüssel verknüpft.
<!--SR:!2026-10-09,1,230-->

Was bedeutet referenzielle Integrität?::Ein Fremdschlüssel muss auf einen existierenden Datensatz verweisen oder, sofern erlaubt, `NULL` sein.
<!--SR:!2026-10-09,1,210-->

Wofür steht ACID?::Atomicity, Consistency, Isolation und Durability, auf Deutsch Atomarität, Konsistenz, Isolation und Dauerhaftigkeit.
<!--SR:!2026-10-09,1,230-->

Warum ist ACID wichtig?::Es sorgt dafür, dass Transaktionen auch bei Fehlern oder parallelen Zugriffen zuverlässig und konsistent verarbeitet werden.
<!--SR:!2026-10-09,1,230-->

Was ist eine View?::Eine benannte, gespeicherte SQL-Abfrage, deren Ergebnis wie eine virtuelle Tabelle verwendet werden kann.
<!--SR:!2026-10-09,1,230-->

## Beziehungen und SQL

Wie wird eine 1:1-Beziehung technisch sichergestellt?::Durch einen Fremdschlüssel mit `UNIQUE`-Constraint oder durch einen gemeinsamen Primärschlüssel.
<!--SR:!2026-10-09,1,210-->

Warum ist Professor zu Kurs eine 1:n-Beziehung?::Ein Professor kann mehrere Kurse halten, während jeder Kurs genau einem Professor zugeordnet ist; der Fremdschlüssel liegt in `Kurse`.
<!--SR:!2026-10-09,1,230-->

Wie wird eine n:m-Beziehung relational umgesetzt?::Mit einer Zwischentabelle, die Fremdschlüssel auf beide beteiligten Tabellen enthält.
<!--SR:!2026-10-09,1,230-->

Warum braucht die Beziehung zwischen Studenten und Kursen eine Zwischentabelle?::Ein Student kann mehrere Kurse besuchen und ein Kurs mehrere Studenten enthalten; die Zwischentabelle löst diese n:m-Beziehung in zwei 1:n-Beziehungen auf.
<!--SR:!2026-10-11,3,250-->

Welchen Zweck erfüllt ein kombinierter Primärschlüssel aus `StudentID` und `KursID`?::Er identifiziert jede Belegung eindeutig und verhindert, dass derselbe Student denselben Kurs mehrfach belegt.
<!--SR:!2026-10-09,1,170-->

Mit welchem DDL-Befehl fügt man einer bestehenden Tabelle eine Spalte hinzu?::Mit `ALTER TABLE`, zum Beispiel `ALTER TABLE Professoren ADD COLUMN Email VARCHAR(100);`.
<!--SR:!2026-10-09,1,210-->

Welcher SQL-Befehl fragt Daten ab und welche Klausel filtert Zeilen?::`SELECT` fragt Daten ab; `WHERE` filtert die gewünschten Zeilen.
<!--SR:!2026-10-09,1,230-->

Wie verbindet man Kurse mit den Namen ihrer Professoren?::Zum Beispiel mit `SELECT P.Name, K.Titel FROM Professoren P INNER JOIN Kurse K ON P.ProfID = K.ProfID;`.
<!--SR:!2026-10-09,1,230-->

Was ist der Unterschied zwischen INNER JOIN und LEFT JOIN?::INNER JOIN liefert nur passende Zeilen beider Tabellen; LEFT JOIN liefert alle Zeilen der linken Tabelle und passende Werte der rechten, sonst `NULL`.
<!--SR:!2026-10-09,1,230-->

Wann verwendet man einen LEFT JOIN?::Wenn alle Datensätze der linken Tabelle erscheinen sollen, auch wenn kein zugehöriger Datensatz in der rechten Tabelle existiert.
<!--SR:!2026-10-09,1,230-->

Wie ermittelt man die Anzahl aller eingeschriebenen Studenten?::Mit der Aggregatfunktion `COUNT`, zum Beispiel `SELECT COUNT(*) FROM Studenten;`.
<!--SR:!2026-10-09,1,190-->

Wie zählt man Kurse pro Professor und zeigt nur Professoren mit mehr als einem Kurs?::Mit `GROUP BY ProfID` und `HAVING COUNT(*) > 1`.
<!--SR:!2026-10-09,1,230-->

Was ist der Unterschied zwischen WHERE und HAVING?::`WHERE` filtert Zeilen vor der Gruppierung; `HAVING` filtert Gruppen nach `GROUP BY`.
<!--SR:!2026-10-09,1,230-->

Wie löscht man Belegungen für den Kurs Datenbanken I über eine Unterabfrage?::`DELETE FROM Belegung WHERE KursID = (SELECT KursID FROM Kurse WHERE Titel = 'Datenbanken I');`.
<!--SR:!2026-10-09,1,210-->

Was bewirkt DELETE im Unterschied zu DROP DATABASE?::`DELETE` entfernt ausgewählte Zeilen aus einer Tabelle; `DROP DATABASE` löscht die gesamte Datenbank mit allen Tabellen und Daten.
<!--SR:!2026-10-09,1,210-->
