Implementiere eine neue UI-Ansicht „Download-Statistik“ für die Sortify-Avalonia-Anwendung.

Wichtig:
- Vorerst ausschließlich die UI umsetzen.
- Noch keine echten Daten, Log-Auswertung oder Persistenz implementieren.
- Für alle Werte ausschließlich statische Mock-Daten im ViewModel verwenden.
- Die Ansicht soll bereits so strukturiert sein, dass später echte aggregierte Statistikdaten angeschlossen werden können.
- Bestehende Sortify-Designrichtlinien, Farben, Abstände, Schriftgrößen und MVVM-Struktur verwenden.
- Keine Logik für Dateioperationen oder Downloads ergänzen.

Layout:
1. Kopfbereich

Oben links:

- Überschrift: „Download-Statistik“
- Darunter eine kurze Beschreibung, zum Beispiel:
  „Eine Übersicht über deine Downloads und deren Quellen.“

Oben rechts:

- Eine zentrale Zeitraum-Auswahl für sämtliche Diagramme.
- Auswahlmöglichkeiten:
  - Tag
  - Woche
  - Monat
  - Jahr
  - Gesamt
- Standardmäßig „Monat“ auswählen.
- Die Auswahl soll zunächst nur visuell funktionieren beziehungsweise den ausgewählten Wert im ViewModel speichern.
- Die Diagramme dürfen vorerst statische Mock-Daten anzeigen.
- Die Auswahl soll später problemlos als Filterparameter an die Statistikabfrage übergeben werden können.

2. KPI-Karten

Direkt unterhalb des Kopfbereichs vier Karten in einer horizontalen Reihe:

Karte 1:

- Titel: „Downloads gesamt“
- Wert: „1.284“
- Download-Icon

Karte 2:

- Titel: „Diese Woche“
- Wert: „126“
- Kalender- oder Zeit-Icon

Karte 3:

- Titel: „Erfolgreich sortiert“
- Wert: „98,4 %“
- Erfolgs- oder Haken-Icon

Karte 4:

- Titel: „Häufigste Quelle“
- Wert: „github.com“
- Darunter optional: „412 Downloads“
- Link- oder Quellen-Icon

Die Karten sollen clean und ruhig gestaltet sein:

- Keine Prozentveränderungen
- Keine Vergleichstexte wie „+12 %“ oder „im Vergleich zur letzten Woche“
- Keine zusätzlichen Trendindikatoren
- Heller Kartenhintergrund
- Dezente Schatten oder Rahmen
- Einheitliche Höhe und Breite
- Gute Lesbarkeit
- Responsive Anordnung bei kleineren Fenstergrößen

3. Diagramm „Downloads im Zeitverlauf“

Unterhalb der KPI-Karten ein großes Liniendiagramm:

- Titel: „Downloads im Zeitverlauf“
- X-Achse mit Zeitwerten
- Y-Achse mit Download-Anzahl
- Statische Mock-Daten verwenden
- Ruhige blaue Sortify-Farbgebung
- Dezente Rasterlinien
- Tooltip oder Anzeige eines Beispielwertes vorsehen, falls die verwendete Chart-Bibliothek dies unterstützt
- Das Diagramm soll den verfügbaren Platz im Hauptbereich gut ausnutzen

Das Diagramm wird später durch die globale Zeitraum-Auswahl aktualisiert. Für die UI-Vorbereitung reicht zunächst eine statische Darstellung.

4. Diagramm „Nach Quelle“

Rechts neben oder unterhalb des Zeitverlaufs ein Kreis- beziehungsweise Donutdiagramm:

- Titel: „Nach Quelle“
- Statische Beispielverteilung verwenden
- Beispielquellen:
  - github.com
  - microsoft.com
  - npmjs.com
  - andere
- Legende mit Quelle, Anzahl und prozentualem Anteil anzeigen
- Gesamtzahl in der Mitte des Donutdiagramms anzeigen
- Die Farbpalette an die Sortify-Farben anpassen
- Das Diagramm soll auch bei längeren Quellenbezeichnungen sauber bleiben

5. Bereich „Top-Quellen“

Unterhalb der Hauptdiagramme einen Bereich „Top-Quellen“ darstellen:

- Horizontale Rangliste der häufigsten Download-Quellen
- Statische Mock-Daten verwenden
- Jede Quelle soll enthalten:
  - App- oder Website-Icon
  - Domainname
  - horizontalen Balken
  - Download-Anzahl
- Beispielquellen:
  - github.com
  - microsoft.com
  - npmjs.com
  - google.com
  - stackoverflow.com
- Die Variante mit App- beziehungsweise Website-Icons verwenden.
- Icons sollen einheitlich groß sein und nicht zu dominant wirken.
- Falls keine echten Markenicons vorhanden sind, zunächst neutrale Platzhalter-Icons verwenden.
- Keine externen Icon- oder Webzugriffe zur Laufzeit einbauen.

Technische Anforderungen:

- Avalonia UI mit bestehender MVVM-Struktur verwenden.
- Views enthalten nur Präsentationslogik.
- Mock-Daten im passenden ViewModel oder in separaten UI-Demo-Modellen kapseln.
- Keine echten Logdateien lesen.
- Keine Datenbank oder Persistenz ergänzen.
- Die Datenmodelle so benennen und strukturieren, dass sie später durch echte Statistikmodelle ersetzt werden können.
- Zeitraum-Auswahl als bindbare Property im ViewModel umsetzen.
- Diagramme über bindbare Collections beziehungsweise geeignete Chart-Series anbinden.
- Bestehende Architektur und Namenskonventionen des Projekts einhalten.
- Keine Änderungen an Core- oder Infrastructure-Logik vornehmen, sofern sie für das reine UI-Mockup nicht notwendig sind.
- Die Ansicht soll im aktuellen Sortify-Design konsistent wirken und sich sauber in die bestehende Navigation einfügen.

Akzeptanzkriterien:

- Eine neue Statistikansicht ist in der Anwendung sichtbar.
- Die vier KPI-Karten werden angezeigt.
- Das Liniendiagramm „Downloads im Zeitverlauf“ wird angezeigt.
- Das Donutdiagramm „Nach Quelle“ wird angezeigt.
- Die Liste „Top-Quellen“ enthält Icons, Domains, Balken und Mock-Werte.
- Die Kalender-Heatmap „Aktivität nach Tag“ ist nicht vorhanden.
- Die Zeitraum-Auswahl enthält Tag, Woche, Monat, Jahr und Gesamt.
- Es werden ausschließlich statische Mock-Daten verwendet.
- Es gibt keine Vergleichsprozentwerte oder Trendtexte auf den KPI-Karten.
- Die View folgt der bestehenden MVVM- und Avalonia-Struktur.