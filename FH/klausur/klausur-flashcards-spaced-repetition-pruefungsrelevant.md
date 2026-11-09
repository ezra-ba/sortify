#fh #flashcards/fh/klausur

# Prüfungsrelevante Spaced-Repetition-Flashcards

Die Karten basieren auf den Unterlagen in `FH/klausur`. Ausgeschlossen wurden: JUnit, GitHub Actions, Shadow Deployment, Versioned Deployment, Grafana, Prometheus, der gesamte Themenblock Statische Code Analyse, der Monitoring-Teil von Deployment & Monitoring sowie sämtliche Java-Inhalte.

## Softwarequalität, Fehlerbegriffe und Testebenen

Was beweist formale Verifikation?::Sie beweist mathematisch, dass ein Programm eine formale Spezifikation erfüllt. Sie ist aufwendig und wird vor allem bei besonders kritischen Systemen eingesetzt.

Was kann dynamisches Testen zeigen und was kann es nicht beweisen?::Es kann zeigen, dass konkrete Eingaben funktionieren oder dass ein Fehler auftritt. Vollständige Fehlerfreiheit kann es nicht beweisen.

Wie unterscheiden sich Error, Fault, Failure und Incident?::Error ist eine menschliche Handlung mit falschem Ergebnis. Fault ist die daraus hervorgehende fehlerhafte Stelle im System. Failure ist die Nichterfüllung einer Anforderung durch das System. Incident ist das vom Benutzer wahrgenommene Symptom.

Wie lautet die typische Fehlerkette?::Error → Fault → Failure → Incident.

Was testet ein Unit-Test?::Eine kleine, klar abgegrenzte Einheit, häufig eine einzelne Methode oder Klasse. Unit-Tests sind meist schnell, einfach zu automatisieren und überwiegend White-Box-Tests.

Was testet ein Integrationstest?::Das Zusammenspiel mehrerer Komponenten, Klassen, Schichten oder externer Systeme.

Was testet ein Systemtest?::Das vollständige System gegen seine Anforderungen und Spezifikation. Systemtests sind oft umfassend, komplex, langsam und überwiegend Black-Box-Tests.

Wie ist die Testpyramide aufgebaut?::Aus vielen schnellen und günstigen Unit-Tests, weniger Integrationstests und wenigen langsamen, komplexen und teuren Systemtests.
<!--SR:!2026-11-13,4,270-->

Warum sollen Fehler möglichst früh gefunden werden?::Der Kontext der Änderung ist noch präsent, die Ursache lässt sich leichter identifizieren und die Behebung ist schneller und günstiger.

Wer trägt Verantwortung für die Produktqualität?::Das gesamte Entwicklungsteam beziehungsweise insbesondere die Entwickler, nicht ausschließlich eine separate Testabteilung.

## Unit Testing und testbares Design

Was bedeutet SUT?::System Under Test: die Klasse, Methode oder Einheit, die durch einen Test geprüft wird.

Welche Vorteile bieten Unit-Tests?::Schnelles Feedback, Schutz vor Regressionen, Sicherheit beim Refactoring, ausführbare Dokumentation, Unterstützung testbaren Designs und Fokus auf das fachliche Was statt auf das technische Wie.

Was ist Testbarkeit?::Die Eigenschaft eines Systems, leicht isoliert, kontrolliert und zuverlässig getestet werden zu können. Sie ist ein Qualitätsmerkmal der Wartbarkeit.

Welche Faktoren verschlechtern die Testbarkeit?::Nicht isolierbare Einheiten, starke Kopplung, unflexible Initialisierung, verborgene Zustände, nicht erreichbare Kollaboratoren, fehlende Kontrolle und nicht beobachtbare Ergebnisse.

Was bedeutet SRP?::Single Responsibility Principle: Eine Softwareeinheit soll eine einzelne, klar abgegrenzte Verantwortung beziehungsweise nur einen wesentlichen Grund für Änderungen haben.

Was bedeutet OCP?::Open/Closed Principle: Softwareeinheiten sollen für Erweiterungen offen, für Änderungen am bestehenden Verhalten aber möglichst geschlossen sein.

Was bedeutet LSP?::Liskov Substitution Principle: Untertypen müssen ihre Basistypen ersetzen können, ohne grundlegende Erwartungen an deren Verhalten zu verletzen.

Was bedeutet ISP?::Interface Segregation Principle: Clients sollen nicht von Methoden abhängen müssen, die sie nicht verwenden. Kleine, spezifische Schnittstellen reduzieren Kopplung.

Was bedeutet DIP?::Dependency Inversion Principle: Übergeordnete und untergeordnete Module sollen von Abstraktionen abhängen, nicht voneinander. Abhängigkeiten werden beispielsweise per Dependency Injection gesetzt.

Was bedeutet FIRST bei Unit-Tests?::Fast, Independent, Repeatable, Self-validating, Timely: schnell, unabhängig, wiederholbar, selbstvalidierend und rechtzeitig geschrieben.

Was bedeutet AAA?::Arrange, Act, Assert: Test und Abhängigkeiten vorbereiten, die zu testende Funktion ausführen und das erwartete Ergebnis oder die relevante Interaktion prüfen.
<!--SR:!2026-11-13,4,270-->

Was passiert im Arrange-Teil?::Das SUT, Eingaben, Anfangszustände und benötigte Test-Doubles werden vorbereitet.

Was passiert im Act-Teil?::Die zu testende Funktion wird ausgeführt, möglichst genau einmal.

Was passiert im Assert-Teil?::Erwartete Werte, Zustände, Exceptions oder fachlich relevante Interaktionen werden überprüft.

Was ist Äquivalenzklassenbildung?::Eingaben werden in Klassen eingeteilt, die voraussichtlich gleich verarbeitet werden. Aus jeder gültigen und ungültigen Klasse wird mindestens ein repräsentativer Wert getestet.

Was ist Grenzwertanalyse?::Das gezielte Testen von Werten an und unmittelbar außerhalb der Grenzen von Eingabeklassen.
<!--SR:!2026-11-13,4,270-->

Welche Werte sind bei einem erlaubten Bereich von 18 bis 70 besonders wichtig?::17, 18, 70 und 71: die beiden Grenzwerte und die jeweils angrenzenden ungültigen Werte.

Was ist ein Exploration Test?::Ein Test, mit dem das tatsächliche Verhalten einer unbekannten Bibliothek oder API erkundet und verstanden wird.

## Test-Doubles und Moq

Warum verwendet man Test-Doubles?::Um Kollaboratoren des SUT zu ersetzen und eine Einheit isoliert, schnell und kontrolliert testen zu können.
<!--SR:!2026-11-13,4,270-->

Was ist ein Dummy?::Ein Objekt, das nur übergeben wird, damit eine Schnittstelle erfüllt ist. Sein Inhalt und Verhalten sind für den Test nicht relevant.

Was ist ein Stub?::Ein Test-Double, das für bestimmte Aufrufe festgelegte Rückgabewerte liefert.

Was ist ein Spy?::Ein Test-Double, das zusätzlich Interaktionen aufzeichnet. Es ist ein Stub mit Beobachtungsfunktion.
<!--SR:!2026-11-13,4,270-->

Was ist ein Fake?::Eine vereinfachte, aber funktionsfähige Implementierung, zum Beispiel eine Datenbank im Arbeitsspeicher.

Was ist ein Mock?::Ein Ersatzobjekt, mit dem erwartete Interaktionen definiert und verifiziert werden. Mocks werden häufig durch ein Mocking-Framework erzeugt.

Was ist State Verification?::Die Prüfung, ob das SUT nach der Ausführung den erwarteten fachlichen Zustand erreicht hat. Dazu werden Ist- und Sollwerte verglichen.

Was ist Behavior Verification?::Die Prüfung, ob das SUT bestimmte Interaktionen mit einem Kollaborator ausgeführt hat. Dafür werden Mocks oder Spies verwendet.

Warum sind Behavior-Tests oft fragiler als State-Tests?::Sie prüfen konkrete interne Abläufe und koppeln den Test dadurch stärker an die Implementierung statt an das fachliche Ergebnis.

Wie lautet der typische Moq-Ablauf?::Mock erzeugen, falls nötig Verhalten mit `Setup` festlegen, SUT ausführen und relevante Interaktionen mit `Verify` prüfen.
<!--SR:!2026-11-13,4,270-->

Wofür dient `Setup` in Moq?::Es legt fest, wie ein Mock auf einen bestimmten Aufruf reagieren soll, zum Beispiel mit einem Rückgabewert.

Was bedeutet `It.IsAny<T>()`?::Der Argument-Matcher akzeptiert jeden Wert des Typs `T`.

Wofür dient `It.Is<T>(...)`?::Es akzeptiert nur Argumente, die eine selbst definierte Bedingung erfüllen.

Wofür dient `SetupSequence`?::Damit werden mehrere aufeinanderfolgende Rückgabewerte für wiederholte Aufrufe festgelegt.

Wozu dient `Throws` in Moq?::Damit wird simuliert, dass eine Abhängigkeit bei einem bestimmten Aufruf eine Exception auslöst.

Was macht `Verify`?::Es prüft, ob eine erwartete Interaktion mit dem Mock stattgefunden hat, gegebenenfalls mit einer bestimmten Aufrufanzahl.

Was ist der Unterschied zwischen Loose- und Strict-Mock?::Ein Loose-Mock liefert für nicht vorbereitete Aufrufe Standardwerte. Ein Strict-Mock wirft bei nicht vorbereiteten Aufrufen eine Exception.

Was bedeutet Oververification?::Es werden zu viele oder zu genaue Implementierungsdetails verifiziert. Dadurch werden Tests unnötig fragil.

## TDD und Testqualität

Wie lautet der TDD-Zyklus?::Red – Green – Refactor: einen fehlschlagenden Test schreiben, minimalen Produktionscode ergänzen und danach den Code verbessern.

Was besagen die drei TDD-Gesetze?::Vor Produktionscode existiert ein fehlschlagender Test; es wird nur so viel Testcode geschrieben, dass er fehlschlägt; danach wird nur so viel Produktionscode ergänzt, dass der aktuelle Test besteht.

Was ist ein Obscure Test?::Ein Test, dessen Zweck oder Ablauf nicht sofort verständlich ist.

Was ist Assertion Roulette?::Ein Test enthält mehrere Assertions, aber es ist unklar, welche Assertion den Fehlschlag verursacht hat.

Was ist ein fragiler Test?::Ein Test, der bereits bei einer fachlich harmlosen Änderung oder einem Refactoring fehlschlägt.

Was ist ein erratischer Test?::Ein Test, der scheinbar zufällig besteht oder fehlschlägt.

Warum ist Testcode genauso zu pflegen wie Produktionscode?::Schlechter Testcode wird teuer, erschwert Änderungen und kann dazu führen, dass Tests deaktiviert oder nicht mehr ausgeführt werden.
<!--SR:!2026-11-13,4,270-->

## CI/CD-Grundlagen

Warum ist Testautomatisierung wichtig?::Sie liefert schnelles Feedback, spart manuelle Arbeit, prüft reproduzierbar in einer definierten Umgebung und verhindert das Problem »It works on my machine«.

Was bedeutet Continuous Integration?::Änderungen werden häufig in eine gemeinsame Codebasis integriert, automatisch kompiliert und durch automatisierte Tests überprüft.

Welche Voraussetzungen braucht CI?::Ein Versionskontrollsystem, einen CI- beziehungsweise Build-Server, eine ausführbare Beschreibung des Build- und Testprozesses, bei Bedarf Build-Agenten und einen Feedback-Kanal.

Was ist eine CI-Pipeline?::Die definierte Folge von Schritten, die nach Änderungen ausgeführt wird, um Software zu bauen, zu testen und Ergebnisse zu berichten.

Welche Mindeststufen hat eine CI-Pipeline?::Mindestens Build und Test; häufig kommen Packaging, Veröffentlichung, Deployment oder zusätzliche Prüfungen hinzu.

Was ist die goldene Regel bei einer fehlschlagenden CI-Pipeline?::Sie muss sofort behoben werden, damit die gemeinsame Codebasis jederzeit vertrauenswürdig bleibt.

Was bedeutet Continuous Delivery?::Die Software wird automatisiert geprüft und in einen auslieferbaren Zustand gebracht, oft in einer produktionsnahen Testumgebung. Das Produktionsdeployment kann noch manuell ausgelöst werden.

Was bedeutet Continuous Deployment?::Nach erfolgreicher automatisierter Prüfung wird die Software automatisch in die Produktionsumgebung deployt.

Warum sollte dasselbe Build-Artefakt durch mehrere Umgebungen wandern?::Damit nicht jede Umgebung neu baut und dadurch unbemerkte Unterschiede zwischen Test und Produktion entstehen.

Was ist ein Build-Artefakt?::Das reproduzierbar erzeugte, auslieferbare Ergebnis eines Builds, zum Beispiel ein Paket, Container-Image oder Release-Archiv.

## Deployment

Welche Ziele hat ein gutes Deployment?::Software soll Benutzern bereitgestellt werden, die Nutzung bestehender Benutzer möglichst wenig beeinträchtigen und bei Problemen ein Rollback auf eine funktionierende Version ermöglichen.

Warum sollte Deployment automatisiert werden?::Manuelle Deployments sind zeitaufwendig und fehleranfällig. Ein stabiles und sicheres Deployment gehört deshalb in die CI/CD-Pipeline.

Was ist Reckless beziehungsweise Recreate Deployment?::Die alte Version wird vollständig entfernt und anschließend die neue Version bereitgestellt. Die Strategie ist einfach, verursacht aber effektive Downtime und berücksichtigt die Nutzerwirkung kaum.

Was ist Rolling Deployment?::Instanzen werden schrittweise durch die neue Version ersetzt. Während des Übergangs laufen alte und neue Version parallel; dafür ist Rückwärtskompatibilität wichtig.

Was ist Blue/Green Deployment?::Zwei identische Umgebungen existieren parallel. Nach dem Test der neuen Version wird der Datenverkehr auf die neue Umgebung umgeschaltet; die alte Umgebung bleibt als Rückfalloption erhalten.

Was ist Canary Deployment?::Eine neue Version wird zuerst nur einer kleinen Benutzer- oder Instanzengruppe bereitgestellt. Nach Beobachtung wird der Anteil schrittweise erhöht.

Welche Strategieeigenschaft ist bei Rolling Deployments besonders wichtig?::Während der Übergangsphase muss das System mit alten und neuen Versionen zugleich funktionieren; APIs, Nachrichtenformate und Datenbankschemata müssen daher vorübergehend kompatibel sein.

Welche Deployment-Strategien bieten eine gute Rollback-Möglichkeit?::Vor allem Blue/Green und Canary; auch bei anderen Strategien muss ein Rückweg zu einer guten Version vorbereitet werden.
