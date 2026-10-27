using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.Core.Tests;

public sealed class RuleEngineTests
{
    private readonly RuleEngine engine = new(
    new RuleValidator(),
    new RuleTargetValidator());

    private static readonly TargetFolder TestTarget = new(
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        Path.Combine(Path.GetTempPath(), "Sortify-TestTarget"),
        "Testziel");

    private static TargetFolder[] TestTargets => [TestTarget];

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
            Array.Empty<SortRule>(),
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.Ignored, result.Status);
        Assert.Empty(result.Candidates);
        Assert.Empty(result.Diagnostics);
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".PDF")]
    [InlineData(".png")]
    [InlineData(".xlsx")]
    public void EvaluateReturnsNoMatchWithoutRules(string extension)
    {
        var result = engine.Evaluate(
            CreateFile(extension),
            Array.Empty<SortRule>(),
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);
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
    true)
    {
        TargetFolderId = TestTarget.Id
    };
    [Fact]
    public void EvaluateSkipsDisabledRules()
    {
        var rule = CreateRule() with
        {
            IsEnabled = false,
            FileNamePattern = "["
        };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule], TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void EvaluateReportsInvalidActiveRule()
    {
        var rule = CreateRule() with { FileNamePattern = "[" };

        var result = engine.Evaluate(CreateFile(".pdf"), [rule], TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);

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

        var result = engine.Evaluate(CreateFile(".pdf"), [rule], TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Equal(2, result.Diagnostics.Count);
        Assert.Empty(result.Candidates);

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

        var result = engine.Evaluate(CreateFile(".pdf"), [rule], TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal("Dokumente", candidate.Category);
        Assert.Equal(TestTarget.Id, candidate.TargetFolderId);
        Assert.Equal("", candidate.TargetDirectory);
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

        var result = engine.Evaluate(CreateFile(".pdf"), [rule], TestTargets);

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

        var result = engine.Evaluate(CreateFile(".pdf"), [rule], TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);
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

        var result = engine.Evaluate(file, [rule], TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
    }

    [Fact]
    public void EvaluateExcludesInvalidRuleWithoutBlockingValidRule()
    {
        var invalidRule = CreateRule() with { FileNamePattern = "[" };
        var validRule = CreateRule();

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [invalidRule, validRule],
            TestTargets);

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

        var result = engine.Evaluate(CreateFile(".pdf"), rules, TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(higher.TargetFolderId, candidate.TargetFolderId);
        Assert.Equal("", candidate.TargetDirectory);
        Assert.Equal(higher.Id, Assert.Single(candidate.Rules).Id);
    }

    [Fact]
    public void EvaluateCombinesEqualPriorityRulesWithSameTarget()
    {
        var first = CreateRule();
        var second = CreateRule() with { Name = "Weitere Downloadregel" };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [first, second],
            TestTargets);

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
        var otherTarget = new TargetFolder(
            Guid.NewGuid(),
            Path.Combine(Path.GetTempPath(), "Sortify-OtherTarget"),
            "Anderes Ziel");

        var first = CreateRule();
        var second = CreateRule() with
        {
            TargetFolderId = otherTarget.Id
        };

        SortRule[] rules = reverseOrder
            ? [second, first]
            : [first, second];

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            rules,
            [TestTarget, otherTarget]);

        Assert.Equal(RuleEvaluationStatus.Ambiguous, result.Status);
        Assert.Equal(2, result.Candidates.Count);

        Assert.Contains(result.Candidates,
            candidate => candidate.TargetFolderId == TestTarget.Id);
        Assert.Contains(result.Candidates,
            candidate => candidate.TargetFolderId == otherTarget.Id);

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
            [matching, nonMatching],
            TestTargets);

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

        var result = engine.Evaluate(CreateFile(".pdf"), [rule], TestTargets);

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

        var result = engine.Evaluate(CreateFile(".PDF"), [rule], TestTargets);

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
            [first, second],
            TestTargets);

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
            [first, second],
            TestTargets);

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

        var result = engine.Evaluate(file, [rule], TestTargets);

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

        var result = engine.Evaluate(file, [rule], TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);
        Assert.Equal("download{ext}.pdf",
            Assert.Single(result.Candidates).FileName);
    }
    [Theory]
    [InlineData("CON")]
    [InlineData("rechnung?")]
    public void EvaluateDoesNotGenerateTargetNameWithoutMatchingRule(
        string name)
    {
        var file = CreateFile(".pdf") with
        {
            FileNameWithoutExtension = name,
            RelativePath = name + ".pdf",
            SourcePath = "Eingang/" + name + ".pdf"
        };

        var result = engine.Evaluate(
            file,
            Array.Empty<SortRule>(),
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);
        Assert.Empty(result.Diagnostics);
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

        var result = engine.Evaluate(file, [rule], TestTargets);

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
            [matchingRule, timeoutRule],
            TestTargets);

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
        [".pdf"], null, "Rechnungen", null, 200, true)
    {
        TargetFolderId = TestTarget.Id
    },

    new(
        Guid.NewGuid(), "Mathematik",
        @"^(?:mathe|mathematik)(?:[-_ ]|$)",
        [".pdf", ".docx"], null,
        "Schule/Mathematik", null, 150, true)
    {
        TargetFolderId = TestTarget.Id
    },

    new(
        Guid.NewGuid(), "Stundenpläne",
        @"^stundenplan(?:[-_ ]|$)",
        [".pdf", ".docx"], null,
        "Schule/Stundenpläne", null, 180, true)
    {
        TargetFolderId = TestTarget.Id
    },

    new(
        Guid.NewGuid(), "Schulpräsentationen",
        @"^(?:referat|präsentation)(?:[-_ ]|$)",
        [".pptx"], null, "Schule", null, 150, true)
    {
        TargetFolderId = TestTarget.Id
    },

    new(
        Guid.NewGuid(), "Notenlisten",
        @"^(?:noten|notenliste)(?:[-_ ]|$)",
        [".xlsx", ".csv"], null,
        "Schule/Noten", null, 180, true)
    {
        TargetFolderId = TestTarget.Id
    },

    new(
        Guid.NewGuid(), "Screenshots",
        @"^(?:screenshot|bildschirmfoto)(?:[-_ ]|$)",
        [".png", ".jpg", ".jpeg"], null,
        "Screenshots", null, 120, true)
    {
        TargetFolderId = TestTarget.Id
    },

    new(
        Guid.NewGuid(), "Quartalsübersichten",
        @"^Q[1-4][-_ ]",
        [".xlsx"], null,
        "Quartalsübersichten", null, 160, true)
    {
        TargetFolderId = TestTarget.Id
    }
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
        "rechnung_2026.pdf", "Dokumente", "Rechnungen")]
    [InlineData(
    "rechnungen_2026.pdf", "Dokumente", "Rechnungen")]
    [InlineData(
        "mathe_aufgaben.docx", "Dokumente", "Mathematik")]
    [InlineData(
        "stundenplan_2026.pdf", "Dokumente", "Stundenpläne")]
    [InlineData(
        "referat_biologie.pptx", "Präsentationen", "Schulpräsentationen")]
    [InlineData(
        "notenliste_2026.xlsx", "Tabellen", "Notenlisten")]
    [InlineData(
        "Screenshot_2026-09-30.png", "Bilder", "Screenshots")]
    [InlineData(
        "Q3_Präsentation.xlsx", "Tabellen", "Quartalsübersichten")]
    public void EvaluateMatchesConceptExamples(
        string fileName,
        string expectedCategory,
        string expectedRuleName)
    {
        var rules = CreateConceptRules();

        var result = engine.Evaluate(CreateNamedFile(fileName), rules, TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(expectedCategory, candidate.Category);
        Assert.Equal(TestTarget.Id, candidate.TargetFolderId);
        Assert.Equal("", candidate.TargetDirectory);
        Assert.Equal(fileName, candidate.FileName);

        var reference = Assert.Single(candidate.Rules);
        Assert.Equal(expectedRuleName, reference.Name);
        Assert.Equal(
            rules.Single(rule => rule.Name == expectedRuleName).Id,
            reference.Id);

        Assert.Empty(result.Diagnostics);
    }

    [Theory]
    [InlineData("mathematiker.pdf")]
    [InlineData("ferienplan.pdf")]
    [InlineData("referat_biologie.pdf")]
    [InlineData("notendienst.xlsx")]
    [InlineData("screenshotter.png")]
    [InlineData("Q5_Übersicht.xlsx")]
    public void EvaluateReturnsNoMatchForConceptCounterexamples(
        string fileName)
    {
        var result = engine.Evaluate(
            CreateNamedFile(fileName),
            CreateConceptRules(),
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);
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
            CreateConceptRules(),
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.Ignored, result.Status);
        Assert.Empty(result.Candidates);
        Assert.Empty(result.Diagnostics);
    }
    [Fact]
    public void EvaluateReportsMissingTargetReference()
    {
        var rule = CreateRule() with { TargetFolderId = null };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [rule],
            TestTargets);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("InvalidTargetReference", diagnostic.Code);
        Assert.Equal(rule.Id, diagnostic.RuleId);
        Assert.Equal(nameof(SortRule.TargetFolderId), diagnostic.FieldName);
        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);

        Assert.DoesNotContain(
            result.Candidates.SelectMany(candidate => candidate.Rules),
            reference => reference.Id == rule.Id);
    }
    [Fact]
    public void EvaluateIgnoresLegacyDirectoriesForSameTarget()
    {
        var first = CreateRule() with { TargetDirectory = "Schule" };
        var second = CreateRule() with { TargetDirectory = "Privat" };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [first, second],
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(TestTarget.Id, candidate.TargetFolderId);
        Assert.Equal("", candidate.TargetDirectory);
        Assert.Equal(2, candidate.Rules.Count);
        Assert.Empty(result.Diagnostics);
    }
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public void EvaluateRejectsEmptyOrUnknownTargetReference(string targetId)
    {
        var rule = CreateRule() with
        {
            TargetFolderId = Guid.Parse(targetId)
        };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [rule],
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.NoMatch, result.Status);
        Assert.Empty(result.Candidates);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("InvalidTargetReference", diagnostic.Code);
        Assert.Equal(rule.Id, diagnostic.RuleId);
        Assert.Equal(nameof(SortRule.TargetFolderId), diagnostic.FieldName);
    }
    [Fact]
    public void EvaluateDoesNotRedirectRuleAfterTargetWasRemoved()
    {
        var rule = CreateRule();

        var beforeRemoval = engine.Evaluate(
            CreateFile(".pdf"),
            [rule],
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, beforeRemoval.Status);
        Assert.Equal(
            TestTarget.Id,
            Assert.Single(beforeRemoval.Candidates).TargetFolderId);

        // Gleicher Pfad und Name, aber eine andere stabile ID.
        var replacement = TestTarget with { Id = Guid.NewGuid() };

        var afterRemoval = engine.Evaluate(
            CreateFile(".pdf"),
            [rule],
            [replacement]);

        Assert.Equal(RuleEvaluationStatus.NoMatch, afterRemoval.Status);
        Assert.Empty(afterRemoval.Candidates);

        var diagnostic = Assert.Single(afterRemoval.Diagnostics);
        Assert.Equal("InvalidTargetReference", diagnostic.Code);
        Assert.Equal(rule.Id, diagnostic.RuleId);
    }
    [Fact]
    public void EvaluateExcludesUnknownTargetWithoutBlockingValidRule()
    {
        var invalidRule = CreateRule() with
        {
            TargetFolderId = Guid.NewGuid(),
            Priority = 500
        };

        var validRule = CreateRule() with
        {
            Priority = 100
        };

        var result = engine.Evaluate(
            CreateFile(".pdf"),
            [invalidRule, validRule],
            TestTargets);

        Assert.Equal(RuleEvaluationStatus.Matched, result.Status);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(TestTarget.Id, candidate.TargetFolderId);
        Assert.Equal(
            validRule.Id,
            Assert.Single(candidate.Rules).Id);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("InvalidTargetReference", diagnostic.Code);
        Assert.Equal(invalidRule.Id, diagnostic.RuleId);
    }
}