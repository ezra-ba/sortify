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
            "Schuldokumente",
            @"^fragenkatalog",
            extensions,
            @"^Schule/",
            "Schule/Dokumente",
            "{name}-school{ext}",
            100,
            true);

        Assert.Equal(id, rule.Id);
        Assert.Equal("Schuldokumente", rule.Name);
        Assert.Equal(@"^fragenkatalog", rule.FileNamePattern);
        Assert.Equal(extensions, rule.AllowedExtensions);
        Assert.Equal(@"^Schule/", rule.RelativePathPattern);
        Assert.Equal("Schule/Dokumente", rule.TargetDirectory);
        Assert.Equal("{name}-school{ext}", rule.RenamePattern);
        Assert.Equal(100, rule.Priority);
        Assert.True(rule.IsEnabled);
    }
    [Fact]
    public void RulePreservesTargetFolderReference()
    {
        var targetId = Guid.NewGuid();

        var rule = new SortRule(
            Guid.NewGuid(),
            "Rechnungen",
            "^rechnung",
            [".pdf"],
            null,
            "",
            null,
            100,
            true)
        {
            TargetFolderId = targetId
        };

        Assert.Equal(targetId, rule.TargetFolderId);
    }

    [Fact]
    public void LegacyRuleHasNoTargetFolderReference()
    {
        var rule = new SortRule(
            Guid.NewGuid(),
            "Alte Rechnungsregel",
            "^rechnung",
            [".pdf"],
            null,
            "Rechnungen",
            null,
            100,
            true);

        Assert.Null(rule.TargetFolderId);
        Assert.Equal("Rechnungen", rule.TargetDirectory);
    }
}