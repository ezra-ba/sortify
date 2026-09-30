# Sortify

Sortify ist eine geplante Desktop-Anwendung, die neue Dateien erkennt und Benutzer beim einheitlichen Benennen und Einsortieren unterstützt. Statt Dateien automatisch und unbemerkt zu verändern, zeigt Sortify zuerst einen verständlichen Vorschlag an. Erst nach einer Bestätigung wird die Datei umbenannt oder verschoben.

Das Projekt wird von **ETM Software Solutions** im Rahmen des Projektpraktikums entwickelt.

## Projektstatus

Sortify befindet sich in der Planungs- und Startphase. Projektidee, Name und grundlegende Anforderungen wurden festgelegt. Als UI-Technologie wurde **Avalonia** ausgewählt. Die Projektstruktur und der erste lauffähige Prototyp werden als Nächstes aufgebaut.

## Problemstellung

Heruntergeladene Dateien landen häufig mit uneinheitlichen oder wenig aussagekräftigen Namen in einem gemeinsamen Ordner. Das manuelle Prüfen, Umbenennen und Verschieben kostet Zeit, wird leicht vergessen und muss für ähnliche Dateien immer wieder durchgeführt werden.

Sortify soll diesen Ablauf vereinfachen, ohne dem Benutzer die Kontrolle über seine Dateien zu nehmen.

## Geplanter Ablauf

1. Der Benutzer wählt einen Ordner aus, der beobachtet werden soll.
2. Sortify erkennt eine neue, vollständig verfügbare Datei.
3. Die Datei wird anhand der gespeicherten Regeln geprüft.
4. Sortify schlägt einen neuen Dateinamen und einen Zielordner vor.
5. Ein Dialog zeigt den aktuellen Namen, den neuen Namen und das Ziel an.
6. Der Benutzer bestätigt oder verwirft den Vorschlag.
7. Nur bestätigte Änderungen werden ausgeführt.

## MVP

Für die erste verwendbare Version sind folgende Kernfunktionen vorgesehen:

- überwachte Ordner hinzufügen und verwalten,
- Regeln für Dateinamen und Zielordner anlegen, bearbeiten und entfernen,
- neue Dateien erkennen und auf vollständige Verfügbarkeit warten,
- regelbasierte Sortiervorschläge erstellen,
- Vorschläge vor der Ausführung anzeigen,
- Vorschläge bestätigen oder ablehnen,
- bestätigte Dateien sicher umbenennen und verschieben,
- Einstellungen lokal speichern,
- Fehler verständlich anzeigen und bestehende Dateien vor unbeabsichtigtem Überschreiben schützen.

## Mögliche Erweiterungen

- Verlauf ausgeführter und abgelehnter Vorschläge,
- Rückgängig-Funktion,
- Profile für Schule, Privat und Arbeit,
- Prioritäten für Regeln,
- Duplikaterkennung,
- Import und Export von Regeln,
- Systembenachrichtigungen,
- automatischer Start mit dem Betriebssystem,
- Probelauf für ganze Ordner,
- Statistiken über sortierte Dateien.

## Technologie

- **C# und .NET** für die Anwendung
- **Avalonia UI** für die plattformübergreifende Desktop-Oberfläche
- **MVVM** zur Trennung von Oberfläche, Anwendungslogik und Daten
- **FileSystemWatcher** zur Erkennung neuer Dateien
- lokale Speicherung für Regeln und Einstellungen
- automatisierte Tests für Regelprüfung und Dateioperationen

Die erste Version ist als Desktop-Anwendung vorgesehen. Durch Avalonia bleibt eine spätere Nutzung auf mehreren Betriebssystemen möglich.

## Geplante Architektur

| Bereich | Verantwortung |
|---|---|
| Benutzeroberfläche | Ordner, Regeln und Vorschläge anzeigen und verwalten |
| Ordnerüberwachung | Neue und vollständig verfügbare Dateien erkennen |
| Regelprüfung | Passende Benennung und Zielordner bestimmen |
| Dateioperationen | Bestätigte Änderungen sicher ausführen |
| Speicherung | Einstellungen, Regeln und später den Verlauf lokal sichern |

## Datenschutz und Sicherheit

Sortify soll vollständig lokal funktionieren. KI-Endpunkte, Cloud-Dienste und externe Schnittstellen sind nicht als Kernbestandteil vorgesehen. Dateien werden nur nach ausdrücklicher Bestätigung verändert. Vorhandene Dateien dürfen nicht unbemerkt überschrieben werden; bei Fehlern muss die ursprüngliche Datei erhalten bleiben.

## Projektteam

| Name | Rolle |
|---|---|
| Ezra Bauchinger | Projektleitung |
| Tunahan Barak | Entwicklung und Logo |
| Manuel Stromberger | Dokumentation und Qualitätssicherung |

## Nächste Schritte

- Avalonia-Projektstruktur anlegen,
- grundlegendes MVVM-Gerüst erstellen,
- Verwaltung eines überwachten Ordners umsetzen,
- Regelmodell und lokale Speicherung definieren,
- ersten Ablauf von Dateierkennung bis Vorschau als Prototyp entwickeln,
- Kernfunktionen mit Tests absichern.