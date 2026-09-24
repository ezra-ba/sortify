#itl

# SQL-Script – Online-Reisebuchung

```sql
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
```
