# Regelziele über Target-IDs

Stand: 27. Oktober 2026. Arbeitsstand für Issue #46.

## Implementiertes Verhalten

`SortRule.TargetFolderId` referenziert einen gespeicherten `TargetFolder` über dessen stabile ID. `IRuleEngine.Evaluate(file, rules, targets)` erhält die verfügbaren Targets ausdrücklich. Core greift nicht auf das Dateisystem zu. Existenz bedeutet hier einen Eintrag in dieser Liste, nicht die Erreichbarkeit des tatsächlichen Ordners.

Die Engine überspringt deaktivierte Regeln. Nach der bestehenden Regelvalidierung prüft `IRuleTargetValidator` die Zielreferenz. Fehlende IDs, Guid.Empty und unbekannte IDs erzeugen `InvalidTargetReference` mit Regel-ID und Feldname. Die betroffene Regel wird ausgeschlossen; andere gültige Regeln können weiterhin treffen. Gleiche Pfade oder Spitznamen ersetzen niemals eine entfernte ID.

Passende Regeln werden weiterhin nach expliziter Priorität ausgewählt. Das ist keine berechnete Konfidenz. Kandidaten enthalten `TargetFolderId`, Dateiname und auslösende Regeln. Gleiche Target-ID und gleicher Dateiname werden zusammengefasst, unterschiedliche höchstpriorisierte Kandidaten ergeben `Ambiguous`. `Category` bleibt Dateitypinformation; `TargetDirectory` ist im erzeugten Kandidaten leer. Die Kategorie wird nicht an den frei gewählten Target-Pfad angehängt.

Ohne passenden gültigen Treffer liefert die Engine `NoMatch` ohne Kandidaten. Nicht zugelassene Endungen liefern `Ignored`. Regex-Timeouts und ungültige erzeugte Namen ergeben weiterhin `Failed`. Diagnosen dürfen nicht als normaler fachlicher Nichttreffer verborgen werden. Der frühere `CategoryFallback` wird nicht mehr verwendet.

## Altregeln und Speicherung

Formatversion 3 speichert die neue Referenz. Versionen 1, 2 und 3 können geladen werden. Eine fehlende Referenz bleibt `null`; die Datei wird beim Laden nicht verändert. Erst ein erfolgreiches Speichern schreibt Version 3. Ältere App-Versionen können das neue Format ablehnen.

`ArchiveRootPath`, das alte `SortRule.TargetDirectory` und sämtliche weiteren Einstellungen bleiben erhalten. Aus diesen Altwerten, Spitznamen, Pfaden oder einem Standardziel wird keine Target-ID erraten. Bei einer gesetzten ID wird das alte Zielfeld nicht mehr fachlich zur Zielpfadprüfung verwendet. Ohne ID bleibt die bisherige Prüfung des alten Felds bestehen. Die strukturelle Dateivalidierung verlangt weiterhin das vorhandene Regelformat einschließlich TargetDirectory; neue Regeln übergeben dafür einen leeren String.

Fehlende beziehungsweise ungültige Zielreferenzen machen eine ansonsten gültige Konfiguration nicht unladbar. `SettingsLoadResult.Warnings` enthält strukturierte `RuleTargetWarning`-Einträge mit Regel-ID, Name und Meldung. Andere strukturelle oder fachliche Fehler können die Konfiguration weiterhin ablehnen.

## Explizite Zuordnung und sichtbare Hinweise

`MainViewModel.AssignRuleTargetAsync(ruleId, targetId)` ist der Backend-Aufruf für die Regelverwaltung. Er prüft Regel und Target, speichert ausschließlich die Referenz und erhält die übrigen Felder. Ungültige Auswahlen werden vor einem Schreibzugriff abgelehnt; ein Speicherfehler erhält den bisherigen Stand.

`RuleTargetWarnings` wird getrennt von `SettingsStatus` angezeigt und beim Laden sowie nach UI-Speicheraktionen aktualisiert. Nach Entfernung eines referenzierten Targets bleibt die Regel mit ihrer alten ID erhalten; die Warnung erscheint unmittelbar. Nach erfolgreicher Reparatur verschwindet sie. Auch deaktivierte Regeln werden auf Überarbeitungsbedarf geprüft.

Die Regelmaske aus #27 bindet den Aufruf unter „Einstellungen → Sortierregeln → Ziel einer vorhandenen Regel zuordnen“ an. Neue Regeln erhalten ihre explizite Target-ID über `MainViewModel.AddRuleAsync`. Die Implementierung liegt auf dem separaten #27-Branch, der auf #46 aufbaut; daraus folgt noch keine Integration in master.

## Teamentscheidung und Folgearbeiten

Vereinbartes Zielverhalten: Für eine neue Datei wird der Target-Ordner mit der höchsten ausreichend eindeutigen Regelübereinstimmung vorgeschlagen. Bei unsicherer Zuordnung wird ein Standardordner vorgeschlagen. Verschoben wird weiterhin erst nach ausdrücklicher Bestätigung.

Eine berechnete Gewichtung der Übereinstimmung bleibt offen. Die Popup-Integration nutzt derzeit die explizite Regelpriorität: Ein eindeutiger Kandidat belegt Ziel und Dateinamen vor; bei NoMatch oder Ambiguous wird das konfigurierte Standardziel vorgeschlagen. Ohne Standardziel bleibt die Zielauswahl leer. Ein technischer Auswertungsfehler darf nicht unbemerkt in einen erfolgreichen Fallback umgedeutet werden.

Das Popup wertet nach erfolgreicher Bereitschaftsprüfung die aktuellen gespeicherten Regeln über die Engine aus. Der Kandidat wird ausschließlich über seine Target-ID einer vorhandenen Target-Karte zugeordnet. Die Begründung wird angezeigt. Bei Ignored, Failed oder diagnostizierten Problemen ohne verwendbaren Treffer wird kein Standardziel vorbelegt. Derzeit werden nur Vorschläge mit unveränderter Dateiendung übernommen. Nach der ersten Auswertung überschreiben Wiederholungsprüfungen keine manuellen Eingaben. Keine Auswertung führt selbst eine Dateioperation aus. Zielerreichbarkeit, Pfadgrenzen und bestätigte Dateioperationen gehören in Infrastructure und die entsprechenden Folge-Issues.

## Verifikation und offene Abnahme

Core-Tests prüfen Zielreferenzen, entfernte Targets ohne Umleitung, Prioritäten, Mehrdeutigkeiten und das Zusammenfassen anhand der Target-ID. Infrastructure-Tests prüfen vollständige Speicherung, Altformate, Warnungen, explizite Reparatur und Engine-Auswertung nach erneutem Laden. App-Tests prüfen Zuordnung, ungültige Auswahl ohne Schreiben, Speicherfehler und aktuelle Warnungen nach Target-Entfernung.

Die Regel-/Target-Auswahlmaske ist auf dem abhängigen #27-Branch implementiert. Ein Headless-UI-Test prüft gebundene Eingaben, Validierungsfehler, Anlegen, Reparatur einer Altregel sowie Speichern und erneutes Laden. Anschließend prüft er den regelbasierten Popup-Vorschlag mit einem Testdienst, der erst nach expliziter Bestätigung einen Verschiebeauftrag erhält. Am 27. Oktober 2026 bestätigte der Benutzer den manuellen Regelablauf unter Linux einschließlich Neustart, passendem Popup-Vorschlag und bestätigter Dateioperation als erfolgreich. Dieser Nachweis gilt für den kombinierten Stand aus #46 und #27; Windows und macOS bleiben ungeprüft. #27 benötigt vor dem Merge die Target-ID-Grundlagen aus #46.
