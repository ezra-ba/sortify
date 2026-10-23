# Regelziele über Target-IDs

Stand: 23. Oktober 2026. Arbeitsstand für Issue #46.

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

Der Aufruf ist noch nicht an eine tatsächliche Regel-/Target-Auswahlmaske angebunden. Diese Integration muss mit der Regelverwaltung aus #27 abgestimmt werden. Ein vorhandener Backend-Aufruf und automatisierte Tests sind kein Nachweis einer bereits bedienbaren Auswahlmaske.

## Teamentscheidung und Folgearbeiten

Vereinbartes Zielverhalten: Für eine neue Datei wird der Target-Ordner mit der höchsten ausreichend eindeutigen Regelübereinstimmung vorgeschlagen. Bei unsicherer Zuordnung wird ein Standardordner vorgeschlagen. Verschoben wird weiterhin erst nach ausdrücklicher Bestätigung.

Gewichtung, Eindeutigkeit und Verhalten ohne Standardziel sind noch festzulegen. Die Berechnung einer Übereinstimmung und der Standardziel-Fallback sind separate Folgearbeiten und nicht als implementiert zu verstehen. Ein technischer Auswertungsfehler darf nicht unbemerkt in einen erfolgreichen Fallback umgedeutet werden.

Die integrierte Dateierkennung öffnet derzeit ein Popup über `DownloadMonitorController`, ruft aber noch nicht diese Engine auf. Die Verbindung von Dateibeschreibung, gespeicherten Regeln, Engine-Ergebnis und tatsächlichem Target-Pfad bleibt als Integration offen. Zielerreichbarkeit, Pfadgrenzen und bestätigte Dateioperationen gehören in Infrastructure und die entsprechenden Folge-Issues.

## Verifikation und offene Abnahme

Core-Tests prüfen Zielreferenzen, entfernte Targets ohne Umleitung, Prioritäten, Mehrdeutigkeiten und das Zusammenfassen anhand der Target-ID. Infrastructure-Tests prüfen vollständige Speicherung, Altformate, Warnungen, explizite Reparatur und Engine-Auswertung nach erneutem Laden. App-Tests prüfen Zuordnung, ungültige Auswahl ohne Schreiben, Speicherfehler und aktuelle Warnungen nach Target-Entfernung.

Offen für die bedienbare Gesamtabnahme: Regel-/Target-Auswahlmaske, Verbindung der laufenden Dateierkennung mit der Target-ID-basierten Engine sowie der Ablauf mit echten Benutzerentscheidungen. Vor einem Abschluss von #46 ist insbesondere zu klären, ob die Auswahlmaske als Abhängigkeit von #27 oder als Bestandteil dieses Issues nachgewiesen werden soll.
