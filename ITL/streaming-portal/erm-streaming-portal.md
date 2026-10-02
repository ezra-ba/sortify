#itl

# ERM-Diagramm – Streaming-Portal

```mermaid
erDiagram
    TITEL ||--o| FILM : "ist ein"
    TITEL ||--o| SERIE : "ist eine"
    SERIE ||--o{ STAFFEL : "hat"
    STAFFEL ||--o{ EPISODE : "enthält"

    TITEL ||--o{ TITEL_GENRE : "gehört zu"
    GENRE ||--o{ TITEL_GENRE : "ordnet zu"
    TITEL ||--o{ TITEL_PERSON : "hat Mitwirkende"
    PERSON ||--o{ TITEL_PERSON : "wirkt mit"

    TITEL ||--o{ TITEL_DIENST : "ist verfügbar bei"
    STREAMING_DIENST ||--o{ TITEL_DIENST : "bietet an"

    BENUTZER ||--o{ BEWERTUNG : "vergibt"
    TITEL ||--o{ BEWERTUNG : "erhält"
    BENUTZER ||--o{ WATCHLIST : "führt"
    TITEL ||--o{ WATCHLIST : "steht auf"

    TITEL {
        int titel_id PK
        string name
        string beschreibung
        date erscheinungsdatum
        string altersfreigabe
        string bild_url
        decimal durchschnittliche_bewertung
    }
    FILM {
        int titel_id PK, FK
        int laufzeit_minuten
    }
    SERIE {
        int titel_id PK, FK
        int anzahl_staffeln
    }
    STAFFEL {
        int staffel_id PK
        int titel_id FK
        int staffelnummer
        date erscheinungsdatum
    }
    EPISODE {
        int episode_id PK
        int staffel_id FK
        int episodenummer
        string name
        string beschreibung
        int laufzeit_minuten
        date erscheinungsdatum
    }
    GENRE {
        int genre_id PK
        string name UK
    }
    TITEL_GENRE {
        int titel_id PK, FK
        int genre_id PK, FK
    }
    PERSON {
        int person_id PK
        string vorname
        string nachname
    }
    TITEL_PERSON {
        int titel_id PK, FK
        int person_id PK, FK
        string rolle PK
        string figurname
    }
    STREAMING_DIENST {
        int dienst_id PK
        string name UK
        string website
    }
    TITEL_DIENST {
        int titel_id PK, FK
        int dienst_id PK, FK
        string zugriffsart PK
        date verfuegbar_ab
        date verfuegbar_bis
    }
    BENUTZER {
        int benutzer_id PK
        string benutzername UK
        string email UK
        string rolle
        date registrierungsdatum
    }
    BEWERTUNG {
        int benutzer_id PK, FK
        int titel_id PK, FK
        int sterne
        string kommentar
        datetime bewertet_am
    }
    WATCHLIST {
        int benutzer_id PK, FK
        int titel_id PK, FK
        datetime hinzugefuegt_am
        string status
    }
```

## Erste Entwurfsannahmen

- `TITEL` enthält die gemeinsamen Daten von Filmen und Serien. `FILM` und `SERIE` ergänzen die jeweils speziellen Daten.
- Eine Serie besitzt mehrere Staffeln, eine Staffel mehrere Episoden. Filme besitzen keine Staffeln oder Episoden.
- `TITEL_PERSON` löst die n:m-Beziehung zwischen Titeln und Personen auf. Über `rolle` kann eine Person beispielsweise als Schauspieler oder Regisseur zugeordnet werden; `figurname` ist nur bei Schauspielrollen relevant.
- `TITEL_DIENST` löst die n:m-Beziehung zwischen Titeln und Streaming-Diensten auf. `zugriffsart` kann beispielsweise `Abo`, `Leihen` oder `Kaufen` enthalten.
- `BEWERTUNG` und `WATCHLIST` verwenden jeweils einen zusammengesetzten Primärschlüssel aus Benutzer und Titel. Dadurch kann ein Benutzer einen Titel nur einmal bewerten und nicht doppelt auf die Watchlist setzen.
- `BENUTZER.rolle` unterscheidet zwischen normalen Benutzern (`user`) und Administratoren (`admin`). Zulässig sind ausschließlich diese beiden Werte.
- `durchschnittliche_bewertung` in `TITEL` ist als abgeleiteter Wert gedacht. Für eine strengere Normalisierung sollte dieser Wert nicht gespeichert, sondern mit SQL aus `BEWERTUNG` berechnet werden.

## Noch zu prüfen

- Ob Episoden ebenfalls bewertet, gesucht oder in die Watchlist aufgenommen werden sollen.
- Welche zusätzlichen Filter das Mockup genau benötigt, zum Beispiel Sprache, Land oder Erscheinungsjahr als eigener Filterbereich.
- Ob `zugriffsart` als eigene Tabelle modelliert werden soll, falls die Werte zentral verwaltet werden müssen.
