# Dateierkennung bis zum Popup

Stand: 22.10.2026. Teilumsetzung von Issue #29 / MUSS M4.2.1.

## Aktueller Ablauf

1. Nach dem Laden der Einstellungen startet die Überwachung des gespeicherten Eingangsordners. Eine erfolgreich gespeicherte Ordnerauswahl startet sie neu, auch wenn derselbe Ordner erneut ausgewählt wird.
2. `FolderMonitor` beobachtet neue und umbenannte Einträge mit `FileSystemWatcher`. Bereits beim Start vorhandene Dateien werden nicht nachträglich angeboten. Ein zweiter Abgleich schließt die Lücke zwischen der ersten Bestandsaufnahme und dem Start der Ereignisüberwachung.
3. `.crdownload`, `.part`, `.partial`, `.tmp` und Office-Sperrdateien mit `~$` werden zurückgestellt. Nach einer Umbenennung auf den endgültigen Namen kann die Datei verarbeitet werden.
4. Die Überwachung prüft alle 250 ms Dateigröße und Änderungszeit und versucht einen exklusiven Lesezugriff. Erst nach einer Sekunde unveränderter Metadaten bei erfolgreichem Zugriff wird eine `FileDescriptor`-Beschreibung geliefert. Schreibende oder gesperrte Dateien warten weiter, während andere Kandidaten geprüft werden können.
5. `DownloadNotificationWorkflow` holt den aktuellen Zielordnerstand und fragt den Vorschlagsanbieter. Der derzeitige `LocalFileSuggestionProvider` behält den Originalnamen und wählt nur ein ausdrücklich konfiguriertes Standardziel vor. Ohne Standard bleibt die Zielauswahl leer.
6. `AvaloniaDownloadPresenter` öffnet das vorhandene Popup mit dem echten Dateinamen, der tatsächlichen Endung, Zielauswahl und Erklärung. Weitere Kandidaten warten, bis das aktive Popup geschlossen wurde.
7. Ordnerwechsel und App-Ende brechen den laufenden Ablauf ab, schließen das automatische Popup und geben die Überwachung frei.

Eine bereits gemeldete Datei wird in derselben Überwachungssitzung nicht nochmals angeboten. Änderungen an ihrem Inhalt erzeugen kein neues Popup. Nach tatsächlichem Löschen kann eine neue Datei unter demselben Pfad wieder erkannt werden.

## Umfang und Grenzen

- Nur die erste Ordnerebene wird überwacht. Die gespeicherte Option für Unterordner wird in dieser Teilumsetzung noch nicht verwendet.
- Änderungen beschränken sich auf Erkennung und Vorschau. Es werden keine Dateien umbenannt, verschoben oder gelöscht; der Einsortieren-Button ist deaktiviert. Abbrechen und Schließen verändern die Quelldatei nicht.
- Die Verfügbarkeitsprüfung ist eine Heuristik. Ein fremder Prozess kann eine Datei nach der Prüfung erneut öffnen oder verändern. Vor einer späteren Dateioperation muss der Zustand erneut geprüft werden.
- Dateien außerhalb der Kategorie-Freigabeliste werden ebenfalls als neue Dateien angezeigt. Die spätere fachliche Regelauswertung entscheidet, ob ein Sortiervorschlag zulässig ist.
- Dateisystem-Verknüpfungen werden nicht als Quelldateien verfolgt. Ein nicht verfügbarer Ordner oder ein Watcher-Fehler, einschließlich Pufferüberlauf, stoppt die Sitzung mit einer Statusmeldung. Die erneute Ordnerauswahl startet sie neu.
- Keine Überwachung bei geschlossener App, kein Autostart, keine Browser-/Teams-Integration und kein Nachholen früherer Downloads.
- Die Engine aus PR #33 ist nicht im zugrunde liegenden master enthalten und wird hier nicht dupliziert. Die Übergabe an diese Engine bleibt für den vollständigen Abschluss von #29 offen. Das Popup erhält bereits echte Dateidaten.

## Vorbereitung für weitere Vorschlagsanbieter

`IFileSuggestionProvider.CreateAsync` ist asynchron und abbrechbar. Seine Anfrage enthält ausschließlich Dateiname, Endung, Zielkennungen und Ziel-Spitznamen sowie die Kennung des optionalen Standardziels. Absolute Quell- und Zielpfade und Dokumentinhalte sind nicht Teil der Anfrage. Die lokale Koordination ordnet Zielkennungen den tatsächlich konfigurierten Pfaden zu.

Ein Vorschlag enthält den vollständigen vorgeschlagenen Dateinamen, eine optionale Zielkennung und eine Erklärung. Die Koordination prüft Namensgültigkeit, unveränderte Endung und bekannte Zielkennung. Fehler, ungültige Antworten und ein Zeitlimit von zehn Sekunden führen zum lokalen Vorschlag mit Originalname und optionalem Standardziel. Der Abbruch der gesamten Sitzung wird nicht als Anbieterfehler verschluckt.

Ein späterer KI-Adapter gehört in Infrastructure und kann diese Schnittstelle implementieren. Aktuell gibt es keine HTTP-Aufrufe, API-Konfiguration oder Schlüssel. Bevor eine externe Verarbeitung aktiviert wird, müssen API-Vertrag und Authentifizierung geklärt, Schlüssel außerhalb des Repositorys gespeichert und die Übertragung von Dateinamen und Ziel-Spitznamen bewusst aktiviert werden. Anbieterantworten dürfen nie direkt Dateioperationen auslösen. Die lokale Nutzung bleibt ohne Endpunkt möglich.

## Prüfung

![Automatisches Popup mit erkanntem DOCX-Dateinamen und Standardziel](images/download-popup.png)

Automatisierte Tests prüfen neue Dateien statt Altbestand, temporäre Downloadnamen und anschließende Umbenennung, exklusive Schreibzugriffe, Mehrfachereignisse, Abbruch und Ordnerwechsel. Workflow-Tests prüfen echte Dateierzeugung bis zum Presenter, unbekannte Ziele, ungültige Namen, geänderte Endungen, Anbieterausfall und Zeitlimit.

Ein Avalonia-Headless-Test öffnet aus einer neu erzeugten Datei das tatsächliche Popup, prüft die Dateidaten und Zielvorschau, zeigt die nächste Datei nach dem Schließen und prüft das Schließen bei Abbruch. Ein PNG der Vorschau wird im Build-Ausgabeverzeichnis der App-Tests erzeugt. Headless-Rendering prüft nicht die Windows-spezifische Fensterdekoration.

Manueller Test:

1. `dotnet run --project src/Sortify.App` starten, einen leeren Testordner auswählen und optional einen Zielordner als Standard konfigurieren.
2. Eine Datei in den Testordner kopieren. Nach kurzer Wartezeit erscheint das Popup mit dem tatsächlichen Namen und der tatsächlichen Endung.
3. Zwei Dateien hinzufügen. Das zweite Popup erscheint nach dem Schließen des ersten.
4. Eine Datei zunächst als `beispiel.pdf.crdownload` ablegen und anschließend zu `beispiel.pdf` umbenennen. Nur der endgültige Name wird angeboten.
5. Während ein Popup geöffnet ist den Überwachungsordner wechseln. Das bisherige Popup wird geschlossen; neue Dateien im alten Ordner werden nicht mehr angeboten.
6. Nach Abbrechen oder App-Ende prüfen, dass alle Quelldateien unverändert vorliegen.
