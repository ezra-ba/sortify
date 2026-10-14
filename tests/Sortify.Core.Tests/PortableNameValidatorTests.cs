using Sortify.Core.Rules;

namespace Sortify.Core.Tests;

public sealed class PortableNameValidatorTests
{
    [Theory]
    [InlineData("Rechnungen")]
    [InlineData("Stundenpläne")]
    [InlineData("Rechnungen alt")]
    [InlineData("rechnung_2026.pdf")]
    [InlineData("rechnung (1).pdf")]
    [InlineData("CONSOLE")]
    [InlineData("COM10")]
    public void IsValidAcceptsPortableNames(string name)
    {
        Assert.True(PortableNameValidator.IsValid(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("Rechnungen.")]
    [InlineData("Rechnungen ")]
    [InlineData("Rechnungen?")]
    [InlineData("Rechnungen*")]
    [InlineData("Rechnungen|alt")]
    [InlineData("Rechnungen<alt")]
    [InlineData("Rechnungen>alt")]
    [InlineData("Rechnungen\"alt")]
    [InlineData("C:")]
    [InlineData("Schule/Mathe")]
    [InlineData(@"Schule\Mathe")]
    [InlineData("rechnung\u0000.pdf")]
    [InlineData("rechnung\n.pdf")]
    [InlineData("CON")]
    [InlineData("con.pdf")]
    [InlineData("NUL.txt")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("COM1")]
    [InlineData("LPT9.pdf")]
    [InlineData("COM¹")]
    [InlineData("LPT².txt")]
    [InlineData("CONIN$")]
    [InlineData("CONOUT$")]
    public void IsValidRejectsInvalidNames(string name)
    {
        Assert.False(PortableNameValidator.IsValid(name));
    }
}