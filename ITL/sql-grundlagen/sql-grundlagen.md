#itl

# SQL-Grundlagen – kleines Beispielschema

Das Skript verwendet MariaDB/MySQL-Syntax und deckt die wichtigsten Grundlagen ab: Datenbank und Tabellen erstellen, Daten einfügen und ändern, Abfragen, Joins, Gruppierungen, Unterabfragen und Transaktionen.

```sql
-- ============================================================
-- 1. Datenbank erstellen und auswählen
-- ============================================================
DROP DATABASE IF EXISTS schulshop;
CREATE DATABASE schulshop
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE schulshop;

-- ============================================================
-- 2. Tabellen mit Primär- und Fremdschlüsseln erstellen
-- ============================================================
CREATE TABLE kunde (
    kunde_id INT AUTO_INCREMENT PRIMARY KEY,
    vorname VARCHAR(50) NOT NULL,
    nachname VARCHAR(50) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    erstellt_am DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE produkt (
    produkt_id INT AUTO_INCREMENT PRIMARY KEY,
    bezeichnung VARCHAR(100) NOT NULL,
    kategorie VARCHAR(50),
    preis DECIMAL(10, 2) NOT NULL CHECK (preis >= 0),
    lagerstand INT NOT NULL DEFAULT 0
);

CREATE TABLE bestellung (
    bestellung_id INT AUTO_INCREMENT PRIMARY KEY,
    kunde_id INT NOT NULL,
    bestelldatum DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'offen',
    FOREIGN KEY (kunde_id) REFERENCES kunde(kunde_id)
);

CREATE TABLE bestellposition (
    bestellung_id INT NOT NULL,
    produkt_id INT NOT NULL,
    menge INT NOT NULL CHECK (menge > 0),
    einzelpreis DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (bestellung_id, produkt_id),
    FOREIGN KEY (bestellung_id) REFERENCES bestellung(bestellung_id),
    FOREIGN KEY (produkt_id) REFERENCES produkt(produkt_id)
);

-- Tabellen und Spalten untersuchen
SHOW TABLES;
DESCRIBE kunde;

-- ============================================================
-- 3. Beispieldaten einfügen (INSERT)
-- ============================================================
INSERT INTO kunde (vorname, nachname, email) VALUES
    ('Anna', 'Huber', 'anna.huber@example.com'),
    ('Lukas', 'Bauer', 'lukas.bauer@example.com'),
    ('Sophie', 'Wagner', 'sophie.wagner@example.com');

INSERT INTO produkt (bezeichnung, kategorie, preis, lagerstand) VALUES
    ('USB-Stick 64 GB', 'Zubehör', 9.90, 25),
    ('Tastatur', 'Zubehör', 24.50, 12),
    ('Notebook', 'Computer', 799.00, 5),
    ('Maus', 'Zubehör', 15.00, 0);

INSERT INTO bestellung (kunde_id, bestelldatum, status) VALUES
    (1, '2026-10-01', 'bezahlt'),
    (2, '2026-10-02', 'offen');

INSERT INTO bestellposition (bestellung_id, produkt_id, menge, einzelpreis) VALUES
    (1, 1, 2, 9.90),
    (1, 2, 1, 24.50),
    (2, 3, 1, 799.00);

-- ============================================================
-- 4. Daten abfragen (SELECT)
-- ============================================================
SELECT * FROM produkt;
SELECT bezeichnung, preis FROM produkt WHERE preis < 100;
SELECT * FROM produkt WHERE bezeichnung LIKE '%a%';
SELECT * FROM produkt WHERE lagerstand BETWEEN 1 AND 20;
SELECT * FROM produkt WHERE kategorie IN ('Zubehör', 'Computer');
SELECT * FROM produkt ORDER BY preis DESC LIMIT 3;

-- NULL korrekt prüfen
SELECT * FROM produkt WHERE kategorie IS NULL;

-- Berechnete Spalte und Alias
SELECT
    bezeichnung,
    preis,
    preis * 1.20 AS preis_inklusive_mwst
FROM produkt;

-- ============================================================
-- 5. Daten bearbeiten (UPDATE und DELETE)
-- ============================================================
UPDATE produkt
SET preis = 19.90, lagerstand = lagerstand + 5
WHERE produkt_id = 1;

UPDATE bestellung
SET status = 'versendet'
WHERE bestellung_id = 1;

-- Immer eine WHERE-Bedingung verwenden!
DELETE FROM bestellposition
WHERE bestellung_id = 2 AND produkt_id = 3;

-- ============================================================
-- 6. Kleine JOINS
-- ============================================================
-- INNER JOIN: nur passende Datensätze
SELECT
    b.bestellung_id,
    CONCAT(k.vorname, ' ', k.nachname) AS kunde,
    b.bestelldatum,
    b.status
FROM bestellung AS b
INNER JOIN kunde AS k ON k.kunde_id = b.kunde_id;

-- Mehrere Tabellen verbinden und den Bestellwert berechnen
SELECT
    b.bestellung_id,
    p.bezeichnung,
    bp.menge,
    bp.einzelpreis,
    bp.menge * bp.einzelpreis AS positionswert
FROM bestellposition AS bp
JOIN bestellung AS b ON b.bestellung_id = bp.bestellung_id
JOIN produkt AS p ON p.produkt_id = bp.produkt_id;

-- LEFT JOIN: auch Kunden ohne Bestellung anzeigen
SELECT
    k.kunde_id,
    k.vorname,
    k.nachname,
    COUNT(b.bestellung_id) AS anzahl_bestellungen
FROM kunde AS k
LEFT JOIN bestellung AS b ON b.kunde_id = k.kunde_id
GROUP BY k.kunde_id, k.vorname, k.nachname;

-- ============================================================
-- 7. Aggregatfunktionen, GROUP BY und HAVING
-- ============================================================
SELECT
    kategorie,
    COUNT(*) AS anzahl_produkte,
    MIN(preis) AS billigstes,
    MAX(preis) AS teuerstes,
    ROUND(AVG(preis), 2) AS durchschnittspreis
FROM produkt
GROUP BY kategorie
HAVING AVG(preis) > 10;

-- Gesamtwert aller Bestellungen
SELECT SUM(menge * einzelpreis) AS gesamtwert
FROM bestellposition;

-- ============================================================
-- 8. Unterabfrage und CASE
-- ============================================================
SELECT bezeichnung, preis
FROM produkt
WHERE preis > (SELECT AVG(preis) FROM produkt);

SELECT
    bezeichnung,
    CASE
        WHEN lagerstand = 0 THEN 'ausverkauft'
        WHEN lagerstand < 10 THEN 'knapp'
        ELSE 'verfügbar'
    END AS lagerstatus
FROM produkt;

-- ============================================================
-- 9. Transaktion: mehrere Änderungen sicher durchführen
-- ============================================================
START TRANSACTION;

UPDATE produkt
SET lagerstand = lagerstand - 1
WHERE produkt_id = 3 AND lagerstand > 0;

INSERT INTO bestellposition
    (bestellung_id, produkt_id, menge, einzelpreis)
VALUES
    (1, 3, 1, 799.00);

-- Bei Erfolg speichern, bei einem Fehler stattdessen ROLLBACK verwenden.
COMMIT;
-- ROLLBACK;

-- ============================================================
-- 10. Nützliche Änderungen am Tabellenaufbau
-- ============================================================
ALTER TABLE kunde ADD telefon VARCHAR(30);
CREATE INDEX idx_produkt_kategorie ON produkt(kategorie);
```

## Merksätze

- `CREATE`, `ALTER` und `DROP` verändern die Struktur der Datenbank.
- `INSERT`, `UPDATE` und `DELETE` verändern Daten.
- `SELECT` liest Daten; `WHERE` filtert vor der Gruppierung.
- `GROUP BY` bildet Gruppen, `HAVING` filtert danach.
- `JOIN` verbindet Tabellen über passende Schlüssel.
- Bei `UPDATE` und `DELETE` zuerst die passende `WHERE`-Bedingung prüfen.
