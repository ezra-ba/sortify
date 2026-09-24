#fh

# Unit Testing - Zusammenfassung

## 1. Softwaretests und Fehlerbegriffe

Softwarequalität kann grundsätzlich durch formale Verifikation oder durch empirisches Testen untersucht werden. Formale Verifikation beweist die Korrektheit mathematisch, ist aber aufwendig und nur für spezielle, besonders kritische Systeme üblich. Tests führen das Programm mit konkreten Eingaben aus und zeigen, dass bestimmte Fälle funktionieren. Sie können jedoch nicht beweisen, dass eine Software vollständig fehlerfrei ist.

Wichtige Begriffe bei Softwarefehlern:

- **Error:** Menschliche Handlung, die ein falsches Ergebnis erzeugt.
- **Fault:** Fehlerhafte Stelle bzw. Ausprägung im Softwaresystem.
- **Failure:** Das System erfüllt seine Anforderungen nicht.
- **Incident:** Vom Benutzer wahrgenommenes Symptom eines Fehlers.

Statische Analyse, etwa Code-Reviews oder statische Codeanalyse, untersucht den Quellcode ohne Ausführung. Dynamische Tests führen den Code aus und suchen dadurch nach Failures.

## 2. Testebenen und Testpyramide

- **Unit-Tests:** Testen eine kleine, klar abgegrenzte Einheit, oft eine einzelne Methode. Sie sind schnell, einfach zu automatisieren und meist White-Box-Tests.
- **Integrationstests:** Testen das Zusammenspiel mehrerer Klassen, Komponenten, Schichten oder Systeme.
- **Systemtests:** Testen das gesamte System gegen die Spezifikation. Sie sind umfassend, langsam, komplex und häufig Black-Box-Tests; UI-Tests gehören oft dazu.

Die Testpyramide empfiehlt viele schnelle Unit-Tests, weniger Integrationstests und wenige langsame Systemtests. Eine umgekehrte Pyramide entsteht häufig durch manuelle Tests. Fehler sollen möglichst früh nach ihrer Einführung gefunden werden, weil Ursache und Kontext dann noch bekannt sind und die Behebung günstiger ist. Für die Produktqualität sind die Entwickler mitverantwortlich; Testautomatisierung ist dafür zentral.

## 3. Zweck und Testbarkeit von Unit-Tests

Unit-Tests:

- fördern ein testbares und dadurch qualitativ besseres Design,
- geben sofortiges Feedback bei Änderungen,
- schützen beim Refactoring und vor Regressionen,
- ermöglichen Veränderungen am Code,
- lenken den Fokus auf das **Was** statt auf das **Wie**,
- dienen als ausführbare und kompilierbare Dokumentation,
- können zum Erkunden neuer Bibliotheks-APIs verwendet werden.

**Testbarkeit** beschreibt, wie leicht ein System getestet werden kann. Sie ist ein Qualitätsmerkmal der Wartbarkeit. Schlechte Testbarkeit führt zu weniger Tests, mehr Fehlern, einer umgekehrten Testpyramide und höheren Kosten. Ursachen sind unter anderem starke Kopplung, nicht isolierbare Einheiten, unflexible Initialisierung, verborgene Zustände oder nicht beobachtbare Ergebnisse. Ein hoher Aufwand beim Schreiben und Automatisieren von Tests sowie viele notwendige Test-Doubles sind Warnzeichen.

Testbarkeit sollte während des gesamten Entwicklungsprozesses berücksichtigt werden. Testgetriebene Entwicklung (TDD) unterstützt dabei, testbaren Code zu entwerfen.

## 4. SOLID als Grundlage für testbares Design

- **Single Responsibility Principle (SRP):** Eine Klasse hat nur einen Grund, sich zu ändern. Eine klare, einzelne Aufgabe erleichtert Verständnis und Tests.
- **Open/Closed Principle (OCP):** Softwareeinheiten sind für Erweiterungen offen, für Änderungen am bestehenden Code geschlossen. Abstraktionen machen Tests stabiler.
- **Liskov Substitution Principle (LSP):** Untertypen müssen ihre Basistypen ersetzen können, ohne deren grundlegendes Verhalten zu verletzen.
- **Interface Segregation Principle (ISP):** Clients sollen nicht von Methoden abhängen müssen, die sie nicht verwenden. Kleine, spezifische Schnittstellen reduzieren Kopplung.
- **Dependency Inversion Principle (DIP):** Übergeordnete und untergeordnete Module hängen von Abstraktionen ab, nicht voneinander. Abhängigkeiten werden beispielsweise über Dependency Injection gesetzt und können dadurch leicht durch Test-Doubles ersetzt werden.

## 5. Regeln und Aufbau guter Unit-Tests

Die **FIRST-Regeln** für Unit-Tests lauten:

- **Fast:** Tests laufen schnell und werden daher oft ausgeführt.
- **Independent:** Tests beeinflussen sich nicht gegenseitig.
- **Repeatable:** Tests liefern in jeder Umgebung dasselbe Ergebnis.
- **Self-validating:** Jeder Test zeigt eindeutig Erfolg oder Fehlschlag.
- **Timely:** Tests werden rechtzeitig geschrieben und verbessern das Produktionsdesign.

Beim Aufbau eines Tests wird das **AAA-Muster** verwendet:

1. **Arrange:** Testobjekt, Eingaben und Abhängigkeiten vorbereiten.
2. **Act:** Die zu testende Funktion ausführen.
3. **Assert:** Das erwartete Ergebnis überprüfen.

Ein Test sollte möglichst genau einen Aspekt prüfen und eine klare Assertion besitzen. Direkte Ein- und Ausgaben, interner Zustand sowie Interaktionen mit externen Abhängigkeiten müssen vorbereitet und überprüft werden. Abhängigkeiten wie Datenbanken, Netzwerk oder Dateisystem werden bei Unit-Tests normalerweise durch Test-Doubles ersetzt.

Testnamen sollen angeben, **was** getestet wird, **unter welchen Bedingungen** und **welches Ergebnis** erwartet wird, zum Beispiel `Add_WithTwoNumbers_ReturnsSum`. Tests sollen verständlich und eigenständig sein. Für Testcode gilt daher eher **DAMP** (Descriptive And Meaningful Phrases) als **DRY** (Don't Repeat Yourself): Lesbarkeit und Verständlichkeit sind wichtiger als maximale Wiederverwendung.

## 6. Test-Doubles und Abgrenzung

Unit-Tests konzentrieren sich auf eine einzelne Einheit und beziehen keine anderen Systeme ein. Benötigte Kollaboratoren werden durch Test-Doubles ersetzt. Integrationstests prüfen dagegen das Zusammenspiel, zum Beispiel zwischen einem Service und einer Datenbank. Die Grenze zwischen Unit- und Integrationstests kann in der Praxis unscharf sein.

## 7. Sinnvolle Testfälle

Bei der **Äquivalenzklassenbildung** werden Eingaben in Gruppen eingeteilt, die gleich verarbeitet werden. Aus jeder gültigen und ungültigen Klasse wird mindestens ein repräsentativer Wert getestet.

Die **Grenzwertanalyse** ergänzt diese Werte um die Grenzen der Klassen, da dort besonders häufig Fehler auftreten. Beispiel für eine erlaubte Blutspende zwischen 18 und 70 Jahren:

- ungültig: unter 18, zum Beispiel 7 oder 15,
- gültig: 18 bis 70, zum Beispiel 25 oder 57,
- ungültig: über 70, zum Beispiel 73 oder 95,
- wichtige Grenzwerte: 17, 18, 70 und 71.

## 8. xUnit für .NET

xUnit ist ein quelloffenes und flexibles Unit-Testing-Framework für .NET. Ein Test besteht aus einer öffentlichen Testklasse und einer öffentlichen Methode mit dem Attribut `[Fact]`. Weitere wichtige Konzepte sind:

- `[Trait]` zur Kategorisierung,
- `DisplayName` für lesbarere Testnamen,
- `Skip` zum vorübergehenden Überspringen,
- `Assert`-Methoden wie `Equal`, `True`, `Throws` oder `Contains`,
- `[Theory]` für parameterisierte Tests.

Testdaten können mit `[InlineData]`, `[MemberData]` oder `[ClassData]` geliefert werden. Konstruktor und `Dispose` dienen zum Einrichten und Aufräumen. `IClassFixture` teilt eine Fixture innerhalb einer Testklasse, `ICollectionFixture` innerhalb einer Testsammlung. Die Ausführungsreihenfolge soll für Unit-Tests keine Rolle spielen. Zusätzliche Ausgaben können über `ITestOutputHelper` erfasst werden.

## 9. JUnit für Java

JUnit ist ein quelloffenes und in Java verbreitetes Unit-Testing-Framework. JUnit 5 besteht aus:

- **JUnit Platform:** Grundlage zum Starten von Test-Engines,
- **JUnit Jupiter:** Programmier- und Erweiterungsmodell für neue Tests,
- **JUnit Vintage:** Test-Engine für ältere JUnit-Versionen.

Ein Test wird mit `@Test` markiert. Häufige Annotationen sind `@DisplayName`, `@Tag` und `@Disabled`. Assertions stammen aus `Assertions`, zum Beispiel `assertEquals`, `assertTrue`, `assertNotNull`, `assertThrows` und `assertAll`.

Weitere JUnit-Funktionen:

- **Annahmen:** `assumeTrue`, `assumeFalse` und `assumingThat` überspringen Tests, wenn Voraussetzungen nicht erfüllt sind.
- **Bedingte Ausführung:** Tests können abhängig von Betriebssystem, Java-Version, Systemeigenschaften oder Umgebungsvariablen aktiviert bzw. deaktiviert werden.
- **Eigene Annotationen:** Mehrere Annotationen, Tags und Bedingungen können zu einer benutzerdefinierten Annotation kombiniert werden.
- **Fixtures:** `@BeforeAll`, `@AfterAll`, `@BeforeEach` und `@AfterEach` steuern Setup und Teardown. Die Standard-Lebensdauer erzeugt pro Testmethode eine neue Testinstanz.
- **Ausführungsreihenfolge:** Mit `@TestMethodOrder` und Orderern kann sie festgelegt werden; unabhängige Unit-Tests sollten dies nicht benötigen.
- **Verschachtelte Tests:** `@Nested` bildet eine hierarchische Struktur und gruppiert zusammengehörige Tests.
- **Parameterisierte Tests:** `@ParameterizedTest` verwendet Datenquellen wie `@ValueSource`, `@EnumSource`, `@MethodSource`, `@CsvSource`, `@NullSource` und `@EmptySource`.
- **Wiederholte Tests:** `@RepeatedTest` führt denselben Test mehrfach aus.
- **Metadaten und Berichte:** `TestInfo` liefert Testmetadaten, `TestReporter` schreibt zusätzliche Informationen in die Testausgabe.
- **Interface-Default-Methoden:** Gemeinsame Setup- und Teardown-Logik kann in Interfaces gekapselt werden.

Neben den eingebauten Assertions können Bibliotheken wie **Fluent Assertions**, **Hamcrest** oder **AssertJ** verwendet werden, wenn dadurch komplexe Prüfungen lesbarer werden.

## Kernaussage

Gute Unit-Tests sind schnell, unabhängig, verständlich und automatisiert. Sie werden durch ein testbares Design, geringe Kopplung, klare Verantwortlichkeiten und passende Testfälle unterstützt. Viele zuverlässige Unit-Tests bilden gemeinsam mit gezielten Integrations- und Systemtests die Grundlage für frühes Feedback und nachhaltige Softwarequalität.
