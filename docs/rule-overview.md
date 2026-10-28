# Regelübersicht und getrennte Formularansichten

Arbeitsstand für #74, 28. Oktober 2026. Noch nicht in master integriert.

## Bedienung

„Einstellungen → Sortierregeln“ öffnet zunächst die Übersicht. Sind keine Regeln vorhanden, erscheint ein Leerzustand mit „Neue Regel erstellen“. Andernfalls werden die gespeicherten Regeln mit Ziel, Bedingungen, Inaktiv-Markierung und den bisherigen Aktionen angezeigt. Die explizite Zielreparatur bleibt unterhalb der Liste verfügbar.

Das Plus rechts neben „Sortierregeln“ öffnet eine leere Neuanlage. Die feste Überschrift und der Plus-Button bleiben auch bei langen Listen erreichbar. Der Plus-Button erscheint nur im Bereich Sortierregeln und nur in der Übersicht. Sein Symbol ist eine Vektorgrafik und benötigt keine spezielle Symbolschrift.

„Neue Regel erstellen“ im Leerzustand öffnet dieselbe Neuanlage. „Bearbeiten“ öffnet ausschließlich die gewählte Regel im Formular; Liste, Leerzustand und Zielreparatur sind währenddessen ausgeblendet. Die Aktiv-Checkbox steht beim Bearbeiten weiterhin an erster Stelle, inaktive Parameter bleiben gesperrt und neue Regeln sind automatisch aktiv.

„Zur Übersicht“ beziehungsweise „Abbrechen“ verwirft den Formularentwurf ohne Speichern. Nach erfolgreichem Anlegen oder Bearbeiten geht die Ansicht ebenfalls zur aktualisierten Übersicht zurück. Validierungs- und Speicherfehler erhalten das Formular und sämtliche Eingaben. Nach Entfernen der letzten Regel erscheint wieder der Leerzustand.

Die Einstiege im Hauptfenster bleiben erhalten: Ein Regeltag öffnet direkt die zugehörige Bearbeitung; der Plus-Button an einem Target ohne Regel und „Regel für Target-Ordner anlegen“ öffnen eine Neuanlage mit vorbelegtem Target. Diese Einstiege werden nicht durch die Standardübersicht umgeleitet.

Beim Formularwechsel wird an den Anfang gescrollt und das erste passende Eingabeelement fokussiert. Nach Rückkehr zur Übersicht steht die Liste wieder oben. Die Auswahl der Seite „Sortierregeln“ in der Navigation öffnet erneut die Übersicht und verwirft einen zuvor dort offenen Entwurf.

## Technische Grenzen

`RuleEditorViewModel` hält den flüchtigen Ansichtsstatus über `IsFormVisible`, `IsOverview`, `ShowEmptyState` und `ShowRuleList`. `BeginCreateCommand`, `ShowOverviewCommand` und die bestehenden Bearbeitungsaktionen steuern den Wechsel. Der Zustand wird nicht gespeichert. Regeldefinitionen, Speicherformat, Regelauswertung und Dateioperationen bleiben unverändert.

Die feste Überschrift und das Plus liegen in SettingsWindow oberhalb des scrollbaren Inhalts. RuleEditorView enthält Leerzustand, Formular und Liste mit gegenseitig ausschließender Sichtbarkeit. Fokus und Scrollposition bleiben reine Darstellungsaufgaben der View.

## Ansichten

Automatisierte Headless-Avalonia-Aufnahmen mit synthetischen Testdaten:

| Ansicht | Lightmode | Darkmode |
| --- | --- | --- |
| Ohne Regeln | [Aufnahme](screenshots/rule-overview-empty-light.png) | [Aufnahme](screenshots/rule-overview-empty-dark.png) |
| Gespeicherte Regeln | [Aufnahme](screenshots/rule-overview-list-light.png) | [Aufnahme](screenshots/rule-overview-list-dark.png) |
| Bearbeitung | [Aufnahme](screenshots/rule-overview-edit-light.png) | [Aufnahme](screenshots/rule-overview-edit-dark.png) |

## Verifikation

Release-Build ohne Fehler und Warnungen; 404 Tests erfolgreich (239 Core, 98 Infrastructure, 67 App). Neue Tests prüfen Leerzustand und Liste, reale Mausklicks auf Plus und Erstellen/Bearbeiten, isolierte Formulare, Rückkehr nach Erfolg/Abbruch, erhaltene Entwürfe bei Fehlern, Löschung der letzten Regel und beide Farbmodi. Bestehende Tests prüfen weiterhin Target-Tag-Einstiege, Aktivstatus, Zielreparatur, Speicherung nach erneutem Laden und den regelbasierten Vorschlag.

Die manuelle Abnahme der neuen Ansichten in der laufenden App steht noch aus. Windows und macOS wurden für diese Änderung nicht praktisch geprüft.

## Ergänzung: Target ohne Regel

Auf ausdrücklichen Benutzerwunsch wird diese Anpassung innerhalb von #74 umgesetzt: Im Hauptfenster ersetzt ein violetter Plus-Button die rote Kennzeichnung „Keine Regel“. Der Button ist 36 × 36 groß und hat stärker abgerundete Ecken (Radius 12), während vorhandene Regeln ihre ovalen Tags behalten. Er verwendet die dynamischen violetten Tagfarben des jeweiligen Themes. Tooltip und zugängliche Beschriftung erklären „Neue Regel erstellen“; der Klick öffnet weiterhin direkt die Neuanlage mit dem richtigen Target. Die Aufnahme und Tests stehen bei [target-rule-tags.md](target-rule-tags.md). Nach der Ergänzung sind alle 67 App-Tests erfolgreich; Core und Infrastructure bleiben unverändert.

## Optische Feinabstimmung

Normale Regel-Tags reservieren keinen Abstand für das ausgeblendete Warnsymbol; ihre Namen sind dadurch horizontal zentriert. Der Abstand vor einem sichtbaren Warnsymbol bleibt erhalten. Target-Plus-Buttons verwenden ein 14 × 14 großes Symbol und einen Eckenradius von 12; der Plus-Button an der Seitenüberschrift verwendet ein 16 × 16 großes Symbol und ebenfalls Radius 12. Die Klickflächen bleiben unverändert. Die horizontale Zentrierung wird in beiden Themes geprüft; alle 67 App-Tests sind erfolgreich.

## Testbereinigung

Zwei reine Record-Zuweisungstests wurden entfernt. Die Zielreferenz und die Regelfelder werden weiterhin durch Regelverarbeitung und Speicherung geprüft. Der Übersichtstest prüft bei Fehlern nur den sichtbaren Formularzustand; detaillierte Prüfungen zur Erhaltung der Eingaben und gespeicherten Regeln bleiben in den Regelerstellungstests. Dadurch sinkt die Gesamtzahl von 404 auf 402 Testfälle.
