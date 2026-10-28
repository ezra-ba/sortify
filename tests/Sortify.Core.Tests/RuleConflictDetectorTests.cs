using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.Core.Tests;

public sealed class RuleConflictDetectorTests
{
    private static readonly TargetFolder First = new(Guid.NewGuid(), "/first", "First");
    private static readonly TargetFolder Second = new(Guid.NewGuid(), "/second", "Second");
    private static SortRule Rule(TargetFolder target, string name = "Rule")
    {
        FileNameCondition[] conditions = [new(FileNameConditionKind.Contains, "CSS"), new(FileNameConditionKind.StartsWith, "Fragen")];
        return new(Guid.NewGuid(), name, SimpleRuleCompiler.Compile(conditions).Pattern!, [".docx", ".pdf"], null, "", null, 5, true)
            { TargetFolderId = target.Id, NameConditions = conditions };
    }

    [Fact]
    public void NamesRenamingConditionAndExtensionOrderDoNotAffectRecognition()
    {
        var first = Rule(First, "Schule");
        var second = Rule(Second, "Andere Bezeichnung");
        var conditions = second.NameConditions!.Reverse().Select(condition => condition with { Text = condition.Text.ToLowerInvariant() }).ToArray();
        second = second with { NameConditions = conditions, FileNamePattern = SimpleRuleCompiler.Compile(conditions).Pattern!,
            AllowedExtensions = [".PDF", ".DOCX", ".pdf"], RenamePattern = "copy_{name}{ext}" };
        var result = Assert.Single(RuleConflictDetector.Find([first, second], [First, Second]));
        Assert.Equal(new[] { first.Id, second.Id }, result.RuleIds);
        Assert.Equal(new[] { First.Id, Second.Id }, result.TargetIds);
    }

    [Theory]
    [InlineData("priority")]
    [InlineData("disabled")]
    [InlineData("same-target")]
    [InlineData("missing-target")]
    [InlineData("extension")]
    [InlineData("path")]
    [InlineData("name-condition")]
    [InlineData("invalid")]
    public void DifferentOrInactiveDefinitionsDoNotWarn(string scenario)
    {
        var first = Rule(First);
        var second = Rule(Second);
        second = scenario switch
        {
            "priority" => second with { Priority = 9 },
            "disabled" => second with { IsEnabled = false },
            "same-target" => second with { TargetFolderId = First.Id },
            "missing-target" => second with { TargetFolderId = Guid.NewGuid() },
            "extension" => second with { AllowedExtensions = [".png"] },
            "path" => second with { RelativePathPattern = "^other" },
            "invalid" => second with { FileNamePattern = "[" },
            _ => second with { NameConditions = [new(FileNameConditionKind.Contains, "different")],
                FileNamePattern = SimpleRuleCompiler.Compile([new(FileNameConditionKind.Contains, "different")]).Pattern! }
        };
        Assert.Empty(RuleConflictDetector.Find([first, second], [First, Second]));
    }

    [Fact]
    public void LegacyRegexComparisonIsStructuralAndDoesNotGuessOverlap()
    {
        var first = Rule(First) with { NameConditions = null, FileNamePattern = "^CSS" };
        var second = Rule(Second) with { NameConditions = null, FileNamePattern = "^CSS" };
        Assert.Single(RuleConflictDetector.Find([first, second], [First, Second]));
        Assert.Empty(RuleConflictDetector.Find([first, second with { FileNamePattern = "CSS" }], [First, Second]));
    }
}
