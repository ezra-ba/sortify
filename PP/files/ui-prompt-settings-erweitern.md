#pp

# Allgemeine Settings erweitern
Wir haben bereits implementiert, dass sich die App beim "schließen" nicht komplett schließt sondern quasi nur minimiert und im Hintergrund weiterläuft. Dafür haben wir ja eine Erweiterung in der Taskleiste wodurch man das auch steuern kann.
Diese Logik soll nun auch in der Einstellungen eingebaut werden. 
## Allgemeine Einstellungen
Im Tab "Allgemein" ergänzen wir folgende Settings;
- Ein Toggle Button für "Sortify automatisch starten, wenn dein Computer startet"
- Ein Toggle Button für "Sortify minimiert starten"
	- mit einem kleinen Infotext darunter: "Sortify im Hintergrund starten, damit es dich nicht gleich erschlägt"
	- dieser Button soll nur klickbar sein wenn der automatisch starten Button auf true ist 
- Ein Toggle Button für "Sortify in den Infobereich minimieren"
	- mit einem kleinen Infotext darunter: "Durch Klicken auf X wird Sortify minimiert, statt es vollständig zu schließen"
Alle drei Button sollen so wie der Darkmode Button formatiert sein: 
- Eigene Card rund um Text/Button
- Die Animation vom Darkmode Button soll übernommen werden
Außedem sollen alle drei Settings in der settings.json abgespeichert werden.
Passe alle UI Elemente so an, dass sie sowohl im Darkmode als auch im Lightmode die richtigen Farben haben.
