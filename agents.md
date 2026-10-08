# Agents.md

> Projektkontext und Arbeitsregeln für KI-Agenten.
> Diese Datei gilt für das gesamte Verzeichnis, sofern in einem Unterverzeichnis keine spezifischere `agents.md` liegt.

## 1. Zweck und Kontext

- **Programm:** `Obsidian`
- **Projekt/Vault:** `4aDAC`
- **Zweck:** `Organisation von Dateien für den Schulunterricht (Mitschriften, Arbeitsaufträge, ...)`
- **Zielgruppe:** `Ich`
- **Sprache:** Deutsch, sofern nicht anders angegeben.
- **Zeitzone/Datumsformat:** `Europe/Vienna`

## 2. Verzeichnisstruktur

| Bereich | Zweck                                                    | Hinweise                                                            |
| ------- | -------------------------------------------------------- | ------------------------------------------------------------------- |
| `4aDAC` | `Beinhaltet alle Fächer`                                 | `Dient als Root Ordner`                                             |
| `AMA`   | `Dieser Ordner für das Fach Angewandte Mathematik`       | `Wenn ich im chat "AMA" erwähne, ist immer dieser Ordner gemeint.`  |
| `AWL`   | `Dieser Ordner für das Fach Angewandte wirtschaftslehre` | `Wenn ich im chat "AWL" erwähne, ist immer dieser Ordner gemeint.`  |
| `DTSM`  | `Dieser Ordner für das Fach Daten und Systemmanagement`  | `Wenn ich im chat "DTSM" erwähne, ist immer dieser Ordner gemeint.` |
| `INF`   | `Dieser Ordner für das Fach Informatik`                  | `Wenn ich im chat "INF" erwähne, ist immer dieser Ordner gemeint.`  |
| `ITL`   | `Dieser Ordner für das Fach IT-Labor`                    | `Wenn ich im chat "ITL" erwähne, ist immer dieser Ordner gemeint.`  |
| `PP`    | `Dieser Ordner für Projektpraktikum`                     | `Wenn ich im chat "PP" erwähne, ist immer dieser Ordner gemeint.`   |
| `FH`    | `Dieser Ordner für die FH-Kurse`                         | `Wenn ich im chat "FH" erwähne, ist immer dieser Ordner gemeint.`   |

## 3. Arbeitsregeln

1. Vor Änderungen zuerst den relevanten Kontext und bestehende Inhalte lesen.
2. Bestehende Struktur, Benennungen und Formatierung beibehalten.
3. Keine Dateien löschen oder größere Umstrukturierungen durchführen, ohne dies ausdrücklich zu bestätigen.
4. Nur Dateien ändern, die für die Aufgabe erforderlich sind.
5. Bei Unsicherheit Annahmen klar kennzeichnen und die kleinste sinnvolle Änderung wählen.
6. Keine vertraulichen Inhalte, Zugangsdaten oder personenbezogenen Daten in neue Dateien übernehmen.

## 4. Schreib- und Formatkonventionen

- Markdown verwenden.
- Überschriften hierarchisch und sparsam einsetzen.
- Dateinamen: `kebab-case`
- Tags: `Am Anfang der Datei immer einen Tag mit dem Namen des Faches erstellen: Bei einer Datei in AWL, sollte also am Anfang ein #awl sein.`
- Aufgaben: `- [ ] Aufgabe` beziehungsweise `- [x] Erledigt`

## 5. Vorgehen bei Aufgaben

### Vor der Bearbeitung

- [ ] Aufgabe und gewünschtes Ergebnis verstehen
- [ ] Betroffene Dateien identifizieren
- [ ] Bestehende Inhalte und Abhängigkeiten prüfen
- [ ] Risiken oder offene Annahmen notieren

### Nach der Bearbeitung

- [ ] Änderungen auf Konsistenz und Formatierung prüfen
- [ ] Links und Verweise kontrollieren
- [ ] Keine unbeabsichtigten Änderungen hinterlassen
- [ ] Kurz zusammenfassen, was geändert wurde und wo

## 6. Rückfragen und Abschlussbericht

Wenn eine Entscheidung nicht aus dem Kontext hervorgeht, frage nach, sofern sie das Ergebnis wesentlich beeinflusst. Andernfalls triff eine vorsichtige Annahme und dokumentiere sie.

Am Ende kurz berichten:

- Was wurde geändert?
- Welche Dateien sind betroffen?
- Gibt es offene Punkte oder Empfehlungen?

## 7. "cleanup"

Wenn ich "cleanup" in den Chat schreibe, dann prüfe zuerst mit Git, welche Dateien und Ordner Änderungen aufweisen. Berücksichtige anschließend ausschließlich diese von Git als geändert, neu oder nicht versioniert ausgewiesenen Dateien und Ordner. Durchsuche nicht den gesamten Vault.

Prüfe bei den gefundenen Dateien, ob sie sich im richtigen Unterfolder befinden und ob ihre Dateinamen der Konvention `kebab-case` entsprechen. Falls du nicht weißt, wohin eine Datei gehört, frage nach. Die `agents.md` bleibt natürlich wo sie ist. Gelöschte Dateien werden nicht wiederhergestellt oder verschoben.

Nach dem Cleanup alle vorgenommenen Änderungen mit Git hinzufügen und committen. Verwende dafür eine aussagekräftige Commit-Nachricht, zum Beispiel `Cleanup: Dateien und Ordner bereinigt`. Anschließend mit `git status` prüfen, dass keine offenen Änderungen mehr vorhanden sind. Falls der Commit fehlschlägt, den Cleanup nicht als vollständig abgeschlossen melden und auf das Problem hinweisen.
