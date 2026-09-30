# Technisches Konzept: Regelbasierte Sortiervorschläge

Stand: 30. September 2026. Status: Konzept für die spätere Implementierung.

## 1. Ziel und Umfang

Sortify soll neue Dateien in einem ausdrücklich ausgewählten Eingangsordner erkennen und nachvollziehbare Vorschläge zum Umbenennen und Archivieren erzeugen. Die Verarbeitung erfolgt vollständig lokal, ohne KI- oder Cloud-Dienste. Dateien und Zielordner werden erst nach ausdrücklicher Bestätigung verändert.

Dieses Dokument beschreibt die fachlichen Entscheidungen und die geplanten Schnittstellen. Es implementiert weder Ordnerüberwachung noch Regelprüfung, Einrichtung oder Dateioperationen. Die Aufteilung folgt [architecture.md](architecture.md).

Für die erste Version werden Dateiendung, Dateiname und optional der relative Quellpfad ausgewertet. Dokumentinhalte und unscharfe Ähnlichkeitssuche werden zunächst nicht implementiert.

## 2. Dateierkennung und Ablauf

1. Der Benutzer wählt Eingangsordner und Archiv-Stammordner. Beide dürfen sich für die erste Version weder entsprechen noch ineinander liegen, damit archivierte Dateien nicht erneut verarbeitet werden.
2. Die Ordnerüberwachung meldet neue Dateien. Temporäre Downloads werden zurückgestellt. Wiederholte Ereignisse dürfen keine mehrfachen Vorschläge für dieselbe unveränderte Datei erzeugen.
3. Infrastructure prüft die Verfügbarkeit: Dateigröße und Änderungszeit müssen über ein konfiguriertes Zeitfenster stabil sein; außerdem wird die Lesbarkeit geprüft. Diese Prüfungen sind Indizien und garantieren nicht, dass ein anderer Prozess später nichts mehr verändert.
4. Eine Freigabeliste entscheidet anhand der letzten Dateiendung, ob die Datei berücksichtigt wird.
5. Core bestimmt die Kategorie und wertet alle aktiven, für den Dateityp zulässigen Regeln aus.
6. Prioritäten und Mehrdeutigkeiten werden aufgelöst. Infrastructure ergänzt den Zustand von Zielordner und Zieldatei.
7. Die Oberfläche zeigt Quelle, Ziel, angewandte Regel, mögliche Umbenennung und gegebenenfalls die notwendige Ordneranlage an.
8. Nach Bestätigung werden Quelle und Ziel erneut geprüft. Erst dann werden erforderliche Ordner angelegt und die Datei ohne Überschreiben verschoben.

Es gibt keinen Zwischenumzug in einen Kategorieordner: Zuerst wird der endgültige Vorschlag berechnet, danach wird einmal verschoben. Bei Ablehnung bleibt die Datei unverändert; dasselbe unveränderte Ereignis löst nicht sofort ein weiteres Popup aus.

## 3. Ausgewertete Eigenschaften

| Eigenschaft | Prüfung | Bedeutung |
|---|---|---|
| Dateiendung | `Path.GetExtension`, Vergleich ohne Groß-/Kleinschreibung | Freigabe und Kategorie; hierfür ist keine Regex erforderlich |
| Dateiname ohne letzte Endung | Regex | Fachliche Zuordnung, beispielsweise Rechnungen oder Mathematik |
| Relativer Quellpfad inklusive Dateiname | Optionale Regex | Einschränkung auf Unterordner des beobachteten Ordners |
| Dateigröße und Änderungszeit | Technische Prüfung | Verfügbarkeit und Erkennung veralteter Vorschläge, keine fachlichen Regex-Kriterien |

Relative Pfade werden für die Regelprüfung einheitlich mit `/` als Trenner dargestellt, zum Beispiel `Schule/mathe_aufgaben.pdf`. Absolute Benutzerpfade sind keine Regelvoraussetzung. Unterordner werden nur ausgewertet, wenn die Überwachung ausdrücklich rekursiv konfiguriert wurde.

### 3.1 Freigabeliste und Kategorien

Die folgende Zuordnung ist der vorgeschlagene Umfang. Eine Endung bedeutet ausschließlich „zur Sortierung zugelassen“, nicht „Inhalt ist ungefährlich“. Sortify führt Dateien nicht aus.

| Kategorie | Zugelassene Endungen |
|---|---|
| Dokumente | `.pdf`, `.docx`, `.odt`, `.rtf`, `.txt` |
| Tabellen | `.xlsx`, `.ods`, `.csv`, `.tsv` |
| Präsentationen | `.pptx`, `.odp` |
| Bilder | `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`, `.tif`, `.tiff` |
| Audio | `.mp3`, `.wav`, `.flac`, `.aac`, `.ogg`, `.m4a` |
| Videos | `.mp4`, `.mkv`, `.mov`, `.avi`, `.webm` |
| E-Books | `.epub` |

Alle anderen Endungen, insbesondere `.exe`, `.zip`, `.winmd`, `.sh`, `.docm`, `.xlsm` und `.pptm`, sowie Dateien ohne Endung werden ignoriert. `rechnung.pdf.exe` wird aufgrund der letzten Endung `.exe` nicht zugelassen. Großgeschriebene Endungen wie `.PDF` werden erkannt.

Bei der bestätigten Ersteinrichtung können diese Kategorieordner im gewählten Archiv angelegt werden. Vorhandene Ordner werden weiterverwendet. Der Archivpfad und der Einrichtungsstatus werden lokal gespeichert. Ist das Archiv später nicht erreichbar, pausiert die Verarbeitung bis zur Klärung.

### 3.2 Dateinamenprüfung gegenüber Inhaltsprüfung

Bei `rechnung_2026.pdf` prüft die Dateinamenregel nur `rechnung_2026`. Sie kann nicht feststellen, ob das Dokument tatsächlich eine Rechnung enthält. Ein neutraler Dateiname wie `download_123.pdf` bleibt ohne passende Namensregel beim allgemeinen Kategorie-Vorschlag.

Eine Inhaltsregel würde zuerst Text aus dem Dokument extrahieren und danach etwa nach `Rechnungsnummer` suchen. Regex verarbeitet den extrahierten Text, nicht unmittelbar die binären PDF- oder Office-Dateien.

| Format | Aufwand | Grenzen und Risiken |
|---|---|---|
| TXT, CSV | Gering bis mittel | Zeichencodierung, sehr große Dateien, strukturierte Felder |
| DOCX, XLSX, PPTX | Mittel | Formatspezifische Extraktion; Absätze, Tabellen, Folien und geteilte Zeichenketten müssen berücksichtigt werden |
| PDF mit Textschicht | Mittel bis hoch | Zusätzlicher Parser; Reihenfolge, Silbentrennung und Schriftkodierung können Ergebnisse beeinträchtigen |
| Gescannte PDFs und Bilder | Hoch | Zusätzliche lokale OCR; Erkennungsfehler, Sprachmodelle und höherer Ressourcenbedarf |
| Verschlüsselte oder beschädigte Dokumente | Nicht zuverlässig auswertbar | Extraktion kann scheitern; keine automatische Umgehung des Schutzes |

Eine spätere lokale Inhaltsprüfung benötigt gepflegte Parser, Größen-, Textmengen- und Zeitlimits sowie Abbruchmöglichkeiten. Office-Anwendungen, Makros und eingebettete Programme dürfen dafür nicht gestartet werden. Extrahierter Dokumenttext wird nicht protokolliert oder dauerhaft gespeichert. Ein Extraktionsfehler muss als „nicht auswertbar“ vom normalen Nichttreffer unterscheidbar sein.

**Empfehlung:** Inhaltsprüfung als separate Erweiterung vorsehen, zunächst ausschließlich Metadaten prüfen. Damit bleibt der erste Ablauf schnell, verständlich und ohne zusätzliche Dokumentparser testbar. Auch eine spätere Inhaltsprüfung ist ohne Cloud und KI möglich; OCR ist für das MVP nicht erforderlich.

## 4. Vorgeschlagenes Regelmodell

Das vorhandene `SortRule` enthält bereits `Id`, `Name`, `FileNamePattern`, `TargetDirectory`, `RenamePattern`, `Priority` und `IsEnabled`. Es wird konzeptionell um Endungsfilter und einen optionalen Pfadfilter ergänzt.

| Feld | Typ | Vorgabe |
|---|---|---|
| `Id` | `Guid` | Eindeutig und dauerhaft stabil |
| `Name` | `string` | Nicht leer; verständliche Bezeichnung für die Vorschau |
| `FileNamePattern` | `string` | Pflicht-Regex für den Dateinamen ohne letzte Endung; `.*` erlaubt eine bewusst allgemeine Regel |
| `AllowedExtensions` | Liste von Strings | Nicht leer; normalisierte Endungen mit Punkt; nur Teilmenge der globalen Freigabeliste |
| `RelativePathPattern` | `string?` | Optional; Regex für den normalisierten relativen Quellpfad inklusive Dateiname |
| `TargetDirectory` | `string` | Relativer Unterordner innerhalb der ermittelten Kategorie; leer bedeutet Kategorieordner |
| `RenamePattern` | `string?` | Optionales Schema für den vollständigen neuen Dateinamen; leer bedeutet Originalname |
| `Priority` | `int` | Bereich 0–1000; höhere Zahl gewinnt; Standard 100 |
| `IsEnabled` | `bool` | Nur aktive Regeln werden ausgewertet |

Alle angegebenen Bedingungen werden mit UND verknüpft. Eine Regel kann die globale Freigabeliste nicht umgehen. Alle Regex-Prüfungen verwenden zunächst einheitlich `IgnoreCase` und `CultureInvariant`; freie Regex-Optionen sind kein MVP-Bestandteil.

`TargetDirectory` ist bewusst relativ zur Kategorie. Beispielsweise ergibt `.pdf` mit `TargetDirectory = "Rechnungen"` den Pfad `<Archiv>/Dokumente/Rechnungen`. Kategorieübergreifende Ziele sind nicht Teil der ersten Version. Eine Regex wird nicht direkt als Ordnername verwendet.

### 4.1 Umbenennung

Die erste Version unterstützt ausschließlich die Platzhalter `{name}` (Originalname ohne letzte Endung) und `{ext}` (Originalendung inklusive Punkt). Beispiel: `Schule_{name}{ext}` erzeugt aus `mathe_aufgaben.pdf` den Namen `Schule_mathe_aufgaben.pdf`.

Bei einem gesetzten Schema ist `{ext}` genau einmal am Ende erforderlich; die Endung bleibt erhalten. Unbekannte Platzhalter, Pfadtrenner, leere Ergebnisse und ungültige beziehungsweise reservierte Dateinamen werden abgewiesen. Regex-Ersetzungszeichen wie `$1` werden nicht als Platzhalter interpretiert. Datumsvariablen und benannte Regex-Gruppen können später mit eigener Spezifikation ergänzt werden.

### 4.2 Vollständiges Regelbeispiel

```json
{
  "Id": "97334013-2177-4dc5-baf4-8d9f9139b60d",
  "Name": "PDF-Rechnungen",
  "FileNamePattern": "^rechnungen?(?:[-_ ]|$)",
  "AllowedExtensions": [".pdf"],
  "RelativePathPattern": null,
  "TargetDirectory": "Rechnungen",
  "RenamePattern": null,
  "Priority": 200,
  "IsEnabled": true
}
```

Die Darstellung ist ein Modellbeispiel, noch kein festgelegtes Speicherformat. Backslashes in Regex müssen in JSON zusätzlich maskiert werden.

## 5. Beispielregeln

Alle Muster beziehen sich auf den Dateinamen ohne Endung. Alle Regeln sind aktiv, behalten standardmäßig den Originalnamen und haben keinen Pfadfilter, sofern nichts anderes angegeben ist. Ziele in der Tabelle sind relativ zum Archiv; im Regelmodell wird nur der Anteil unterhalb der Kategorie gespeichert.

| Regel / Priorität | Endungen | Dateinamen-Regex | Passendes Beispiel → Ziel |
|---|---|---|---|
| Rechnungen / 200 | `.pdf` | `^rechnungen?(?:[-_ ]|$)` | `rechnung_2026.pdf` → `Dokumente/Rechnungen/` |
| Mathematik / 150 | `.pdf`, `.docx` | `^(?:mathe|mathematik)(?:[-_ ]|$)` | `mathe_aufgaben.docx` → `Dokumente/Schule/Mathematik/` |
| Stundenpläne / 180 | `.pdf`, `.docx` | `^stundenplan(?:[-_ ]|$)` | `stundenplan_2026.pdf` → `Dokumente/Schule/Stundenpläne/` |
| Schulpräsentationen / 150 | `.pptx` | `^(?:referat|präsentation)(?:[-_ ]|$)` | `referat_biologie.pptx` → `Präsentationen/Schule/` |
| Notenlisten / 180 | `.xlsx`, `.csv` | `^(?:noten|notenliste)(?:[-_ ]|$)` | `notenliste_2026.xlsx` → `Tabellen/Schule/Noten/` |
| Screenshots / 120 | `.png`, `.jpg`, `.jpeg` | `^(?:screenshot|bildschirmfoto)(?:[-_ ]|$)` | `Screenshot_2026-09-30.png` → `Bilder/Screenshots/` |
| Quartalsübersichten / 160 | `.xlsx` | `^Q[1-4][-_ ]` | `Q3_Präsentation.xlsx` → `Tabellen/Quartalsübersichten/` |

Gegenbeispiele: `rechnung_2026.exe` ist nicht zugelassen; `mathematiker.pdf` passt nicht zur Mathematik-Regel; `ferienplan.pdf` passt nicht zu Stundenplänen; `referat_biologie.pdf` passt wegen der Endung nicht zur Präsentationsregel; `notendienst.xlsx` passt nicht zu Notenlisten; `screenshotter.png` passt nicht zu Screenshots; `Q5_Übersicht.xlsx` passt nicht zur Quartalsregel.

Beispiel für einen optionalen Pfadfilter: `^Schule/` beschränkt die Mathematik-Regel auf Dateien unter `Schule/` im beobachteten Ordner. `Schule/mathe_aufgaben.pdf` passt, `Privat/mathe_aufgaben.pdf` nicht. Ohne diesen Filter kann die Regel in jedem beobachteten Unterordner greifen.

## 6. Regex-Validierung und Fehlerbehandlung

1. Beim Anlegen oder Bearbeiten werden Pflichtfelder, Endungsfilter, Priorität, Ziel und Umbenennungsschema geprüft.
2. Jedes Muster wird durch Erzeugen einer .NET-Regex syntaktisch validiert. Ungültige Muster verhindern Speichern beziehungsweise Aktivieren; die Oberfläche zeigt das betroffene Feld und eine verständliche Ursache.
3. Geladene Regeln werden ebenfalls validiert. Fehlerhafte Regeln werden für die Sitzung ausgeschlossen und sichtbar gemeldet, ohne die gültigen Regeln zu blockieren.
4. Jeder Regex-Abgleich erhält ein explizites Zeitlimit. Startwert für Metadaten: 100 ms pro Abgleich, später anhand von Messungen anzupassen. Eine Obergrenze für Musterlänge (Vorschlag: 2048 Zeichen) und ein abbrechbarer Hintergrundablauf begrenzen zusätzliche Belastung.
5. Ein `RegexMatchTimeoutException` wird als Auswertungsfehler erfasst. Die betroffene Datei erhält keinen automatisch bevorzugten Vorschlag, da eine möglicherweise höher priorisierte Regel nicht vollständig geprüft wurde. Kein automatischer Wiederholungsversuch mit unbegrenzter Laufzeit.
6. Ein Testbereich kann vor Aktivierung passende und nicht passende Beispieldateinamen anzeigen. Syntaktische Gültigkeit allein beweist weder fachliche Richtigkeit noch unproblematische Laufzeit.

Feste Infrastrukturfehler werden von Nichttreffern getrennt. Die Oberfläche darf einen Fehler nicht als „keine passende Regel“ ausgeben.

## 7. Prioritäten, Zielordner und Konflikte

| Situation | Vorgeschlagenes Verhalten |
|---|---|
| Genau eine passende Regel | Ihren Zielpfad vorschlagen |
| Mehrere Treffer mit unterschiedlicher Priorität | Höchste Priorität bevorzugen; angewandte Regel anzeigen |
| Mehrere höchstpriorisierte Treffer mit gleichem endgültigem Ziel | Zu einem Vorschlag zusammenfassen; auslösende Regeln nachvollziehbar halten |
| Mehrere höchstpriorisierte Treffer mit unterschiedlichen Zielen oder Dateinamen | Mehrdeutigkeit anzeigen und Benutzer wählen lassen; keine versteckte Entscheidung über Listenreihenfolge |
| Keine Regel passt, Endung freigegeben | Allgemeinen Kategorieordner vorschlagen und als Kategorie-Fallback kennzeichnen |
| Endung nicht freigegeben | Datei ignorieren; kein Verschiebevorschlag |
| Passender Zielordner existiert | Vorhandenen Ordner verwenden |
| Zielordner fehlt | Erstellung dieses konkreten Ordners zusammen mit dem Verschieben bestätigen lassen |
| Ähnlich benannter Ordner vorhanden | Keine automatische Gleichsetzung; `Rechnungen alt` ist nicht `Rechnungen` |
| Zieldatei existiert | Alternativen Namen mit Suffix vorschlagen, etwa `rechnung_2026 (1).pdf`; niemals überschreiben |
| Ziel ist mit Quelle identisch | Keine Dateioperation anbieten |
| Datei wurde nach Vorschlag verändert oder entfernt | Vorschlag als veraltet markieren und erneut prüfen beziehungsweise entfernen |
| Fehlende Rechte, nicht erreichbares Archiv oder gesperrte Datei | Verständliche Fehlermeldung; keine Erfolgsmeldung und keine absichtliche Löschung der Quelle |

Ordnernamen werden nach den Regeln des tatsächlichen Dateisystems verglichen. Eine plattformübergreifend pauschale Kleinschreibung von Zielpfaden ist ungeeignet.

Infrastructure löst den vollständigen Zielpfad auf und prüft, dass er innerhalb der Kategorie und des Archivs liegt. Absolute Ziele, `..`-Segmente und Ausbrüche über symbolische Links oder Windows-Junctions sind auszuschließen. In der ersten Version werden solche Verknüpfungen in Quell- und Zielpfaden nicht verfolgt.

Zielkonflikte werden unmittelbar vor der Ausführung erneut geprüft. Entsteht inzwischen eine Kollision, wird der Vorschlag aktualisiert und erneut bestätigt. Die Dateioperation selbst muss Überschreiben ebenfalls ausschließen. Bei laufwerksübergreifenden Vorgängen wird die Quelle erst nach erfolgreich abgeschlossenem und geprüftem Kopieren entfernt; ein Teilfehler muss offen gemeldet werden.

## 8. Klassen und Schnittstellen

### 8.1 Sortify.Core

Core enthält keine Avalonia-Abhängigkeit und führt keine Dateisystemzugriffe aus.

| Typ | Verantwortung |
|---|---|
| `SortRule` (bestehend, erweitern) | Regelmodell aus Abschnitt 4 |
| `FileDescriptor` (neu) | Quelle, Dateiname ohne Endung, Endung, normalisierter relativer Pfad sowie erfasste Größe und Änderungszeit |
| `FileCategoryMap` (neu) | Freigabeliste und Zuordnung von Endungen zu Kategorien |
| `IRuleValidator` / `RuleValidator` (neu) | Regelkonfiguration prüfen und feldbezogene Fehler liefern |
| `IRuleEngine` / `RuleEngine` (Schnittstelle anpassen, Implementierung neu) | Aktive Regeln auswerten, Prioritäten behandeln und relative Zielkandidaten berechnen |
| `RuleEvaluationResult` (neu) | Status `Ignored`, `Matched`, `Ambiguous`, `CategoryFallback` oder `Failed`; Kandidaten und Diagnosen |
| `SortSuggestion` (bestehend, erweitern) | Quelle, endgültiges Ziel, auslösende Regel-IDs und Namen, Begründung, erforderliche Ordneranlage und Konfliktstatus |

Die bisherige Rückgabe `SortSuggestion?` von `IRuleEngine.CreateSuggestion` reicht nicht aus, um Mehrdeutigkeit, ignorierte Dateien und Fehler auseinanderzuhalten. Vorgeschlagen wird stattdessen eine Auswertung von `FileDescriptor` und Regeln mit Rückgabe eines `RuleEvaluationResult`. Die Ergebnisse enthalten zunächst relative Zielkandidaten; eine nachgelagerte Koordination ergänzt den tatsächlichen Dateisystemzustand zur Vorschau.

Eine optionale Schnittstelle `IDocumentTextExtractor` kann später in Core definiert werden. Ihre formatspezifischen Implementierungen gehören in Infrastructure; Parserbibliotheken gehören nicht in Core. Für das MVP wird diese Schnittstelle noch nicht benötigt.

### 8.2 Sortify.Infrastructure und Sortify.App

| Bereich | Vorgeschlagene Verantwortung |
|---|---|
| Infrastructure: `FolderMonitor` | Dateiereignisse, Zusammenfassung mehrfacher Ereignisse, Verfügbarkeit und Erzeugen von `FileDescriptor` |
| Infrastructure: `ArchiveInitializer` | Bestätigte Einrichtung der Kategorieordner |
| Infrastructure: `DestinationInspector` | Existenz, Pfadgrenzen und Namenskonflikte prüfen; Vorschau ergänzen |
| Infrastructure: `SettingsStore` / `RuleStore` | Einstellungen und Regeln lokal speichern und laden |
| Infrastructure: `FileOperationService` | Bestätigte Dateioperation mit erneuter Zustandsprüfung ausführen |
| App: `MainViewModel` beziehungsweise spätere spezialisierte ViewModels | Ablauf koordinieren, Status und Vorschläge anzeigen, Benutzerentscheidungen entgegennehmen |
| App: `App.axaml.cs` | Komponenten beim Start verbinden |

Die Erkennung einer Datei und die Anzeige eines Popups sind damit von der fachlichen Regex-Auswertung getrennt. Core-Tests können mit künstlichen Dateibeschreibungen ohne echte Ordner ausgeführt werden.

## 9. Empfehlung für die Implementierung und Prüfung

1. Regelmodell, Kategoriezuordnung und Ergebniszustände festlegen; `RuleValidator` und `RuleEngine` mit Metadaten implementieren.
2. Die Beispiele einschließlich Gegenbeispielen als automatisierte Tests verwenden. Zusätzlich Groß-/Kleinschreibung, doppelte Endungen, deaktivierte Regeln, Pfadfilter, Prioritätsgleichstand, Kategorie-Fallback, ungültige Muster und Timeout-Verhalten prüfen.
3. Lokale Speicherung, Archiv-Einrichtung und Ordnerüberwachung ergänzen. Wiederholte Ereignisse sowie noch laufende Downloads gezielt prüfen.
4. Vorschau und Bestätigung anbinden; sichere Dateioperationen mit temporären Testordnern prüfen. Wesentliche Fälle: vorhandene Zieldatei, zwischenzeitliche Kollision, verschwundene Quelle, ungültiges Ziel, fehlende Rechte und Teilfehler.
5. Erst nach einem zuverlässigen vollständigen Ablauf entscheiden, ob Inhaltsprüfung, dynamische Regex-Gruppen im Ziel oder unscharfe Ordnersuche tatsächlich benötigt werden.

Erster fachlicher Meilenstein: `rechnung_2026.pdf` wird im Eingangsordner erkannt und als Vorschlag für `Dokumente/Rechnungen` angezeigt. Ein fehlender Unterordner wird angekündigt. Bestätigung archiviert die Datei ohne Überschreiben; Ablehnung lässt sie unverändert. Eine gleichzeitig eingehende `.exe` bleibt unbeachtet.

## 10. Abdeckung der Abnahmekriterien

| Abnahmekriterium | Behandlung im Konzept |
|---|---|
| Unterschied zwischen Dateiname und Dokumentinhalt | Abschnitt 3.2, einschließlich Aufwand, Grenzen und Empfehlung |
| Vollständiges und verständliches Regelmodell | Abschnitt 4 mit vollständigem Beispiel und Abschnitt 5 mit sieben Regeln |
| Priorität, Konflikte und Fehlerfälle | Abschnitte 6 und 7 |
| Lokal, ohne KI oder Cloud | Abschnitte 1 und 3.2 |
| Vereinbar mit Core/Infrastructure | Abschnitt 8 |

## 11. Technische Referenzen

- [Microsoft: Best Practices for Regular Expressions in .NET](https://learn.microsoft.com/dotnet/standard/base-types/best-practices) – Zeitlimits und Umgang mit problematischen Eingaben.
- [Microsoft: About the Open XML SDK](https://learn.microsoft.com/en-us/office/open-xml/about-the-open-xml-sdk) – lokale Verarbeitung von Office-Dokumentstrukturen.
- [Microsoft: Working with the shared string table](https://learn.microsoft.com/en-us/office/open-xml/spreadsheet/working-with-the-shared-string-table) – Beispiel für die zusätzliche Extraktionslogik bei XLSX.
