using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.Core.Tests;

public sealed class RuleTargetValidatorTests
{
    private readonly RuleTargetValidator validator = new();

    private static SortRule CreateRule(Guid? targetId) => new(
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

    [Fact]
    public void ValidateAcceptsExistingTargetReference()
    {
        var target = new TargetFolder(
            Guid.NewGuid(),
            "NichtVomValidatorGepruefterPfad",
            "Rechnungen");

        var result = validator.Validate(
            CreateRule(target.Id),
            [target]);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    public void ValidateRejectsMissingEmptyOrUnknownReference(
        string? targetId)
    {
        var rule = CreateRule(
            targetId is null ? null : Guid.Parse(targetId));

        var result = validator.Validate(
            rule,
            Array.Empty<TargetFolder>());

        Assert.False(result.IsValid);

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(SortRule.TargetFolderId), error.FieldName);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [Fact]
    public void ValidateDoesNotMatchTargetByNickname()
    {
        var target = new TargetFolder(
            Guid.NewGuid(),
            "NichtVomValidatorGepruefterPfad",
            "Rechnungen");

        var rule = CreateRule(Guid.NewGuid());

        var result = validator.Validate(rule, [target]);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }
}