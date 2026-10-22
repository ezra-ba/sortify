#ama

# Übungen: Bitraten und Übertragungszeiten

## Bitrate und Datenmenge

- [x] **1. Bitrate berechnen**

  18 000 bit werden in 2,5 Sekunden übertragen. Berechne die Bitrate in bit/s und kbit/s.

- [x] **2. Datenmenge berechnen**

  Eine Netzwerkverbindung arbeitet mit 12 Mbit/s. Wie viele bit und Byte werden in 15 Sekunden übertragen?

- [x] **3. Übertragungszeit berechnen**

  Eine Datei ist 6,4 MB groß und wird mit 8 Mbit/s übertragen. Wie lange dauert die Übertragung mindestens? Gib das Ergebnis in Sekunden an.

## Zeichenrate

- [x] **4. Zeichenrate aus der Bitrate**

  Eine Leitung hat eine Bitrate von 19 200 bit/s. Jedes Zeichen wird mit 8 bit codiert. Wie groß ist die Zeichenrate in Zeichen/s?

- [x] **5. Übertragungszeit eines Textes**

  Ein Text besteht aus 7 500 Zeichen und wird mit 7-bit-ASCII codiert. Die Leitung erreicht 9 600 bit/s. Berechne die Übertragungszeit in Sekunden.

## Einheiten umrechnen

- [x] **6. MiB und Mbit**

  Eine Datei ist 2,5 MiB groß. Sie wird mit 40 Mbit/s übertragen. Berechne die theoretische Übertragungszeit. Verwende 1 MiB = 1 024 KiB und 1 KiB = 1 024 Byte.

- [x] **7. Minuten in Sekunden**

  Mit einer Bitrate von 25 Mbit/s werden Daten 3,5 Minuten lang übertragen. Wie viele GB Daten entsprechen dieser Datenmenge? Verwende 1 GB = 1 000 000 000 Byte.

## Datendurchsatz

- [x] **8. Nutzdaten und Datendurchsatz**

  Bei einer Übertragung werden insgesamt 80 Mbit übertragen. Davon sind 72 Mbit Nutzdaten. Berechne den Datendurchsatz in Prozent.

- [x] **9. Reale Übertragungsrate**

  Ein WLAN hat eine maximale Bitrate von 108 Mbit/s. Wegen der tatsächlichen Bedingungen stehen nur 65 % davon zur Verfügung. Wie groß ist die reale Bitrate? Wie lange dauert damit die Übertragung einer Datei von 250 MiB?

## Gemischte Aufgabe

- [ ] **10. Backup planen**

  Ein Backup ist 18 GB groß und soll über eine Leitung mit 250 Mbit/s übertragen werden. Berechne die Mindestdauer in Minuten. Das Backup soll um 06:00 Uhr fertig sein. Um wie viel Uhr muss die Übertragung spätestens beginnen?

## Hinweise

- Vor dem Rechnen alle Größen in passende Einheiten umwandeln.
- `1 Byte = 8 bit`
- Bei Mbit/s und GB in diesen Aufgaben gilt: `1 Mbit = 1 000 000 bit` und `1 GB = 1 000 000 000 Byte`.
- Für die theoretische Mindestzeit werden Verzögerungen und Protokoll-Overhead nicht berücksichtigt.

## Lösungen

### 1. Bitrate berechnen

`r_b = 18 000 bit / 2,5 s = 7 200 bit/s = 7,2 kbit/s`

### 2. Datenmenge berechnen

`12 000 000 bit/s · 15 s = 180 000 000 bit`

`180 000 000 bit / 8 = 22 500 000 Byte`

Ergebnis: **180 Mbit bzw. 22,5 MB**.

### 3. Übertragungszeit berechnen

`6,4 MB · 8 = 51,2 Mbit`

`t = 51,2 Mbit / 8 Mbit/s = 6,4 s`

Ergebnis: **6,4 Sekunden**.

### 4. Zeichenrate aus der Bitrate

`r_z = 19 200 bit/s / 8 bit pro Zeichen = 2 400 Zeichen/s`

### 5. Übertragungszeit eines Textes

`7 500 Zeichen · 7 bit = 52 500 bit`

`t = 52 500 bit / 9 600 bit/s = 5,46875 s`

Ergebnis: **ca. 5,47 Sekunden**.

### 6. MiB und Mbit

`2,5 MiB = 2,5 · 1 024 · 1 024 Byte = 2 621 440 Byte`

`2 621 440 Byte · 8 = 20 971 520 bit`

`t = 20 971 520 bit / 40 000 000 bit/s = 0,524288 s`

Ergebnis: **ca. 0,524 Sekunden**.

### 7. Minuten in Sekunden

`3,5 min = 210 s`

`25 000 000 bit/s · 210 s = 5 250 000 000 bit`

`5 250 000 000 bit / 8 = 656 250 000 Byte`

Ergebnis: **0,65625 GB**.

### 8. Nutzdaten und Datendurchsatz

`d = 72 Mbit / 80 Mbit · 100 = 90 %`

### 9. Reale Übertragungsrate

`108 Mbit/s · 0,65 = 70,2 Mbit/s`

`250 MiB = 250 · 1 024 · 1 024 Byte = 262 144 000 Byte`

`t = (262 144 000 Byte · 8) / 70 200 000 bit/s = 29,84 s`

Ergebnis: **70,2 Mbit/s und ca. 29,84 Sekunden**.

### 10. Backup planen

`18 GB · 8 = 144 Gbit`

`t = 144 000 000 000 bit / 250 000 000 bit/s = 576 s`

`576 s = 9 min 36 s`

Das Backup muss spätestens um **05:50:24 Uhr** starten.
