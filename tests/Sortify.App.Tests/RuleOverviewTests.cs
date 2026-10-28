using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

public sealed class RuleOverviewTests
{
    [Fact]
    public async Task EmptyCreateFailureSuccessEditCancelAndLastDeletionHaveExclusiveViews()
    {
        var target = new TargetFolder(Guid.NewGuid(), Path.GetTempPath(), "Documents");
        var store = new Store(new AppSettings(null, null, false, []) { TargetFolders = [target] });
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        Assert.True(editor.ShowEmptyState);
        Assert.False(editor.ShowRuleList);
        Assert.False(editor.IsFormVisible);
        editor.BeginCreateForTarget(main.TargetFolders[0]);
        Assert.True(editor.IsFormVisible);
        Assert.False(editor.IsOverview);
        Assert.False(editor.IsEditing);
        editor.RuleName = "Invoices";
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.True(editor.IsFormVisible);
        editor.NameConditions[0].Text = "invoice";
        store.FailSave = true;
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.True(editor.IsFormVisible);
        store.FailSave = false;
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.True(editor.ShowRuleList);
        Assert.False(editor.IsFormVisible);
        Assert.Equal("1 Regel", editor.RuleCountLabel);
        editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
        Assert.True(editor.IsFormVisible);
        Assert.True(editor.IsEditing);
        Assert.False(editor.ShowRuleList);
        editor.RuleName = "Unsaved";
        editor.CancelEditCommand.Execute(null);
        Assert.True(editor.ShowRuleList);
        Assert.Equal("Invoices", Assert.Single(main.CurrentSettings!.Rules).Name);
        editor.RequestDeleteCommand.Execute(editor.ExistingRules[0]);
        await editor.ConfirmDeleteCommand.ExecuteAsync(null);
        Assert.True(editor.ShowEmptyState);
        Assert.False(editor.ShowRuleList);
        Assert.False(editor.IsFormVisible);
        Assert.Empty(main.CurrentSettings!.Rules);
    }

    private sealed class Store(AppSettings settings) : ISettingsStore
    {
        public bool FailSave { get; set; }
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsLoadResult(SettingsLoadStatus.Loaded, settings, null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings updated, CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsSaveResult(!FailSave, FailSave ? "Test error" : null));
    }
}
