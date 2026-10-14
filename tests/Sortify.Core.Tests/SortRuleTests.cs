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
}