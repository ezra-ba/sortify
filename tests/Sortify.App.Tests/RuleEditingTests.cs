using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

public sealed class RuleEditingTests
{
    private static AppSettings Settings(bool simple)
    {
        var target = new TargetFolder(Guid.NewGuid(), Path.GetTempPath(), "School");
        FileNameCondition[] conditions = [new(FileNameConditionKind.StartsWith, "old")];
        var rule = new SortRule(Guid.NewGuid(), "Old rule", simple ? SimpleRuleCompiler.Compile(conditions).Pattern! : "^old",
            [".pdf"], "^school", "Legacy/path", "{name}{ext}", 77, true)
            { TargetFolderId = target.Id, NameConditions = simple ? conditions : null };
        return new AppSettings(Path.GetTempPath(), null, true, [rule]) { TargetFolders = [target] };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EditingPreservesIdentityHiddenFieldsAndConditionMode(bool simple)
    {
        var initial = Settings(simple);
        var store = new Store(initial);
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
        Assert.True(editor.IsEditing);
        Assert.Equal(!simple, editor.IsAdvancedMode);
        Assert.Equal("Old rule", editor.RuleName);
        Assert.Equal(initial.Rules[0].FileNamePattern, editor.FileNamePattern);
        if (simple) Assert.Equal(initial.Rules[0].NameConditions![0], editor.NameConditions[0].ToCondition());
        editor.RuleName = "Updated";
        editor.RenamePattern = "renamed_{name}{ext}";
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.False(editor.IsEditing);
        var saved = Assert.Single(main.CurrentSettings!.Rules);
        Assert.Equal(initial.Rules[0].Id, saved.Id);
        Assert.Equal(77, saved.Priority);
        Assert.True(saved.IsEnabled);
        Assert.Equal("^school", saved.RelativePathPattern);
        Assert.Equal("Legacy/path", saved.TargetDirectory);
        Assert.Equal("Updated", saved.Name);
        Assert.Equal("renamed_{name}{ext}", saved.RenamePattern);
        Assert.Equal(initial with { Rules = main.CurrentSettings.Rules }, main.CurrentSettings);
        var reloaded = new MainViewModel(new SettingsService(store));
        await reloaded.InitializeSettingsAsync();
        Assert.Equal("Updated", Assert.Single(reloaded.CurrentSettings!.Rules).Name);
    }

    [Fact]
    public async Task DeleteRequiresConfirmationAndCanBeCancelled()
    {
        var initial = Settings(true);
        var store = new Store(initial);
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        await editor.ConfirmDeleteCommand.ExecuteAsync(null);
        Assert.Equal(0, store.SaveCalls);
        editor.RequestDeleteCommand.Execute(editor.ExistingRules[0]);
        Assert.True(editor.IsDeletePending);
        Assert.Equal(0, store.SaveCalls);
        editor.CancelDeleteCommand.Execute(null);
        Assert.False(editor.IsDeletePending);
        await editor.ConfirmDeleteCommand.ExecuteAsync(null);
        Assert.Equal(0, store.SaveCalls);
        editor.RequestDeleteCommand.Execute(editor.ExistingRules[0]);
        await editor.ConfirmDeleteCommand.ExecuteAsync(null);
        Assert.False(editor.IsDeletePending);
        Assert.Empty(editor.ExistingRules);
        Assert.Empty(main.CurrentSettings!.Rules);
        Assert.Equal(initial.TargetFolders, main.CurrentSettings.TargetFolders);
        var reloaded = new MainViewModel(new SettingsService(store));
        await reloaded.InitializeSettingsAsync();
        Assert.Empty(reloaded.CurrentSettings!.Rules);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveFailureKeepsRuleAndEditOrDeleteState(bool deleting)
    {
        var initial = Settings(true);
        var store = new Store(initial) { SaveSucceeds = false };
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        if (deleting)
        {
            editor.RequestDeleteCommand.Execute(editor.ExistingRules[0]);
            await editor.ConfirmDeleteCommand.ExecuteAsync(null);
            Assert.True(editor.IsDeletePending);
        }
        else
        {
            editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
            editor.RuleName = "Draft";
            await editor.CreateRuleCommand.ExecuteAsync(null);
            Assert.True(editor.IsEditing);
            Assert.Equal("Draft", editor.RuleName);
        }
        Assert.Same(initial, main.CurrentSettings);
        Assert.Contains("Failed", editor.Message);
        Assert.False(editor.IsSaving);
    }

    [Fact]
    public async Task StaleEditAndDeleteDoNotOverwriteOtherChanges()
    {
        var original = Settings(true);
        var store = new Store(original);
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
        editor.RuleName = "Stale draft";
        editor.RequestDeleteCommand.Execute(editor.ExistingRules[0]);
        Assert.True((await main.UpdateRuleAsync(original.Rules[0], original.Rules[0] with { Name = "Other window" })).IsSuccess);
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.Equal("Stale draft", editor.RuleName);
        Assert.Contains("inzwischen geändert", editor.Message);
        await editor.ConfirmDeleteCommand.ExecuteAsync(null);
        Assert.Contains("inzwischen geändert", editor.Message);
        Assert.Equal("Other window", Assert.Single(main.CurrentSettings!.Rules).Name);
        Assert.Equal(1, store.SaveCalls);
    }

    [Fact]
    public async Task InvalidEditDoesNotWriteAndCancelLeavesOriginalUnchanged()
    {
        var initial = Settings(false);
        var store = new Store(initial);
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main);
        editor.EditRuleCommand.Execute(editor.ExistingRules[0]);
        editor.FileNamePattern = "[";
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.Equal(0, store.SaveCalls);
        Assert.True(editor.IsEditing);
        Assert.Equal("[", editor.FileNamePattern);
        editor.CancelEditCommand.Execute(null);
        Assert.False(editor.IsEditing);
        Assert.Same(initial, main.CurrentSettings);
    }

    private sealed class Store(AppSettings initial) : ISettingsStore
    {
        private AppSettings stored = initial;
        public bool SaveSucceeds { get; init; } = true;
        public int SaveCalls { get; private set; }
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsLoadResult(SettingsLoadStatus.Loaded, stored, null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            if (SaveSucceeds) stored = settings;
            return Task.FromResult(new SettingsSaveResult(SaveSucceeds, SaveSucceeds ? null : "Failed to save"));
        }
    }
}
