#fh

# Flashcards: Softwarequalität, Testing, CI/CD und Betrieb

Die Karten sind im Format **Frage → Antwort** aufgebaut und nach Themengebieten sortiert.

## Softwarequalität und Fehlerbegriffe

### Karte 1
**Frage:** Was ist formale Verifikation?

**Antwort:** Der mathematische Beweis, dass ein Programm eine Spezifikation erfüllt. Sie ist aufwendig und wird vor allem bei besonders kritischen Systemen eingesetzt.

### Karte 2
**Frage:** Was kann dynamisches Testen zeigen und was nicht?

**Antwort:** Es kann zeigen, dass konkrete Eingaben funktionieren oder dass ein Fehler existiert. Es kann jedoch nicht beweisen, dass die Software vollständig fehlerfrei ist.

### Karte 3
**Frage:** Was bedeutet Error?

**Antwort:** Eine menschliche Handlung oder Fehlentscheidung, die ein falsches Ergebnis hervorbringt, zum Beispiel ein Missverständnis einer Anforderung.

### Karte 4
**Frage:** Was bedeutet Fault?

**Antwort:** Eine fehlerhafte Stelle oder Ausprägung im Quellcode beziehungsweise im Softwaresystem.

### Karte 5
**Frage:** Was bedeutet Failure?

**Antwort:** Ein beobachtbares Fehlverhalten des Systems, bei dem die Spezifikation nicht erfüllt wird.

### Karte 6
**Frage:** Was bedeutet Incident?

**Antwort:** Ein vom Benutzer oder Betrieb wahrgenommenes Ereignis beziehungsweise Symptom eines Fehlers.

### Karte 7
**Frage:** Wie lautet die Fehlerkette?

**Antwort:** `Error → Fault → Failure → Incident`.

### Karte 8
**Frage:** Welche drei Testebenen sind besonders wichtig?

**Antwort:** Unit-Tests, Integrationstests und Systemtests.

### Karte 9
**Frage:** Was testet ein Unit-Test?

**Antwort:** Eine kleine, klar abgegrenzte Einheit, häufig eine einzelne Methode oder Klasse.

### Karte 10
**Frage:** Was testet ein Integrationstest?

**Antwort:** Das Zusammenspiel mehrerer Einheiten, Komponenten, Schichten oder externer Systeme.

### Karte 11
**Frage:** Was testet ein Systemtest?

**Antwort:** Das vollständige System gegen Anforderungen und Spezifikation.

### Karte 12
**Frage:** Wie sieht die Testpyramide aus?

**Antwort:** Viele schnelle Unit-Tests bilden die Basis, darüber liegen weniger Integrationstests und an der Spitze wenige langsame Systemtests.

## Unit Testing

### Karte 13
**Frage:** Was bedeutet SUT?

**Antwort:** **System Under Test** – die Klasse, Methode oder Einheit, die durch einen Test geprüft wird.

### Karte 14
**Frage:** Welche Vorteile haben Unit-Tests?

**Antwort:** Sie geben schnelles Feedback, schützen vor Regressionen, unterstützen Refactorings, dokumentieren Verhalten und fördern testbares Design.

### Karte 15
**Frage:** Was ist Testbarkeit?

**Antwort:** Die Eigenschaft eines Systems, leicht isoliert und zuverlässig getestet werden zu können.

### Karte 16
**Frage:** Welche Ursachen verschlechtern die Testbarkeit?

**Antwort:** Starke Kopplung, große Verantwortungsbereiche, globale Zustände, direkte externe Abhängigkeiten, unflexible Initialisierung und nicht beobachtbare Ergebnisse.

### Karte 17
**Frage:** Wofür steht SRP?

**Antwort:** **Single Responsibility Principle:** Eine Klasse soll nur eine wesentliche Verantwortung beziehungsweise einen Grund für Änderungen haben.

### Karte 18
**Frage:** Wofür steht OCP?

**Antwort:** **Open/Closed Principle:** Ein Modul soll für Erweiterungen offen und für Änderungen am bestehenden Verhalten möglichst geschlossen sein.

### Karte 19
**Frage:** Wofür steht LSP?

**Antwort:** **Liskov Substitution Principle:** Untertypen müssen ihren Basistyp ersetzen können, ohne grundlegende Erwartungen zu verletzen.

### Karte 20
**Frage:** Wofür steht ISP?

**Antwort:** **Interface Segregation Principle:** Clients sollen nicht von Methoden abhängen müssen, die sie nicht verwenden.

### Karte 21
**Frage:** Wofür steht DIP?

**Antwort:** **Dependency Inversion Principle:** Module sollen von Abstraktionen statt von konkreten Implementierungen abhängen.

### Karte 22
**Frage:** Was bedeutet FIRST bei Unit-Tests?

**Antwort:** **Fast, Independent, Repeatable, Self-validating, Timely** – schnell, unabhängig, wiederholbar, selbstvalidierend und rechtzeitig erstellt.

### Karte 23
**Frage:** Was bedeutet AAA?

**Antwort:** **Arrange, Act, Assert:** Test vorbereiten, SUT ausführen und Ergebnis oder Interaktion prüfen.

### Karte 24
**Frage:** Was passiert im Arrange-Teil?

**Antwort:** SUT, Eingaben und Abhängigkeiten werden vorbereitet.

### Karte 25
**Frage:** Was passiert im Act-Teil?

**Antwort:** Die zu testende Funktion wird ausgeführt, möglichst genau einmal.

### Karte 26
**Frage:** Was passiert im Assert-Teil?

**Antwort:** Erwartete Werte, Zustände, Exceptions oder fachlich relevante Interaktionen werden geprüft.

### Karte 27
**Frage:** Was sollte ein guter Testname beschreiben?

**Antwort:** Was getestet wird, unter welchen Bedingungen und welches Ergebnis erwartet wird, zum Beispiel `Add_WithTwoNumbers_ReturnsSum`.

### Karte 28
**Frage:** Was ist Äquivalenzklassenbildung?

**Antwort:** Eingaben werden in Gruppen eingeteilt, die voraussichtlich gleich verarbeitet werden. Aus jeder gültigen und ungültigen Klasse wird mindestens ein Wert getestet.

### Karte 29
**Frage:** Was ist Grenzwertanalyse?

**Antwort:** Das gezielte Testen der Grenzen von Eingabeklassen, da dort besonders häufig Fehler auftreten.

### Karte 30
**Frage:** Welche Werte sind bei einem erlaubten Bereich von 18 bis 70 wichtig?

**Antwort:** 17, 18, 70 und 71: jeweils die Werte direkt außerhalb und an den beiden Grenzen.

### Karte 31
**Frage:** Was ist ein ExplorationTest?

**Antwort:** Ein Test zum Erkunden und Verstehen einer unbekannten Bibliothek oder API.

### Karte 32
**Frage:** Was macht `yield return`?

**Antwort:** Es gibt bei einem Iterator schrittweise Werte zurück und setzt die Ausführung beim nächsten Zugriff fort.

### Karte 33
**Frage:** Was ist eine Permutation?

**Antwort:** Eine mögliche Anordnung aller Elemente einer Menge. Drei Elemente haben sechs Permutationen.

### Karte 34
**Frage:** Was ist ein TSPSolver?

**Antwort:** Eine Klasse oder Methode zur Lösung des Problems des Handlungsreisenden: eine möglichst kurze Rundreise durch mehrere Orte.

## xUnit für .NET

### Karte 35
**Frage:** Wofür steht `[Fact]` in xUnit?

**Antwort:** Für einen einzelnen Test ohne Eingabeparameter.

### Karte 36
**Frage:** Wofür steht `[Theory]` in xUnit?

**Antwort:** Für einen parametrisierten Test, der mit mehreren Eingabedatensätzen ausgeführt wird.

### Karte 37
**Frage:** Wofür steht `[InlineData]`?

**Antwort:** Für konkrete Testdaten eines `[Theory]`-Tests.

### Karte 38
**Frage:** Wofür steht `[MemberData]`?

**Antwort:** Für Testdaten aus einer separaten Eigenschaft oder Methode, auch für größere oder dynamische Datenmengen.

### Karte 39
**Frage:** Was prüft `Assert.Equal`?

**Antwort:** Ob erwarteter und tatsächlicher Wert übereinstimmen.

### Karte 40
**Frage:** Was ist der Unterschied zwischen `Assert.Throws` und `Assert.ThrowsAny`?

**Antwort:** `Assert.Throws` erwartet exakt den angegebenen Ausnahmetyp. `Assert.ThrowsAny` akzeptiert auch davon abgeleitete Ausnahmetypen.

### Karte 41
**Frage:** Was prüft `Assert.NotSame`?

**Antwort:** Ob zwei Variablen nicht auf dieselbe Objektinstanz verweisen. Die Werte dürfen trotzdem gleich sein.

### Karte 42
**Frage:** Wozu dienen xUnit-Fixtures?

**Antwort:** Sie stellen gemeinsame Vorbereitung und Aufräumlogik bereit. `IClassFixture` teilt eine Fixture innerhalb einer Testklasse, `ICollectionFixture` innerhalb einer Testsammlung.

## Test Doubles

### Karte 43
**Frage:** Warum verwendet man Test-Doubles?

**Antwort:** Um Kollaboratoren des SUT zu ersetzen und eine Einheit ohne echte Datenbank, Netzwerkverbindung, Dateien oder andere externe Systeme isoliert zu testen.

### Karte 44
**Frage:** Was ist ein Dummy?

**Antwort:** Ein Objekt, das nur übergeben wird, damit eine Schnittstelle erfüllt ist. Sein Inhalt ist für den Test nicht relevant.

### Karte 45
**Frage:** Was ist ein Stub?

**Antwort:** Ein Test-Double, das für bestimmte Aufrufe festgelegte Rückgabewerte liefert.

### Karte 46
**Frage:** Was ist ein Spy?

**Antwort:** Ein Test-Double, das zusätzlich Interaktionen aufzeichnet.

### Karte 47
**Frage:** Was ist ein Fake?

**Antwort:** Eine vereinfachte, aber funktionsfähige Implementierung, zum Beispiel eine Datenbank im Arbeitsspeicher.

### Karte 48
**Frage:** Was ist ein Mock?

**Antwort:** Ein Ersatzobjekt, mit dem erwartete Interaktionen definiert und verifiziert werden.

### Karte 49
**Frage:** Was ist State Verification?

**Antwort:** Die Prüfung, ob das SUT nach der Ausführung den erwarteten fachlichen Zustand erreicht hat.

### Karte 50
**Frage:** Was ist Behavior Verification?

**Antwort:** Die Prüfung, ob das SUT bestimmte Interaktionen mit einem Kollaborator ausgeführt hat.

### Karte 51
**Frage:** Welche Prüfung ist meist stabiler: State oder Behavior Verification?

**Antwort:** State Verification ist meist stabiler, weil sie das sichtbare Ergebnis prüft und weniger an die interne Implementierung gebunden ist.

### Karte 52
**Frage:** Was macht `Setup` in Moq?

**Antwort:** Es legt fest, wie ein Mock auf einen bestimmten Aufruf reagieren soll.

### Karte 53
**Frage:** Wofür steht `It.IsAny<T>()`?

**Antwort:** Für einen Argument-Matcher, der jeden Wert des Typs `T` akzeptiert.

### Karte 54
**Frage:** Wofür dient `SetupSequence`?

**Antwort:** Damit können mehrere aufeinanderfolgende Rückgabewerte für wiederholte Aufrufe festgelegt werden.

### Karte 55
**Frage:** Wozu dient `Throws` in Moq?

**Antwort:** Damit kann simuliert werden, dass eine Abhängigkeit bei einem bestimmten Aufruf eine Exception auslöst.

### Karte 56
**Frage:** Was macht `Verify`?

**Antwort:** Es prüft, ob eine erwartete Interaktion mit dem Mock stattgefunden hat.

### Karte 57
**Frage:** Welche Aufrufhäufigkeiten können mit Moq geprüft werden?

**Antwort:** Unter anderem `Exactly`, `AtLeastOnce`, `AtLeast`, `AtMost` und `Never`.

### Karte 58
**Frage:** Was ist der Unterschied zwischen Loose- und Strict-Mock?

**Antwort:** Ein Loose-Mock liefert für nicht vorbereitete Aufrufe Standardwerte. Ein Strict-Mock wirft bei einem nicht vorbereiteten Aufruf eine Exception.

### Karte 59
**Frage:** Was bedeutet Oververification?

**Antwort:** Es werden zu viele oder zu genaue Implementierungsdetails verifiziert. Dadurch werden Tests fragil und brechen bei harmlosen Refactorings.

### Karte 60
**Frage:** Was ist transitive Mocking?

**Antwort:** Ein Mock liefert einen weiteren Mock zurück. Das ist meist schwer verständlich und weist auf starke Kopplung oder eine ungeeignete Abstraktion hin.

## TDD und Testqualität

### Karte 61
**Frage:** Wie lautet der TDD-Zyklus?

**Antwort:** Red – Green – Refactor: fehlschlagenden Test schreiben, minimalen Produktionscode ergänzen, danach den Code verbessern.

### Karte 62
**Frage:** Was ist das erste Gesetz von TDD?

**Antwort:** Kein Produktionscode darf geschrieben werden, bevor ein fehlschlagender Test existiert.

### Karte 63
**Frage:** Was ist ein Obscure Test?

**Antwort:** Ein Test, dessen Zweck oder Ablauf nicht sofort verständlich ist.

### Karte 64
**Frage:** Was ist Assertion Roulette?

**Antwort:** Ein Test enthält mehrere Assertions, aber es ist unklar, welche Assertion den Fehlschlag verursacht hat.

### Karte 65
**Frage:** Was ist ein fragiler Test?

**Antwort:** Ein Test, der bereits bei einer fachlich harmlosen Änderung oder einem Refactoring fehlschlägt.

### Karte 66
**Frage:** Was ist ein erratischer Test?

**Antwort:** Ein Test, der scheinbar zufällig besteht oder fehlschlägt.

## CI/CD

### Karte 67
**Frage:** Was bedeutet Continuous Integration?

**Antwort:** Änderungen werden regelmäßig in eine gemeinsame Codebasis integriert und automatisiert überprüft, damit Integrationsprobleme früh auffallen.

### Karte 68
**Frage:** Was bedeutet Continuous Delivery?

**Antwort:** Der Code wird automatisiert geprüft und in einen auslieferbaren Zustand gebracht.

### Karte 69
**Frage:** Was bedeutet Continuous Deployment?

**Antwort:** Eine erfolgreiche Pipeline kann die Bereitstellung automatisch auslösen.

### Karte 70
**Frage:** Was ist ein typischer Pipeline-Ablauf?

**Antwort:** Checkout, Restore, Build, Unit-Tests, Integrationstests, Packaging, Deployment, Smoke- beziehungsweise Health-Checks und Monitoring.

### Karte 71
**Frage:** Was ist ein Build-Artefakt?

**Antwort:** Das reproduzierbar erzeugte, auslieferbare Ergebnis eines Builds, zum Beispiel ein Paket, Container-Image oder Release-Archiv.

### Karte 72
**Frage:** Welche Eigenschaften sollte ein Artefakt besitzen?

**Antwort:** Es sollte eindeutig identifizierbar, reproduzierbar, vollständig und unabhängig von einer Entwicklerumgebung sein.

### Karte 73
**Frage:** Warum sollte dasselbe Artefakt durch mehrere Umgebungen wandern?

**Antwort:** Damit nicht jede Umgebung neu baut und dadurch unbemerkte Unterschiede durch Compiler, Abhängigkeiten oder Konfiguration entstehen.

### Karte 74
**Frage:** Was passiert bei einem Pipeline-Fehler?

**Antwort:** Spätere Stufen sollten nicht als erfolgreich gelten. Der Fehler muss schnell, eindeutig und nachvollziehbar sichtbar werden.

### Karte 75
**Frage:** Welche Vorteile bietet CI/CD?

**Antwort:** Frühes Feedback, kleinere Änderungen, reproduzierbare Abläufe, weniger manuelle Fehler und schnellere Auslieferbarkeit.

### Karte 76
**Frage:** Welche Eigenschaften sollte eine gute Pipeline haben?

**Antwort:** Sie sollte schnell, deterministisch, transparent, wiederholbar und mit aussagekräftigen Fehlermeldungen versehen sein.

## Deployment

### Karte 77
**Frage:** Was bedeutet Deployment?

**Antwort:** Das Bereitstellen einer Anwendung oder eines Artefakts in einer Zielumgebung einschließlich Konfiguration, Abhängigkeiten, Berechtigungen und nötiger Datenänderungen.

### Karte 78
**Frage:** Welche typischen Umgebungen gibt es?

**Antwort:** Entwicklung, Test oder Integration, Staging und Produktion.

### Karte 79
**Frage:** Was ist eine direkte Bereitstellung?

**Antwort:** Die laufende Anwendung wird unmittelbar aktualisiert. Das ist einfach, kann aber zu Unterbrechungen oder Mischzuständen führen.

### Karte 80
**Frage:** Was ist Rolling Deployment?

**Antwort:** Instanzen werden schrittweise aktualisiert, sodass ein Teil des Systems weiter verfügbar bleibt.

### Karte 81
**Frage:** Was ist Canary Deployment?

**Antwort:** Eine kleine Gruppe von Benutzern oder Instanzen erhält die Änderung zuerst. Nach Beobachtung der Ergebnisse wird die Verteilung erweitert.

### Karte 82
**Frage:** Was muss bei schrittweisen Deployments kompatibel sein?

**Antwort:** Vorübergehend müssen insbesondere APIs, Nachrichtenformate und Datenbankschemata zwischen den beteiligten Ständen kompatibel sein.

### Karte 83
**Frage:** Warum sind Datenbankänderungen beim Deployment besonders riskant?

**Antwort:** Daten bleiben dauerhaft gespeichert und können nicht so einfach wie Programmdateien ausgetauscht oder rückgängig gemacht werden.

### Karte 84
**Frage:** Was ist ein Smoke- oder Health-Check?

**Antwort:** Eine schnelle Prüfung, ob der Dienst erreichbar ist, startet, zentrale Anfragen verarbeitet und seine Abhängigkeiten erreicht.

## Monitoring und Observability

### Karte 85
**Frage:** Was ist Monitoring?

**Antwort:** Die laufende Beobachtung eines Systems, um Abweichungen bei technischen oder fachlichen Messwerten sichtbar zu machen.

### Karte 86
**Frage:** Welche drei wichtigen Observability-Signale gibt es?

**Antwort:** Metriken, Logs und Traces.

### Karte 87
**Frage:** Was ist eine Metrik?

**Antwort:** Ein numerischer Messwert über die Zeit, beispielsweise Antwortzeit, Fehlerrate, Auslastung oder Durchsatz.

### Karte 88
**Frage:** Was ist ein Log?

**Antwort:** Ein zeitgestempeltes Ereignis mit Kontextinformationen über das Verhalten einer Anwendung oder Infrastruktur.

### Karte 89
**Frage:** Was ist ein Trace?

**Antwort:** Die nachvollziehbare Darstellung des Ablaufs einer Anfrage über mehrere Komponenten hinweg.

### Karte 90
**Frage:** Was bedeutet Latenz?

**Antwort:** Die Zeit, die eine Anfrage oder Verarbeitung benötigt.

### Karte 91
**Frage:** Was bedeutet Durchsatz?

**Antwort:** Die Anzahl der Anfragen, Nachrichten oder Aufgaben, die pro Zeiteinheit verarbeitet werden.

### Karte 92
**Frage:** Was ist die Fehlerrate?

**Antwort:** Der Anteil fehlgeschlagener Anfragen oder Verarbeitungsvorgänge an allen Vorgängen.

### Karte 93
**Frage:** Was ist Verfügbarkeit?

**Antwort:** Der Anteil der Zeit, in der ein Dienst nutzbar ist.

### Karte 94
**Frage:** Welche Informationen sollten Logs enthalten?

**Antwort:** Zeitstempel, Schweregrad, Korrelations-ID, betroffene Komponente und eine verständliche Meldung. Geheimnisse dürfen nicht geloggt werden.

### Karte 95
**Frage:** Was zeichnet einen guten Alert aus?

**Antwort:** Er ist eindeutig, priorisiert, begründet und enthält eine konkrete Handlungsanweisung.

### Karte 96
**Frage:** Was ist Alert Fatigue?

**Antwort:** Die Überlastung durch zu viele oder irrelevante Fehlalarme, wodurch wichtige Alerts leichter übersehen werden.

### Karte 97
**Frage:** Was bedeutet Observability?

**Antwort:** Wie gut sich der innere Zustand eines Systems aus seinen äußeren Ausgaben wie Metriken, Logs und Traces ableiten lässt.

### Karte 98
**Frage:** Warum können Perzentile bei Antwortzeiten sinnvoller sein als der Durchschnitt?

**Antwort:** Sie zeigen die langsamsten regulären Anfragen und bilden dadurch die Benutzererfahrung besser ab, zum Beispiel mit der 95. oder 99. Perzentile.

## Wiederholungsblock: kurze Prüfungsfragen

### Karte 99
**Frage:** Was ist der wichtigste Unterschied zwischen Stub und Mock?

**Antwort:** Ein Stub liefert vorbereitete Daten; ein Mock wird vor allem zur Prüfung erwarteter Interaktionen verwendet.

### Karte 100
**Frage:** Was ist der wichtigste Unterschied zwischen Unit- und Integrationstest?

**Antwort:** Ein Unit-Test isoliert eine Einheit; ein Integrationstest prüft das Zusammenspiel mehrerer Einheiten oder Systeme.

### Karte 101
**Frage:** Warum sind Tests mit externen Systemen oft keine guten Unit-Tests?

**Antwort:** Sie sind meist langsamer, weniger deterministisch und schwerer zu isolieren. Externe Kollaboratoren werden deshalb häufig durch Test-Doubles ersetzt.

### Karte 102
**Frage:** Warum sollen Tests nicht von einer Reihenfolge abhängen?

**Antwort:** Jeder Test soll unabhängig und wiederholbar sein. Eine Reihenfolgeabhängigkeit führt zu schwer nachvollziehbaren und instabilen Ergebnissen.

### Karte 103
**Frage:** Was soll Monitoring nach einem Deployment erkennen?

**Antwort:** Ob der Dienst erreichbar und funktionsfähig ist und ob sich Fehlerrate, Latenz, Ressourcennutzung oder andere wichtige Signale verschlechtern.

### Karte 104
**Frage:** Was ist das Ziel von CI/CD in einem Satz?

**Antwort:** Änderungen sollen schnell, automatisiert, reproduzierbar und mit möglichst geringem Risiko geprüft und bereitgestellt werden.
