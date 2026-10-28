using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

public sealed class RuleCreationTests
{
    private static AppSettings Settings()
    {
        var target = new TargetFolder(Guid.NewGuid(), Path.GetTempPath(), "Documents");
        var rule = new SortRule(Guid.NewGuid(), "Existing", "^old", [".pdf"], null, "", null, 2, true)
            { TargetFolderId = target.Id };
        return new AppSettings(Path.GetTempPath(), null, false, [rule])
            { TargetFolders = [target], DefaultTargetFolderId = target.Id };
    }

    [Theory]
    [InlineData("invalid-regex")]
    [InlineData("invalid-extension")]
    [InlineData("missing-target")]
    [InlineData("unknown-target")]
    [InlineData("duplicate-id")]
    [InlineData("invalid-rename")]
    public async Task InvalidRuleDoesNotWriteOrReplaceExistingSettings(string scenario)
    {
        var original = Settings();
        var store = new Store(original);
        var model = new MainViewModel(new SettingsService(store));
        await model.InitializeSettingsAsync();
        var rule = original.Rules[0] with { Id = Guid.NewGuid() };
        rule = scenario switch
        {
            "invalid-regex" => rule with { FileNamePattern = "[" },
            "invalid-extension" => rule with { AllowedExtensions = [".not-supported"] },
            "missing-target" => rule with { TargetFolderId = null },
            "unknown-target" => rule with { TargetFolderId = Guid.NewGuid() },
            "duplicate-id" => rule with { Id = original.Rules[0].Id },
            _ => rule with { RenamePattern = "../unsafe{ext}" }
        };
        var result = await model.AddRuleAsync(rule);
        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(0, store.SaveCalls);
        Assert.Same(original, model.CurrentSettings);
        Assert.False(model.IsSavingSettings);
    }

    [Fact]
    public async Task FailedSaveKeepsEditorDraftAndExistingRules()
    {
        var original = Settings();
        var store = new Store(original) { SaveSucceeds = false };
        var main = new MainViewModel(new SettingsService(store));
        await main.InitializeSettingsAsync();
        var editor = new RuleEditorViewModel(main)
        {
            IsAdvancedMode = true, RuleName = "Invoice", FileNamePattern = "^invoice", Extensions = "PDF; .pdf",
            RenamePattern = "sorted_{name}{ext}", SelectedTarget = main.TargetFolders[0]
        };
        await editor.CreateRuleCommand.ExecuteAsync(null);
        Assert.Equal("Invoice", editor.RuleName);
        Assert.Equal("^invoice", editor.FileNamePattern);
        Assert.Equal("sorted_{name}{ext}", editor.RenamePattern);
        Assert.Contains("Test failure", editor.Message);
        Assert.Equal(1, store.SaveCalls);
        Assert.Same(original, main.CurrentSettings);
        Assert.Single(editor.ExistingRules);
        Assert.False(editor.IsSaving);
    }

    [Fact]
    public async Task AddingRulePreservesExistingSettingsAndCopiesEditorExtensions()
    {
        var original = Settings();
        var model = new MainViewModel(new SettingsService(new Store(original)));
        await model.InitializeSettingsAsync();
        var extensions = new[] { ".pdf" };
        var rule = original.Rules[0] with { Id = Guid.NewGuid(), Name = "New", AllowedExtensions = extensions };
        Assert.True((await model.AddRuleAsync(rule)).IsSuccess);
        extensions[0] = ".png";
        var saved = Assert.IsType<AppSettings>(model.CurrentSettings);
        Assert.Equal(2, saved.Rules.Count);
        Assert.Same(original.Rules[0], saved.Rules[0]);
        Assert.Equal(".pdf", Assert.Single(saved.Rules[1].AllowedExtensions));
        Assert.Equal(original with { Rules = saved.Rules }, saved);
    }

    private sealed class Store(AppSettings initial) : ISettingsStore
    {
        public bool SaveSucceeds { get; init; } = true;
        public int SaveCalls { get; private set; }
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsLoadResult(SettingsLoadStatus.Loaded, initial, null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(new SettingsSaveResult(SaveSucceeds, SaveSucceeds ? null : "Test failure"));
        }
    }
}
