```json
{
  "status": "success",
  "data": {
    "id": 42,
    "titel": "Die Reise"
  }
}
```

```text
Content-Type: application/json; charset=utf-8
```

```json
{
  "status": "error",
  "code": "VALIDATION_ERROR",
  "message": "Der Titel darf nicht leer sein.",
  "fields": {
    "titel": "Pflichtfeld"
  }
}
```

```text
Accept: application/json
```

```json
{
  "id": 42,
  "typ": "film",
  "titel": "Die Reise",
  "beschreibung": "Ein Abenteuerfilm.",
  "erscheinungsdatum": "2026-05-12",
  "laufzeitMinuten": 120,
  "altersfreigabe": 12,
  "genres": [
    { "id": 1, "name": "Abenteuer" },
    { "id": 2, "name": "Drama" }
  ],
  "streamingDienste": [
    { "id": 3, "name": "StreamingHeld", "zugriffsart": "Abo" }
  ]
}
```

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "type": "object",
  "required": ["id", "titel", "erscheinungsjahr"],
  "properties": {
    "id": { "type": "integer", "minimum": 1 },
    "titel": { "type": "string", "minLength": 1 },
    "erscheinungsjahr": { "type": "integer", "minimum": 1888 },
    "altersfreigabe": { "type": "integer", "enum": [0, 6, 12, 16, 18] }
  },
  "additionalProperties": false
}
```