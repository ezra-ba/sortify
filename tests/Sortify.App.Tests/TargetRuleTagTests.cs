using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Infrastructure.Persistence;
using Sortify.Core.Services;

namespace Sortify.App.Tests;

public sealed class TargetRuleTagTests
{
    [Fact]
    public async Task TagsTrackSavedAddRenameDeleteAndTargetReassignment()
    {
        var targets = Enumerable.Range(1, 3).Select(index => new TargetFolder(Guid.NewGuid(), Path.GetTempPath(), "Target " + index)).ToArray();
        var first = new SortRule(Guid.NewGuid(), "First", "^invoice", [".pdf"], null, "", null, 0, true) { TargetFolderId = targets[0].Id };
        var initial = new AppSettings(null, null, false, [first]) { TargetFolders = targets };
        var store = new Store(initial);
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        Assert.Equal(first.Id, Assert.Single(main.TargetFolders[0].RuleTags).RuleId);
        Assert.True(Assert.Single(main.TargetFolders[1].RuleTags).IsMissing);

        var second = first with { Id = Guid.NewGuid(), Name = "Another name", TargetFolderId = targets[1].Id };
        Assert.True((await main.AddRuleAsync(second)).IsSuccess);
        Assert.True(main.TargetFolders[0].HasDuplicateRules);
        Assert.True(main.TargetFolders[1].HasDuplicateRules);
        Assert.Contains("Target 2", Assert.Single(main.TargetFolders[0].RuleTags).ToolTip);
        var currentFirst = main.CurrentSettings!.Rules[0];
        Assert.True((await main.UpdateRuleAsync(currentFirst, currentFirst with { Name = "Renamed" })).IsSuccess);
        Assert.Equal("Renamed", Assert.Single(main.TargetFolders[0].RuleTags).Name);
        Assert.True(main.TargetFolders[0].HasDuplicateRules);

        var currentSecond = main.CurrentSettings!.Rules[1];
        store.FailSave = true;
        var retained = Assert.Single(main.TargetFolders[1].RuleTags);
        Assert.False((await main.RemoveRuleAsync(currentSecond)).IsSuccess);
        Assert.Same(retained, Assert.Single(main.TargetFolders[1].RuleTags));
        store.FailSave = false;
        Assert.True((await main.RemoveRuleAsync(currentSecond)).IsSuccess);
        Assert.True(Assert.Single(main.TargetFolders[1].RuleTags).IsMissing);
        Assert.False(main.TargetFolders[0].HasDuplicateRules);

        Assert.True((await main.AssignRuleTargetAsync(first.Id, targets[2].Id)).IsSuccess);
        Assert.True(Assert.Single(main.TargetFolders[0].RuleTags).IsMissing);
        Assert.Equal(first.Id, Assert.Single(main.TargetFolders[2].RuleTags).RuleId);
        var current = Assert.Single(main.CurrentSettings!.Rules);
        Assert.True((await main.UpdateRuleAsync(current, current with { IsEnabled = false })).IsSuccess);
        var inactive = Assert.Single(main.TargetFolders[2].RuleTags);
        Assert.True(inactive.IsDisabled);
        Assert.False(inactive.IsMissing);
        Assert.False(inactive.IsDuplicate);
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
