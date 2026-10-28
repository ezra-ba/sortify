using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.Core.Tests;

public sealed class SimpleRuleCompilerTests
{
    private static readonly TargetFolder Target = new(Guid.NewGuid(), "/test", "School");
    private static SortRule Rule(params FileNameCondition[] conditions)
    {
        var compiled = SimpleRuleCompiler.Compile(conditions);
        Assert.True(compiled.IsValid);
        return new(Guid.NewGuid(), "Webentwicklung", compiled.Pattern!, [".docx"], null, "", null, 0, true)
            { TargetFolderId = Target.Id, NameConditions = conditions };
    }
    private static RuleEvaluationResult Evaluate(SortRule rule, string name, string extension = ".docx")
        => new RuleEngine(new RuleValidator(), new RuleTargetValidator()).Evaluate(
            new("/source", name, extension, name + extension, 1, DateTimeOffset.UnixEpoch), [rule], [Target]);

    [Theory]
    [InlineData("01_3_Fragenkatalog_HTM_CSS_JS_Schueler", ".DOCX", true)]
    [InlineData("01_3_Fragenkatalog_HTM_JS_Schueler", ".docx", false)]
    [InlineData("01_3_Fragenkatalog_HTM_CSS_JS_Schueler", ".png", false)]
    public void AllConditionsAndExtensionMustMatch(string name, string extension, bool matches)
    {
        var rule = Rule(new(FileNameConditionKind.Contains, "Fragenkatalog"), new(FileNameConditionKind.Contains, "CSS"));
        var result = Evaluate(rule, name, extension);
        Assert.Equal(matches ? RuleEvaluationStatus.Matched : RuleEvaluationStatus.NoMatch, result.Status);
        if (!matches) return;
        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(name + extension, candidate.FileName);
        Assert.Equal(Target.Id, candidate.TargetFolderId);
        var reason = Assert.Single(candidate.Reasons);
        Assert.Equal(extension, reason.Extension);
        Assert.Equal(rule.NameConditions!.ToArray(), reason.NameConditions!.ToArray());
    }

    [Theory]
    [InlineData(FileNameConditionKind.Contains, "C++.[test]", "xxC++.[test]yy", true)]
    [InlineData(FileNameConditionKind.Contains, "C++.[test]", "Cxxxxxxxx", false)]
    [InlineData(FileNameConditionKind.StartsWith, "song", "SONG_live", true)]
    [InlineData(FileNameConditionKind.StartsWith, "song", "my_song", false)]
    [InlineData(FileNameConditionKind.EndsWith, "live", "song_LIVE", true)]
    [InlineData(FileNameConditionKind.EndsWith, "live", "live_song", false)]
    [InlineData(FileNameConditionKind.Contains, "CSS", "SCSS", true)]
    [InlineData(FileNameConditionKind.Contains, "a b", "a_b", false)]
    [InlineData(FileNameConditionKind.Contains, "a-b", "a-b", true)]
    [InlineData(FileNameConditionKind.Contains, "a_b", "a_b", true)]
    [InlineData(FileNameConditionKind.AnyName, "", "any", true)]
    public void LiteralSubstringAndBoundarySemantics(FileNameConditionKind kind, string text, string name, bool matches)
        => Assert.Equal(matches ? RuleEvaluationStatus.Matched : RuleEvaluationStatus.NoMatch,
            Evaluate(Rule(new FileNameCondition(kind, text)), name).Status);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptySearchTextIsRejected(string text)
        => Assert.False(SimpleRuleCompiler.Compile([new(FileNameConditionKind.Contains, text)]).IsValid);

    [Fact]
    public void InvalidDefinitionsAndTamperedPatternsAreRejected()
    {
        Assert.False(SimpleRuleCompiler.Compile([]).IsValid);
        Assert.False(SimpleRuleCompiler.Compile([new((FileNameConditionKind)999, "text")]).IsValid);
        Assert.False(SimpleRuleCompiler.Compile([new(FileNameConditionKind.AnyName, ""), new(FileNameConditionKind.Contains, "x")]).IsValid);
        var rule = Rule(new FileNameCondition(FileNameConditionKind.Contains, "invoice"));
        Assert.False(new RuleValidator().Validate(rule with { FileNamePattern = ".*" }).IsValid);
        Assert.Empty(Evaluate(rule with { FileNamePattern = ".*" }, "other").Candidates);
    }
}
