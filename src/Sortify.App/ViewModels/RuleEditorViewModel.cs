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
    private SortRule? editingRule;
    private SortRule? pendingDeleteRule;
    public bool IsEditing => editingRule is not null;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOverview))]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    [NotifyPropertyChangedFor(nameof(ShowRuleList))]
    public partial bool IsFormVisible { get; set; }
    public bool IsOverview => !IsFormVisible;
    public bool HasRules => ExistingRules.Count > 0;
    public bool ShowEmptyState => IsOverview && !HasRules;
    public bool ShowRuleList => IsOverview && HasRules;
    public string RuleCountLabel => ExistingRules.Count == 1 ? "1 Regel" : $"{ExistingRules.Count} Regeln";
    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditRuleFields))]
    public partial bool IsRuleActive { get; set; } = true;
    public bool CanEditRuleFields => !IsSaving && (!IsEditing || IsRuleActive);
    public string EditorTitle => IsEditing ? "Sortierregel bearbeiten" : "Neue Sortierregel";
    public string SaveButtonText => IsEditing ? "Änderungen speichern" : "Regel anlegen";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeletePending))]
    public partial string? PendingDeleteName { get; set; }
    public bool IsDeletePending => PendingDeleteName is not null;

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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    public partial string Message { get; set; } = "";
    [ObservableProperty] public partial string MessageColor { get; set; } = "#B42318";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditRuleFields))]
    public partial bool IsSaving { get; set; }

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
                    + " · " + string.Join(", ", rule.AllowedExtensions)) { IsEnabled = rule.IsEnabled });
            }
        }
        SelectedExistingRule = ExistingRules.FirstOrDefault(rule => rule.Id == selectedId);
        OnPropertyChanged(nameof(HasRules));
        OnPropertyChanged(nameof(ShowEmptyState));
        OnPropertyChanged(nameof(ShowRuleList));
        OnPropertyChanged(nameof(RuleCountLabel));
    }

    [RelayCommand]
    private void ShowOverview()
    {
        if (IsSaving) return;
        CancelDelete();
        ResetEditor();
        RefreshRules();
        Message = "";
    }

    [RelayCommand]
    private void BeginCreate()
    {
        if (IsSaving) return;
        ShowOverview();
        IsFormVisible = true;
    }

    public void BeginCreateForTarget(TargetFolderViewModel? target)
    {
        if (IsSaving) return;
        BeginCreate();
        SelectedTarget = target;
    }

    [RelayCommand]
    private async Task CreateRuleAsync()
    {
        if (IsSaving) return;
        if (editingRule is { } inactiveOriginal && !IsRuleActive)
        {
            // Disabled fields never overwrite the saved definition, including an unsaved draft.
            IsSaving = true;
            try
            {
                var deactivated = await main.SetRuleEnabledAsync(inactiveOriginal, false);
                ShowResult(deactivated);
                if (deactivated.IsSuccess)
                {
                    RefreshRules();
                    ResetEditor();
                }
            }
            finally { IsSaving = false; }
            return;
        }
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
        var original = editingRule;
        var rule = (original ?? new SortRule(Guid.NewGuid(), "", "", [], null, "", null, 0, true)) with
        {
            Name = RuleName.Trim(), FileNamePattern = pattern, AllowedExtensions = extensions,
            RenamePattern = string.IsNullOrWhiteSpace(RenamePattern) ? null : RenamePattern,
            TargetFolderId = SelectedTarget.Id, NameConditions = conditions, IsEnabled = true
        };

        IsSaving = true;
        try
        {
            var result = original is null ? await main.AddRuleAsync(rule) : await main.UpdateRuleAsync(original, rule);
            ShowResult(result);
            if (result.IsSuccess)
            {
                RefreshRules();
                ResetEditor();
            }
        }
        finally { IsSaving = false; }
    }

    public bool OpenRuleForEditing(Guid ruleId)
    {
        if (IsSaving) return false;
        RefreshRules();
        var item = ExistingRules.FirstOrDefault(rule => rule.Id == ruleId);
        if (item is null)
        {
            ShowResult(new(false, "Die ausgewählte Regel ist nicht mehr vorhanden."));
            return false;
        }
        EditRule(item);
        return true;
    }

    [RelayCommand]
    private void EditRule(RuleListItem item)
    {
        if (IsSaving) return;
        var rule = main.CurrentSettings?.Rules.FirstOrDefault(rule => rule.Id == item.Id);
        if (rule is null)
        {
            ShowResult(new(false, "Die Regel ist nicht mehr vorhanden. Bitte die Übersicht neu öffnen."));
            return;
        }
        CancelDelete();
        editingRule = rule;
        IsRuleActive = rule.IsEnabled;
        RuleName = rule.Name;
        FileNamePattern = rule.FileNamePattern;
        Extensions = string.Join(", ", rule.AllowedExtensions);
        RenamePattern = rule.RenamePattern ?? "";
        SelectedTarget = TargetFolders.FirstOrDefault(target => target.Id == rule.TargetFolderId);
        IsAdvancedMode = rule.NameConditions is null;
        NameConditions.Clear();
        if (rule.NameConditions is not null)
            foreach (var condition in rule.NameConditions)
                NameConditions.Add(new(row => NameConditions.Remove(row)) { Kind = condition.Kind, Text = condition.Text });
        else AddCondition();
        Message = "";
        IsFormVisible = true;
        NotifyEditorMode();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ShowOverview();
    }

    private void ResetEditor()
    {
        editingRule = null;
        IsFormVisible = false;
        SelectedTarget = null;
        Extensions = ".pdf";
        IsRuleActive = true;
        RuleName = "";
        FileNamePattern = "";
        RenamePattern = "";
        IsAdvancedMode = false;
        NameConditions.Clear();
        AddCondition();
        NotifyEditorMode();
    }

    private void NotifyEditorMode()
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(CanEditRuleFields));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    [RelayCommand]
    private void RequestDelete(RuleListItem item)
    {
        if (IsSaving) return;
        pendingDeleteRule = main.CurrentSettings?.Rules.FirstOrDefault(rule => rule.Id == item.Id);
        PendingDeleteName = pendingDeleteRule?.Name;
        if (pendingDeleteRule is null) ShowResult(new(false, "Die Regel ist nicht mehr vorhanden."));
    }

    [RelayCommand]
    private void CancelDelete()
    {
        if (IsSaving) return;
        pendingDeleteRule = null;
        PendingDeleteName = null;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        if (IsSaving || pendingDeleteRule is not { } original) return;
        IsSaving = true;
        try
        {
            var result = await main.RemoveRuleAsync(original);
            ShowResult(result);
            if (result.IsSuccess)
            {
                pendingDeleteRule = null;
                PendingDeleteName = null;
                if (editingRule?.Id == original.Id) ResetEditor();
                RefreshRules();
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
    public bool IsEnabled { get; init; } = true;
    public bool IsInactive => !IsEnabled;
    public string Label => Name + " → " + TargetName;
}
