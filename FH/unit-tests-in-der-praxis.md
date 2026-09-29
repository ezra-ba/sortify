#fh

## Wichtige Begriffe

### `[Fact]`
Kennzeichnet in xUnit einen einzelnen Test ohne Eingabeparameter. Der Test wird mit festen Werten ausgeführt.

### `[Theory]`
Kennzeichnet in xUnit einen parametrisierten Test. Derselbe Test kann dadurch mit mehreren Eingabewerten ausgeführt werden, zum Beispiel mithilfe von `[InlineData]`.

### `[InlineData]`
Liefert bei einem `[Theory]`-Test konkrete Eingabewerte. Der Test wird für jede angegebene Datenzeile einmal ausgeführt.

### `Assert.Equal`
Prüft, ob ein erwarteter Wert mit dem tatsächlich erhaltenen Wert übereinstimmt. Beispiel: `Assert.Equal(4, Ergebnis)`.

### `Assert.Throws`
Prüft, ob eine bestimmte Ausnahme ausgelöst wird. Der Test ist erfolgreich, wenn genau der erwartete Ausnahmetyp auftritt.

### `Assert.ThrowsAny`
Prüft, ob irgendeine Ausnahme eines bestimmten Typs oder eines davon abgeleiteten Typs ausgelöst wird. Die Prüfung ist daher weniger genau als bei `Assert.Throws`.

### ExplorationTests
Tests, die zum Erkunden und Verstehen einer unbekannten Bibliothek oder API geschrieben werden. Sie helfen dabei, das tatsächliche Verhalten einer Funktion praktisch auszuprobieren.

### Lambda-Funktion
Eine kurze, anonyme Funktion ohne eigenen Methodennamen. In Unit-Tests wird sie häufig verwendet, um den auszuführenden Code an `Assert.Throws` oder `Assert.ThrowsAny` zu übergeben, zum Beispiel: `() => MethodeMitFehler()`.

