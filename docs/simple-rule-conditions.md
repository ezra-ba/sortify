# Einfache Dateinamenbedingungen

Arbeitsstand für #41, 28. Oktober 2026.

## Bedienung und Semantik

Neue Regeln verwenden standardmäßig Buttons für „Enthält“, „Beginnt mit“, „Endet mit“ und „Beliebiger Name“. Über „Weitere Bedingung“ können zusätzliche Bedingungen angelegt werden. Alle Bedingungen und eine erlaubte Dateiendung müssen gemeinsam passen. „Beliebiger Name“ ist ausschließlich allein und ohne Suchtext zulässig; damit sind reine Dateitypregeln möglich.

Geprüft wird der Dateiname ohne letzte Endung, unabhängig von Groß-/Kleinschreibung. „Enthält“ prüft eine Teilzeichenfolge, keine Wortgrenze: CSS passt auch auf SCSS. Leerzeichen, Bindestriche und Unterstriche werden wörtlich behandelt und nicht gegeneinander ausgetauscht. Suchtexte werden nicht getrimmt. Sonderzeichen wie `.`, `+`, `(` und `[` haben ausschließlich ihre wörtliche Bedeutung. Ein Suchtext benötigt 1 bis 256 Zeichen und darf nicht ausschließlich aus Leerzeichen bestehen. Pro Regel sind 1 bis 20 Bedingungen erlaubt; das erzeugte Muster bleibt auf 2048 Zeichen begrenzt.

Beispiel: Endung `.docx`, Name enthält „Fragenkatalog“ und Name enthält „CSS“. `01_3_Fragenkatalog_HTM_CSS_JS_Schueler.DOCX` passt; derselbe Name mit PNG-Endung oder eine DOCX-Datei ohne CSS passt nicht.

## Backend und Speicherung

`FileNameCondition` speichert Bedingungsart und ursprünglichen Text. `SortRule.NameConditions` ist bei einfachen Regeln gesetzt; null bezeichnet eine bestehende oder ausdrücklich erweiterte Regex-Regel. `SimpleRuleCompiler` maskiert gewöhnlichen Text und übersetzt die Bedingungen in ein Regex-Muster für die vorhandene Engine. Es gibt keine zweite Matching-Engine. Die Regelvalidierung weist fehlerhafte Definitionen sowie Abweichungen zwischen Definition und gespeichertem Muster zurück.

Formatversion 4 speichert die strukturierte Definition. Versionen 1 bis 3 bleiben lesbar; beim Laden wird nichts auf Datenträger umgeschrieben. Ein erfolgreiches Speichern schreibt Version 4. Ältere App-Versionen unterstützen dieses neue Format nicht. Bestehende Regex-Muster, IDs, Targets und sonstige Einstellungen bleiben erhalten. Die UI bietet für bestehende Regex-Nutzung einen ausdrücklich gekennzeichneten erweiterten Modus.

## Trefferbegründung

Ein Treffer enthält strukturierte `RuleMatchReason`-Einträge mit Regel-ID, Regelname, tatsächlich geprüfter Dateiendung und den tatsächlich ausgewerteten Namensbedingungen. Das Popup stellt diese Informationen verständlich dar. Eine reine Erkennung verändert weder Namen noch Dateiendung; eine Umbenennung erfolgt nur durch das vorhandene Schema und nach Bestätigung. Legacy-Regeln zeigen ihren Regelnamen und die tatsächlich geprüfte Endung; ihre Regex-Bedeutung wird nicht als einfache Textbedingung erraten.

## Verifikation

Release-Build: keine Fehler oder Warnungen. 373 Tests erfolgreich, darunter neue Prüfungen für kombinierte Bedingungen und Endungen, Sonderzeichen, Teilzeichenfolgen, Wortgrenzen, ungültige Definitionen, Musterabweichungen, Formatversionen 1 bis 4 und die gebundene Oberfläche mit anschließender Speicherung, erneutem Laden und Popup-Vorschlag. Manuelle Abnahme der neuen Buttons steht noch aus.
