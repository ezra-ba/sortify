#pp

# Einstellungen Window überarbeiten
## Sortierregeln Tab
- Wenn ich bei einer Sortirregeln auf "Entfernen" klicke, erscheint aktuell ganz unten bei den Regeln eine Anfrage für die Bestätigung für die Löschung
	- Diese Frage soll mit einem Popup mitten im Bildschirm erscheinen und klar und deutlich fragen: "Sortierregel löschen?" "Abbrechen" "Bestätigen" (wobei Abbrechen in unseren Akzentfarben sein soll)
- Beim Bearbeiten von Sortierregeln ist unten Übereinenadergereiht "Änderung speichern" und "Abbrechen"
	- Der Button "Änderung speichern" soll nach ganz rechts auf gleicher höher wie der Abbrechen Button sein
- Beim Bearbeiten von sortierregeln ist am anfang eine Checkbox mit "Regeln aktiv"
	- diese Checkbox soll mit einem Switch Toggle Button in unserem Style ersetzt werden - funktionalität soll gleich bleiben
- Beim Bearbeiten von Sortierregeln erfolgt die Auswahl der erlaubten Dateiendungen derzeit über eine Textbox
	- dieses Prinzip ist sehr Userunfreundlich, weswegen wir dort auf ein Dropdown wechseln:
		- alle verfügbaren Dateiendungen sollen in einem Dropdown gelistet werden
		- rechts neben dem Dropdown soll ein "+" Button sein - drückt man diesen, erscheint unter dem Dropdown ein weiteres Dropdown - auf diese weise kann man mehrere Dateiendungen wählen. 
		- Ein Limit der Dateiendungen gibt es in dem sinne nicht - sobald eine dateiendung gewählt wurde ist sie in den anderen dropdowns (falls welche hinzugefügt) natürlich nicht mehr verfügbar. Das limit ist also mehr oder weniger die anzahl der dateiendungen
		- der "+" button soll immer neben dem letzten dropdown stehen
		- Die Logik des sortierens etc. soll gleich bleiben, es soll leidglich die ui / die auswahl der dateiendungen angepasst werden
