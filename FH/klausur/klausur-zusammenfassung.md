#fh

# Klausurzusammenfassung: Softwarequalität, Testing, CI/CD und Betrieb

Diese Zusammenfassung bündelt die Inhalte der Unterlagen im Ordner `FH/klausur`. Die Themen sind nach Sachgebieten geordnet. Zusätzliche Erklärungen, die über die vorhandenen Notizen hinausgehen, sind mit **ERGÄNZT** gekennzeichnet.

## 1. Softwarequalität und Fehlerbegriffe

### 1.1 Formale Verifikation und Testen

Software kann auf unterschiedliche Arten auf Korrektheit untersucht werden:

- **Formale Verifikation:** Die Korrektheit eines Programms wird mathematisch gegenüber einer Spezifikation bewiesen. Das ist besonders aufwendig und wird vor allem bei kritischen oder sicherheitsrelevanten Systemen eingesetzt.
- **Dynamisches Testen:** Das Programm wird mit konkreten Eingaben ausgeführt. Tests können zeigen, dass bestimmte Fälle funktionieren oder dass ein Fehler existiert. Sie können aber niemals beweisen, dass ein Programm vollständig fehlerfrei ist.

**ERGÄNZT:** Ein Test liefert immer nur Evidenz für die geprüften Eingaben und Ausführungspfade. Je besser die Testfälle ausgewählt sind, desto größer ist die gefundene Fehlerwahrscheinlichkeit; eine vollständige Korrektheitsgarantie entsteht daraus trotzdem nicht.

### 1.2 Error, Fault, Failure und Incident

Die Begriffe beschreiben verschiedene Stufen eines Fehlers:

1. **Error:** Eine menschliche Handlung oder Fehlentscheidung, zum Beispiel ein Missverständnis der Anforderungen oder ein Programmierfehler.
2. **Fault:** Die fehlerhafte Stelle oder Ausprägung im Quellcode beziehungsweise im Softwaresystem.
3. **Failure:** Das tatsächlich beobachtbare Fehlverhalten des Systems, bei dem die Spezifikation nicht erfüllt wird.
4. **Incident:** Ein vom Benutzer oder vom Betrieb wahrgenommenes Ereignis beziehungsweise Symptom eines Fehlers.

Die Kette lässt sich vereinfacht so darstellen:

`Error → Fault → Failure → Incident`

Fehler sollen möglichst früh entdeckt werden. Je später ein Fehler gefunden wird, desto mehr abhängige Arbeit kann bereits darauf aufbauen und desto teurer wird die Korrektur.

### 1.3 Testebenen

- **Unit-Tests:** Prüfen eine kleine, abgegrenzte Einheit, häufig eine einzelne Methode oder Klasse. Sie sind schnell und können sehr häufig ausgeführt werden.
- **Integrationstests:** Prüfen das Zusammenspiel mehrerer Einheiten, Komponenten, Schichten oder externer Systeme.
- **Systemtests:** Prüfen das vollständige System gegen die Anforderungen und Spezifikation. Sie sind meist langsamer und aufwendiger.

Die Testpyramide empfiehlt viele schnelle Unit-Tests, weniger Integrationstests und wenige umfassende Systemtests. Dadurch erhält man schnelles Feedback und hält die Gesamtausführungszeit beherrschbar.

## 2. Unit Testing

### 2.1 Zweck und Eigenschaften

Ein Unit-Test prüft eine klar abgegrenzte Einheit isoliert. Die Einheit wird als **SUT (System Under Test)** bezeichnet. Ein guter Unit-Test:

- prüft einen verständlichen fachlichen Aspekt,
- liefert schnelles Feedback,
- ist unabhängig von anderen Tests,
- ist wiederholbar und deterministisch,
- schützt vor Regressionen,
- unterstützt Refactorings,
- dokumentiert das erwartete Verhalten ausführbar.

Unit-Tests prüfen nicht nur direkte Rückgabewerte. Je nach Testfall können auch Exceptions, Zustandsänderungen und fachlich relevante Interaktionen mit Kollaboratoren überprüft werden.

### 2.2 Testbarkeit

**Testbarkeit** beschreibt, wie leicht ein System isoliert und zuverlässig getestet werden kann. Sie ist ein wichtiges Qualitätsmerkmal der Wartbarkeit.

Schlechte Testbarkeit entsteht unter anderem durch:

- starke Kopplung zwischen Klassen,
- große Klassen mit vielen Verantwortlichkeiten,
- versteckte oder globale Zustände,
- direkte Abhängigkeiten zu Datenbank, Netzwerk, Uhrzeit oder Dateisystem,
- unflexible Initialisierung,
- nicht beobachtbare Ergebnisse,
- schwer ersetzbare Abhängigkeiten.

Ein hoher Aufwand beim Einrichten eines Tests oder eine große Zahl notwendiger Test-Doubles sind Warnzeichen. Testgetriebene Entwicklung unterstützt ein testbares Design, weil der Code von Beginn an über eine gut nutzbare Schnittstelle getestet wird.

### 2.3 SOLID als Grundlage für testbares Design

- **Single Responsibility Principle (SRP):** Eine Klasse soll nur eine wesentliche Verantwortung beziehungsweise einen Grund für Änderungen haben. Kleine, klar abgegrenzte Verantwortlichkeiten sind leichter zu testen.
- **Open/Closed Principle (OCP):** Ein Modul soll für Erweiterungen offen und für Änderungen am bestehenden Verhalten möglichst geschlossen sein. Abstraktionen können Tests stabiler machen.
- **Liskov Substitution Principle (LSP):** Untertypen müssen anstelle ihres Basistyps verwendet werden können, ohne grundlegende Erwartungen zu verletzen.
- **Interface Segregation Principle (ISP):** Clients sollen nur von Schnittstellen abhängen, die sie tatsächlich benötigen. Kleine Schnittstellen sind leichter durch Test-Doubles zu ersetzen.
- **Dependency Inversion Principle (DIP):** Übergeordnete und untergeordnete Module hängen von Abstraktionen ab. Durch Dependency Injection können konkrete Abhängigkeiten im Test ersetzt werden.

### 2.4 FIRST-Regeln

Die FIRST-Regeln beschreiben wesentliche Eigenschaften guter Unit-Tests:

- **Fast:** Tests laufen schnell.
- **Independent:** Tests beeinflussen sich nicht gegenseitig.
- **Repeatable:** Tests liefern in jeder Umgebung dasselbe Ergebnis.
- **Self-validating:** Jeder Test entscheidet selbstständig über Erfolg oder Fehlschlag.
- **Timely:** Tests werden zeitnah zum Produktionscode geschrieben und fördern dadurch ein testbares Design.

### 2.5 AAA-Muster

Der typische Aufbau eines Tests folgt dem AAA-Muster:

1. **Arrange:** SUT, Eingaben und Abhängigkeiten vorbereiten.
2. **Act:** Die zu testende Funktion genau einmal ausführen.
3. **Assert:** Erwartetes Ergebnis oder erwartete Interaktion prüfen.

Die Trennung macht Tests leichter lesbar und hilft, die Ursache eines Fehlschlags zu erkennen. Ein Test sollte möglichst einen fachlichen Aspekt prüfen und klare, aussagekräftige Assertions besitzen.

### 2.6 Testnamen und Testdaten

Testnamen sollten beschreiben:

- was getestet wird,
- unter welchen Bedingungen der Test ausgeführt wird,
- welches Ergebnis erwartet wird.

Ein geeignetes Schema ist zum Beispiel `Add_WithTwoNumbers_ReturnsSum`. Für Testcode ist Lesbarkeit wichtiger als maximale Vermeidung von Wiederholung. Testcode darf deshalb beschreibend und bewusst etwas redundant sein.

Bei der Auswahl von Testdaten helfen zwei Verfahren:

- **Äquivalenzklassenbildung:** Eingaben werden in Gruppen eingeteilt, die voraussichtlich gleich verarbeitet werden. Aus jeder gültigen und ungültigen Klasse wird mindestens ein repräsentativer Wert getestet.
- **Grenzwertanalyse:** Zusätzlich werden die Grenzen der Klassen geprüft, weil dort besonders häufig Fehler auftreten. Bei einem erlaubten Bereich von 18 bis 70 sind beispielsweise 17, 18, 70 und 71 wichtige Testwerte.

Weitere wichtige Fälle sind leere Eingaben, `null`, minimale und maximale Werte, ungültige Formate, doppelte Werte sowie unerwartete Reihenfolgen.

### 2.7 xUnit für .NET

xUnit ist ein Unit-Testing-Framework für .NET.

- **`[Fact]`:** Einzelner Test ohne Parameter.
- **`[Theory]`:** Parametrisierter Test.
- **`[InlineData]`:** Konkrete Datenzeilen für einen Theory-Test.
- **`[MemberData]`:** Testdaten aus einer separaten Eigenschaft oder Methode; geeignet für größere oder dynamisch erzeugte Datenmengen.
- **`[ClassData]`:** Testdaten aus einer eigenen Datenklasse.
- **`[Trait]`:** Kategorisierung von Tests.
- **`Skip`:** Vorübergehendes Überspringen eines Tests.
- **`DisplayName`:** Lesbarer Anzeigename.
- **`ITestOutputHelper`:** Zusätzliche Testausgabe.

Wichtige Assertions sind unter anderem:

- `Assert.Equal` für erwarteten und tatsächlichen Wert,
- `Assert.True` und `Assert.False` für Bedingungen,
- `Assert.Contains` für enthaltene Werte,
- `Assert.Throws` für einen exakt erwarteten Ausnahmetyp,
- `Assert.ThrowsAny` für einen Ausnahmetyp oder davon abgeleitete Typen,
- `Assert.NotSame` für die Prüfung, dass zwei Variablen nicht dieselbe Objektinstanz referenzieren.

Eine Lambda-Funktion wird häufig verwendet, um den auszuführenden Code an `Assert.Throws` zu übergeben, zum Beispiel `() => MethodeMitFehler()`.

Fixtures ermöglichen gemeinsame Vorbereitung und Aufräumarbeiten. Der Konstruktor und `Dispose` dienen dem Lebenszyklus einer Testklasse. Mit `IClassFixture` kann eine Fixture innerhalb einer Testklasse geteilt werden, mit `ICollectionFixture` innerhalb einer Testsammlung. Unit-Tests sollten grundsätzlich nicht von einer bestimmten Ausführungsreihenfolge abhängen.

### 2.8 Besondere Begriffe aus den Beispielen

- **ExplorationTests:** Tests zum praktischen Erkunden einer unbekannten Bibliothek oder API. Sie halten fest, wie sich eine Funktion tatsächlich verhält.
- **`yield return`:** Gibt bei einem Iterator schrittweise Werte zurück. Die Ausführung wird beim nächsten Zugriff an der betreffenden Stelle fortgesetzt.
- **Permutation:** Eine mögliche Anordnung aller Elemente einer Menge. Bei drei Elementen gibt es sechs Permutationen.
- **TSPSolver:** Eine Klasse oder Methode zur Lösung des Problems des Handlungsreisenden. Gesucht wird eine möglichst kurze Rundreise, die mehrere Orte genau einmal besucht und zum Ausgangspunkt zurückkehrt.

## 3. Test Doubles

### 3.1 Zweck

Test-Doubles ersetzen Kollaboratoren des SUT. Dadurch kann eine Einheit isoliert getestet werden, ohne echte Datenbanken, Netzwerke, Dateisysteme oder andere langsame beziehungsweise unzuverlässige Systeme verwenden zu müssen.

Die richtige Wahl hängt davon ab, ob ein Test Daten benötigt, Zustände simulieren oder Interaktionen prüfen soll.

### 3.2 Arten von Test-Doubles

- **Dummy:** Wird nur übergeben, damit eine Schnittstelle erfüllt ist. Der Wert wird im Test nicht verwendet.
- **Stub:** Liefert für bestimmte Aufrufe vorbereitete Rückgabewerte.
- **Spy:** Zeichnet zusätzlich auf, welche Interaktionen stattgefunden haben. Ein Spy kann damit teilweise wie ein Stub und teilweise wie eine Verhaltensaufzeichnung funktionieren.
- **Fake:** Eine vereinfachte, aber funktionierende Implementierung, beispielsweise eine Datenbank im Arbeitsspeicher.
- **Mock:** Ein Ersatzobjekt, mit dem erwartete Interaktionen definiert und verifiziert werden.

Ein Mock kann sich technisch wie ein Stub oder Dummy verhalten. Entscheidend ist seine Verwendung: Beim Mock steht die Prüfung des Verhaltens beziehungsweise der Interaktionen im Mittelpunkt.

### 3.3 Zustands- und Verhaltensprüfung

Bei der **State Verification** wird der Zustand nach der Ausführung geprüft. Entscheidend ist das fachliche Ergebnis, nicht der interne Weg dorthin.

Bei der **Behavior Verification** wird geprüft, ob bestimmte Interaktionen mit einem Kollaborator stattgefunden haben. Dafür werden meist Mocks oder Spies verwendet.

Zustandsprüfung ist häufig stabiler, weil sie sich am sichtbaren Ergebnis orientiert. Verhaltensprüfung kann notwendig sein, bindet den Test aber stärker an die konkrete Implementierung. Zu viele Interaktionsprüfungen führen daher schnell zu fragilen Tests.

### 3.4 Moq für .NET

Moq ist ein Mocking-Framework für .NET. Der typische Ablauf ist:

1. Mock-Objekt erstellen.
2. Falls erforderlich, Methoden oder Eigenschaften mit `Setup` vorbereiten.
3. SUT ausführen.
4. Mit `Verify` nur die fachlich relevanten Interaktionen prüfen.

Beispiele:

```csharp
mock.Setup(x => x.ReadPersonById(1))
    .Returns(new Person("a"));
```

Mit Argument-Matchern lassen sich allgemeinere Regeln formulieren:

```csharp
mock.Setup(x => x.ReadPersonById(It.IsAny<int>()))
    .Returns(new Person("a"));
```

Weitere Matcher sind beispielsweise `It.Is<T>(...)`, `It.IsIn`, `It.IsNotIn`, `It.IsInRange`, `It.IsNotNull` und `It.IsRegex`.

Mit `SetupSequence` können mehrere aufeinanderfolgende Rückgabewerte festgelegt werden. Mit `Returns((int id) => ...)` kann der Rückgabewert aus dem Argument berechnet werden. Mit `Throws` lässt sich das Fehlerverhalten einer Abhängigkeit simulieren.

Mit `Verify` lassen sich Aufrufe und ihre Häufigkeit prüfen, zum Beispiel:

- `Times.Exactly(n)`,
- `Times.AtLeastOnce()`,
- `Times.AtLeast(n)`,
- `Times.AtMost(n)`,
- `Times.Never`.

`VerifyNoOtherCalls()` sollte sparsam eingesetzt werden, weil es Tests unnötig eng an die Implementierung binden kann.

Ein **Loose-Mock** liefert für nicht vorbereitete Aufrufe Standardwerte wie `null`, `0` oder ein leeres Ergebnis. Ein **Strict-Mock** wirft bei einem nicht vorbereiteten Aufruf eine Exception. Strict-Mocks machen unerwartete Interaktionen sichtbar, können aber ebenfalls zu stark an konkrete Abläufe koppeln.

Bei einem Spy bleibt grundsätzlich eine vorhandene Implementierung aktiv, während zusätzlich Interaktionen aufgezeichnet werden. Mit `CallBase = true` werden nicht gestubbte Methoden der Basisklasse ausgeführt.

### 3.5 Typische Gefahren

- **Oververification:** Zu viele oder zu genaue Erwartungen prüfen Implementierungsdetails statt fachlichen Nutzen.
- **Mocks überall:** Eine echte, einfache Klasse ist oft verständlicher als ein Mock.
- **Transitives Mocking:** Ein Mock liefert einen weiteren Mock zurück. Das ist meist schwer lesbar und deutet auf zu starke Kopplung hin.
- **Zu viele Test-Doubles:** Ein sehr kompliziertes Arrange deutet häufig auf ein problematisches Produktionsdesign hin.

## 4. Test Driven Development und Testqualität

### 4.1 TDD-Zyklus

Test Driven Development folgt dem kurzen Zyklus **Red – Green – Refactor**:

1. Einen Test schreiben, der zunächst fehlschlägt.
2. Nur so viel Produktionscode schreiben, dass der Test erfolgreich wird.
3. Den Code bei weiterhin grünen Tests verbessern und vereinfachen.

Die drei Gesetze werden oft so formuliert:

- Kein Produktionscode ohne zuvor fehlschlagenden Test.
- Nur so viel Testcode schreiben, wie nötig ist, um einen Fehlschlag zu erzeugen.
- Nur so viel Produktionscode schreiben, wie nötig ist, um den aktuellen Test zu erfüllen.

TDD unterstützt eine klare API und testbares Design. Es ersetzt aber nicht die fachliche Auswahl sinnvoller Testfälle.

### 4.2 Test-Smells

Testcode muss wie Produktionscode gepflegt werden. Schlechte Tests werden langsam, schwer verständlich und teuer. Typische Code-Smells sind:

- **Obscure tests:** Der Zweck des Tests ist nicht sofort erkennbar.
- **Conditional test logic:** Der Test enthält Bedingungen, wodurch Teile manchmal nicht ausgeführt werden.
- **Hard-to-test code:** Das Produktionsdesign lässt sich nur schwer isolieren.
- **Test code duplication:** Testlogik wird unkontrolliert kopiert.
- **Test logic in production:** Produktionscode enthält Logik, die nur für Tests benötigt wird.

Typische Verhaltens-Smells sind:

- **Assertion roulette:** Bei vielen Assertions ist unklar, welche den Fehlschlag ausgelöst hat.
- **Erratic tests:** Tests bestehen oder scheitern scheinbar zufällig.
- **Fragile tests:** Harmloses Refactoring führt zu Fehlschlägen.
- **Frequent debugging:** Fehlschläge erfordern regelmäßig manuelle Fehlersuche.
- **Manual intervention:** Tests benötigen manuelle Eingriffe.
- **Slow tests:** Lange Laufzeiten verhindern häufiges Ausführen.

## 5. Continuous Integration und Continuous Delivery

### 5.1 Grundidee

**Continuous Integration (CI)** bedeutet, dass Änderungen regelmäßig in eine gemeinsame Codebasis integriert und automatisiert überprüft werden. Ziel ist, Integrationsprobleme früh zu erkennen, statt sie am Ende eines Projekts gesammelt zu entdecken.

**Continuous Delivery** erweitert diesen Ansatz um die automatisierte Vorbereitung einer auslieferbaren Version. Der Code soll nach erfolgreicher Pipeline grundsätzlich in einem Zustand sein, in dem er kontrolliert bereitgestellt werden kann.

**Continuous Deployment** geht noch einen Schritt weiter: Eine erfolgreiche Pipeline kann die Bereitstellung automatisch auslösen.

**ERGÄNZT:** Delivery bedeutet nicht zwingend eine automatische Veröffentlichung für Endbenutzer. Es bedeutet vor allem, dass der Auslieferungsprozess technisch vorbereitet und reproduzierbar ist. Deployment kann zusätzlich eine manuelle Freigabe oder einen fachlichen Kontrollschritt enthalten.

### 5.2 Typischer Ablauf einer Pipeline

Eine CI/CD-Pipeline besteht aus aufeinanderfolgenden Stufen. Ein typischer Ablauf ist:

1. **Checkout:** Quellcode und zugehörige Konfiguration werden aus der Versionsverwaltung geladen.
2. **Restore:** Abhängigkeiten werden aufgelöst und geladen.
3. **Build:** Der Quellcode wird kompiliert oder in ein auslieferbares Paket umgewandelt.
4. **Unit-Tests:** Schnelle Tests prüfen einzelne Einheiten.
5. **Integrationstests:** Das Zusammenspiel wichtiger Komponenten wird geprüft.
6. **Packaging:** Ein reproduzierbares Artefakt wird erzeugt.
7. **Deployment:** Das Artefakt wird in eine Zielumgebung gebracht.
8. **Smoke- oder Health-Checks:** Grundlegende Funktionen und Erreichbarkeit werden überprüft.
9. **Monitoring:** Metriken, Logs und Zustände werden nach der Bereitstellung beobachtet.

Schlägt eine frühe Stufe fehl, sollten spätere Stufen nicht automatisch als erfolgreich gelten. Ein fehlerhaftes Ergebnis soll möglichst schnell und deutlich sichtbar werden.

### 5.3 Build und Artefakte

Ein Build wandelt den Quellcode in ein auslieferbares Ergebnis um. Das Ergebnis kann beispielsweise ein Paket, ein Container-Image oder ein komprimiertes Release-Archiv sein.

Ein gutes Artefakt ist:

- eindeutig identifizierbar,
- reproduzierbar erzeugt,
- vollständig mit den benötigten Dateien,
- unabhängig von einer Entwicklerumgebung,
- nach dem Build unverändert zwischen den Umgebungen weiterverwendbar.

**ERGÄNZT:** Wenn jede Umgebung das Artefakt erneut baut, können unbemerkte Unterschiede durch Compiler, Abhängigkeiten oder Konfiguration entstehen. Besser ist es, ein geprüftes Artefakt zu erzeugen und dieses kontrolliert weiterzugeben.

### 5.4 Vorteile und Risiken von CI/CD

Vorteile:

- frühes Feedback,
- kleinere und leichter nachvollziehbare Änderungen,
- reproduzierbare Abläufe,
- weniger manuelle Fehler,
- schnellere Rückmeldung an Entwickler,
- bessere Auslieferbarkeit.

Risiken:

- eine unzuverlässige Pipeline wird ignoriert,
- zu lange Laufzeiten führen zu Umgehungen,
- fehlende Testdaten oder instabile externe Systeme verursachen Scheinausfälle,
- Geheimnisse dürfen nicht im Quellcode oder in Logs landen,
- die Pipeline selbst muss wie produktiver Code gepflegt werden.

### 5.5 Gute Pipeline-Eigenschaften

Eine gute Pipeline ist schnell, deterministisch, transparent und wiederholbar. Dazu gehören:

- klare Stufen mit eindeutigen Verantwortlichkeiten,
- aussagekräftige Fehlermeldungen,
- getrennte Konfiguration für Umgebungen,
- kontrollierter Umgang mit Zugangsdaten,
- Zwischenspeicherung nur dort, wo sie reproduzierbar bleibt,
- Testberichte und Logs als nachvollziehbare Ergebnisse,
- eine Möglichkeit, fehlgeschlagene Schritte sicher erneut auszuführen.

## 6. Deployment

### 6.1 Ziel und Grundsätze

Deployment bezeichnet das Bereitstellen einer Anwendung oder eines Artefakts in einer Zielumgebung. Dabei müssen nicht nur Dateien kopiert, sondern auch Konfiguration, Abhängigkeiten, Datenbankänderungen, Berechtigungen und Erreichbarkeit berücksichtigt werden.

Wichtige Grundsätze sind:

- Automatisierung statt manueller Einzelschritte,
- reproduzierbare Abläufe,
- kleine und nachvollziehbare Änderungen,
- Trennung von Code und umgebungsspezifischer Konfiguration,
- überprüfbare Vor- und Nachbedingungen,
- dokumentierter Rückweg bei Problemen.

### 6.2 Umgebungen

Typischerweise existieren mehrere Umgebungen:

- **Entwicklung:** Schnelles Arbeiten und lokale Überprüfung.
- **Test oder Integration:** Gemeinsame Prüfung von Komponenten.
- **Staging:** Produktionsnahe Umgebung für abschließende Prüfungen.
- **Produktion:** Für Benutzer erreichbare Umgebung.

Das Artefakt sollte möglichst unverändert durch die Umgebungen wandern. Unterschiede werden über Konfiguration und Infrastrukturparameter gesteuert.

### 6.3 Bereitstellungsstrategien

- **Direkte Bereitstellung:** Die laufende Anwendung wird unmittelbar aktualisiert. Das ist einfach, kann aber zu einer kurzen Unterbrechung oder zu Mischzuständen führen.
- **Rolling Deployment:** Instanzen werden schrittweise aktualisiert. Dadurch bleibt ein Teil des Systems verfügbar, während andere Instanzen bereits mit dem neuen Stand laufen.
- **Canary Deployment:** Eine kleine Gruppe von Benutzern oder Instanzen erhält die Änderung zuerst. Nach Beobachtung der Ergebnisse kann die Verteilung erweitert werden.

Bei schrittweisen Verfahren müssen alte und neue Anwendungsversionen vorübergehend miteinander kompatibel sein. Das betrifft besonders APIs, Nachrichtenformate und Datenbankschemata.

### 6.4 Datenbankänderungen

Datenbankänderungen sind riskanter als das Austauschen von Programmdateien, weil Daten dauerhaft gespeichert werden. Sichere Migrationen sind möglichst rückwärtskompatibel und erfolgen in kleinen Schritten.

**ERGÄNZT:** Ein bewährtes Muster ist, zuerst neue Spalten oder Tabellen hinzuzufügen, danach den Code schrittweise umzustellen und erst später nicht mehr benötigte Strukturen zu entfernen. Dadurch können alte und neue Anwendungsteile während einer Übergangsphase koexistieren.

### 6.5 Smoke- und Health-Checks

Nach einem Deployment werden grundlegende Prüfungen durchgeführt:

- Ist der Dienst erreichbar?
- Startet die Anwendung ohne Fehler?
- Funktioniert eine zentrale, ungefährliche Beispielanfrage?
- Sind abhängige Dienste erreichbar?
- Sind Logs und Metriken verfügbar?

Ein Health-Check sollte schnell und zuverlässig sein. Er soll nicht die gesamte Fachlogik ersetzen, sondern grundlegende Betriebsfähigkeit anzeigen.

## 7. Monitoring und Observability

### 7.1 Monitoring

Monitoring beobachtet den Zustand eines Systems und macht Abweichungen sichtbar. Es beantwortet vor allem die Frage, ob definierte technische oder fachliche Messwerte innerhalb akzeptabler Grenzen liegen.

Wichtige Signale sind:

- **Metriken:** Zahlenwerte über Zeit, zum Beispiel Antwortzeit, Fehlerrate, Auslastung oder Durchsatz.
- **Logs:** Zeitgestempelte Ereignisse mit Kontextinformationen.
- **Traces:** Nachvollziehbare Abläufe einer Anfrage über mehrere Komponenten hinweg.

### 7.2 Wichtige Metriken

- **Latenz:** Wie lange eine Anfrage dauert.
- **Durchsatz:** Wie viele Anfragen oder Nachrichten pro Zeit verarbeitet werden.
- **Fehlerrate:** Anteil fehlgeschlagener Anfragen.
- **Verfügbarkeit:** Anteil der Zeit, in der ein Dienst nutzbar ist.
- **Ressourcennutzung:** CPU, Arbeitsspeicher, Speicherplatz und Netzwerk.
- **Queue-Länge:** Anzahl wartender Nachrichten oder Aufgaben.

**ERGÄNZT:** Für die Benutzerwahrnehmung sind Perzentile oft aussagekräftiger als ein Durchschnittswert. Die 95. oder 99. Perzentile zeigt, wie langsam die langsamsten regulären Anfragen sind.

### 7.3 Logs

Logs sollten strukturiert, verständlich und mit ausreichend Kontext geschrieben werden. Sinnvolle Felder sind Zeitstempel, Schweregrad, Korrelations-ID, betroffene Komponente und eine kurze Fehlermeldung.

Geeignete Schweregrade sind beispielsweise `Debug`, `Information`, `Warning` und `Error`. Zugangsdaten, Tokens und andere vertrauliche Werte dürfen nicht geloggt werden.

### 7.4 Alerts

Ein Alert sollte nur ausgelöst werden, wenn eine konkrete Reaktion erforderlich ist. Gute Alerts sind:

- eindeutig,
- priorisiert,
- mit Schwellenwert oder Zeitfenster begründet,
- mit einer kurzen Handlungsanweisung versehen,
- frei von unnötigem Rauschen.

Zu viele Fehlalarme führen zu Alert Fatigue. Ein Alert ohne zuständige Person oder ohne dokumentierte Reaktion ist wenig hilfreich.

### 7.5 Observability

Observability beschreibt, wie gut sich der innere Zustand eines Systems aus seinen äußeren Ausgaben ableiten lässt. Sie stützt sich vor allem auf Metriken, Logs und Traces.

Bei der Fehlersuche helfen Korrelationen: Eine erhöhte Fehlerrate kann gemeinsam mit langen Antwortzeiten, hoher Ressourcennutzung und passenden Logeinträgen auf dieselbe Ursache hinweisen.

## 8. Prüfungsrelevante Merksätze

- Unit-Tests testen einzelne Einheiten schnell und isoliert.
- Integrationstests testen das Zusammenspiel; Systemtests testen das Gesamtsystem.
- Ein Test kann Fehler nachweisen, aber keine vollständige Fehlerfreiheit beweisen.
- Gute Tests sind schnell, unabhängig, wiederholbar, selbstvalidierend und zeitnah erstellt.
- AAA bedeutet Arrange, Act, Assert.
- Äquivalenzklassen reduzieren Testfälle; Grenzwertanalyse findet Fehler an Übergängen.
- Stubs liefern Daten, Mocks prüfen Interaktionen, Spies zeichnen Interaktionen auf, Fakes sind vereinfachte echte Implementierungen.
- State Verification prüft das Ergebnis; Behavior Verification prüft den Ablauf beziehungsweise Interaktionen.
- Übermäßige Verifikation macht Tests fragil.
- TDD folgt Red, Green, Refactor.
- CI integriert und prüft Änderungen automatisiert.
- Continuous Delivery hält ein auslieferbares Ergebnis bereit; Continuous Deployment kann die Bereitstellung automatisch auslösen.
- Ein geprüftes Artefakt sollte reproduzierbar erzeugt und kontrolliert durch Umgebungen bewegt werden.
- Monitoring nutzt Metriken, Logs und Traces, um Fehler und Verschlechterungen früh zu erkennen.
- Gute Alerts sind konkret, relevant und handlungsorientiert.
