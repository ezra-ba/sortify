# Optionale KI-Sortiervorschläge

Die lokale Regelengine bleibt unabhängig vom KI-Dienst. Unter Einstellungen → KI-Unterstützung stehen drei Modi zur Auswahl:

- **Aus** (Standard): keine KI-Anfragen.
- **Bei fehlendem eindeutigen Regeltreffer**: KI ergänzt einen fehlenden oder mehrdeutigen Vorschlag.
- **Ergänzend**: Auch bei einem lokalen Treffer kann eine separate KI-Alternative angeboten werden. Der lokale Treffer bleibt ausgewählt.

Zusätzlich muss die Übertragung der Metadaten ausdrücklich erlaubt werden. Der API-Schlüssel wird aus `NVIDIA_API_KEY` gelesen, nicht in den Sortify-Einstellungen gespeichert. Nach dem Setzen der Umgebungsvariable Sortify neu starten. Niemals Schlüssel ins Repository aufnehmen oder mit einer ausgelieferten EXE verteilen.

## Kontext und Antwort

Der Prompt enthält drei synthetische Beispiele, alle Regeln einschließlich Aktivierungs- und Gültigkeitsstatus, alle Ziel-IDs und Spitznamen sowie Dateiname, Endung, Größe, Änderungszeit und das lokale Regelergebnis. Dateiinhalte und absolute Pfadfelder werden nicht übertragen. Frei eingegebene Namen oder Regelmuster können trotzdem vertrauliche Angaben enthalten; diese Daten gehen an NVIDIA.

Verwendet wird `nvidia/nemotron-3.5-super-vl-120b-a12b` über `https://integrate.api.nvidia.com/v1/chat/completions`, ohne Streaming und mit deaktiviertem Thinking. Die Antwort muss JSON mit `status`, `targetFolderId`, `proposedFileName`, `reason` und `appliedRuleIds` enthalten. Bei Unsicherheit werden Ziel und Dateiname als `null` zurückgegeben.

Sortify prüft Zielzugehörigkeit, portablen Dateinamen, unveränderte Dateiendung und genannte Regel-IDs lokal. Eine KI-Antwort führt niemals selbst eine Dateioperation aus. Änderungen erfordern weiterhin die Bestätigung im Popup. Manuelle Eingaben während einer Anfrage werden nicht überschrieben. Schließen, Bestätigen oder Ändern der KI-Konfiguration verwirft eine laufende Antwort.

Anfragen sind auf insgesamt 25 Sekunden einschließlich Wartezeit und eine gleichzeitige Anfrage beschränkt. Es gibt keine automatischen Wiederholungen. Vollständige Anfragen über 128 KiB werden nicht übertragen, Antworten über 64 KiB verworfen. Bei Fehlern bleibt die lokale Bedienung verfügbar.

## Lokale HTTP-Prüfung

Der lokale Vertrag unter `postman/contracts` beschreibt den verwendeten Ausschnitt der NVIDIA-Schnittstelle. Die Simulation sendet nichts an NVIDIA und akzeptiert ausschließlich einen festen Testschlüssel. Postman CLI ab 1.70 ist erforderlich.

```powershell
postman simulation run postman/simulations/sortify-ai.sim.yaml --port 4917 --no-history
```

In einem zweiten Terminal:

```powershell
$env:SORTIFY_TEST_AI_ENDPOINT = 'http://127.0.0.1:4917/nvidia/v1/chat/completions'
dotnet test Sortify.sln --configuration Release
```

Die Simulation danach mit Strg+C beenden. Ohne diese Testvariable wird der HTTP-Simulationstest ausdrücklich übersprungen. Der Produktionsclient verwendet weiterhin den festen NVIDIA-Endpunkt; die Testvariable wird von der App nicht ausgewertet.

Getestete Antworten: Erfolg, HTTP 401/429/500, ungültiges JSON, zu große und abgeschnittene Antworten, Zeitüberschreitung und Abbruch. Die Simulation bestätigt das HTTP-Verhalten, nicht die Qualität echter Modellantworten. Ein Live-Test mit eigenem Schlüssel steht noch aus.

Referenz: [NVIDIA Nemotron-Modell und API](https://build.nvidia.com/nvidia/nemotron-3.5-super-vl-120b-a12b).
