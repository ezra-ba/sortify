using Sortify.Core.Models;

namespace Sortify.Core.Tests;

public sealed class SortRuleTests
{
    [Fact]
    public void ConstructorPreservesRuleConfiguration()
    {
        var id = Guid.NewGuid();

        var rule = new SortRule(
            id,
            "School documents",
            @"fragenkatalog.*\.docx$",
            "school/documents",
            "{name}-school{extension}",
            10,
            true);

        Assert.Equal(id, rule.Id);
        Assert.Equal("School documents", rule.Name);
        Assert.Equal(@"fragenkatalog.*\.docx$", rule.FileNamePattern);
        Assert.Equal("school/documents", rule.TargetDirectory);
        Assert.Equal("{name}-school{extension}", rule.RenamePattern);
        Assert.Equal(10, rule.Priority);
        Assert.True(rule.IsEnabled);
    }
}
