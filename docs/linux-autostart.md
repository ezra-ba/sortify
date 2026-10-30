# Linux-Autostart (#86)

## Ursache und Abgrenzung

Im untersuchten Stand verwendeten beide App-Verbraucher ausschließlich `WindowsLoginStartup`. Eine gespeicherte AutoStart-Option konnte unter Linux deshalb keine Registrierung erzeugen. Beim untersuchten Benutzer waren AutoStart und StartMinimized zusätzlich ausgeschaltet. Benutzerdateien werden durch den Entwicklungsauftrag nicht automatisch geändert.

`LoginStartupFactory` liefert unter Windows den bestehenden Registry-Dienst und unter Linux `LinuxLoginStartup`. Beide App-Verbraucher erhalten dieselbe Instanz aus der technischen Komposition. macOS/andere Systeme melden weiterhin ehrlich nicht unterstützt; deren Autostart ist ein eigener Folgeauftrag. Bei einem nicht unterstützten injizierten Autostartdienst wird die Aktivierung nicht mehr stillschweigend als erfolgreich gespeichert.

Die Darstellung der Einstellungen bleibt unverändert. Das Tray-Problem ist separat: `App.axaml.cs` schließt Linux im aktuellen Stand noch vom Tray aus. Autostart kann dadurch das Fenster öffnen, aber ein unsichtbarer, erreichbarer Tray-Hintergrundbetrieb benötigt weiterhin die native Tray-Anbindung.

## Registrierung

Benutzerbezogene Datei `$XDG_CONFIG_HOME/autostart/sortify.desktop`, bei fehlendem/relativem XDG_CONFIG_HOME stattdessen `~/.config/autostart/sortify.desktop`. Aktivierung registriert den absoluten ausführbaren apphost mit `--background`. Keine Shell und keine Administratorrechte.

Deaktivierung setzt eine eigene Datei auf `Hidden=true`, sodass auch ein gleichnamiger systemweiter Eintrag deaktiviert bleibt. Registrierung erfolgt über temporäre Datei und anschließendes Umbenennen. Fremde Dateien ohne Sortify-Marker, Symlinks und Verzeichnisse am Registrierungspfad werden nicht überschrieben. Der Marker kennzeichnet von Sortify verwaltete Inhalte, ist keine Sicherheitsgrenze gegenüber anderen Programmen desselben Benutzers. Extern veränderte Dateien können als nicht aktiviert gelten; Einstellungen erneut bewusst aktivieren oder den Eintrag prüfen.

Voraussetzung: vorhandene ausführbare Sortify-App (apphost); der `dotnet`-Host allein wird abgelehnt. Pfade mit Steuerzeichen, Prozentzeichen oder Gleichheitszeichen werden ausdrücklich nicht unterstützt. Leerzeichen, Unicode und weitere Sonderzeichen werden korrekt gequotet. Ein später verschobenes/gelöschtes Programm erfordert erneute Registrierung mit dem aktuellen Pfad. Die registrierte App benötigt weiterhin die passende .NET-Laufzeit, sofern sie nicht self-contained veröffentlicht wurde.

## Manuell prüfen

1. Aktuellen Branch bauen. Sortify bevorzugt aus einem stabilen veröffentlichten Ordner starten; für einen Entwicklungstest funktioniert auch der ausführbare apphost aus `src/Sortify.App/bin/Release/net10.0/Sortify.App`. Nicht direkt den dotnet-Host registrieren.
2. In den allgemeinen Einstellungen Autostart aktivieren. Ein zuvor ausgeschalteter Schalter muss bewusst eingeschaltet werden; ein gespeichertes false wird nicht automatisch geändert.
3. Autostartdatei prüfen: Exec zeigt auf genau diese ausführbare App, enthält `--background`, Hidden ist false. Optional `desktop-file-validate` verwenden.
4. Sortify vollständig beenden und ab-/wieder anmelden. Ein Neustart ist ebenfalls möglich. Erwartet wird der Start nach der Desktop-Anmeldung, kein Systemdienst vor dem Login.
5. Ohne nutzbares Tray zeigt Sortify aus Sicherheitsgründen das Hauptfenster. „Minimiert starten“ allein umgeht die Tray-Anforderung nicht.
6. Autostart deaktivieren und erneut anmelden: Sortify startet nicht mehr. Explizites Starten bleibt möglich.

Einen realen Anmelde-/Neustarttest muss der Benutzer durchführen; automatisierte Tests simulieren keine Desktop-Anmeldung. Die Tests schreiben ausschließlich in temporäre Ordner. Ein nativer Launcher-Test wird nur unter Linux mit installiertem `gio` und `desktop-file-validate` ausgeführt; andere Hosts prüfen diesen Desktop-Ablauf damit nicht.

## Quellen

- [XDG Autostart](https://specifications.freedesktop.org/autostart/latest/)
- [Desktop Entry Exec-Escaping](https://specifications.freedesktop.org/desktop-entry/latest/exec-variables.html)

## Verifikation

Release-Build ohne Warnungen/Fehler; gesamte Suite 429 erfolgreich (240 Core, 113 Infrastructure, 76 App). Der native Test mit `desktop-file-validate` und `gio launch` wurde auf diesem Linux-System tatsächlich ausgeführt. Sieben Tests wurden ergänzt. Echte nächste Desktop-Anmeldung bleibt manuell zu prüfen.
