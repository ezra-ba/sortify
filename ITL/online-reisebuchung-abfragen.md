#itl

# Einfache SQL-Abfragen – Online-Reisebuchung

```sql
USE online_reisebuchung;

-- 1. Kunden und ihre Buchungen anzeigen
SELECT
    kunde.vorname,
    kunde.nachname,
    buchung.buchung_id
FROM kunde
JOIN buchung
    ON kunde.kunde_id = buchung.kunde_id;

-- 2. Buchungen mit dem jeweiligen Reiseziel anzeigen
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
```
