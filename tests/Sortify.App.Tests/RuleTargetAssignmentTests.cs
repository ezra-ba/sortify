using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

public sealed class RuleTargetAssignmentTests
{
    private static AppSettings CreateSettings()
    {
        var target = new TargetFolder(
            Guid.NewGuid(),
            Path.Combine(Path.GetTempPath(), "Sortify-Invoices"),
            "Rechnungen");

        var rule = new SortRule(
            Guid.NewGuid(),
            "Rechnungsregel",
            "^rechnung",
            [".pdf"],
            null,
            "AltesZiel",
            "{name}{ext}",
            100,
            true);

        return new AppSettings(
            Path.Combine(Path.GetTempPath(), "Sortify-Inbox"),
            Path.Combine(Path.GetTempPath(), "Sortify-Archive"),
            true,
            [rule])
        {
            TargetFolders = [target],
            DefaultTargetFolderId = target.Id
        };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AssignmentUpdatesStateOnlyAfterSuccessfulSave(
        bool saveSucceeds)
    {
        var original = CreateSettings();
        var store = new TestStore(original, saveSucceeds);
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        Assert.True(viewModel.HasRuleTargetWarnings);
        Assert.Contains("Rechnungsregel", viewModel.RuleTargetWarnings);

        var rule = original.Rules[0];
        var targetId = original.TargetFolders[0].Id;

        var result = await viewModel.AssignRuleTargetAsync(
            rule.Id,
            targetId);

        Assert.Equal(saveSucceeds, result.IsSuccess);
        Assert.False(viewModel.IsSavingSettings);
        Assert.Equal(1, store.SaveCalls);
        Assert.NotNull(service.Current);

        var expectedRule = saveSucceeds
            ? rule with { TargetFolderId = targetId }
            : rule;

        Assert.Equal(expectedRule, Assert.Single(service.Current.Rules));
        Assert.Equal(
            original.WatchedFolderPath,
            service.Current.WatchedFolderPath);
        Assert.Equal(
            original.ArchiveRootPath,
            service.Current.ArchiveRootPath);
        Assert.Equal(
            original.IncludeSubdirectories,
            service.Current.IncludeSubdirectories);
        Assert.Equal(
            original.DefaultTargetFolderId,
            service.Current.DefaultTargetFolderId);
        Assert.Equal(
            original.TargetFolders.ToArray(),
            service.Current.TargetFolders.ToArray());

        Assert.Equal(!saveSucceeds, viewModel.HasRuleTargetWarnings);

        if (!saveSucceeds)
        {
            Assert.Equal("Test-Speicherfehler.", viewModel.SettingsStatus);
        }
    }

    [Theory]
    [InlineData("unknown-rule")]
    [InlineData("unknown-target")]
    [InlineData("empty-target")]
    public async Task InvalidSelectionDoesNotWriteSettings(string scenario)
    {
        var original = CreateSettings();
        var store = new TestStore(original, true);
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        var ruleId = scenario == "unknown-rule"
            ? Guid.NewGuid()
            : original.Rules[0].Id;

        var targetId = scenario switch
        {
            "unknown-target" => Guid.NewGuid(),
            "empty-target" => Guid.Empty,
            _ => original.TargetFolders[0].Id
        };

        var result = await viewModel.AssignRuleTargetAsync(
            ruleId,
            targetId);

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(0, store.SaveCalls);
        Assert.Same(original, service.Current);
        Assert.True(viewModel.HasRuleTargetWarnings);
        Assert.False(viewModel.IsSavingSettings);
    }

    [Fact]
    public async Task RemovingReferencedTargetShowsWarningImmediately()
    {
        var settings = CreateSettings();
        settings = settings with
        {
            Rules =
            [
                settings.Rules[0] with
                {
                    TargetFolderId = settings.TargetFolders[0].Id
                }
            ]
        };

        var store = new TestStore(settings, true);
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();
        Assert.False(viewModel.HasRuleTargetWarnings);

        viewModel.SelectTargetFolder(
            Assert.Single(viewModel.TargetFolders));

        await viewModel.RemoveTargetFolderCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.TargetFolders);
        Assert.True(viewModel.HasRuleTargetWarnings);
        Assert.Contains("Rechnungsregel", viewModel.RuleTargetWarnings);
        Assert.False(viewModel.IsSavingSettings);

        Assert.NotNull(service.Current);
        Assert.Equal(
            settings.Rules[0].TargetFolderId,
            Assert.Single(service.Current.Rules).TargetFolderId);
    }

    private sealed class TestStore : ISettingsStore
    {
        private readonly AppSettings initial;
        private readonly bool saveSucceeds;

        public int SaveCalls { get; private set; }

        public TestStore(AppSettings initial, bool saveSucceeds)
        {
            this.initial = initial;
            this.saveSucceeds = saveSucceeds;
        }

        public Task<SettingsLoadResult> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new SettingsLoadResult(
                SettingsLoadStatus.Loaded,
                initial,
                null));
        }

        public Task<SettingsSaveResult> SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveCalls++;

            return Task.FromResult(new SettingsSaveResult(
                saveSucceeds,
                saveSucceeds ? null : "Test-Speicherfehler."));
        }
    }
}