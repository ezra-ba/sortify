#pp

# UI Rework

## Theme
## Light Mode

- Fensterhintergrund: `#F5F5F7`
    
- Karten und Eingabeflächen: `#FFFFFF`
    
- Haupttext: `#1C1C1E`
    
- Sekundärtext: `#6E6E73`
    
- Violetter Akzent: `#7657D9`
    
- **Violetter Akzent Hover:** `#6545C5`
    
- Ausgewählte Bereiche: `#F0ECFF`
    
- **Ausgewählte Bereiche Hover:** `#E5DEFF`
    
- Rahmen: `#E1E1E6`
    
- **Rahmen Hover:** `#C8C8D0`
    
- Erfolg / Datei bereit: `#15803D`
    
- Fehler / ungültige Eingaben: `#B42318`
    

### Button-Zustände – Light Mode

- Primärer Button: `#7657D9`
    
- Primärer Button Hover: `#6545C5`
    
- Primärer Button Active / Pressed: `#5536AE`
    
- Sekundärer Button: `#FFFFFF`
    
- Sekundärer Button Hover: `#F0ECFF`
    
- Sekundärer Button Active / Pressed: `#E5DEFF`
    
- Deaktivierter Button: `#E1E1E6`
    
- Deaktivierter Button Text: `#6E6E73`
    

## Dark Mode

- Fensterhintergrund: `#171719`
    
- Karten und Eingabeflächen: `#222225`
    
- Haupttext: `#F5F5F7`
    
- Sekundärtext: `#A1A1AA`
    
- Violetter Akzent: `#9B82F3`
    
- **Violetter Akzent Hover:** `#B09DF8`
    
- Ausgewählte Bereiche: `#302747`
    
- **Ausgewählte Bereiche Hover:** `#40345B`
    
- Rahmen: `#3A3A40`
    
- **Rahmen Hover:** `#52525B`
    
- Erfolg / Datei bereit: `#22C55E`
    
- Fehler / ungültige Eingaben: `#F04438`
    

### Button-Zustände – Dark Mode

- Primärer Button: `#9B82F3`
    
- Primärer Button Hover: `#B09DF8`
    
- Primärer Button Active / Pressed: `#7657D9`
    
- Sekundärer Button: `#2B2B30`
    
- Sekundärer Button Hover: `#393940`
    
- Sekundärer Button Active / Pressed: `#45454D`
    
- Deaktivierter Button: `#393940`
    
- Deaktivierter Button Text: `#A1A1AA`
    

## Interaktionsregeln

- **Hover:** Visuelle Rückmeldung bei Mauszeigerkontakt; auf Touch-Geräten nicht voraussetzen.
    
- **Active / Pressed:** Deutlich erkennbarer Zustand während des Klicks oder Drückens.
    
- **Selected:** Bleibt sichtbar, solange ein Element ausgewählt ist.
    
- **Focus:** Sichtbarer Fokusindikator für Tastaturbedienung, z. B. ein 2 px breiter Ring in `#7657D9` im Light Mode bzw. `#B09DF8` im Dark Mode mit 2 px Abstand.
    
- **Disabled:** Keine Hover- oder Active-Reaktion; Zustand muss auch ohne Farbe erkennbar sein.

## Corner-Radius
- Überprüfe welche der folgenden Elemente in der App vorkommen und passe den Corner-Radius an:
```
<!-- Radius-Tokens für das Designsystem -->
<CornerRadius x:Key="RadiusButton">6</CornerRadius>
<CornerRadius x:Key="RadiusInput">7</CornerRadius>
<CornerRadius x:Key="RadiusTab">8</CornerRadius>
<CornerRadius x:Key="RadiusCard">12</CornerRadius>
<CornerRadius x:Key="RadiusDialog">14</CornerRadius>
<CornerRadius x:Key="RadiusPopover">10</CornerRadius>
<CornerRadius x:Key="RadiusBadge">5</CornerRadius>
```
