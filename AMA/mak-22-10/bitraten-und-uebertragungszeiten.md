#ama

# Bitraten und Übertragungszeiten

## Grundidee

Bei einer Datenübertragung muss man zwischen der Datenmenge und der Geschwindigkeit unterscheiden:

- **Datenmenge:** Wie viele Bits oder Bytes werden übertragen?
- **Bitrate:** Wie viele Bits werden pro Sekunde übertragen?
- **Übertragungszeit:** Wie lange dauert die Übertragung?

Die Unterlagen verwenden außerdem die **Zeichenrate** und den **Datendurchsatz**.

## Bitrate

Die **Bitrate** `r_b` gibt an, wie viele Bits pro Zeiteinheit übertragen werden.

```text
r_b = n_b / t
```

- `r_b`: Bitrate in bit/s
- `n_b`: übertragene Bits
- `t`: Übertragungszeit in Sekunden

Umgestellt:

```text
n_b = r_b · t
t   = n_b / r_b
```

Typische Einheiten:

- 1 kbit/s = 1 000 bit/s
- 1 Mbit/s = 1 000 000 bit/s
- 1 Gbit/s = 1 000 000 000 bit/s

Vor dem Rechnen müssen alle Größen in passende Einheiten umgerechnet werden, besonders Minuten in Sekunden und Byte in Bit.

## Übertragungszeit

Die Übertragungszeit ist die mindestens benötigte Zeit, wenn keine zusätzlichen Verzögerungen berücksichtigt werden:

```text
t = Datenmenge in bit / Bitrate in bit/s
```

Wichtig:

```text
1 Byte = 8 bit
```

Beispiel aus dem Themenbereich: Für eine Datei von 1,25 MiB bei 34 Mbit/s gilt zuerst die Umrechnung der Dateigröße in Bit. Danach wird durch 34 000 000 bit/s geteilt. Bei einer realen Übertragung kann die Dauer länger sein, etwa durch Protokoll-Overhead, Auslastung oder Verzögerungen.

## Zeichenrate

Die **Zeichenrate** `r_z` beschreibt, wie viele Zeichen pro Sekunde übertragen werden:

```text
r_z = n_z / t
```

Wenn jedes Zeichen mit `n_bz` Bits codiert ist, gilt:

```text
r_b = r_z · n_{bz}
r_z = r_b / n_{bz}
```

Beispiel: Bei 64 kbit/s und 7-bit-ASCII werden höchstens `64 000 / 7` Zeichen pro Sekunde übertragen.

## Datenmenge aus einer Bitrate berechnen

Wenn Bitrate und Zeit bekannt sind:

```text
Datenmenge in bit = Bitrate · Zeit
Datenmenge in Byte = Datenmenge in bit / 8
```

Bei einer Zeitangabe in Minuten muss zuerst mit 60 multipliziert werden.

## Datendurchsatz

Der **Datendurchsatz** `d` beschreibt laut Unterlagen das Verhältnis der Nutzdaten zu allen übertragenen Daten:

```text
d = Nutzbits / übertragene Bits
```

Als Prozentwert:

```text
d in % = (Nutzbits / übertragene Bits) · 100
```

Ein Teil der Übertragung kann also aus Steuerinformationen, Prüfsummen oder anderen Zusatzdaten bestehen und ist keine Nutzinformation.

## Häufige Fehler

- **bit und Byte verwechseln:** Byte immer mit 8 multiplizieren, wenn bit benötigt werden.
- **k/M/G falsch behandeln:** In den Aufgaben sind kbit/s und Mbit/s normalerweise dezimale Einheiten.
- **MiB und MB verwechseln:** MiB basiert auf 1024, MB normalerweise auf 1000.
- **Minuten nicht umrechnen:** 3,25 min = 195 s.
- **Einheiten nicht angleichen:** Erst alles in bit und Sekunden umwandeln, dann rechnen.
- **Maximale Bitrate mit realer Geschwindigkeit gleichsetzen:** Wenn nur 60 % der maximalen WLAN-Rate verfügbar sind, gilt z. B. `108 · 0,60 = 64,8 Mbit/s`.

## Merksatz

**Bitrate mal Zeit ergibt Datenmenge; Datenmenge durch Bitrate ergibt Übertragungszeit.**

## Variablenübersicht

| Variable | Ausgeschriebener Name           | Bedeutung bzw. Einheit                                                     |
| -------- | ------------------------------- | -------------------------------------------------------------------------- |
| `t`      | Übertragungszeit                | Benötigte Zeit für die Übertragung, meist in Sekunden (s)                  |
| `r_z`    | Zeichenrate                     | Anzahl der übertragenen Zeichen pro Sekunde (Zeichen/s)                    |
| `r_b`    | Bitrate                         | Anzahl der übertragenen Bits pro Sekunde (bit/s)                           |
| `n_b`    | Anzahl der übertragenen Bits    | Gesamte Menge der übertragenen Bits (bit)                                  |
| `n_z`    | Anzahl der übertragenen Zeichen | Gesamte Menge der übertragenen Zeichen                                     |
| `n_bz`   | Anzahl der Bits pro Zeichen     | Bitanzahl, mit der ein Zeichen codiert wird (bit/Zeichen)                  |
| `n_d`    | Anzahl der Nutzbits             | Bits, die tatsächlich Nutzdaten enthalten (bit)                            |
| `d`      | Datendurchsatz                  | Verhältnis der Nutzdaten zu allen übertragenen Daten, meist in Prozent (%) |

