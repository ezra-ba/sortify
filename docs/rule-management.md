# Eigene Sortierregeln anlegen

Stand: 27. Oktober 2026. Arbeitsstand für #27, abhängig von #46. Noch nicht in master integriert.

## Bedienung

Unter „Einstellungen → Sortierregeln“ können neue Regeln angelegt und gespeicherte Regeln angezeigt werden. Alternativ im Hauptfenster einen Target-Ordner auswählen und „Regel für Target-Ordner anlegen“ öffnen. Dieser Einstieg belegt das gewählte Ziel vor, speichert aber noch keine Regel.

Für eine neue Regel sind Name, Target-Ordner, Dateinamenmuster und mindestens eine erlaubte Dateiendung erforderlich. Die Umbenennungsvorgabe ist optional: Leer bedeutet ausdrücklich, den ursprünglichen Dateinamen beizubehalten. Eine eigene Vorgabe verwendet die vorhandenen Platzhalter `{name}` und `{ext}`; `{ext}` muss genau einmal am Ende stehen.

Beispiel:

| Feld | Wert |
| --- | --- |
| Regelname | Rechnungen |
| Target-Ordner | Ein zuvor hinzugefügter Ordner für Rechnungen |
| Dateinamenmuster | `^rechnung` |
| Erlaubte Dateiendungen | `.pdf` |
| Dateinamen-Vorgabe | `sortiert_{name}{ext}` |

`rechnung_2026.pdf` ergibt den Vorschlag `sortiert_rechnung_2026.pdf` im ausgewählten Ziel. Die Regel wird bei „Regel anlegen“ gespeichert. Ein Popup-Vorschlag führt weiterhin erst nach „Datei einsortieren“ eine Dateioperation aus.

Die Namensbedingung ist in dieser ersten Maske ein regulärer Ausdruck für den Dateinamen ohne letzte Endung. Groß-/Kleinschreibung wird ignoriert. `^rechnung` bedeutet „beginnt mit rechnung“, `.*` passt auf beliebige Namen. Dateiendungen können mit Leerzeichen, Komma oder Semikolon getrennt werden; ein führender Punkt wird bei Bedarf ergänzt. Es gelten weiterhin die unterstützten Endungen aus FileCategoryMap.

Neue Regeln sind aktiv und haben Priorität 0. Bei mehreren gleichpriorisierten unterschiedlichen Kandidaten bleibt die Auswertung mehrdeutig; das Popup schlägt dann ein konfiguriertes Standardziel vor. Ohne Standardziel bleibt die Zielauswahl leer. Fehlerhafte Regeln werden nicht übernommen. Scheitert das Speichern, bleiben Entwurf und bisheriger gespeicherter Stand erhalten.

## Bestehende Regeln und Zielreferenzen

Unter „Ziel einer vorhandenen Regel zuordnen“ lassen sich eine gespeicherte Regel und ein existierender Target-Ordner auswählen. „Zielzuordnung speichern“ ändert nur die Target-ID. Damit können insbesondere Altregeln ohne Zielreferenz repariert werden. Andere Regelfelder werden nicht bearbeitet oder entfernt.

## Schnittstellen und Abgrenzung

`MainViewModel.AddRuleAsync(SortRule)` fügt eine validierte Regel mit neuer eindeutiger ID und expliziter Target-ID hinzu. Vorhandene Regeln und andere Einstellungen bleiben erhalten. Regel- und Target-Validierung erfolgen gegen den aktuellen Stand innerhalb von `ISettingsService.UpdateAsync`. Speicherfehler verändern den aktuellen Zustand nicht. Dateisystemzugriffe und JSON-Speicherung verbleiben in Infrastructure.

`RuleEditorViewModel` übernimmt Formularzustand und Rückmeldungen. `RuleEditorView` enthält die gebundene Darstellung; SettingsWindow und MainWindow öffnen beziehungsweise navigieren zur Maske.

Die vereinfachten Bedingungen „enthält“, „beginnt mit“ und „endet mit“ samt eigenem Speicherformat sind weiterhin Gegenstand von #41. Diese Maske implementiert keine konkurrierende Übersetzung dafür. Bearbeiten sämtlicher Regelfelder und Entfernen von Regeln bleiben optionale Folgearbeiten. Die Target-Reparatur aus #46 ist ausdrücklich verfügbar.

Der #27-Branch baut auf dem #46-Branch auf. Vor einer Integration nach master müssen die Target-ID-Grundlagen integriert sein. Es wurden keine GitHub-Zuständigkeiten automatisch geändert und keine Teammitglieder benachrichtigt.

## Verifikation

Release-Build und Tests prüfen die Eingaben, Validierungsfehler ohne Schreibzugriff, erhaltene Entwürfe bei Speicherfehlern, das Ergänzen vorhandener Regeln, die gebundene Oberfläche und die Persistenz nach erneutem Laden. Ein Headless-UI-Test prüft außerdem die Reparatur einer Altregel und den anschließenden Popup-Vorschlag mit einem Test-Verschiebedienst. Am 27. Oktober 2026 hat der Benutzer den beschriebenen manuellen Ablauf unter Linux als erfolgreich bestätigt: Regelanlage, Erhalt nach vollständigem Neustart, regelbasierter Ziel- und Dateinamenvorschlag sowie bestätigtes Einsortieren. Windows und macOS wurden dafür noch nicht manuell geprüft. Der letzte Release-Build war ohne Fehler und Warnungen; alle 352 Tests bestanden.
