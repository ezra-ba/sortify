using Sortify.Core.Models;

namespace Sortify.Core.Tests;

public sealed class SortRuleTests
{
    [Fact]
    public void ConstructorPreservesRuleConfiguration()
    {
        var id = Guid.NewGuid();
        string[] extensions = [".docx"];

        var rule = new SortRule(
            id,
            "School documents",
            @"^questionnaire",
            extensions,
            @"^School/",
            "School/Documents",
            "{name}-school{ext}",
            100,
            true);

        Assert.Equal(id, rule.Id);
        Assert.Equal("School documents", rule.Name);
        Assert.Equal(@"^questionnaire", rule.FileNamePattern);
        Assert.Equal(extensions, rule.AllowedExtensions);
        Assert.Equal(@"^School/", rule.RelativePathPattern);
        Assert.Equal("School/Documents", rule.TargetDirectory);
        Assert.Equal("{name}-school{ext}", rule.RenamePattern);
        Assert.Equal(100, rule.Priority);
        Assert.True(rule.IsEnabled);
    }
}