-- SQL-Schema für das ERM-Diagramm Online-Reisebuchung
-- Zielsystem: MySQL 8.x

CREATE DATABASE IF NOT EXISTS online_reisebuchung;
USE online_reisebuchung;

CREATE TABLE IF NOT EXISTS kunde (
    kunde_id INT UNSIGNED AUTO_INCREMENT,
    vorname VARCHAR(50) NOT NULL,
    nachname VARCHAR(50) NOT NULL,
    email VARCHAR(150) NOT NULL,
    telefon VARCHAR(30),
    PRIMARY KEY (kunde_id),
    UNIQUE KEY uq_kunde_email (email)
);

CREATE TABLE IF NOT EXISTS reise (
    reise_id INT UNSIGNED AUTO_INCREMENT,
    bezeichnung VARCHAR(150) NOT NULL,
    zielland VARCHAR(80) NOT NULL,
    startdatum DATE NOT NULL,
    enddatum DATE NOT NULL,
    grundpreis DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (reise_id),
    CONSTRAINT chk_reise_zeitraum CHECK (enddatum >= startdatum),
    CONSTRAINT chk_reise_preis CHECK (grundpreis >= 0)
);

CREATE TABLE IF NOT EXISTS unterkunft (
    unterkunft_id INT UNSIGNED AUTO_INCREMENT,
    name VARCHAR(150) NOT NULL,
    typ VARCHAR(50) NOT NULL,
    sterne TINYINT UNSIGNED,
    PRIMARY KEY (unterkunft_id),
    CONSTRAINT chk_unterkunft_sterne CHECK (sterne BETWEEN 1 AND 5)
);

CREATE TABLE IF NOT EXISTS flug (
    flug_id INT UNSIGNED AUTO_INCREMENT,
    fluggesellschaft VARCHAR(100) NOT NULL,
    flugnummer VARCHAR(20) NOT NULL,
    abflugzeit DATETIME NOT NULL,
    ankunftszeit DATETIME NOT NULL,
    preis DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (flug_id),
    UNIQUE KEY uq_flug_nummer_zeit (flugnummer, abflugzeit),
    CONSTRAINT chk_flug_preis CHECK (preis >= 0),
    CONSTRAINT chk_flug_zeit CHECK (ankunftszeit > abflugzeit)
);

CREATE TABLE IF NOT EXISTS buchung (
    buchung_id INT UNSIGNED AUTO_INCREMENT,
    kunde_id INT UNSIGNED NOT NULL,
    reise_id INT UNSIGNED NOT NULL,
    buchungsdatum DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status ENUM('offen', 'bestätigt', 'abgeschlossen', 'storniert')
        NOT NULL DEFAULT 'offen',
    PRIMARY KEY (buchung_id),
    CONSTRAINT fk_buchung_kunde
        FOREIGN KEY (kunde_id) REFERENCES kunde (kunde_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_buchung_reise
        FOREIGN KEY (reise_id) REFERENCES reise (reise_id)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS buchung_person (
    buchung_person_id INT UNSIGNED AUTO_INCREMENT,
    buchung_id INT UNSIGNED NOT NULL,
    vorname VARCHAR(50) NOT NULL,
    nachname VARCHAR(50) NOT NULL,
    PRIMARY KEY (buchung_person_id),
    CONSTRAINT fk_buchung_person_buchung
        FOREIGN KEY (buchung_id) REFERENCES buchung (buchung_id)
        ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS reise_unterkunft (
    reise_id INT UNSIGNED NOT NULL,
    unterkunft_id INT UNSIGNED NOT NULL,
    naechte SMALLINT UNSIGNED NOT NULL,
    PRIMARY KEY (reise_id, unterkunft_id),
    CONSTRAINT fk_reise_unterkunft_reise
        FOREIGN KEY (reise_id) REFERENCES reise (reise_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_reise_unterkunft_unterkunft
        FOREIGN KEY (unterkunft_id) REFERENCES unterkunft (unterkunft_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_reise_unterkunft_naechte CHECK (naechte > 0)
);

CREATE TABLE IF NOT EXISTS reise_flug (
    reise_id INT UNSIGNED NOT NULL,
    flug_id INT UNSIGNED NOT NULL,
    reihenfolge SMALLINT UNSIGNED NOT NULL,
    PRIMARY KEY (reise_id, flug_id),
    UNIQUE KEY uq_reise_flug_reihenfolge (reise_id, reihenfolge),
    CONSTRAINT fk_reise_flug_reise
        FOREIGN KEY (reise_id) REFERENCES reise (reise_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_reise_flug_flug
        FOREIGN KEY (flug_id) REFERENCES flug (flug_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_reise_flug_reihenfolge CHECK (reihenfolge > 0)
);

CREATE TABLE IF NOT EXISTS zahlung (
    zahlung_id INT UNSIGNED AUTO_INCREMENT,
    buchung_id INT UNSIGNED NOT NULL,
    zahlungsdatum DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    betrag DECIMAL(10, 2) NOT NULL,
    zahlungsmethode ENUM('Kreditkarte', 'PayPal', 'Überweisung', 'Debitkarte')
        NOT NULL,
    PRIMARY KEY (zahlung_id),
    CONSTRAINT fk_zahlung_buchung
        FOREIGN KEY (buchung_id) REFERENCES buchung (buchung_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT chk_zahlung_betrag CHECK (betrag > 0)
);









-- Befüllung

USE online_reisebuchung;

INSERT INTO kunde (vorname, nachname, email, telefon) VALUES
    ('Anna', 'Huber', 'anna.huber@example.com', '+43 664 1111111'),
    ('Lukas', 'Bauer', 'lukas.bauer@example.com', '+43 664 2222222'),
    ('Sophie', 'Wagner', 'sophie.wagner@example.com', '+43 664 3333333'),
    ('Max', 'Gruber', 'max.gruber@example.com', '+43 664 4444444'),
    ('Laura', 'Steiner', 'laura.steiner@example.com', '+43 664 5555555');

INSERT INTO reise
    (bezeichnung, zielland, startdatum, enddatum, grundpreis)
VALUES
    ('Sommerurlaub am Meer', 'Italien', '2026-07-05', '2026-07-12', 899.00),
    ('Städtereise nach Paris', 'Frankreich', '2026-08-10', '2026-08-14', 649.00),
    ('Kulturreise nach Japan', 'Japan', '2026-09-01', '2026-09-12', 2199.00),
    ('Winterurlaub in den Alpen', 'Österreich', '2026-12-20', '2026-12-27', 799.00),
    ('Rundreise durch Spanien', 'Spanien', '2027-04-03', '2027-04-13', 1499.00);

INSERT INTO unterkunft (name, typ, sterne) VALUES
    ('Hotel Mare Blu', 'Hotel', 4),
    ('Paris Central', 'Hotel', 3),
    ('Tokyo Garden Hotel', 'Hotel', 4),
    ('Alpenblick Resort', 'Resort', 4),
    ('Madrid Plaza', 'Hotel', 5);

INSERT INTO flug
    (fluggesellschaft, flugnummer, abflugzeit, ankunftszeit, preis)
VALUES
    ('Austrian Airlines', 'OS501', '2026-07-05 08:00:00', '2026-07-05 09:30:00', 180.00),
    ('Air France', 'AF123', '2026-08-10 10:15:00', '2026-08-10 12:00:00', 210.00),
    ('Japan Airlines', 'JL408', '2026-09-01 13:00:00', '2026-09-02 08:00:00', 850.00),
    ('Austrian Airlines', 'OS201', '2026-12-20 07:30:00', '2026-12-20 08:25:00', 120.00),
    ('Iberia', 'IB3125', '2027-04-03 09:45:00', '2027-04-03 12:35:00', 240.00);

INSERT INTO buchung
    (kunde_id, reise_id, buchungsdatum, status)
VALUES
    (1, 1, '2026-02-01 10:15:00', 'abgeschlossen'),
    (2, 2, '2026-02-03 14:30:00', 'bestätigt'),
    (3, 3, '2026-02-05 09:00:00', 'abgeschlossen'),
    (4, 4, '2026-02-07 16:45:00', 'abgeschlossen'),
    (5, 5, '2026-02-10 11:20:00', 'abgeschlossen');

INSERT INTO buchung_person
    (buchung_id, vorname, nachname)
VALUES
    (1, 'Anna', 'Huber'),
    (1, 'David', 'Huber'),
    (2, 'Lukas', 'Bauer'),
    (3, 'Sophie', 'Wagner'),
    (4, 'Max', 'Gruber');

INSERT INTO reise_unterkunft (reise_id, unterkunft_id, naechte) VALUES
    (1, 1, 7),
    (2, 2, 4),
    (3, 3, 11),
    (4, 4, 7),
    (5, 5, 10);

INSERT INTO reise_flug (reise_id, flug_id, reihenfolge) VALUES
    (1, 1, 1),
    (2, 2, 1),
    (3, 3, 1),
    (4, 4, 1),
    (5, 5, 1);

INSERT INTO zahlung
    (buchung_id, zahlungsdatum, betrag, zahlungsmethode)
VALUES
    (1, '2026-02-01 10:20:00', 899.00, 'Kreditkarte'),
    (2, '2026-02-03 14:35:00', 649.00, 'PayPal'),
    (3, '2026-02-05 09:05:00', 2199.00, 'Überweisung'),
    (4, '2026-02-07 16:50:00', 300.00, 'Debitkarte'),
    (5, '2026-02-10 11:25:00', 1499.00, 'Kreditkarte');



-- Abfragen

SELECT
    kunde.vorname,
    kunde.nachname,
    buchung.buchung_id
FROM kunde
JOIN buchung
    ON kunde.kunde_id = buchung.kunde_id;

SELECT
    buchung.buchung_id,
    reise.bezeichnung,
    reise.zielland
FROM buchung
JOIN reise
    ON buchung.reise_id = reise.reise_id;

-- 3. Anzahl der Buchungen pro Reise anzeigen
SELECT
    reise.bezeichnung,
    COUNT(buchung.buchung_id) AS anzahl_buchungen
FROM reise
LEFT JOIN buchung
    ON reise.reise_id = buchung.reise_id
GROUP BY reise.reise_id, reise.bezeichnung;