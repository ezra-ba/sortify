# Eigene Sortierregeln anlegen

Stand: 28. Oktober 2026. Regelanlage (#27), einfache Bedingungen (#41), Bearbeiten/Entfernen (#65) und Regel-Tags mit Aktivstatus (#70) sind in master integriert. Die getrennten Ansichten aus #74 liegen auf einem neuen Feature-Branch und sind noch nicht integriert.

## Bedienung

„Einstellungen → Sortierregeln“ zeigt zunächst die vorhandenen Regeln beziehungsweise „Neue Regel erstellen“, wenn noch keine existiert. Der Plus-Button rechts an der Überschrift öffnet die Neuanlage, „Bearbeiten“ nur das Formular der gewählten Regel. Nach Speichern oder Abbrechen geht es zurück zur Übersicht. Siehe [rule-overview.md](rule-overview.md). Alternativ im Hauptfenster einen Target-Ordner auswählen und „Regel für Target-Ordner anlegen“ öffnen. Dieser Einstieg belegt das gewählte Ziel vor, speichert aber noch keine Regel.

Für eine neue Regel sind Name, Target-Ordner, Dateinamenmuster und mindestens eine erlaubte Dateiendung erforderlich. Die Umbenennungsvorgabe ist optional: Leer bedeutet ausdrücklich, den ursprünglichen Dateinamen beizubehalten. Eine eigene Vorgabe verwendet die vorhandenen Platzhalter `{name}` und `{ext}`; `{ext}` muss genau einmal am Ende stehen.

Beispiel:

| Feld | Wert |
| --- | --- |
| Regelname | Rechnungen |
| Target-Ordner | Ein zuvor hinzugefügter Ordner für Rechnungen |
| Namensbedingung | Beginnt mit: `rechnung` |
| Erlaubte Dateiendungen | `.pdf` |
| Dateinamen-Vorgabe | `sortiert_{name}{ext}` |

`rechnung_2026.pdf` ergibt den Vorschlag `sortiert_rechnung_2026.pdf` im ausgewählten Ziel. Die Regel wird bei „Regel anlegen“ gespeichert. Ein Popup-Vorschlag führt weiterhin erst nach „Datei einsortieren“ eine Dateioperation aus.

Die Namensbedingung kann jetzt über einfache Buttons erfasst werden; Details stehen in [simple-rule-conditions.md](simple-rule-conditions.md). Im ausdrücklich erweiterten Modus bleibt ein regulärer Ausdruck für den Dateinamen ohne letzte Endung möglich. Groß-/Kleinschreibung wird ignoriert. `^rechnung` bedeutet „beginnt mit rechnung“, `.*` passt auf beliebige Namen. Dateiendungen können mit Leerzeichen, Komma oder Semikolon getrennt werden; ein führender Punkt wird bei Bedarf ergänzt. Es gelten weiterhin die unterstützten Endungen aus FileCategoryMap.

Neue Regeln sind aktiv und haben Priorität 0. Bei mehreren gleichpriorisierten unterschiedlichen Kandidaten bleibt die Auswertung mehrdeutig; das Popup schlägt dann ein konfiguriertes Standardziel vor. Ohne Standardziel bleibt die Zielauswahl leer. Fehlerhafte Regeln werden nicht übernommen. Scheitert das Speichern, bleiben Entwurf und bisheriger gespeicherter Stand erhalten.

## Bestehende Regeln und Zielreferenzen

Unter „Ziel einer vorhandenen Regel zuordnen“ lassen sich eine gespeicherte Regel und ein existierender Target-Ordner auswählen. „Zielzuordnung speichern“ ändert nur die Target-ID. Damit können insbesondere Altregeln ohne Zielreferenz repariert werden. Andere Regelfelder werden nicht bearbeitet oder entfernt.

## Schnittstellen und Abgrenzung

`MainViewModel.AddRuleAsync(SortRule)` fügt eine validierte Regel mit neuer eindeutiger ID und expliziter Target-ID hinzu. Vorhandene Regeln und andere Einstellungen bleiben erhalten. Regel- und Target-Validierung erfolgen gegen den aktuellen Stand innerhalb von `ISettingsService.UpdateAsync`. Speicherfehler verändern den aktuellen Zustand nicht. Dateisystemzugriffe und JSON-Speicherung verbleiben in Infrastructure.

`RuleEditorViewModel` übernimmt Formularzustand und Rückmeldungen. `RuleEditorView` enthält die gebundene Darstellung; SettingsWindow und MainWindow öffnen beziehungsweise navigieren zur Maske.

Die vereinfachten Bedingungen „enthält“, „beginnt mit“ und „endet mit“ samt eigenem Speicherformat sind im Arbeitsstand von #41 implementiert. Die Umsetzung von #41 ergänzt nun die Buttons und eine gemeinsame Übersetzung für die vorhandene Engine; der aktuelle Stand ist separat dokumentiert. Der Arbeitsstand von #65 ergänzt Bearbeiten und ausdrücklich bestätigtes Entfernen. Priorität und relative Pfadbedingung werden beim Bearbeiten erhalten; der Aktivstatus wird über „Regel aktiv“ gesteuert. Die Target-Reparatur aus #46 ist ausdrücklich verfügbar.

Die Target-ID-Grundlagen sowie #27, #41 und #65 sind bereits integriert. Der aktuelle #74-Branch baut auf dem aktuellen master auf. Es wurden keine GitHub-Zuständigkeiten automatisch geändert und keine Teammitglieder benachrichtigt.

## Verifikation

Release-Build und Tests prüfen die Eingaben, Validierungsfehler ohne Schreibzugriff, erhaltene Entwürfe bei Speicherfehlern, das Ergänzen vorhandener Regeln, die gebundene Oberfläche und die Persistenz nach erneutem Laden. Ein Headless-UI-Test prüft außerdem die Reparatur einer Altregel und den anschließenden Popup-Vorschlag mit einem Test-Verschiebedienst. Am 27. Oktober 2026 hat der Benutzer den beschriebenen manuellen Ablauf unter Linux als erfolgreich bestätigt: Regelanlage, Erhalt nach vollständigem Neustart, regelbasierter Ziel- und Dateinamenvorschlag sowie bestätigtes Einsortieren. Windows und macOS wurden dafür noch nicht manuell geprüft. Dieser damalige Prüfstand hatte einen warnungsfreien Release-Build und 352 bestandene Tests. Für den aktuellen #74-Arbeitsstand bestehen 404 Tests; Details siehe [rule-overview.md](rule-overview.md).

## Bearbeiten und Entfernen (#65)

Bei einer gespeicherten Regel öffnet „Bearbeiten“ das Formular mit der vorhandenen Definition. Die Ansicht springt unmittelbar zur Maske und setzt den Fokus in das Namensfeld; das gilt auch beim Wechsel zwischen zwei Regeln während einer Bearbeitung. Einfache Regeln stellen sämtliche Bedingungsbuttons und Originalsuchtexte wieder her. Bestehende Regex-Regeln öffnen im erweiterten Modus. „Änderungen speichern“ erhält ID und nicht bearbeitete Felder. „Bearbeiten abbrechen“ verändert den gespeicherten Stand nicht.

„Entfernen“ zeigt zunächst den Regelnamen und eine ausdrückliche Bestätigung. Erst „Entfernen bestätigen“ speichert die Löschung; „Abbrechen“ verändert nichts. Dateien und Target-Ordner werden dabei nicht gelöscht. Ein Speicherfehler erhält die Regel, den Entwurf beziehungsweise die offene Löschbestätigung.

`UpdateRuleAsync(original, replacement)` und `RemoveRuleAsync(original)` vergleichen innerhalb der Einstellungsdienstsperre den aktuellen mit dem ursprünglich geöffneten Regelstand. Eine inzwischen geänderte oder entfernte Regel wird nicht überschrieben. Die Meldung fordert zum erneuten Laden auf; der Entwurf bleibt zunächst erhalten.

Bereits geöffnete Datei-Popups behalten den angezeigten Vorschlag. Neue Auswertungen verwenden den aktuellen Regelbestand. Eine Regeländerung führt selbst keine Dateioperation aus.

Verifikation des kombinierten Stands: Release-Build ohne Fehler und Warnungen; 380 Tests erfolgreich. Prüfungen umfassen einfache und Regex-Regeln, erhaltene versteckte Felder, Speicherfehler, Abbruch und Bestätigung der Löschung, Persistenz und veraltete Bearbeitungsstände. Die manuelle Prüfung dieser neuen Funktionen steht noch aus. Die geplante neue Listenansicht und Target-Tags bleiben separate Folgearbeiten.

## Einstieg über Target-Tags (#70)

Der Arbeitsstand für #70 zeigt die zugeordneten Regeln als klickbare Tags rechts in den Target-Karten. Ein Klick öffnet exakt die gewählte Regel zur Bearbeitung. „Keine Regel“ öffnet die Neuanlage mit vorbelegtem Target; gleiche aktive Erkennungsbedingungen und Prioritäten in verschiedenen Targets erhalten eine Warnkennzeichnung. Siehe [target-rule-tags.md](target-rule-tags.md) für Definition, Bedienung und Light-/Darkmode-Aufnahmen.

## Regeln aktivieren und deaktivieren

Der aktuelle #70-Arbeitsstand ergänzt beim Bearbeiten die erste Checkbox „Regel aktiv“. Ohne Haken sind die Parameterfelder gesperrt; die Statusänderung wird mit „Änderungen speichern“ übernommen. Neue Regeln sind automatisch aktiv. Die gespeicherte Liste kennzeichnet deaktivierte Regeln mit „Inaktiv“, Target-Tags erscheinen grau. Details, Speicherverhalten und Theme-Aufnahmen stehen in [target-rule-tags.md](target-rule-tags.md#aktivstatus-bedienen).
