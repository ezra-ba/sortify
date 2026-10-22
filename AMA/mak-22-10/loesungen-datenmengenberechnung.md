#ama

# Lösungen: Datenmengenberechnung

## Umrechnungen

- `1 Byte = 8 Bit`
- `1 KiB = 1024 Byte`, `1 MiB = 1024² Byte`, `1 GiB = 1024³ Byte`
- `1 kB = 1000 Byte`, `1 MB = 10⁶ Byte`, `1 GB = 10⁹ Byte`

## 1. Bilddaten in KiB und kB

Formel: `Datenmenge = Breite × Höhe × Datentiefe × Kanäle / 8`

- **a)** RGB: `360 × 270 × 8 × 3 / 8 = 291.600 Byte`  
  **Ergebnis:** `284,77 KiB = 291,60 kB`
- **b)** Graustufen: `1300 × 1900 × 16 / 8 = 4.940.000 Byte`  
  **Ergebnis:** `4.824,22 KiB = 4.940,00 kB`
- **c)** CMYK: `2250 × 1520 × 8 × 4 / 8 = 13.680.000 Byte`  
  **Ergebnis:** `13.359,38 KiB = 13.680,00 kB`

## 2. Bilddaten in MiB und MB

- **a)** `37 × 140 × 25 × 140 × 8 / 8 = 18.130.000 Byte`  
  **Ergebnis:** `17,29 MiB = 18,13 MB`
- **b)** `13 × 120 × 18 × 120 × 16 × 3 / 8 = 20.217.600 Byte`  
  **Ergebnis:** `19,28 MiB = 20,22 MB`
- **c)** `4.096.000 × 8 × 4 / 8 = 16.384.000 Byte`  
  **Ergebnis:** `15,63 MiB = 16,38 MB`

## 3. Audiodateien in MiB und MB

Formel: `Datenmenge = Datenrate × Zeit / 8`

- **a)** `128.000 × 280 / 8 = 4.480.000 Byte`  
  **Ergebnis:** `4,27 MiB = 4,48 MB`
- **b)** `192.000 × (12,5 × 60) / 8 = 18.000.000 Byte`  
  **Ergebnis:** `17,17 MiB = 18,00 MB`

## 4. Unkomprimierte Audiodaten in MiB und MB

Formel: `Datenmenge = Sampling-Rate × Dauer × Bit-Tiefe × Kanäle / 8`

- **a)** `48.000 × 180 × 16 × 2 / 8 = 34.560.000 Byte`  
  **Ergebnis:** `32,96 MiB = 34,56 MB`
- **b)** `44.100 × (15 × 60) × 16 × 2 / 8 = 158.760.000 Byte`  
  **Ergebnis:** `151,41 MiB = 158,76 MB`
- **c)** `48.000 × 570 × 24 × 6 / 8 = 492.480.000 Byte`  
  **Ergebnis:** `469,67 MiB = 492,48 MB`

## 5. Videodateien in MiB und MB

- **a)** `1.536.000 × 150 / 8 = 28.800.000 Byte`  
  **Ergebnis:** `27,47 MiB = 28,80 MB`
- **b)** `2.560.000 × (24 × 60) / 8 = 460.800.000 Byte`  
  **Ergebnis:** `439,45 MiB = 460,80 MB`
- **c)** Gesamtrate: `512 + 128 = 640 kbit/s`  
  `640.000 × (7 × 60 + 30) / 8 = 36.000.000 Byte`  
  **Ergebnis:** `34,33 MiB = 36,00 MB`

## 6. Videodateien in GiB und GB

Formel: `Datenmenge = Breite × Höhe × Frames/s × Datentiefe × Dauer / 8`

- **a)** `960 × 540 × 30 × 24 × 90 / 8 = 4.199.040.000 Byte`  
  **Ergebnis:** `3,91 GiB = 4,20 GB`
- **b)** `512 × 288 × 25 × 24 × (16 × 60 + 20) / 8 = 10.838.016.000 Byte`  
  **Ergebnis:** `10,09 GiB = 10,84 GB`
- **c)** `1920 × 1080 × 30 × 16 × (120 × 60) / 8 = 895.795.200.000 Byte`  
  **Ergebnis:** `834,27 GiB = 895,80 GB`
