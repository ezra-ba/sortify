using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.Core.Tests;

public sealed class RuleEngineTests
{
    private readonly RuleEngine engine = new(new RuleValidator());

    private static FileDescriptor CreateFile(string extension) => new(
        "Eingang/download" + extension,
        "download",
        extension,
        "download" + extension,
        100,
        new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero));

    [Theory]
    [InlineData(".exe")]
    [InlineData(".zip")]
    [InlineData("")]
    public void EvaluateIgnoresUnsupportedExtensions(string extension)
    {
        var result = engine.Evaluate(
            CreateFile(extension),
            Array.Empty<SortRule>());

        Assert.Equal(RuleEvaluationStatus.Ignored, result.Status);
        Assert.Empty(result.Candidates);
        Assert.Empty(result.Diagnostics);
    }

    [Theory]
    [InlineData(".pdf", "Dokumente")]
    [InlineData(".PDF", "Dokumente")]
    [InlineData(".png", "Bilder")]
    [InlineData(".xlsx", "Tabellen")]
    public void EvaluateReturnsCategoryFallbackWithoutRules(
        string extension,
        string expectedCategory)
    {
        var result = engine.Evaluate(
            CreateFile(extension),
            Array.Empty<SortRule>());

        Assert.Equal(RuleEvaluationStatus.CategoryFallback, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(expectedCategory, candidate.Category);
        Assert.Equal("", candidate.TargetDirectory);
        Assert.Equal("download" + extension, candidate.FileName);
        Assert.Empty(candidate.Rules);
        Assert.Empty(result.Diagnostics);
    }
    private static SortRule CreateRule() => new(
        Guid.NewGuid(),
        "Downloads",
        "^download$",
        [".pdf"],
        null,
        "Downloads",
        null,
        100,
        true);
    [Fact]
    public void EvaluateSkipsDisabledRules()
    {
        var rule = CreateRule() with
        {
            IsEnabled = false,
            FileNamePattern = "["
        };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule]);

        Assert.Equal(RuleEvaluationStatus.CategoryFallback, result.Status);
        Assert.Single(result.Candidates);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void EvaluateReportsInvalidActiveRule()
    {
        var rule = CreateRule() with { FileNamePattern = "[" };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule]);

        Assert.Equal(RuleEvaluationStatus.CategoryFallback, result.Status);
        Assert.Single(result.Candidates);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(rule.Id, diagnostic.RuleId);
        Assert.Equal("InvalidRule", diagnostic.Code);
        Assert.Equal(nameof(SortRule.FileNamePattern), diagnostic.FieldName);
        Assert.False(string.IsNullOrWhiteSpace(diagnostic.Message));
    }

    [Fact]
    public void EvaluateReportsAllValidationErrors()
    {
        var rule = CreateRule() with
        {
            Name = "",
            Priority = -1
        };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule]);

        Assert.Equal(RuleEvaluationStatus.CategoryFallback, result.Status);
        Assert.Equal(2, result.Diagnostics.Count);

        Assert.All(result.Diagnostics, diagnostic =>
        {
            Assert.Equal(rule.Id, diagnostic.RuleId);
            Assert.Equal("InvalidRule", diagnostic.Code);
        });

        Assert.Contains(result.Diagnostics,
            diagnostic => diagnostic.FieldName == nameof(SortRule.Name));
        Assert.Contains(result.Diagnostics,
            diagnostic => diagnostic.FieldName == nameof(SortRule.Priority));
    }
    [Fact]
    public void EvaluateReturnsMatchingRule()
    {
        var rule = CreateRule();

        var result = engine.Evaluate(CreateFile(".pdf"), [rule]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal("Dokumente", candidate.Category);
        Assert.Equal("Downloads", candidate.TargetDirectory);
        Assert.Equal("download.pdf", candidate.FileName);

        var reference = Assert.Single(candidate.Rules);
        Assert.Equal(rule.Id, reference.Id);
        Assert.Equal(rule.Name, reference.Name);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void EvaluateMatchesWithoutCaseSensitivity()
    {
        var rule = CreateRule() with
        {
            FileNamePattern = "^DOWNLOAD$",
            AllowedExtensions = [".PDF"]
        };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
    }

    [Theory]
    [InlineData(".png", "^download$", null)]
    [InlineData(".pdf", "^rechnung$", null)]
    [InlineData(".pdf", "^download$", "^Schule/")]
    public void EvaluateRequiresAllRuleConditions(
        string allowedExtension,
        string namePattern,
        string? pathPattern)
    {
        var rule = CreateRule() with
        {
            AllowedExtensions = [allowedExtension],
            FileNamePattern = namePattern,
            RelativePathPattern = pathPattern
        };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule]);

        Assert.Equal(RuleEvaluationStatus.CategoryFallback, result.Status);
        Assert.Empty(Assert.Single(result.Candidates).Rules);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void EvaluateMatchesRelativePathFilter()
    {
        var rule = CreateRule() with
        {
            RelativePathPattern = "^Schule/"
        };

        var file = CreateFile(".pdf") with
        {
            RelativePath = "Schule/download.pdf"
        };

        var result = engine.Evaluate(file, [rule]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
    }

    [Fact]
    public void EvaluateExcludesInvalidRuleWithoutBlockingValidRule()
    {
        var invalidRule = CreateRule() with { FileNamePattern = "[" };
        var validRule = CreateRule();

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [invalidRule, validRule]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(validRule.Id, Assert.Single(candidate.Rules).Id);
        Assert.Equal(invalidRule.Id, Assert.Single(result.Diagnostics).RuleId);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EvaluatePrefersHighestPriorityRegardlessOfOrder(
    bool reverseOrder)
    {
        var lower = CreateRule() with
        {
            Priority = 100,
            TargetDirectory = "Allgemein"
        };

        var higher = CreateRule() with
        {
            Priority = 200,
            TargetDirectory = "Wichtig"
        };

        SortRule[] rules = reverseOrder
            ? [higher, lower]
            : [lower, higher];

        var result = engine.Evaluate(CreateFile(".pdf"), rules);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal("Wichtig", candidate.TargetDirectory);
        Assert.Equal(higher.Id, Assert.Single(candidate.Rules).Id);
    }

    [Fact]
    public void EvaluateCombinesEqualPriorityRulesWithSameTarget()
    {
        var first = CreateRule();
        var second = CreateRule() with { Name = "Weitere Downloadregel" };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [first, second]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(2, candidate.Rules.Count);
        Assert.Contains(candidate.Rules, rule => rule.Id == first.Id);
        Assert.Contains(candidate.Rules, rule => rule.Id == second.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EvaluateReturnsAmbiguousForEqualPriorityDifferentTargets(
        bool reverseOrder)
    {
        var first = CreateRule() with { TargetDirectory = "Schule" };
        var second = CreateRule() with { TargetDirectory = "Privat" };

        SortRule[] rules = reverseOrder
            ? [second, first]
            : [first, second];

        var result = engine.Evaluate(CreateFile(".pdf"), rules);

        Assert.Equal(RuleEvaluationStatus.Ambiguous, result.Status);
        Assert.Equal(2, result.Candidates.Count);
        Assert.Contains(result.Candidates,
            candidate => candidate.TargetDirectory == "Schule");
        Assert.Contains(result.Candidates,
            candidate => candidate.TargetDirectory == "Privat");
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void EvaluateDoesNotPreferNonMatchingHigherPriorityRule()
    {
        var matching = CreateRule() with { Priority = 100 };
        var nonMatching = CreateRule() with
        {
            Priority = 500,
            FileNamePattern = "^rechnung$"
        };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [matching, nonMatching]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(matching.Id, Assert.Single(candidate.Rules).Id);
    }
    [Theory]
    [InlineData("{name}{ext}", "download.pdf")]
    [InlineData("Schule_{name}{ext}", "Schule_download.pdf")]
    [InlineData("Rechnung{ext}", "Rechnung.pdf")]
    [InlineData("$1_{name}{ext}", "$1_download.pdf")]
    public void EvaluateAppliesRenamePattern(
    string pattern,
    string expectedFileName)
    {
        var rule = CreateRule() with { RenamePattern = pattern };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
        Assert.Equal(
            expectedFileName,
            Assert.Single(result.Candidates).FileName);
    }

    [Fact]
    public void EvaluatePreservesOriginalExtensionCaseWhenRenaming()
    {
        var rule = CreateRule() with
        {
            RenamePattern = "Schule_{name}{ext}"
        };

        var result = engine.Evaluate(CreateFile(".PDF"), [rule]);

        Assert.Equal("Schule_download.PDF",
            Assert.Single(result.Candidates).FileName);
    }

    [Fact]
    public void EvaluateReturnsAmbiguousForDifferentRenamedFiles()
    {
        var first = CreateRule() with
        {
            RenamePattern = "Schule_{name}{ext}"
        };
        var second = CreateRule() with
        {
            RenamePattern = "Privat_{name}{ext}"
        };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [first, second]);

        Assert.Equal(RuleEvaluationStatus.Ambiguous, result.Status);
        Assert.Equal(2, result.Candidates.Count);
    }

    [Fact]
    public void EvaluateCombinesDifferentSchemasProducingSameTarget()
    {
        var first = CreateRule();
        var second = CreateRule() with
        {
            RenamePattern = "{name}{ext}"
        };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [first, second]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
        Assert.Equal(2, Assert.Single(result.Candidates).Rules.Count);
    }

    [Fact]
    public void EvaluateRejectsInvalidGeneratedFileName()
    {
        var file = CreateFile(".pdf") with
        {
            FileNameWithoutExtension = "CON",
            RelativePath = "CON.pdf",
            SourcePath = "Eingang/CON.pdf"
        };
        var rule = CreateRule() with
        {
            FileNamePattern = ".*",
            RenamePattern = "{name}{ext}"
        };

        var result = engine.Evaluate(file, [rule]);

        Assert.Equal(RuleEvaluationStatus.Failed, result.Status);
        Assert.Empty(result.Candidates);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("InvalidFileName", diagnostic.Code);
        Assert.Equal(rule.Id, diagnostic.RuleId);
    }

    [Fact]
    public void EvaluateDoesNotReplacePlaceholdersInsideOriginalName()
    {
        var file = CreateFile(".pdf") with
        {
            FileNameWithoutExtension = "download{ext}",
            RelativePath = "download{ext}.pdf",
            SourcePath = "Eingang/download{ext}.pdf"
        };
        var rule = CreateRule() with
        {
            FileNamePattern = ".*",
            RenamePattern = "{name}{ext}"
        };

        var result = engine.Evaluate(file, [rule]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
        Assert.Equal("download{ext}.pdf",
            Assert.Single(result.Candidates).FileName);
    }
    [Theory]
    [InlineData("CON")]
    [InlineData("rechnung?")]
    public void EvaluateRejectsInvalidFallbackFileName(string name)
    {
        var file = CreateFile(".pdf") with
        {
            FileNameWithoutExtension = name,
            RelativePath = name + ".pdf",
            SourcePath = "Eingang/" + name + ".pdf"
        };

        var result = engine.Evaluate(file, Array.Empty<SortRule>());

        Assert.Equal(RuleEvaluationStatus.Failed, result.Status);
        Assert.Empty(result.Candidates);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("InvalidFileName", diagnostic.Code);
        Assert.Null(diagnostic.RuleId);
    }

    [Fact]
    public void EvaluateCanRenameInvalidOriginalNameToValidName()
    {
        var file = CreateFile(".pdf") with
        {
            FileNameWithoutExtension = "CON",
            RelativePath = "CON.pdf",
            SourcePath = "Eingang/CON.pdf"
        };

        var rule = CreateRule() with
        {
            FileNamePattern = ".*",
            RenamePattern = "Rechnung{ext}"
        };

        var result = engine.Evaluate(file, [rule]);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
        Assert.Equal(
            "Rechnung.pdf",
            Assert.Single(result.Candidates).FileName);
        Assert.Empty(result.Diagnostics);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EvaluateReturnsFailedOnRegexTimeout(bool usePathPattern)
    {
        // Absichtlich problematisches Muster mit starkem Backtracking.
        const string timeoutPattern = "^(a+)+$";
        var difficultInput = new string('a', 10_000) + "!";

        var file = CreateFile(".pdf") with
        {
            FileNameWithoutExtension = usePathPattern
                ? "download"
                : difficultInput,
            RelativePath = difficultInput + ".pdf",
            SourcePath = "Eingang/test.pdf"
        };

        var matchingRule = CreateRule() with
        {
            FileNamePattern = ".*",
            Priority = 100,
            RenamePattern = "Ergebnis{ext}"
        };

        var timeoutRule = CreateRule() with
        {
            FileNamePattern = usePathPattern ? ".*" : timeoutPattern,
            RelativePathPattern = usePathPattern ? timeoutPattern : null,
            Priority = 200
        };

        var result = engine.Evaluate(
            file,
            [matchingRule, timeoutRule]);

        Assert.Equal(RuleEvaluationStatus.Failed, result.Status);
        Assert.Empty(result.Candidates);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("RegexTimeout", diagnostic.Code);
        Assert.Equal(timeoutRule.Id, diagnostic.RuleId);
        Assert.Equal(
            usePathPattern
                ? nameof(SortRule.RelativePathPattern)
                : nameof(SortRule.FileNamePattern),
            diagnostic.FieldName);
    }
    private static SortRule[] CreateConceptRules() =>
    [
        new(
        Guid.NewGuid(), "Rechnungen",
        @"^rechnung(?:en)?(?:[-_ ]|$)",
        [".pdf"], null, "Rechnungen", null, 200, true),

    new(
        Guid.NewGuid(), "Mathematik",
        @"^(?:mathe|mathematik)(?:[-_ ]|$)",
        [".pdf", ".docx"], null,
        "Schule/Mathematik", null, 150, true),

    new(
        Guid.NewGuid(), "Stundenpläne",
        @"^stundenplan(?:[-_ ]|$)",
        [".pdf", ".docx"], null,
        "Schule/Stundenpläne", null, 180, true),

    new(
        Guid.NewGuid(), "Schulpräsentationen",
        @"^(?:referat|präsentation)(?:[-_ ]|$)",
        [".pptx"], null, "Schule", null, 150, true),

    new(
        Guid.NewGuid(), "Notenlisten",
        @"^(?:noten|notenliste)(?:[-_ ]|$)",
        [".xlsx", ".csv"], null,
        "Schule/Noten", null, 180, true),

    new(
        Guid.NewGuid(), "Screenshots",
        @"^(?:screenshot|bildschirmfoto)(?:[-_ ]|$)",
        [".png", ".jpg", ".jpeg"], null,
        "Screenshots", null, 120, true),

    new(
        Guid.NewGuid(), "Quartalsübersichten",
        @"^Q[1-4][-_ ]",
        [".xlsx"], null,
        "Quartalsübersichten", null, 160, true)
    ];

    private static FileDescriptor CreateNamedFile(string fileName) => new(
        "Eingang/" + fileName,
        Path.GetFileNameWithoutExtension(fileName),
        Path.GetExtension(fileName),
        fileName,
        100,
        new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero));
    [Theory]
    [InlineData(
        "rechnung_2026.pdf", "Dokumente", "Rechnungen", "Rechnungen")]
    [InlineData(
    "rechnungen_2026.pdf", "Dokumente", "Rechnungen", "Rechnungen")]
    [InlineData(
        "mathe_aufgaben.docx", "Dokumente",
        "Schule/Mathematik", "Mathematik")]
    [InlineData(
        "stundenplan_2026.pdf", "Dokumente",
        "Schule/Stundenpläne", "Stundenpläne")]
    [InlineData(
        "referat_biologie.pptx", "Präsentationen",
        "Schule", "Schulpräsentationen")]
    [InlineData(
        "notenliste_2026.xlsx", "Tabellen",
        "Schule/Noten", "Notenlisten")]
    [InlineData(
        "Screenshot_2026-09-30.png", "Bilder",
        "Screenshots", "Screenshots")]
    [InlineData(
        "Q3_Präsentation.xlsx", "Tabellen",
        "Quartalsübersichten", "Quartalsübersichten")]
    public void EvaluateMatchesConceptExamples(
        string fileName,
        string expectedCategory,
        string expectedDirectory,
        string expectedRuleName)
    {
        var rules = CreateConceptRules();

        var result = engine.Evaluate(CreateNamedFile(fileName), rules);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(expectedCategory, candidate.Category);
        Assert.Equal(expectedDirectory, candidate.TargetDirectory);
        Assert.Equal(fileName, candidate.FileName);

        var reference = Assert.Single(candidate.Rules);
        Assert.Equal(expectedRuleName, reference.Name);
        Assert.Equal(
            rules.Single(rule => rule.Name == expectedRuleName).Id,
            reference.Id);

        Assert.Empty(result.Diagnostics);
    }

    [Theory]
    [InlineData("mathematiker.pdf", "Dokumente")]
    [InlineData("ferienplan.pdf", "Dokumente")]
    [InlineData("referat_biologie.pdf", "Dokumente")]
    [InlineData("notendienst.xlsx", "Tabellen")]
    [InlineData("screenshotter.png", "Bilder")]
    [InlineData("Q5_Übersicht.xlsx", "Tabellen")]
    public void EvaluateUsesFallbackForConceptCounterexamples(
        string fileName,
        string expectedCategory)
    {
        var result = engine.Evaluate(
            CreateNamedFile(fileName),
            CreateConceptRules());

        Assert.Equal(RuleEvaluationStatus.CategoryFallback, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(expectedCategory, candidate.Category);
        Assert.Equal("", candidate.TargetDirectory);
        Assert.Equal(fileName, candidate.FileName);
        Assert.Empty(candidate.Rules);
        Assert.Empty(result.Diagnostics);
    }

    [Theory]
    [InlineData("rechnung_2026.exe")]
    [InlineData("rechnung.pdf.exe")]
    [InlineData("rechnung_2026")]
    public void EvaluateIgnoresUnsupportedConceptFiles(string fileName)
    {
        var result = engine.Evaluate(
            CreateNamedFile(fileName),
            CreateConceptRules());

        Assert.Equal(RuleEvaluationStatus.Ignored, result.Status);
        Assert.Empty(result.Candidates);
        Assert.Empty(result.Diagnostics);
    }
}