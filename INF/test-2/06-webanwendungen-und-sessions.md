#flashcards/2_INF_Test_Deck/web-und-sessions

**Welchen Weg nimmt eine HTTP-Anfrage bei einer serverseitig gerenderten Filmseite?**
?
Browser sendet Request an den Webserver. Die Anwendung verarbeitet ihn, fragt bei Bedarf die DB ab und rendert HTML. Der Webserver antwortet mit Status, Headern und Inhalt. Der Browser zeigt das HTML an und lädt Ressourcen wie CSS und JavaScript. Servercode und SQL bleiben auf dem Server.
<!--SR:!2026-11-13,3,250-->

**Wie unterscheiden sich indizierte, assoziative und mehrdimensionale Arrays?**
?
Indiziert: Zugriff über Position, etwa Index 0. Assoziativ: Zugriff über Schlüssel wie `titel`. Mehrdimensional: Arrays enthalten weitere Arrays. Ein Abfrageergebnis kann eine Liste sein, deren Einträge benannte Spalten enthalten.
<!--SR:!2026-11-13,3,250-->

**Wie trennt MVC Logik und Darstellung bei einer Filmliste?**
?
Das Modell stellt Daten und Geschäftslogik bereit. Der Controller koordiniert den Ablauf, die View rendert HTML. Die View iteriert über die Filmliste. Für eine Nummerierung ab 1 kann sie bei einem Index ab 0 jeweils Index + 1 ausgeben.
<!--SR:!2026-11-13,3,250-->

**Wann GET, wann POST bei Formularen?**
?
GET für Suche, Filter und Blättern. Werte stehen in der URL, die sich teilen lässt. POST für Login und Zustandsänderungen, etwa Speichern oder Löschen. Werte stehen im Request-Body. Das `name`-Attribut bestimmt den Feldschlüssel. HTTPS schützt die Übertragung.
<!--SR:!2026-11-13,3,250-->

**Warum kann F5 einen Film doppelt speichern und wie hilft PRG?**
?
Ist POST die letzte Anfrage, kann F5 ihn erneut absenden. Bei Post/Redirect/Get speichert der Server, sendet eine Weiterleitung (z. B. 303 mit `Location`) und der Browser ruft das Ziel per GET ab. F5 wiederholt nur den GET.
<!--SR:!2026-11-13,3,250-->

**Wie entsteht SQL-Injection und wie verhindern gebundene Parameter sie?**
?
Eingaben werden direkt in SQL eingebaut. Bei einem unsicheren Login kann `admin' #` den Wert schließen und den Rest der Zeile auskommentieren. Ein Prepared Statement wie `SELECT * FROM benutzer WHERE name = ?` behandelt den separat gebundenen Namen als Wert.
<!--SR:!2026-11-12,3,250-->

**Was ist XSS und was bewirkt HTML-Escaping?**
?
Unverarbeitete Eingaben können im Browser zu ausführbarem Code werden. Ein gespeicherter Filmtitel mit einem Skript-Tag könnte beim Anzeigen ein Skript starten. Kontextgerechtes Escaping lässt den Titel als Text erscheinen.
<!--SR:!2026-11-13,3,250-->

**Warum braucht HTTP für Login und Warenkorb eine Session?**
?
HTTP ist zustandslos. Die Session ermöglicht der Anwendung, mehrere Anfragen derselben Sitzung zuzuordnen und Daten wie Benutzer-ID, Warenkorb oder Filter zu behalten.
<!--SR:!2026-11-13,3,250-->

**Wie arbeiten Session-ID, Cookie und Server-Speicher zusammen?**
?
Der Server speichert Session-Daten unter einer zufälligen ID und setzt ein Cookie mit der ID. Der Browser sendet es bei passenden Anfragen mit. Der Server lädt die zugehörigen Daten. Cookies liegen im Browser, sind veränderbar und meist auf ca. 4 KB pro Cookie begrenzt. Serverdaten haben eine eigene Lebensdauer.
<!--SR:!2026-11-12,3,250-->

**Was ist Session-Hijacking und wie schützt du dich?**
?
Wer eine gültige Session-ID stiehlt, kann die Sitzung übernehmen. HTTPS schützt den Transport, `Secure` bindet das Cookie an HTTPS und `HttpOnly` verhindert direktes Auslesen per JavaScript. Nach dem Login eine neue ID vergeben und die alte beim Logout serverseitig ungültig machen.
<!--SR:!2026-11-13,3,250-->

**Wie speichert und prüft die Anwendung den Login-Status?**
?
Nach erfolgreicher Anmeldung neue Session-ID erzeugen und Benutzer-ID serverseitig speichern. Cookie-Header vor dem Antwortinhalt setzen. Bei geschützten Seiten Session und nötige Rechte prüfen und bei fehlender Anmeldung Zugriff verweigern oder umleiten.
<!--SR:!2026-11-12,3,250-->

**Warum genügt es beim Logout nicht, nur das Cookie zu löschen?**
?
Eine kopierte ID könnte weiterhin serverseitig gültig sein. Beim Logout Session-Daten bzw. Anmeldestatus serverseitig ungültig machen und das Cookie im Browser ablaufen lassen. Einzelne Session-Werte zu entfernen lässt andere Werte bestehen.
<!--SR:!2026-11-13,3,250-->

**Wie aktiviert man Sessions in ASP.NET Core?**
?
In `Program.cs` Speicher mit `AddDistributedMemoryCache()` und Sessions mit `AddSession(...)` registrieren. `UseSession()` nach `UseRouting()` und vor dem Mapping der Controller aufrufen. Optionen sind etwa `Cookie.HttpOnly` und `IdleTimeout` (standardmäßig 20 Minuten).
<!--SR:!2026-11-12,3,250-->

**Wie merkt sich ASP.NET Core das zuletzt gewählte Genre?**
?
Übergebenes Genre mit `HttpContext.Session.SetString("Genre", genre)` speichern. Per `GetString("Genre") ?? "alle"` lesen. Bei „alle“ keine Filterung, sonst nach dem gespeicherten Genre filtern. Requests derselben Session verwenden den Wert weiter.
<!--SR:!2026-11-13,3,250-->

**Wie speichert ASP.NET Core Session-Werte und wie leert man sie?**
?
Die Session speichert Bytes. `SetString` und `SetInt32` helfen bei einfachen Werten, komplexe Objekte lassen sich als JSON serialisieren. Der RAM-Beispielspeicher verliert Daten beim Neustart. `Session.Clear()` leert die Daten, beim Logout kann zusätzlich das Cookie gelöscht werden.
<!--SR:!2026-11-13,3,250-->
