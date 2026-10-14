using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.Core.Tests;

public sealed class RuleValidatorTests
{
    private readonly RuleValidator validator = new();

    private static SortRule CreateValidRule() => new(
        Guid.NewGuid(),
        "Rechnungen",
        @"^rechnung",
        [".pdf"],
        null,
        "Rechnungen",
        null,
        100,
        true);

    [Fact]
    public void ValidateAcceptsValidRule()
    {
        var result = validator.Validate(CreateValidRule());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRejectsBlankName(string name)
    {
        var rule = CreateValidRule() with { Name = name };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.Name), error.FieldName);
    }

    [Fact]
    public void ValidateRejectsEmptyExtensions()
    {
        var rule = CreateValidRule() with
        {
            AllowedExtensions = Array.Empty<string>()
        };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.AllowedExtensions), error.FieldName);
    }

    [Theory]
    [InlineData("pdf")]
    [InlineData(".exe")]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRejectsInvalidExtension(string extension)
    {
        var rule = CreateValidRule() with
        {
            AllowedExtensions = [extension]
        };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.AllowedExtensions), error.FieldName);
    }

    [Fact]
    public void ValidateAcceptsUppercaseExtension()
    {
        var rule = CreateValidRule() with
        {
            AllowedExtensions = [".PDF"]
        };

        var result = validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public void ValidateAcceptsPriorityBoundaries(int priority)
    {
        var rule = CreateValidRule() with { Priority = priority };

        var result = validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void ValidateRejectsOutOfRangePriority(int priority)
    {
        var rule = CreateValidRule() with { Priority = priority };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.Priority), error.FieldName);
    }

    [Fact]
    public void ValidateReturnsErrorsForAllInvalidFields()
    {
        var rule = CreateValidRule() with
        {
            Name = "",
            AllowedExtensions = Array.Empty<string>(),
            Priority = -1
        };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors,
            error => error.FieldName == nameof(SortRule.Name));
        Assert.Contains(result.Errors,
            error => error.FieldName == nameof(SortRule.AllowedExtensions));
        Assert.Contains(result.Errors,
            error => error.FieldName == nameof(SortRule.Priority));
    }
    [Fact]
    public void ValidateRejectsEmptyFileNamePattern()
    {
        var rule = CreateValidRule() with { FileNamePattern = "" };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.FileNamePattern), error.FieldName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidateRejectsInvalidRegex(bool usePathPattern)
    {
        var rule = usePathPattern
            ? CreateValidRule() with { RelativePathPattern = "[" }
            : CreateValidRule() with { FileNamePattern = "[" };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(
            usePathPattern
                ? nameof(SortRule.RelativePathPattern)
                : nameof(SortRule.FileNamePattern),
            error.FieldName);
    }

    [Theory]
    [InlineData(false, 2048, true)]
    [InlineData(false, 2049, false)]
    [InlineData(true, 2048, true)]
    [InlineData(true, 2049, false)]
    public void ValidateChecksRegexLength(
        bool usePathPattern,
        int length,
        bool expectedValid)
    {
        var pattern = new string('a', length);
        var rule = usePathPattern
            ? CreateValidRule() with { RelativePathPattern = pattern }
            : CreateValidRule() with { FileNamePattern = pattern };

        var result = validator.Validate(rule);

        Assert.Equal(expectedValid, result.IsValid);

        if (expectedValid)
        {
            Assert.Empty(result.Errors);
        }
        else
        {
            var error = Assert.Single(result.Errors);
            Assert.Equal(
                usePathPattern
                    ? nameof(SortRule.RelativePathPattern)
                    : nameof(SortRule.FileNamePattern),
                error.FieldName);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("^Schule/")]
    public void ValidateAcceptsOptionalPathPattern(string? pattern)
    {
        var rule = CreateValidRule() with
        {
            RelativePathPattern = pattern
        };

        var result = validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
    [Theory]
    [InlineData("")]
    [InlineData("Rechnungen")]
    [InlineData("Schule/Mathematik")]
    [InlineData("Rechnungen alt")]
    public void ValidateAcceptsRelativeTargetDirectory(string directory)
    {
        var rule = CreateValidRule() with
        {
            TargetDirectory = directory
        };

        var result = validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("/tmp")]
    [InlineData(@"C:\Archive")]
    [InlineData("C:/Archive")]
    [InlineData(@"\\server\archive")]
    [InlineData(@"Schule\Mathematik")]
    [InlineData("..")]
    [InlineData("../Rechnungen")]
    [InlineData("Schule/../Privat")]
    [InlineData("./Rechnungen")]
    [InlineData("Schule//Mathematik")]
    [InlineData("Rechnungen/")]
    [InlineData("   ")]
    [InlineData("Rechnungen?")]
    [InlineData("CON")]
    [InlineData("Schule/NUL.txt")]
    [InlineData("Schule/Mathematik.")]
    [InlineData("Schule/Mathematik ")]
    public void ValidateRejectsInvalidTargetDirectory(string directory)
    {
        var rule = CreateValidRule() with
        {
            TargetDirectory = directory
        };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.TargetDirectory), error.FieldName);
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{name}{ext}")]
    [InlineData("Schule_{name}{ext}")]
    [InlineData("Rechnung{ext}")]
    [InlineData("$1_{name}{ext}")]
    public void ValidateAcceptsValidRenamePattern(string? pattern)
    {
        var rule = CreateValidRule() with { RenamePattern = pattern };

        var result = validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("{name}")]
    [InlineData("{name}{ext}_neu")]
    [InlineData("{ext}{name}{ext}")]
    [InlineData("{date}_{name}{ext}")]
    [InlineData("{extension}")]
    [InlineData("{name}{ext}}")]
    [InlineData("Schule/{name}{ext}")]
    [InlineData(@"Schule\{name}{ext}")]
    [InlineData("Rechnung?{ext}")]
    [InlineData("CON{ext}")]
    [InlineData("{ext}")]
    public void ValidateRejectsInvalidRenamePattern(string pattern)
    {
        var rule = CreateValidRule() with { RenamePattern = pattern };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.RenamePattern), error.FieldName);
    }
    [Fact]
    public void ValidateReturnsErrorsAcrossAllValidationAreas()
    {
        var rule = CreateValidRule() with
        {
            Name = "",
            AllowedExtensions = [".exe"],
            Priority = -1,
            FileNamePattern = "[",
            RelativePathPattern = "[",
            TargetDirectory = "../Archiv",
            RenamePattern = "{date}{ext}"
        };

        var result = validator.Validate(rule);

        Assert.False(result.IsValid);

        string[] expectedFields =
        [
            nameof(SortRule.Name),
        nameof(SortRule.AllowedExtensions),
        nameof(SortRule.Priority),
        nameof(SortRule.FileNamePattern),
        nameof(SortRule.RelativePathPattern),
        nameof(SortRule.TargetDirectory),
        nameof(SortRule.RenamePattern)
        ];

        Assert.Equal(expectedFields.Length, result.Errors.Count);

        foreach (var field in expectedFields)
        {
            Assert.Contains(result.Errors, error => error.FieldName == field);
        }
    }
}