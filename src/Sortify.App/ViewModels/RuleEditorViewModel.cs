using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.App.Services;

namespace Sortify.App.ViewModels;

public partial class RuleEditorViewModel : ViewModelBase
{
    private readonly MainViewModel main;

    public RuleEditorViewModel(MainViewModel main)
    {
        this.main = main;
        RefreshRules();
        AddCondition();
    }

    public ObservableCollection<TargetFolderViewModel> TargetFolders => main.TargetFolders;
    public ObservableCollection<FileNameConditionViewModel> NameConditions { get; } = [];
    [ObservableProperty] public partial bool IsAdvancedMode { get; set; }
    [RelayCommand] private void AddCondition() => NameConditions.Add(new(condition => NameConditions.Remove(condition)));

    public ObservableCollection<RuleListItem> ExistingRules { get; } = [];

    [ObservableProperty] public partial string RuleName { get; set; } = "";
    [ObservableProperty] public partial string FileNamePattern { get; set; } = "";
    [ObservableProperty] public partial string Extensions { get; set; } = ".pdf";
    [ObservableProperty] public partial string RenamePattern { get; set; } = "";
    [ObservableProperty] public partial TargetFolderViewModel? SelectedTarget { get; set; }
    [ObservableProperty] public partial RuleListItem? SelectedExistingRule { get; set; }
    [ObservableProperty] public partial TargetFolderViewModel? RepairTarget { get; set; }
    [ObservableProperty] public partial string Message { get; set; } = "";
    [ObservableProperty] public partial string MessageColor { get; set; } = "#B42318";
    [ObservableProperty] public partial bool IsSaving { get; set; }

    public void RefreshRules()
    {
        var selectedId = SelectedExistingRule?.Id;
        ExistingRules.Clear();
        if (main.CurrentSettings is { } settings)
        {
            foreach (var rule in settings.Rules)
            {
                var target = settings.TargetFolders.FirstOrDefault(folder => folder.Id == rule.TargetFolderId);
                ExistingRules.Add(new(rule.Id, rule.Name,
                    target is null ? "Zielzuordnung fehlt" : target.Nickname,
                    (rule.NameConditions is null ? "Regex: " + rule.FileNamePattern
                        : string.Join(" und ", rule.NameConditions.Select(RuleExplanationFormatter.Describe)))
                    + " · " + string.Join(", ", rule.AllowedExtensions)));
            }
        }
        SelectedExistingRule = ExistingRules.FirstOrDefault(rule => rule.Id == selectedId);
    }

    [RelayCommand]
    private async Task CreateRuleAsync()
    {
        if (IsSaving) return;
        if (SelectedTarget is null || !TargetFolders.Contains(SelectedTarget))
        {
            ShowResult(new(false, "Bitte einen vorhandenen Target-Ordner auswählen."));
            return;
        }

        var extensions = Extensions.Split([',', ';', ' ', '\t', '\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(extension => extension.StartsWith('.') ? extension : "." + extension)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var conditions = IsAdvancedMode ? null : NameConditions.Select(condition => condition.ToCondition()).ToArray();
        var pattern = FileNamePattern;
        if (conditions is not null)
        {
            var compiled = SimpleRuleCompiler.Compile(conditions);
            if (!compiled.IsValid)
            {
                ShowResult(new(false, string.Join(Environment.NewLine, compiled.Errors.Select(error => error.Message))));
                return;
            }
            pattern = compiled.Pattern!;
        }
        var rule = new SortRule(Guid.NewGuid(), RuleName.Trim(), pattern,
            extensions, null, "", string.IsNullOrWhiteSpace(RenamePattern) ? null : RenamePattern,
            0, true) { TargetFolderId = SelectedTarget.Id, NameConditions = conditions };

        IsSaving = true;
        try
        {
            var result = await main.AddRuleAsync(rule);
            ShowResult(result);
            if (result.IsSuccess)
            {
                RefreshRules();
                RuleName = "";
                FileNamePattern = "";
                NameConditions.Clear();
                AddCondition();
                RenamePattern = "";
            }
        }
        finally { IsSaving = false; }
    }

    [RelayCommand]
    private async Task AssignTargetAsync()
    {
        if (IsSaving) return;
        if (SelectedExistingRule is null || RepairTarget is null || !TargetFolders.Contains(RepairTarget))
        {
            ShowResult(new(false, "Bitte eine vorhandene Regel und einen Target-Ordner auswählen."));
            return;
        }

        var ruleId = SelectedExistingRule.Id;
        var targetId = RepairTarget.Id;
        IsSaving = true;
        try
        {
            var result = await main.AssignRuleTargetAsync(ruleId, targetId);
            ShowResult(result);
            if (result.IsSuccess) RefreshRules();
        }
        finally { IsSaving = false; }
    }

    private void ShowResult(SettingsSaveResult result)
    {
        Message = result.IsSuccess ? main.SettingsStatus : result.ErrorMessage ?? "Speichern fehlgeschlagen.";
        MessageColor = result.IsSuccess ? "#15803D" : "#B42318";
    }
}

public sealed record RuleListItem(Guid Id, string Name, string TargetName, string Conditions)
{
    public string Label => Name + " → " + TargetName;
}
