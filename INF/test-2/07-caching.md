#flashcards/2_INF_Test_Deck/caching

**Was ist ein Cache Hit und was passiert bei einem Miss?**
?
Hit: Frischer Eintrag liegt vor. Miss: Eintrag fehlt oder ist abgelaufen. Die Anwendung lädt die Trending-Liste aus der DB, legt sie im Cache ab und liefert sie aus. Die nächste gleichartige Anfrage ist meist ein Hit.
<!--SR:!2026-11-13,3,250-->

**Welche drei Vorteile hat Caching und welchen Preis hat es?**
?
Schnellere Antworten, weniger Arbeit für Server und DB und je nach Konfiguration weiter verfügbare Daten bei kurzem Ausfall der Quelle. Die Kopie kann veraltet sein.
<!--SR:!2026-11-13,3,250-->

**Welche vier Cache-Ebenen durchläuft eine typische Anfrage?**
?
Browser → CDN → Anwendung → Datenbank. Je früher ein Treffer erfolgt, desto mehr nachgelagerte Zugriffe entfallen. Ein frischer Browser-Hit kann die Netzwerkanfrage sparen.
<!--SR:!2026-11-13,3,250-->

**Was bedeutet Cache-Control: public, max-age=604800?**
?
Gemeinsam genutzte Caches dürfen die Antwort speichern. `max-age` setzt ihre Frische auf 604 800 Sekunden, also sieben Tage.
<!--SR:!2026-11-13,3,250-->

**Wie funktioniert ein ETag nach Ablauf der Frische?**
?
Der ETag identifiziert die Version. Der Browser sendet ihn als `If-None-Match`. Ist die Ressource unverändert, kommt `304 Not Modified` ohne neuen Inhalt. Andernfalls liefert der Server die neue Ressource und einen neuen ETag.
<!--SR:!2026-11-12,3,250-->

**Was ist ein CDN und welche Portaldateien passen dazu?**
?
Ein Netz geografisch verteilter Server mit Kopien. Ein naher Standort senkt die Latenz und entlastet den Ursprung. Typisch sind Trailer, Poster, Thumbnails, Logos, CSS und JavaScript.
<!--SR:!2026-11-13,3,250-->

**Wie unterscheiden sich In-Memory-, Datenbank- und Objekt-Caches?**
?
In-Memory speichert Daten im RAM, z. B. Redis für Genrelisten. Ein DB-Abfragecache kann bei entsprechender Unterstützung wiederholte Abfrageergebnisse halten. Ein Objekt-Cache hält fertige Strukturen wie ein Filmdetail aus mehreren JOINs.
<!--SR:!2026-11-13,3,250-->

**Wie funktioniert Cache-Aside bei getTrendingMovies()?**
?
Prüfen, ob `trending.json` existiert und jünger als 600 Sekunden ist. Hit: JSON lesen und zurückgeben. Miss: zehn meistgesehene Filme aus der DB holen, als JSON speichern und zurückgeben. Die Anwendung füllt den Cache bei Bedarf.
<!--SR:!2026-11-13,3,250-->

**Was ist Cache-Invalidierung und warum ist sie schwierig?**
?
Alte Kopien löschen oder als ungültig markieren. Die Quelle ändert sich unabhängig von den Kopien. Nach einem Posterwechsel können Browser, CDN und Anwendung jeweils noch die alte Fassung haben.
<!--SR:!2026-11-13,3,250-->

**Wann setzt du TTL, gezieltes Löschen oder Versionierung ein?**
?
TTL (Time to Live) lässt Einträge automatisch ablaufen, z. B. Trending nach einigen Minuten. Gezieltes Löschen bei einer Änderung, z. B. Filmdetails. Versionierung für statische Dateien, etwa `style.css?v=3`, damit Browser eine neue URL laden.
<!--SR:!2026-11-13,3,250-->

**Welche Caches invalidierst du nach Änderung einer Filmbeschreibung?**
?
Alle Einträge, die diese Beschreibung enthalten. Neben der Detailseite können das Suchergebnisse, Trending oder „Neu hinzugefügt“ sein.
<!--SR:!2026-11-13,3,250-->

**Welche Daten gehören nicht in einen gemeinsamen Cache?**
?
Passwörter, Login-Daten, Tokens und persönliche Inhalte wie Watchlist oder Profil. Nutzerspezifische Filter können in der jeweiligen Session liegen. Schnell wechselnde Werte wie Wiedergabeposition veralten rasch. Besonders geeignet sind häufig gelesene, selten geänderte Daten.
<!--SR:!2026-11-13,3,250-->
