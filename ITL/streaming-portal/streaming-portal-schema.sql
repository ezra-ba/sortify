-- Datenbankschema für das Streaming-Portal
-- Grundlage: erm-streaming-portal.md

CREATE DATABASE IF NOT EXISTS streamingheld;
USE streamingheld;

CREATE TABLE IF NOT EXISTS titel (
    titel_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    beschreibung TEXT,
    erscheinungsdatum DATE,
    altersfreigabe VARCHAR(20),
    bild_url VARCHAR(500),
    erstellt_am TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS film (
    titel_id INT UNSIGNED PRIMARY KEY,
    laufzeit_minuten SMALLINT UNSIGNED NOT NULL,
    CONSTRAINT fk_film_titel
        FOREIGN KEY (titel_id) REFERENCES titel (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_film_laufzeit
        CHECK (laufzeit_minuten > 0)
);

CREATE TABLE IF NOT EXISTS serie (
    titel_id INT UNSIGNED PRIMARY KEY,
    anzahl_staffeln SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    CONSTRAINT fk_serie_titel
        FOREIGN KEY (titel_id) REFERENCES titel (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_serie_staffeln
        CHECK (anzahl_staffeln >= 0)
);

CREATE TABLE IF NOT EXISTS staffel (
    staffel_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    titel_id INT UNSIGNED NOT NULL,
    staffelnummer SMALLINT UNSIGNED NOT NULL,
    erscheinungsdatum DATE,
    CONSTRAINT uq_staffel_nummer
        UNIQUE (titel_id, staffelnummer),
    CONSTRAINT fk_staffel_serie
        FOREIGN KEY (titel_id) REFERENCES serie (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_staffel_nummer
        CHECK (staffelnummer > 0)
);

CREATE TABLE IF NOT EXISTS episode (
    episode_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    staffel_id INT UNSIGNED NOT NULL,
    episodenummer SMALLINT UNSIGNED NOT NULL,
    name VARCHAR(255) NOT NULL,
    beschreibung TEXT,
    laufzeit_minuten SMALLINT UNSIGNED,
    erscheinungsdatum DATE,
    CONSTRAINT uq_episode_nummer
        UNIQUE (staffel_id, episodenummer),
    CONSTRAINT fk_episode_staffel
        FOREIGN KEY (staffel_id) REFERENCES staffel (staffel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_episode_nummer
        CHECK (episodenummer > 0),
    CONSTRAINT chk_episode_laufzeit
        CHECK (laufzeit_minuten IS NULL OR laufzeit_minuten > 0)
);

CREATE TABLE IF NOT EXISTS genre (
    genre_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS titel_genre (
    titel_id INT UNSIGNED NOT NULL,
    genre_id INT UNSIGNED NOT NULL,
    PRIMARY KEY (titel_id, genre_id),
    CONSTRAINT fk_titel_genre_titel
        FOREIGN KEY (titel_id) REFERENCES titel (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_titel_genre_genre
        FOREIGN KEY (genre_id) REFERENCES genre (genre_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE IF NOT EXISTS person (
    person_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vorname VARCHAR(100) NOT NULL,
    nachname VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS titel_person (
    titel_id INT UNSIGNED NOT NULL,
    person_id INT UNSIGNED NOT NULL,
    rolle VARCHAR(50) NOT NULL,
    figurname VARCHAR(150),
    PRIMARY KEY (titel_id, person_id, rolle),
    CONSTRAINT fk_titel_person_titel
        FOREIGN KEY (titel_id) REFERENCES titel (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_titel_person_person
        FOREIGN KEY (person_id) REFERENCES person (person_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE IF NOT EXISTS streaming_dienst (
    dienst_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    website VARCHAR(500)
);

CREATE TABLE IF NOT EXISTS titel_dienst (
    titel_id INT UNSIGNED NOT NULL,
    dienst_id INT UNSIGNED NOT NULL,
    zugriffsart VARCHAR(20) NOT NULL,
    verfuegbar_ab DATE,
    verfuegbar_bis DATE,
    PRIMARY KEY (titel_id, dienst_id, zugriffsart),
    CONSTRAINT fk_titel_dienst_titel
        FOREIGN KEY (titel_id) REFERENCES titel (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_titel_dienst_dienst
        FOREIGN KEY (dienst_id) REFERENCES streaming_dienst (dienst_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_zugriffsart
        CHECK (zugriffsart IN ('Abo', 'Leihen', 'Kaufen')),
    CONSTRAINT chk_verfuegbarkeit
        CHECK (verfuegbar_bis IS NULL OR verfuegbar_ab IS NULL OR verfuegbar_bis >= verfuegbar_ab)
);

CREATE TABLE IF NOT EXISTS benutzer (
    benutzer_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    benutzername VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    rolle VARCHAR(10) NOT NULL DEFAULT 'user',
    registrierungsdatum DATE NOT NULL DEFAULT (CURRENT_DATE),
    CONSTRAINT chk_benutzer_rolle
        CHECK (rolle IN ('user', 'admin'))
);

CREATE TABLE IF NOT EXISTS bewertung (
    benutzer_id INT UNSIGNED NOT NULL,
    titel_id INT UNSIGNED NOT NULL,
    sterne TINYINT UNSIGNED NOT NULL,
    kommentar TEXT,
    bewertet_am DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (benutzer_id, titel_id),
    CONSTRAINT fk_bewertung_benutzer
        FOREIGN KEY (benutzer_id) REFERENCES benutzer (benutzer_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_bewertung_titel
        FOREIGN KEY (titel_id) REFERENCES titel (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_bewertung_sterne
        CHECK (sterne BETWEEN 1 AND 5)
);

CREATE TABLE IF NOT EXISTS watchlist (
    benutzer_id INT UNSIGNED NOT NULL,
    titel_id INT UNSIGNED NOT NULL,
    hinzugefuegt_am DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) NOT NULL DEFAULT 'offen',
    PRIMARY KEY (benutzer_id, titel_id),
    CONSTRAINT fk_watchlist_benutzer
        FOREIGN KEY (benutzer_id) REFERENCES benutzer (benutzer_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_watchlist_titel
        FOREIGN KEY (titel_id) REFERENCES titel (titel_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_watchlist_status
        CHECK (status IN ('offen', 'gesehen', 'entfernt'))
);

-- Durchschnittliche Bewertung eines Titels, bewusst als Abfrage statt als redundantes Feld:
-- SELECT titel_id, AVG(sterne) AS durchschnittliche_bewertung
-- FROM bewertung
-- GROUP BY titel_id;
