using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

public sealed class RuleActivationTests
{
    private static AppSettings Settings(bool targetExists = true)
    {
        var target = new TargetFolder(Guid.NewGuid(), Path.GetTempPath(), "Documents");
        var rule = new SortRule(Guid.NewGuid(), "Invoices", "^invoice", [".pdf"], null, "", null, 0, true)
            { TargetFolderId = target.Id };
        return new AppSettings(null, null, false, [rule]) { TargetFolders = targetExists ? [target] : [] };
    }

    [Fact]
    public async Task NewRulesAreAlwaysActiveAndCancelDoesNotChangeExistingStatus()
    {
        var main = new MainViewModel(new SettingsService(new Store(Settings())));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        Assert.True(editor.IsRuleActive);
        editor.RuleName = "New";
        editor.IsAdvancedMode = true;
        editor.FileNamePattern = "^new";
        editor.SelectedTarget = main.TargetFolders[0];
        editor.IsRuleActive = false; // Creation ignores an inactive draft flag; no checkbox is offered there.
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.True(main.CurrentSettings!.Rules.Single(rule => rule.Name == "New").IsEnabled);
        Assert.True(editor.IsRuleActive);
        editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
        editor.IsRuleActive = false;
        Assert.False(editor.CanEditRuleFields);
        editor.CancelEditCommand.Execute(null);
        Assert.All(main.CurrentSettings.Rules, rule => Assert.True(rule.IsEnabled));
    }

    [Fact]
    public async Task FailedDeactivationPreservesSavedRuleAndPendingCheckboxState()
    {
        var initial = Settings();
        var main = new MainViewModel(new SettingsService(new Store(initial) { FailSave = true }));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
        editor.IsRuleActive = false;
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.Same(initial, main.CurrentSettings);
        Assert.True(editor.IsEditing);
        Assert.False(editor.IsRuleActive);
        Assert.False(editor.CanEditRuleFields);
        Assert.False(editor.IsSaving);
        Assert.False(Assert.Single(main.TargetFolders[0].RuleTags).IsDisabled);
        Assert.Contains("Test failure", editor.Message);
    }

    [Fact]
    public async Task OrphanRuleCanBeDeactivatedButCannotBeReactivatedUntilTargetIsRepaired()
    {
        var store = new Store(Settings(false));
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
        Assert.Null(editor.SelectedTarget);
        editor.IsRuleActive = false;
        await editor.CreateRuleCommand.ExecuteAsync(null);
        var inactive = Assert.Single(main.CurrentSettings!.Rules);
        Assert.False(inactive.IsEnabled);
        Assert.Equal(1, store.SaveCalls);
        var enabled = await main.SetRuleEnabledAsync(inactive, true);
        Assert.False(enabled.IsSuccess);
        Assert.False(Assert.Single(main.CurrentSettings.Rules).IsEnabled);
        Assert.Equal(1, store.SaveCalls);
    }

    private sealed class Store(AppSettings settings) : ISettingsStore
    {
        public bool FailSave { get; init; }
        public int SaveCalls { get; private set; }
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsLoadResult(SettingsLoadStatus.Loaded, settings, null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings updated, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(new SettingsSaveResult(!FailSave, FailSave ? "Test failure" : null));
        }
    }
}
