using Sortify.Core.Models;

namespace Sortify.Core.Tests;

public sealed class SortRuleTests
{
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
