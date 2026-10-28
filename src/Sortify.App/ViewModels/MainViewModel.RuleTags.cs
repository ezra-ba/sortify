using Sortify.Core.Rules;

namespace Sortify.App.ViewModels;

public partial class MainViewModel
{
    private void RefreshTargetRuleTags()
    {
        var settings = settingsService?.Current;
        var conflicts = settings is null ? [] : RuleConflictDetector.Find(settings.Rules, settings.TargetFolders);
        foreach (var folder in TargetFolders)
        {
            var tags = settings?.Rules.Where(rule => rule.TargetFolderId == folder.Id).Select(rule =>
            {
                var conflict = conflicts.FirstOrDefault(group => group.RuleIds.Contains(rule.Id));
                var others = conflict is null ? "" : string.Join(", ", settings.TargetFolders
                    .Where(target => target.Id != folder.Id && conflict.TargetIds.Contains(target.Id))
                    .Select(target => target.Nickname));
                var tooltip = conflict is null ? $"Regel „{rule.Name}“ bearbeiten."
                    : $"Gleiche Erkennungsbedingungen und Priorität auch bei: {others}. " +
                      "Je nach Datei sind mehrere Zielvorschläge möglich. Klicken, um die Regel zu bearbeiten.";
                if (!rule.IsEnabled) tooltip += " Diese Regel ist deaktiviert.";
                return new TargetRuleTagViewModel(folder.Id, rule.Id, rule.Name,
                    conflict is not null, !rule.IsEnabled, tooltip);
            }).ToArray() ?? [];
            folder.SetRuleTags(tags.Length > 0 ? tags : [TargetRuleTagViewModel.Missing(folder.Id)]);
        }
    }
}
