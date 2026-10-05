#fh

# Test Doubles - Zusammenfassung

## 1. Aufbau eines Unit-Tests

Ein Unit-Test prüft eine abgegrenzte Einheit isoliert. Dabei müssen sowohl direkte Eingaben und Ausgaben als auch interne Zustände und Interaktionen mit externen Abhängigkeiten vorbereitet beziehungsweise überprüft werden.

Das **AAA-Muster** strukturiert den Test:

1. **Arrange:** Testobjekt, Eingaben und Abhängigkeiten vorbereiten.
2. **Act:** Die zu testende Funktion ausführen.
3. **Assert:** Ergebnis oder erwartete Interaktionen überprüfen.

Externe Zustände und Kollaboratoren werden bei Unit-Tests häufig durch Test-Doubles ersetzt.

## 2. Arten von Test-Doubles

Test-Doubles ersetzen andere Einheiten, damit das System under Test (SUT) isoliert geprüft werden kann.

- **Dummy:** Wird nur benötigt, damit eine Schnittstelle erfüllt ist. Es enthält keine relevante Implementierung und wird im Test nicht verwendet.
- **Stub:** Liefert für bestimmte Aufrufe festgelegte Werte zurück.
- **Spy:** Ein Stub, der zusätzlich bestimmte Interaktionen aufzeichnet. Er ermöglicht eine teilweise Verhaltensprüfung.
- **Fake:** Eine vereinfachte, aber funktionsfähige Implementierung, zum Beispiel eine Datenbank im Arbeitsspeicher.
- **Mock:** Ersetzt einen Kollaborator und dient vor allem dazu, erwartete Interaktionen zu überprüfen. Mocks werden häufig durch Mocking-Bibliotheken erzeugt.

Mocks können sich technisch wie Dummies oder Stubs verhalten. Ihr eigentlicher Zweck ist jedoch die Prüfung des Verhaltens. Ein übermäßiger Einsatz von Test-Doubles deutet oft auf schlechte Testbarkeit oder eine zu starke Kopplung hin.

## 3. Zustands- und Verhaltensprüfung

Bei der **State Verification** wird geprüft, ob das SUT nach der Ausführung den erwarteten Zustand erreicht hat. Assertions vergleichen den aktuellen mit dem erwarteten Zustand. Entscheidend ist also das Ergebnis, nicht der Weg dorthin.

Bei der **Behavior Verification** wird geprüft, ob das SUT bestimmte Interaktionen mit einem Kollaborator ausgeführt hat. Dafür werden Mocks oder Spies verwendet.

Die Verhaltensprüfung kann wichtige Aktionen absichern, koppelt den Test aber stärker an die interne Implementierung. Dadurch werden Tests tendenziell fragiler: Eine zulässige Änderung des Ablaufs kann den Test brechen, obwohl das fachliche Ergebnis unverändert bleibt.

## 4. Moq für .NET

**Moq** ist ein Open-Source-Mocking-Framework für .NET. Es unterstützt unter anderem Lambda-Ausdrücke und LINQ-Expression-Trees. Anders als manche ältere Frameworks erzwingt Moq kein Record/Replay/Verify-Schema. Stubbing und Verifikation bleiben getrennt, und Erwartungen können auch nach der Verwendung des Mock-Objekts geprüft werden.

Der typische Ablauf lautet:

1. Mock-Objekt erstellen.
2. Falls nötig, Methoden oder Eigenschaften mit `Setup` stubben.
3. SUT ausführen.
4. Interaktionen mit `Verify` prüfen, falls dies für den Test erforderlich ist.

## 5. Stubbing mit Moq

### Feste Parameter

Mit `Setup` und `Returns` werden konkrete Rückgabewerte definiert:

```csharp
mock.Setup(x => x.ReadPersonById(1))
    .Returns(new Person("a"));
```

Nicht gestubbte Methoden liefern bei einem standardmäßigen Loose-Mock den jeweiligen Standardwert zurück, zum Beispiel `null`, `0` oder ein leeres Ergebnis. Stubs können grundsätzlich nur für überschreibbare beziehungsweise virtuelle Methoden verwendet werden.

### Beliebige oder geprüfte Parameter

Argument-Matcher erlauben allgemeinere Regeln:

- `It.IsAny<T>()`: Jeder Wert eines Typs wird akzeptiert.
- `It.Is<T>(...)`: Eine eigene Bedingung wird geprüft.
- Weitere Matcher sind unter anderem `It.IsIn`, `It.IsNotIn`, `It.IsInRange`, `It.IsNotNull` und `It.IsRegex`.

Beispiel:

```csharp
mock.Setup(x => x.ReadPersonById(It.IsAny<int>()))
    .Returns(new Person("a"));
```

### Mehrere Rückgabewerte

Mit `SetupSequence` können aufeinanderfolgende Rückgabewerte festgelegt werden:

```csharp
mock.SetupSequence(x => x.ReadPersonById(It.IsAny<int>()))
    .Returns(new Person("a"))
    .Returns(new Person("b"));
```

Nach dem Ende der Sequenz wird wieder der Standardwert zurückgegeben.

### Mehrere passende Stubs

Wenn mehrere Stubs auf einen Aufruf passen, gilt der zuletzt definierte passende Stub. Dadurch können allgemeine Standarddefinitionen im Testfall gezielt überschrieben werden. Ein später definierter Stub für die ID `3` kann beispielsweise einen zuvor definierten Stub für beliebige IDs überlagern.

### Rückgabewerte aus Argumenten und Exceptions

Ein Argument kann verwendet werden, um den Rückgabewert dynamisch zu erzeugen:

```csharp
mock.Setup(x => x.ReadPersonById(It.IsAny<int>()))
    .Returns((int id) => new Person(id, "a", 25));
```

Mit `Throws` kann ein Stub das Fehlerverhalten einer Abhängigkeit simulieren:

```csharp
mock.Setup(x => x.ReadPersonById(-1))
    .Throws<ArgumentException>();
```

## 6. Verifikation von Interaktionen

Mit `Verify` wird geprüft, ob ein Aufruf stattgefunden hat. Dabei können konkrete Parameter oder Matcher verwendet werden:

```csharp
mock.Verify(x => x.ReadPersonById(0));
mock.Verify(x => x.ReadPersonById(It.IsAny<int>()));
```

Auch die Anzahl der Aufrufe kann eingeschränkt werden, zum Beispiel mit:

- `Times.Exactly(n)`
- `Times.AtLeastOnce()`
- `Times.AtLeast(n)`
- `Times.AtMost(n)`
- `Times.Never`

`VerifyNoOtherCalls()` prüft, dass keine zusätzlichen Interaktionen stattgefunden haben. Diese Prüfung sollte sparsam eingesetzt werden, weil zu genaue Verifikationen Tests unnötig an die Implementierung binden.

## 7. Loose und Strict Mocks

Ein **Loose-Mock** ist das Standardverhalten von Moq. Nicht gestubbte Methoden liefern Standardwerte zurück.

Ein **Strict-Mock** wirft dagegen eine Exception, wenn eine nicht vorbereitete Methode aufgerufen wird. Strict-Mocks machen unerwartete Interaktionen sichtbar, können Tests aber ebenfalls stärker an konkrete Abläufe koppeln.

```csharp
var mock = new Mock<IList>(MockBehavior.Strict);
```

## 8. Mocks und Spies im Vergleich

- Ein **Mock** ist ein neues Objekt ohne eigene fachliche Implementierung. Es kann für Interfaces erstellt werden und erhält bei Bedarf Stubs für virtuelle Methoden.
- Ein **Spy** basiert auf einer vorhandenen, instanziierbaren Klasse. Die ursprüngliche Implementierung bleibt grundsätzlich erhalten, während zusätzlich Interaktionen aufgezeichnet oder einzelne virtuelle Methoden ersetzt werden.

Mit `CallBase = true` wird festgelegt, dass bei nicht gestubbten Methoden die Basisklassenimplementierung ausgeführt wird. Standardmäßig ist `CallBase` deaktiviert.

## 9. Gefahren und typische Fehler

### Oververification

Jede zusätzliche Verifikation prüft einen Teil der internen Implementierung des SUT. Zu viele oder zu präzise Erwartungen führen zu fragilen Tests, die bereits bei harmlosen Refactorings fehlschlagen. Es sollte vor allem das fachlich relevante Verhalten geprüft werden.

### Mocks überall

Mocks sind leicht zu erzeugen, aber nicht immer sinnvoll. Wenn eine reale Klasse für den Test ausreicht, sollte sie verwendet werden. Typen, die nicht selbst kontrolliert werden, sollten grundsätzlich nicht ohne guten Grund gemockt werden.

### Transitives Mocking

Wenn ein Mock wiederum einen Mock zurückgibt, ist das ein Warnsignal. Eine transitive Überprüfung von Interaktionen ist meist schwer verständlich und deutet häufig auf zu starke Kopplung oder eine ungeeignete Abstraktion hin.

## 10. Test Driven Development

Die drei Gesetze von TDD beschreiben eine enge Schleife:

1. Es darf kein Produktionscode geschrieben werden, bevor ein fehlschlagender Test existiert.
2. Es darf nur so viel Testcode geschrieben werden, wie nötig ist, damit der Test fehlschlägt. Auch nicht kompilierender Code gilt als Fehlschlag.
3. Es darf nur so viel Produktionscode geschrieben werden, wie nötig ist, damit der aktuelle Test erfolgreich wird.

Danach wird der Code refaktoriert. Reines TDD ist in der Praxis anspruchsvoll, aber das Prinzip unterstützt ein testbares Design und eine klare, gewünschte API.

## 11. Qualität von Testcode und Test-Smells

Testcode braucht dieselbe Pflege wie Produktionscode. Schlechte Tests werden mit der Zeit teuer, erschweren Änderungen und können schließlich dazu führen, dass Tests deaktiviert oder ganz aufgegeben werden. Ohne Tests steigt die Fehlerzahl und die Angst vor Änderungen nimmt zu.

Typische **Code-Smells** sind:

- **Obscure tests:** Der Test ist nicht auf Anhieb verständlich.
- **Conditional test logic:** Der Test enthält bedingte Logik, sodass Teile manchmal gar nicht ausgeführt werden.
- **Hard-to-test code:** Das Produktionsdesign lässt sich nur schwer isoliert testen.
- **Test code duplication:** Gleicher Testcode wird mehrfach kopiert.
- **Test logic in production:** Produktionscode enthält Logik, die ausschließlich Testzwecken dient.

Typische **Behavior-Smells** sind:

- **Assertion roulette:** Bei mehreren Assertions ist unklar, welche den Fehlschlag verursacht hat.
- **Erratic tests:** Tests bestehen oder scheitern scheinbar zufällig.
- **Fragile tests:** Tests brechen bei Änderungen, die das fachliche Verhalten nicht verändern.
- **Frequent debugging:** Fehler erfordern regelmäßig manuelles Debugging.
- **Manual intervention:** Tests benötigen manuelle Eingriffe.
- **Slow tests:** Tests laufen zu langsam und werden deshalb seltener ausgeführt.

## Kernaussage

Test-Doubles ermöglichen schnelle und isolierte Unit-Tests, indem sie externe Kollaboratoren kontrolliert ersetzen. Stubs liefern vorbereitete Daten, während Mocks und Spies zusätzlich Interaktionen prüfen. Diese Werkzeuge sollten gezielt eingesetzt werden: Gute Tests prüfen vor allem fachliche Ergebnisse, vermeiden unnötige Kopplung an Implementierungsdetails und bleiben auch nach Refactorings stabil.
