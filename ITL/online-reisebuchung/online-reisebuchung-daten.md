#itl

# SQL-Script – Beispieldaten für Online-Reisebuchung

```sql
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
```

Hinweis: `buchung_person` enthält sechs Datensätze, weil die erste Buchung zwei Reisende umfasst. Damit wird die Mehrpersonenbuchung demonstriert.
