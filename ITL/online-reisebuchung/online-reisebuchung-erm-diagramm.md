#itl

# ERM-Diagramm – Online-Reisebuchung

```mermaid
erDiagram
    KUNDE ||--o{ BUCHUNG : "tätigt"
    REISE ||--o{ BUCHUNG : "wird gebucht"
    BUCHUNG ||--|{ BUCHUNG_PERSON : "enthält"
    BUCHUNG ||--o{ ZAHLUNG : "wird bezahlt durch"
    REISE ||--o{ REISE_UNTERKUNFT : "beinhaltet"
    UNTERKUNFT ||--o{ REISE_UNTERKUNFT : "ist verfügbar bei"
    REISE ||--o{ REISE_FLUG : "beinhaltet"
    FLUG ||--o{ REISE_FLUG : "ist Bestandteil von"

    KUNDE {
        int kunde_id PK
        string vorname
        string nachname
        string email UK
        string telefon
    }
    REISE {
        int reise_id PK
        string bezeichnung
        string zielland
        date startdatum
        date enddatum
        decimal grundpreis
    }
    BUCHUNG {
        int buchung_id PK
        int kunde_id FK
        int reise_id FK
        date buchungsdatum
        string status
    }
    BUCHUNG_PERSON {
        int buchung_person_id PK
        int buchung_id FK
        string vorname
        string nachname
    }
    UNTERKUNFT {
        int unterkunft_id PK
        string name
        string typ
        int sterne
    }
    REISE_UNTERKUNFT {
        int reise_id PK, FK
        int unterkunft_id PK, FK
        int naechte
    }
    FLUG {
        int flug_id PK
        string fluggesellschaft
        string flugnummer UK
        datetime abflugzeit
        datetime ankunftszeit
        decimal preis
    }
    REISE_FLUG {
        int reise_id PK, FK
        int flug_id PK, FK
        int reihenfolge
    }
    ZAHLUNG {
        int zahlung_id PK
        int buchung_id FK
        date zahlungsdatum
        decimal betrag
        string zahlungsmethode
    }
```

## Begründung

- `REISE_UNTERKUNFT`, `REISE_FLUG` und `BUCHUNG_PERSON` lösen die n:m-Beziehungen beziehungsweise die Mehrpersonenbuchung auf.
- Reiseziel, Fluggesellschaft und Zahlungsmethode bleiben als Attribute erhalten, da für sie keine zusätzlichen Stammdaten erforderlich sind.
- Eine Buchung kann mehrere Zahlungen enthalten und gilt bei vorhandenem Zahlungseingang als abgeschlossen.
