#pp

# Aufgabe: Discord-inspirierten Toggle-Switch in Avalonia implementieren

Implementiere einen hochwertigen, animierten Toggle-Switch für meine bestehende Desktop-Anwendung auf Basis von **Avalonia UI, XAML und C#**.

## Ziel

Der Toggle soll sich an den Toggle-Buttons in den Einstellungen von Discord orientieren. Er soll modern, dynamisch und hochwertig wirken. Die Animation soll sich unmittelbar auf Benutzereingaben reagieren und ein dezentes, physisch wirkendes Bewegungsgefühl vermitteln.

Untersuche zuerst die bestehende Projektstruktur, die verwendete Avalonia-Version, das vorhandene Styling-System und bereits existierende Controls. Integriere die Lösung in die bestehende Architektur, statt unnötig ein separates Beispielprojekt oder eine parallele UI-Struktur anzulegen.

## 1. Visuelles Design

- Kompakter Toggle mit abgerundetem Hintergrund und kreisförmigem Regler.
    
- Standardgröße ungefähr 44 × 24 DIP; passe die Maße bei Bedarf an das bestehende Designsystem an.
    
- Deaktivierter Zustand: neutraler, dunkler oder grauer Hintergrund.
    
- Aktivierter Zustand: dezenter dem vorhandenen Theme definierter Akzenthintergrund.
    
- Weißer oder kontrastreich gestalteter Knob.
    
- Saubere Rundungen, korrekte Ausrichtung und keine sichtbaren Rendering-Artefakte.
    
- Berücksichtige Light- und Dark-Theme.
    

Der Toggle soll sich in bestehende Einstellungsseiten integrieren lassen und optisch mit der übrigen Anwendung harmonieren.

## 2. Animationsverhalten

Implementiere eine flüssige Micro-Interaction, die sich an Discord orientiert, ohne zu behaupten, die exakten internen Discord-Animationsparameter zu reproduzieren.

Die Animation soll folgende Eigenschaften kombinieren:

1. **Knob-Translation:** Der Regler bewegt sich horizontal zwischen den beiden Zuständen.
    
2. **Natürliches Easing:** Verwende eine passende Easing-Kurve oder eine Spring-Animation mit leichtem, kontrolliertem Überschwingen.
    
3. **Settling:** Nach dem Überschwingen kehrt der Regler weich in seine exakte Zielposition zurück.
    
4. **Farbtransition:** Der Hintergrund wechselt weich zwischen den Zuständen. Der Farbwechsel darf etwas früher abgeschlossen sein als die Bewegung des Reglers.
    
5. **Subtile Squash-/Stretch-Bewegung:** Wenn technisch sinnvoll, wird der Knob während der Bewegung kurz horizontal gestaucht oder gestreckt.
    
6. **Responsives Verhalten:** Schnelles, wiederholtes Klicken darf die Animation nicht beschädigen. Der Zustand und die endgültige Position müssen stets synchron bleiben.
    

Orientiere dich an folgenden Richtwerten:

- Bewegungsdauer: ungefähr 200–320 ms.
    
- Farbtransition: ungefähr 150–220 ms.
    
- Overshoot: dezent und kontrolliert.
    
- Keine übertriebenen Bounce-Effekte oder unnötigen Verzögerungen.
    

Diese Werte sind Ausgangspunkte. Optimiere die Animation nach visueller Qualität und konsistentem Verhalten.

## 3. Technische Umsetzung

- Verwende Avalonia-kompatible APIs, die zur tatsächlich installierten Avalonia-Version passen.
    
- Bevorzuge einen wiederverwendbaren, eigenständigen Control mit einem sauber definierten XAML-Template.
    
- Nutze native Avalonia-Animationsmechanismen, insbesondere Control Transitions, Keyframe Animations und geeignete Easing-Funktionen.
    
- Verwende C# nur dort, wo es für die Zustandssteuerung, das Auslösen von Animationen oder die Koordination mehrerer animierter Eigenschaften tatsächlich erforderlich ist.
    
- Vermeide unnötige Abhängigkeiten von Drittanbieterbibliotheken.
    
- Vermeide konkurrierende Animationen, die gleichzeitig dieselbe Eigenschaft steuern.
    
- Stelle sicher, dass der Knob in beiden Zuständen exakt an der richtigen Position endet.
    
- Verwende keine Timer- oder Thread-basierten Workarounds, wenn Avalonias Animationssystem die Aufgabe übernehmen kann.
    

Wenn eine Spring-Animation mit den vorhandenen APIs nicht zuverlässig implementiert werden kann, verwende eine geeignete Keyframe-Animation als Alternative. Erfinde keine nicht existierenden Avalonia-Properties, Methoden oder XAML-Attribute.

## 4. Control-API und Datenbindung

Der Toggle muss wie ein reguläres Avalonia-Control verwendbar sein.

- Unterstütze einen bindbaren booleschen Zustand über `IsChecked`.
    
- Unterstütze `TwoWay`-Datenbindung an ein ViewModel.
    
- Verwende nach Möglichkeit `ToggleButton`-Semantik oder eine gleichwertige, korrekt implementierte Avalonia-Control-API.
    
- Erhalte die üblichen Interaktionsmöglichkeiten wie Maus, Tastatur und Fokus.
    
- Stelle sicher, dass der visuelle Zustand dem tatsächlichen Zustand entspricht.
    
- Vermeide doppelte Zustandsverwaltung zwischen XAML, Code-behind und ViewModel.
    
- Ermögliche die einfache Verwendung in einer Einstellungszeile mit Titel und optionaler Beschreibung.
    

Beispiel für die gewünschte Verwendung:

```
<controls:AnimatedToggle
    IsChecked="{Binding LaunchAtStartup, Mode=TwoWay}" />
```

Passe den Namespace und die API an die bestehende Projektstruktur an.

## 5. Barrierefreiheit und Robustheit

- Unterstütze Tastaturbedienung und einen sichtbaren Fokuszustand.
    
- Stelle sicher, dass die Animation die Funktionalität des Controls nicht beeinträchtigt.
    
- Berücksichtige `prefers-reduced-motion` beziehungsweise eine geeignete, in Avalonia tatsächlich verfügbare Einstellung für reduzierte Animationen. Falls keine automatische Systemintegration vorhanden ist, ermögliche eine konfigurierbare Option auf Anwendungsebene.
    
- Stelle sicher, dass deaktivierte Controls korrekt dargestellt werden und keine Zustandsänderungen zulassen.
    
- Vermeide unnötige Layout-Neuberechnungen und achte auf flüssiges Rendering.
    
- Verifiziere, dass die Lösung mit der verwendeten Avalonia-Version kompiliert.
    

## 6. Integration in das bestehende Projekt

1. Analysiere die bestehende Architektur und identifiziere den geeigneten Ort für das neue Control.
    
2. Erstelle oder erweitere die benötigten XAML-, C#- und Style-Dateien.
    
3. Registriere das Control und seine Styles entsprechend der vorhandenen Projektkonventionen.
    
4. Integriere das Control in mindestens eine geeignete bestehende Einstellungszeile, sofern das Projekt einen passenden Einsatzort enthält.
    
5. Verwende bestehende Theme-Farben, Ressourcen und Design-Tokens, wo sinnvoll.
    
6. Entferne keine vorhandenen Funktionen und verändere keine sachfremden Komponenten.
    

## 7. Validierung

Nach der Implementierung:

- Baue das Projekt und behebe alle durch die Änderung verursachten Compilerfehler.
    
- Überprüfe die Animation für beide Zustandswechsel.
    
- Prüfe schnelles, wiederholtes Umschalten.
    
- Prüfe die Synchronisierung mit der `TwoWay`-Datenbindung.
    
- Prüfe Fokus, Tastaturbedienung und deaktivierten Zustand.
    
- Kontrolliere, ob der Regler nach jeder Animation exakt an der Zielposition landet.
    
- Dokumentiere verbleibende Einschränkungen, falls bestimmte Effekte mit der vorhandenen Avalonia-Version nicht zuverlässig realisierbar sind.
    

## Erwartetes Ergebnis

Liefere eine vollständig integrierte, wiederverwendbare Implementierung und keine bloße Demo.

Priorität haben:

1. Korrekte Funktionalität und Datenbindung.
    
2. Flüssige, hochwertige Animation.
    
3. Saubere Integration in das bestehende Projekt.
    
4. Wartbarer, versionskompatibler Code.
    
5. Barrierefreiheit und zuverlässiges Verhalten.
    

Triff sinnvolle Implementierungsentscheidungen selbstständig. Stelle nur dann Rückfragen, wenn eine wesentliche Information fehlt, die sich nicht aus dem bestehenden Projekt ableiten lässt.
