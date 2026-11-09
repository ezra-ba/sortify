#fh #flashcards/fh/klausur

# Klausur-Flashcards

## Softwarequalität und Testebenen

Was ist formale Verifikation?::Der mathematische Beweis, dass ein Programm eine Spezifikation erfüllt. Sie ist aufwendig und besonders für kritische Systeme geeignet.
<!--SR:!2026-11-12,3,230-->

Was kann dynamisches Testen zeigen?::Es kann zeigen, dass konkrete Eingaben funktionieren oder dass ein Fehler existiert. Vollständige Fehlerfreiheit kann es nicht beweisen.
<!--SR:!2026-11-12,3,250-->

Was bedeutet Error?::Eine menschliche Handlung oder Fehlentscheidung, die ein falsches Ergebnis erzeugt.

Was bedeutet Fault?::Eine fehlerhafte Stelle oder Ausprägung im Quellcode beziehungsweise im Softwaresystem.
<!--SR:!2026-11-11,2,150-->

Was bedeutet Failure?::Ein beobachtbares Fehlverhalten, bei dem das System seine Spezifikation nicht erfüllt.

Was bedeutet Incident?::Ein vom Benutzer oder Betrieb wahrgenommenes Ereignis beziehungsweise Symptom eines Fehlers.
<!--SR:!2026-11-11,3,210-->

Wie lautet die Fehlerkette?::Error → Fault → Failure → Incident.
<!--SR:!2026-11-12,3,250-->

Welche drei wichtigen Testebenen gibt es?::Unit-Tests, Integrationstests und Systemtests.

Was testet ein Unit-Test?::Eine kleine, klar abgegrenzte Einheit, häufig eine einzelne Methode oder Klasse.

Was testet ein Integrationstest?::Das Zusammenspiel mehrerer Einheiten, Komponenten, Schichten oder externer Systeme.
<!--SR:!2026-11-10,2,150-->

Was testet ein Systemtest?::Das vollständige System gegen Anforderungen und Spezifikation.
<!--SR:!2026-11-11,3,230-->

Wie ist die Testpyramide aufgebaut?::Viele schnelle Unit-Tests bilden die Basis, darüber liegen weniger Integrationstests und an der Spitze wenige langsame Systemtests.
<!--SR:!2026-11-13,4,270-->

## Unit Testing

Was bedeutet SUT?::System Under Test – die Klasse, Methode oder Einheit, die durch einen Test geprüft wird.

Welche Vorteile haben Unit-Tests?::Sie geben schnelles Feedback, schützen vor Regressionen, unterstützen Refactorings, dokumentieren Verhalten und fördern testbares Design.

Was ist Testbarkeit?::Die Eigenschaft eines Systems, leicht isoliert und zuverlässig getestet werden zu können.
<!--SR:!2026-11-12,3,210-->

Welche Faktoren verschlechtern die Testbarkeit?::Starke Kopplung, große Verantwortungsbereiche, globale Zustände, direkte externe Abhängigkeiten, unflexible Initialisierung und nicht beobachtbare Ergebnisse.
<!--SR:!2026-11-12,3,230-->

Wofür steht SRP?::Single Responsibility Principle: Eine Klasse soll nur eine wesentliche Verantwortung beziehungsweise einen Grund für Änderungen haben.
<!--SR:!2026-11-12,3,250-->

Wofür steht OCP?::Open/Closed Principle: Ein Modul soll für Erweiterungen offen und für Änderungen am bestehenden Verhalten möglichst geschlossen sein.

Wofür steht LSP?::Liskov Substitution Principle: Untertypen müssen ihren Basistyp ersetzen können, ohne grundlegende Erwartungen zu verletzen.

Wofür steht ISP?::Interface Segregation Principle: Clients sollen nicht von Methoden abhängen müssen, die sie nicht verwenden.
<!--SR:!2026-11-11,3,230-->

Wofür steht DIP?::Dependency Inversion Principle: Module sollen von Abstraktionen statt von konkreten Implementierungen abhängen.
<!--SR:!2026-11-10,2,190-->

Wofür steht FIRST?::Fast, Independent, Repeatable, Self-validating, Timely – schnell, unabhängig, wiederholbar, selbstvalidierend und rechtzeitig erstellt.
<!--SR:!2026-11-12,4,270-->

Was bedeutet AAA?::Arrange, Act, Assert: Test vorbereiten, SUT ausführen und Ergebnis oder Interaktion prüfen.
<!--SR:!2026-11-12,4,270-->

Was passiert im Arrange-Teil?::SUT, Eingaben und Abhängigkeiten werden vorbereitet.
<!--SR:!2026-11-11,3,230-->

Was passiert im Act-Teil?::Die zu testende Funktion wird ausgeführt, möglichst genau einmal.

Was passiert im Assert-Teil?::Erwartete Werte, Zustände, Exceptions oder fachlich relevante Interaktionen werden geprüft.
<!--SR:!2026-11-11,2,150-->

Was ist Äquivalenzklassenbildung?::Eingaben werden in Gruppen eingeteilt, die voraussichtlich gleich verarbeitet werden. Aus jeder gültigen und ungültigen Klasse wird mindestens ein Wert getestet.
<!--SR:!2026-11-10,2,190-->

Was ist Grenzwertanalyse?::Das gezielte Testen der Grenzen von Eingabeklassen, da dort besonders häufig Fehler auftreten.

Welche Werte sind bei einem erlaubten Bereich von 18 bis 70 besonders wichtig?::17, 18, 70 und 71: die Werte direkt außerhalb und an den beiden Grenzen.

Was ist ein ExplorationTest?::Ein Test zum Erkunden und Verstehen einer unbekannten Bibliothek oder API.
<!--SR:!2026-11-11,3,230-->

## xUnit für .NET

Wofür steht `[Fact]` in xUnit?::Für einen einzelnen Test ohne Eingabeparameter.
<!--SR:!2026-11-12,4,270-->

Wofür steht `[Theory]` in xUnit?::Für einen parametrisierten Test, der mit mehreren Eingabedatensätzen ausgeführt wird.

Wofür steht `[InlineData]`?::Für konkrete Testdaten eines `[Theory]`-Tests.
<!--SR:!2026-11-12,3,250-->

Wofür steht `[MemberData]`?::Für Testdaten aus einer separaten Eigenschaft oder Methode, auch für größere oder dynamische Datenmengen.
<!--SR:!2026-11-11,2,190-->

Was prüft `Assert.Equal`?::Ob erwarteter und tatsächlicher Wert übereinstimmen.
<!--SR:!2026-11-12,4,270-->

Was ist der Unterschied zwischen `Assert.Throws` und `Assert.ThrowsAny`?::`Assert.Throws` erwartet exakt den angegebenen Ausnahmetyp. `Assert.ThrowsAny` akzeptiert auch davon abgeleitete Typen.

Was prüft `Assert.NotSame`?::Ob zwei Variablen nicht auf dieselbe Objektinstanz verweisen. Die Werte dürfen trotzdem gleich sein.
<!--SR:!2026-11-12,3,230-->

## Test Doubles und Moq

Warum verwendet man Test-Doubles?::Um Kollaboratoren des SUT zu ersetzen und eine Einheit ohne echte externe Systeme isoliert zu testen.

Was ist ein Dummy?::Ein Objekt, das nur übergeben wird, damit eine Schnittstelle erfüllt ist. Sein Inhalt ist für den Test nicht relevant.

Was ist ein Stub?::Ein Test-Double, das für bestimmte Aufrufe festgelegte Rückgabewerte liefert.
<!--SR:!2026-11-12,3,210-->

Was ist ein Spy?::Ein Test-Double, das zusätzlich Interaktionen aufzeichnet.
<!--SR:!2026-11-11,3,230-->

Was ist ein Fake?::Eine vereinfachte, aber funktionsfähige Implementierung, zum Beispiel eine Datenbank im Arbeitsspeicher.

Was ist ein Mock?::Ein Ersatzobjekt, mit dem erwartete Interaktionen definiert und verifiziert werden.
<!--SR:!2026-11-11,2,190-->

Was ist State Verification?::Die Prüfung, ob das SUT nach der Ausführung den erwarteten fachlichen Zustand erreicht hat.
<!--SR:!2026-11-11,3,230-->

Was ist Behavior Verification?::Die Prüfung, ob das SUT bestimmte Interaktionen mit einem Kollaborator ausgeführt hat.
<!--SR:!2026-11-13,4,270-->

Was macht `Setup` in Moq?::Es legt fest, wie ein Mock auf einen bestimmten Aufruf reagieren soll.

Wofür steht `It.IsAny<T>()`?::Für einen Argument-Matcher, der jeden Wert des Typs `T` akzeptiert.
<!--SR:!2026-11-11,3,210-->

Wofür dient `SetupSequence`?::Damit werden mehrere aufeinanderfolgende Rückgabewerte für wiederholte Aufrufe festgelegt.
<!--SR:!2026-11-11,2,150-->

Wozu dient `Throws` in Moq?::Damit wird simuliert, dass eine Abhängigkeit bei einem bestimmten Aufruf eine Exception auslöst.
<!--SR:!2026-11-11,3,210-->

Was macht `Verify`?::Es prüft, ob eine erwartete Interaktion mit dem Mock stattgefunden hat.

Was ist der Unterschied zwischen Loose- und Strict-Mock?::Ein Loose-Mock liefert für nicht vorbereitete Aufrufe Standardwerte. Ein Strict-Mock wirft dabei eine Exception.
<!--SR:!2026-11-11,3,250-->

Was bedeutet Oververification?::Es werden zu viele oder zu genaue Implementierungsdetails verifiziert. Dadurch werden Tests fragil.

## TDD und Testqualität

Wie lautet der TDD-Zyklus?::Red – Green – Refactor: fehlschlagenden Test schreiben, minimalen Produktionscode ergänzen, danach den Code verbessern.

Was ist ein Obscure Test?::Ein Test, dessen Zweck oder Ablauf nicht sofort verständlich ist.

Was ist Assertion Roulette?::Ein Test enthält mehrere Assertions, aber es ist unklar, welche Assertion den Fehlschlag verursacht hat.
<!--SR:!2026-11-12,4,270-->

Was ist ein fragiler Test?::Ein Test, der bereits bei einer fachlich harmlosen Änderung oder einem Refactoring fehlschlägt.

Was ist ein erratischer Test?::Ein Test, der scheinbar zufällig besteht oder fehlschlägt.
<!--SR:!2026-11-13,4,270-->

## CI/CD

Was bedeutet Continuous Integration?::Änderungen werden regelmäßig in eine gemeinsame Codebasis integriert und automatisiert überprüft.

Was bedeutet Continuous Delivery?::Der Code wird automatisiert geprüft und in einen auslieferbaren Zustand gebracht.
<!--SR:!2026-11-11,3,210-->

Was bedeutet Continuous Deployment?::Eine erfolgreiche Pipeline kann die Bereitstellung automatisch auslösen.
<!--SR:!2026-11-12,3,250-->

Wie sieht ein typischer Pipeline-Ablauf aus?::Checkout, Restore, Build, Unit-Tests, Integrationstests, Packaging, Deployment, Health-Checks und Monitoring.

Was ist ein Build-Artefakt?::Das reproduzierbar erzeugte, auslieferbare Ergebnis eines Builds, zum Beispiel ein Paket, Container-Image oder Release-Archiv.

Warum sollte dasselbe Artefakt durch mehrere Umgebungen wandern?::Damit nicht jede Umgebung neu baut und dadurch unbemerkte Unterschiede entstehen.

Welche Eigenschaften sollte eine gute Pipeline haben?::Sie sollte schnell, deterministisch, transparent, wiederholbar und mit aussagekräftigen Fehlermeldungen versehen sein.
<!--SR:!2026-11-11,3,230-->

## Deployment

Was bedeutet Deployment?::Das Bereitstellen einer Anwendung oder eines Artefakts in einer Zielumgebung einschließlich Konfiguration, Abhängigkeiten und nötiger Datenänderungen.
<!--SR:!2026-11-11,3,210-->

Welche typischen Umgebungen gibt es?::Entwicklung, Test oder Integration, Staging und Produktion.
<!--SR:!2026-11-112,3,210-->

Was ist eine direkte Bereitstellung?::Die laufende Anwendung wird unmittelbar aktualisiert. Das kann zu Unterbrechungen oder Mischzuständen führen.
<!--SR:!2026-11-13,4,270-->

Was ist Rolling Deployment?::Instanzen werden schrittweise aktualisiert, sodass ein Teil des Systems weiter verfügbar bleibt.

Was ist Canary Deployment?::Eine kleine Gruppe von Benutzern oder Instanzen erhält die Änderung zuerst. Nach Beobachtung wird die Verteilung erweitert.

Was muss bei schrittweisen Deployments kompatibel sein?::Vorübergehend insbesondere APIs, Nachrichtenformate und Datenbankschemata.
<!--SR:!2026-11-13,4,270-->

Was ist ein Smoke- oder Health-Check?::Eine schnelle Prüfung, ob der Dienst erreichbar ist, startet, zentrale Anfragen verarbeitet und Abhängigkeiten erreicht.
<!--SR:!2026-11-11,3,230-->

## Monitoring und Observability

Was ist Monitoring?::Die laufende Beobachtung eines Systems, um Abweichungen bei technischen oder fachlichen Messwerten sichtbar zu machen.
<!--SR:!2026-11-12,4,270-->

Welche drei wichtigen Observability-Signale gibt es?::Metriken, Logs und Traces.
<!--SR:!2026-11-13,4,270-->

Was ist eine Metrik?::Ein numerischer Messwert über die Zeit, beispielsweise Antwortzeit, Fehlerrate, Auslastung oder Durchsatz.
<!--SR:!2026-11-12,3,230-->

Was ist ein Log?::Ein zeitgestempeltes Ereignis mit Kontextinformationen über das Verhalten einer Anwendung oder Infrastruktur.

Was ist ein Trace?::Die nachvollziehbare Darstellung des Ablaufs einer Anfrage über mehrere Komponenten hinweg.
<!--SR:!2026-11-12,3,230-->

Was bedeutet Latenz?::Die Zeit, die eine Anfrage oder Verarbeitung benötigt.
<!--SR:!2026-11-12,4,270-->

Was bedeutet Durchsatz?::Die Anzahl der Anfragen, Nachrichten oder Aufgaben, die pro Zeiteinheit verarbeitet werden.
<!--SR:!2026-11-13,4,270-->

Was ist die Fehlerrate?::Der Anteil fehlgeschlagener Anfragen oder Verarbeitungsvorgänge an allen Vorgängen.
<!--SR:!2026-11-13,4,270-->

Was ist Verfügbarkeit?::Der Anteil der Zeit, in der ein Dienst nutzbar ist.

Was zeichnet einen guten Alert aus?::Er ist eindeutig, priorisiert, begründet und enthält eine konkrete Handlungsanweisung.

Was ist Alert Fatigue?::Die Überlastung durch zu viele oder irrelevante Fehlalarme, wodurch wichtige Alerts leichter übersehen werden.
<!--SR:!2026-11-12,3,250-->
<!--SR:!2026-11-08,0,230-->

Was bedeutet Observability?::Wie gut sich der innere Zustand eines Systems aus seinen äußeren Ausgaben wie Metriken, Logs und Traces ableiten lässt.
<!--SR:!2026-11-12,4,270-->

Warum können Perzentile bei Antwortzeiten sinnvoller sein als der Durchschnitt?::Sie zeigen die langsamsten regulären Anfragen und bilden dadurch die Benutzererfahrung besser ab.
<!--SR:!2026-11-12,4,270-->
