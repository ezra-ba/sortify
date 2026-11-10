# Optionale KI-Sortiervorschläge

Die lokale Regelengine bleibt unabhängig vom KI-Dienst. Unter Einstellungen → KI-Unterstützung stehen drei Modi zur Auswahl:

- **Aus** (Standard): keine KI-Anfragen.
- **Bei fehlendem eindeutigen Regeltreffer**: KI ergänzt einen fehlenden oder mehrdeutigen Vorschlag.
- **Ergänzend**: Auch bei einem lokalen Treffer kann eine separate KI-Alternative angeboten werden. Der lokale Treffer bleibt ausgewählt.

Zusätzlich muss die Übertragung der Metadaten ausdrücklich erlaubt werden. Der API-Schlüssel wird aus `NVIDIA_API_KEY` gelesen, nicht in den Sortify-Einstellungen gespeichert. Nach dem Setzen der Umgebungsvariable Sortify neu starten. Niemals Schlüssel ins Repository aufnehmen oder mit einer ausgelieferten EXE verteilen.

Unter „Deine Sortify-Anweisung“ können bis zu 4000 Zeichen als Markdown eingegeben und ausdrücklich gespeichert werden. Sortify legt den Text als `agents.md` neben der lokalen `settings.json` ab. Beim nächsten KI-Aufruf wird der aktuelle Inhalt als `sortifyInstructions` im Dateikontext an NVIDIA gesendet. Der Text ist eine Präferenz, keine Berechtigung zur Dateioperation; gültige Ziel-IDs, Dateiendung und Bestätigung bleiben verbindlich. Änderungen brechen laufende KI-Anfragen ab. Die Anweisung selbst kann sensible Angaben enthalten und ist Teil der erlaubten Übertragung.

Die separate Option „Namen der Unterordner und Dateien“ ist standardmäßig ausgeschaltet. Wenn sie eingeschaltet ist und die allgemeine KI-Zustimmung gilt, liest Sortify beim Anfrageaufbau eine Momentaufnahme aller konfigurierten Zielordner. Übertragen werden Ziel-ID und relative Datei-/Ordnernamen, keine Dateiinhalte oder absoluten Pfade. Versteckte, System- und verknüpfte Einträge werden ausgelassen. Die Erfassung endet nach drei Ebenen, 120 Einträgen pro Ziel, 1200 Einträgen oder 24 000 Zeichen insgesamt; ein `truncated`-Flag kennzeichnet unvollständige Übersichten. Die Namen können trotzdem sensible Informationen verraten.

Zusätzlich kann ein eigenes Kontextverzeichnis, zum Beispiel `C:\Vault`, ausgewählt und separat freigegeben werden. Sortify liest dessen sichtbare Verzeichnis- und Dateinamen rekursiv bis zu 16 Ebenen. Es prüft höchstens 20 000 Einträge und bevorzugt anschließend Namen mit Begriffen aus dem aktuellen Dateinamen. Pro Anfrage werden höchstens 1200 Einträge beziehungsweise 24 000 Zeichen als relative Namen übertragen; `truncated` zeigt an, wenn die Übersicht unvollständig ist. Der KI-Dienst erhält keinen direkten Zugriff auf die Festplatte und keine Dateiinhalte. Aus der Übersicht können weiterhin nur bereits konfigurierte Zielordner ausgewählt werden. Für feinere Ziele müssen diese Unterordner als Zielordner in Sortify hinzugefügt werden. Ein ganzer großer Vault passt nicht in eine einzelne Modellanfrage.

## Kontext und Antwort

Der Prompt enthält drei synthetische Beispiele, alle Regeln einschließlich Aktivierungs- und Gültigkeitsstatus, alle Ziel-IDs und Spitznamen sowie Dateiname, Endung, Größe, Änderungszeit und das lokale Regelergebnis. Dateiinhalte und absolute Pfadfelder werden nicht übertragen. Frei eingegebene Namen oder Regelmuster können trotzdem vertrauliche Angaben enthalten; diese Daten gehen an NVIDIA.

Verwendet wird `nvidia/nemotron-3.5-super-vl-120b-a12b` über `https://integrate.api.nvidia.com/v1/chat/completions`, ohne Streaming und mit deaktiviertem Thinking. Die Antwort muss JSON mit `status`, `targetFolderId`, `proposedFileName`, `reason` und `appliedRuleIds` enthalten. Bei Unsicherheit werden Ziel und Dateiname als `null` zurückgegeben.

Sortify prüft Zielzugehörigkeit, portablen Dateinamen, unveränderte Dateiendung und genannte Regel-IDs lokal. Wenn keine eindeutige lokale Regel passt, wird ein gültiger KI-Vorschlag auch bei Unsicherheit mit Begründung vorausgewählt. Eine KI-Antwort führt niemals selbst eine Dateioperation aus. Änderungen erfordern weiterhin die Bestätigung im Popup. Manuelle Eingaben während einer Anfrage werden nicht überschrieben. Schließen, Bestätigen oder Ändern der KI-Konfiguration verwirft eine laufende Antwort. Das Popup zeigt an, wenn der KI-Modus, die Zustimmung oder ein Zielordner fehlt und meldet Dienst- oder Antwortfehler getrennt.

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
