#fh

## Wichtige Begriffe

### `[Fact]`
Kennzeichnet in xUnit einen einzelnen Test ohne Eingabeparameter. Der Test wird mit festen Werten ausgeführt.

### `[Theory]`
Kennzeichnet in xUnit einen parametrisierten Test. Derselbe Test kann dadurch mit mehreren Eingabewerten ausgeführt werden, zum Beispiel mithilfe von `[InlineData]`.

### `[InlineData]`
Liefert bei einem `[Theory]`-Test konkrete Eingabewerte. Der Test wird für jede angegebene Datenzeile einmal ausgeführt.

### `[MemberData]`
Liefert einem `[Theory]`-Test Testdaten aus einer separaten Eigenschaft oder Methode. Damit können auch größere oder dynamisch erzeugte Datenmengen verwendet werden.

### `Assert.Equal`
Prüft, ob ein erwarteter Wert mit dem tatsächlich erhaltenen Wert übereinstimmt. Beispiel: `Assert.Equal(4, Ergebnis)`.

### `Assert.Throws`
Prüft, ob eine bestimmte Ausnahme ausgelöst wird. Der Test ist erfolgreich, wenn genau der erwartete Ausnahmetyp auftritt.

### `Assert.ThrowsAny`
Prüft, ob irgendeine Ausnahme eines bestimmten Typs oder eines davon abgeleiteten Typs ausgelöst wird. Die Prüfung ist daher weniger genau als bei `Assert.Throws`.

### `Assert.NotSame`
Prüft, ob zwei Variablen nicht auf dieselbe Objektinstanz verweisen. Die Werte der Objekte dürfen dabei gleich sein.

### ExplorationTests
Tests, die zum Erkunden und Verstehen einer unbekannten Bibliothek oder API geschrieben werden. Sie helfen dabei, das tatsächliche Verhalten einer Funktion praktisch auszuprobieren.

### Lambda-Funktion
Eine kurze, anonyme Funktion ohne eigenen Methodennamen. In Unit-Tests wird sie häufig verwendet, um den auszuführenden Code an `Assert.Throws` oder `Assert.ThrowsAny` zu übergeben, zum Beispiel: `() => MethodeMitFehler()`.

### `yield return`
Gibt in einem Iterator jeweils ein Element zurück, ohne die gesamte Ergebnisliste sofort zu erstellen. Beim nächsten Zugriff wird die Ausführung an dieser Stelle fortgesetzt.

### Permutation
Eine mögliche Anordnung aller Elemente einer Menge. Bei drei Elementen gibt es beispielsweise sechs verschiedene Permutationen.

### TSPSolver
Eine Klasse oder Methode zur Lösung des Problems des Handlungsreisenden (Travelling Salesman Problem). Sie sucht eine möglichst kurze Rundreise, die mehrere Orte genau einmal besucht und zum Ausgangspunkt zurückkehrt.

### SUT (System Under Test)
Bezeichnung für das System, die Klasse oder die Methode, die durch einen Test überprüft wird.
