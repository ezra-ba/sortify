using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Tests;

public sealed class SettingsValidatorTests
{
    private readonly SettingsValidator validator =
        new(new RuleValidator());

    private static SortRule CreateRule() => new(
        Guid.NewGuid(),
        "Rechnungen",
        "^rechnung",
        [".pdf"],
        null,
        "Rechnungen",
        null,
        100,
        true);

    private static AppSettings CreateSettings() => new(
        Path.Combine(Path.GetTempPath(), "Sortify-Inbox"),
        Path.Combine(Path.GetTempPath(), "Sortify-Archive"),
        false,
        [CreateRule()]);

    [Fact]
    public void ValidateAcceptsValidSettings()
    {
        Assert.Empty(validator.Validate(CreateSettings()));
    }

    [Fact]
    public void ValidateAcceptsUnconfiguredSettings()
    {
        var settings = new AppSettings(
            null,
            null,
            false,
            Array.Empty<SortRule>());

        Assert.Empty(validator.Validate(settings));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("relative/inbox")]
    [InlineData("invalid\0path")]
    public void ValidateRejectsInvalidFolderPaths(string path)
    {
        var settings = CreateSettings() with
        {
            WatchedFolderPath = path,
            ArchiveRootPath = path
        };

        var errors = validator.Validate(settings);

        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, error => error.Contains("Eingangsordner"));
        Assert.Contains(errors, error => error.Contains("Archivordner"));
    }

    [Fact]
    public void ValidateRejectsDuplicateRuleIds()
    {
        var rule = CreateRule();
        var settings = CreateSettings() with
        {
            Rules = [rule, rule with { Name = "Weitere Regel" }]
        };

        Assert.Single(validator.Validate(settings));
    }

    [Fact]
    public void ValidateRejectsEmptyRuleId()
    {
        var settings = CreateSettings() with
        {
            Rules = [CreateRule() with { Id = Guid.Empty }]
        };

        Assert.Single(validator.Validate(settings));
    }

    [Fact]
    public void ValidateReportsInvalidRuleFields()
    {
        var settings = CreateSettings() with
        {
            Rules = [CreateRule() with { Priority = -1 }]
        };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains(nameof(SortRule.Priority), error);
    }

    [Fact]
    public void ValidateRejectsMissingRules()
    {
        var settings = CreateSettings() with { Rules = null! };

        Assert.Single(validator.Validate(settings));
    }

    [Fact]
    public void ValidateRejectsNullRuleEntry()
    {
        var settings = CreateSettings() with { Rules = [null!] };

        Assert.Single(validator.Validate(settings));
    }

    [Fact]
    public void ValidateRejectsMissingRequiredRuleFields()
    {
        var settings = CreateSettings() with
        {
            Rules =
            [
                CreateRule() with { AllowedExtensions = null! },
                CreateRule() with { TargetDirectory = null! }
            ]
        };

        Assert.Equal(2, validator.Validate(settings).Count);
    }
    private static TargetFolder CreateTarget() => new(
    Guid.NewGuid(),
    Path.Combine(Path.GetTempPath(), "Sortify-Target"),
    "Dokumente");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidateAcceptsTargetsWithOptionalDefault(bool useDefault)
    {
        var target = CreateTarget();
        var settings = CreateSettings() with
        {
            TargetFolders = [target],
            DefaultTargetFolderId = useDefault ? target.Id : null
        };

        Assert.Empty(validator.Validate(settings));
    }

    [Fact]
    public void ValidateRejectsDuplicateTargetIds()
    {
        var target = CreateTarget();
        var settings = CreateSettings() with
        {
            TargetFolders =
            [
                target,
            target with
            {
                Path = Path.Combine(Path.GetTempPath(), "OtherTarget"),
                Nickname = "Weiteres Ziel"
            }
            ]
        };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("IDs", error);
    }

    [Fact]
    public void ValidateRejectsEmptyTargetId()
    {
        var settings = CreateSettings() with
        {
            TargetFolders = [CreateTarget() with { Id = Guid.Empty }]
        };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("IDs", error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("relative/target")]
    [InlineData("invalid\0path")]
    public void ValidateRejectsInvalidTargetPath(string? path)
    {
        var settings = CreateSettings() with
        {
            TargetFolders = [CreateTarget() with { Path = path! }]
        };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("Pfad", error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRejectsBlankTargetNickname(string? nickname)
    {
        var settings = CreateSettings() with
        {
            TargetFolders = [CreateTarget() with { Nickname = nickname! }]
        };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("Spitzname", error);
    }

    [Fact]
    public void ValidateRejectsUnknownDefaultTarget()
    {
        var settings = CreateSettings() with
        {
            TargetFolders = [CreateTarget()],
            DefaultTargetFolderId = Guid.NewGuid()
        };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("Standardziel", error);
    }

    [Fact]
    public void ValidateRejectsDefaultTargetWithoutTargets()
    {
        var settings = CreateSettings() with
        {
            DefaultTargetFolderId = Guid.NewGuid()
        };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("Standardziel", error);
    }

    [Fact]
    public void ValidateRejectsMissingTargetList()
    {
        var settings = CreateSettings() with { TargetFolders = null! };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("Target-Ordnerliste", error);
    }

    [Fact]
    public void ValidateRejectsNullTargetEntry()
    {
        var settings = CreateSettings() with { TargetFolders = [null!] };

        var error = Assert.Single(validator.Validate(settings));

        Assert.Contains("leeren Eintrag", error);
    }

    [Fact]
    public void ValidateAcceptsTargetWhoseDirectoryDoesNotExist()
    {
        var missingPath = Path.Combine(
            Path.GetTempPath(),
            "Sortify-Missing-" + Guid.NewGuid().ToString("N"));

        Assert.False(Directory.Exists(missingPath));

        var settings = CreateSettings() with
        {
            TargetFolders = [CreateTarget() with { Path = missingPath }]
        };

        Assert.Empty(validator.Validate(settings));
    }
}