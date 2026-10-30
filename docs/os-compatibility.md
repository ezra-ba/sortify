# OS-Kompatibilität: Hintergrundbetrieb und Tray (#81)

## Fokus und Zuständigkeit

Gewünschtes Verhalten: Mit aktivierter Option „in den Infobereich minimieren“ schließt X nur das Hauptfenster. Sortify überwacht weiter und bleibt über ein sichtbares Tray-/Menüleisten-Icon erreichbar. Automatischer Start nach Anmeldung ist ein eigener, zurückgestellter Punkt.

Tuni implementiert Ablaufsteuerung und deren Anbindung im vorhandenen `DesktopSession`-Dienst; Manuel übernimmt native Tray-Anbindung, Icon und Menügestaltung. Ezra übernimmt KI-Tools. Views, ViewModels, `App.axaml.cs` und `TrayMenu` bleiben in diesem Branch unverändert. Die Zuordnung erfolgt nach Funktion, nicht allein nach dem Projektordner.

## Belegte Ursache im Ausgangsstand

`src/Sortify.App/App.axaml.cs` initialisiert `trayAvailable` mit:

```csharp
var trayAvailable = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();
```

Dadurch wird unter Linux kein TrayIcon erzeugt. `DesktopSession.OnClosing` versteckt das Fenster nur bei `TrayAvailable && CloseToTray`. Andernfalls wird nach dem Schließen `Exit` ausgeführt: Monitor beenden, Sitzung aufräumen, App herunterfahren. Das erklärt die gemeldete Beobachtung, ohne einen Fehler des FileSystemWatcher vorauszusetzen.

Der Desktop-Lifetime verwendet bereits `ShutdownMode.OnExplicitShutdown`. Bei erreichbarem Tray wird `window.Hide()` ausgeführt, ohne den Monitor zu entsorgen. Der bisherige Mechanismus zum Weiterlaufen existiert damit bereits. Eine weitere Backend-Schleife oder ein Betriebssystemdienst ist für diesen Anwendungsfall nicht erforderlich.

## Implementierte Backend-Steuerung

`DesktopLifetimeController` in Core enthält keine Avalonia-, Betriebssystem- oder Dateisystemabhängigkeit. Über `IDesktopLifetimeHost` fordert er Zeigen, Verstecken oder ein nach dem Schließereignis eingeplantes Beenden an. Der vorhandene `DesktopSession`-Dienst adaptiert diese drei Aktionen auf die bestehende App.

- `Start(background)` versteckt nur mit nutzbarem Tray; sonst wird das Fenster gezeigt. Die bestehende Prüfung auf minimierten Start, Konfiguration und Überwachung bleibt in `DesktopSession` erhalten.
- `RequestClose(closeToTray, systemShutdown)` liefert, ob der Fenster-Schließvorgang abgebrochen werden muss. X mit aktiviertem CloseToTray und nutzbarem Tray versteckt das Fenster, ohne Monitor oder Sitzung zu entsorgen.
- Beenden ohne CloseToTray/nutzbaren Tray wird nur einmal angefordert. OS-/Anwendungs-Shutdown wird nicht abgefangen.
- `SetTrayAvailable(false)` stellt ein verborgenes Fenster wieder her; Wiederverfügbarkeit erzwingt kein erneutes Verstecken. Nach Stop/Dispose wird kein Fenster wieder geöffnet.
- `DesktopSession.SetTrayAvailability(bool)` ist der Einstieg für Manuels native Tray-Anbindung. Auf dem UI-Thread aufrufen. `TrayAvailable` und `IsInBackground` liefern den aktuellen Zustand; `StateChanged` aktualisiert bestehende Menübeobachter.

Der aktuelle Linux-Ausschluss in `App.axaml.cs` ist absichtlich noch unverändert. Die Backend-Änderung alleine erzeugt kein Linux-Icon und behebt die vollständige App-Funktion erst zusammen mit Manuels Anbindung.

## Übergabe an Manuel

1. Den pauschalen Linux-Ausschluss durch eine tatsächliche Tray-Anbindung mit Capability-/Fehlerprüfung ersetzen. Nicht einfach `trayAvailable = true` setzen.
2. Native Tray-Initialisierung und das vorhandene Menü zusammen behandeln. Ein `NativeMenuExporter` ist ein Hinweis auf Plattformimplementierung, aber kein Nachweis für ein im Desktop sichtbares Icon oder einen laufenden StatusNotifier-Host.
3. Die tatsächlich nutzbare Tray-Verfügbarkeit beim Konstruktor und bei Änderungen über `session.SetTrayAvailability(available)` an `DesktopSession` weitergeben (UI-Thread). Bei eingeschaltetem `CloseToTray` X abfangen und nur das Fenster verstecken; den existierenden Monitor/Sitzungszustand erhalten.
4. „Öffnen“ und „Beenden“ im Tray erhalten. Kein zweites Hauptfenster und keine zweite Überwachung erzeugen.
5. Bei fehlendem Tray bewusst einen sichtbaren Fallback und verständlichen Hinweis vorsehen. Den Prozess ohne erreichbare Oberfläche weiterlaufen zu lassen erfüllt die gewünschte Funktion nicht. Den bisherigen sicheren Fallback bis zur bewussten Produktentscheidung erhalten.
6. Später die Laufzeitverfügbarkeit und Fehler über das vorbereitete Logging anbinden. In diesem Backend-Branch wird die App-Komposition nicht geändert.

## OS-Matrix

| Bereich | Windows | Linux | macOS |
| --- | --- | --- | --- |
| Tray im aktuellen App-Code | wird versucht; Nutzer berichtet funktionsfähig | ausdrücklich ausgeschlossen | wird versucht; real nicht bestätigt |
| Schließen bei nutzbarem Tray | bestehende gemeinsame Sitzungslogik | dieselbe Logik möglich, Anbindung fehlt | dieselbe Logik vorhanden, real prüfen |
| Autostart mit #86 | Windows-Registry-Dienst | Linux-XDG-Dienst über Factory; bewusste Aktivierung nötig | ausdrücklich noch nicht unterstützt |
| Überwachung/Dateioperationen | .NET-Backend; bestehende Tests | .NET-Backend hier getestet | .NET-Backend vorhanden; real prüfen |
| Icon-Position | Infobereich | abhängig von Desktop/Erweiterung | Menüleiste, nicht zwingend unten rechts |

Avalonia beschreibt Linux-Tray-Unterstützung für Desktopumgebungen mit StatusNotifierItem/AppIndicator. GNOME kann eine entsprechende Erweiterung benötigen; die Distribution allein bestimmt die Fähigkeit nicht. Unter macOS öffnet ein Klick auf das Icon das Menü; die Windows-Klickbedienung darf nicht vorausgesetzt werden.

## Manuelle Abnahme nach Manuels Anbindung

- OS-Version und unter Linux Desktopumgebung, X11/Wayland sowie vorhandene Tray-Unterstützung festhalten.
- Sortify normal starten, einen Test-Überwachungsordner konfigurieren und „in den Infobereich minimieren“ aktivieren.
- Vor dem Verstecken prüfen, dass Icon und Menü wirklich erreichbar sind.
- X drücken: Hauptfenster verschwindet, Prozess und Tray bleiben. Eine neue Testdatei im Überwachungsordner muss weiterhin erkannt werden.
- Über das Tray-Menü öffnen: dieselbe Sitzung und das bestehende Hauptfenster erscheinen.
- Über das Tray-Menü beenden: Prozess und Überwachung enden; Neustart ist möglich.
- Mit deaktivierter Einstellung X drücken: vollständiges Beenden bleibt korrekt.
- Ohne unterstützenden Tray-Host testen: sichtbarer Fallback, kein unsichtbarer unerreichbarer Prozess.
- Windows als Regression und macOS auf echter Hardware prüfen. Ein Linux-Test beweist keine macOS-Funktion.

## Automatisierte Absicherung

Drei Core-Tests prüfen Übergänge bei Tray-Verlust/-Rückkehr, einmaligem Exit und OS-Shutdown. Der vorhandene Integrationstest wurde um X, fortgesetzte Dateierkennung und Tray-Verlust/-Rückkehr erweitert. `DesktopSessionTests` prüft außerdem Hintergrundstart mit weiterlaufender Überwachung, Schließen/Verstecken und Rückkehr zum normalen Beenden sowie den sicheren Fallback ohne Tray oder Konfiguration. Headless-Tests übergeben simulierte Tray-Verfügbarkeit; sie beweisen kein sichtbares natives Tray-Icon auf einem echten Desktop.

## Quellen

- [Avalonia TrayIcon](https://docs.avaloniaui.net/controls/navigation/trayicon): unterstützte Plattformen, Linux-Voraussetzungen und Unterschiede bei der Bedienung.
- [Avalonia TrayIcon-Implementierung](https://github.com/AvaloniaUI/Avalonia/blob/main/src/Avalonia.Controls/TrayIcon.cs): NativeMenuExporter ist die native Menüanbindung, kein Desktop-Sichtbarkeitssignal.
- [XDG Autostart](https://specifications.freedesktop.org/autostart/latest/): separate Anmeldung/Autostart-Funktion.
- [Apple LaunchAgents](https://developer.apple.com/library/archive/documentation/MacOSX/Conceptual/BPSystemStartup/Chapters/CreatingLaunchdJobs.html): separate benutzerbezogene Startfunktion; aktuelle Paketierung/Anmeldeverwaltung muss gesondert geprüft werden.

## Zurückgestellter Entwurf

Der begonnene, noch ungetestete Autostart-Entwurf ist lokal im Git-Stash mit der Beschreibung `wip: defer untested OS autostart draft after tray focus clarification` gesichert. Er gehört nicht zum Tray-Fix und wird nicht als fertige Lösung ausgeliefert.

## Verifikation dieses Backend-Stands

Release-Build ohne Warnungen/Fehler. Vollständige Testsuite: 422 erfolgreich (240 Core, 107 Infrastructure, 75 App). Drei neue Core-Tests und der erweiterte bestehende Sitzungstest sichern die Änderung ab. Die gegenüber dem letzten Logging-Stand zusätzlichen App-Tests stammen aus dem bereits integrierten Splashscreen-PR. `git diff --check` ist sauber. Reale Tray-/Desktop-Abnahme unter Linux, Windows und macOS steht weiterhin aus.

## Folgearbeit #86

Linux-Autostart ist inzwischen separat implementiert und in der App-Komposition über eine OS-Factory angebunden. Anleitung und Grenzen: [linux-autostart.md](linux-autostart.md). Die native Tray-Anbindung bleibt unabhängig davon offen; ein Autostart ohne nutzbaren Tray öffnet das Hauptfenster. Der ältere vollständige Autostart-Stash bleibt zur Referenz erhalten, insbesondere sein ungetesteter macOS-Entwurf wird nicht ausgeliefert.
