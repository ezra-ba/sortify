using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.App.ViewModels;

public partial class MainViewModel
{
    public Task<SettingsSaveResult> AddRuleAsync(SortRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        var snapshot = SnapshotRule(rule);
        return ChangeRuleAsync(current =>
        {
            ValidateRule(snapshot, current);
            if (current.Rules.Any(existing => existing.Id == snapshot.Id))
                throw new RuleChangeRejectedException("Eine Regel mit dieser ID ist bereits vorhanden.");
            return current with { Rules = current.Rules.Append(snapshot).ToArray() };
        }, "Sortierregel angelegt.");
    }

    public Task<SettingsSaveResult> UpdateRuleAsync(SortRule original, SortRule replacement)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(replacement);
        var snapshot = SnapshotRule(replacement);
        return ChangeRuleAsync(current =>
        {
            RequireUnchangedRule(original, current);
            if (snapshot.Id != original.Id)
                throw new RuleChangeRejectedException("Die Regel-ID darf nicht geändert werden.");
            ValidateRule(snapshot, current);
            return current with { Rules = current.Rules.Select(rule => rule.Id == original.Id ? snapshot : rule).ToArray() };
        }, "Sortierregel aktualisiert.");
    }

    public Task<SettingsSaveResult> RemoveRuleAsync(SortRule original)
    {
        ArgumentNullException.ThrowIfNull(original);
        return ChangeRuleAsync(current =>
        {
            RequireUnchangedRule(original, current);
            return current with { Rules = current.Rules.Where(rule => rule.Id != original.Id).ToArray() };
        }, "Sortierregel entfernt.");
    }

    private static SortRule SnapshotRule(SortRule rule) => rule with
    {
        AllowedExtensions = rule.AllowedExtensions.ToArray(),
        NameConditions = rule.NameConditions?.ToArray()
    };

    private static void ValidateRule(SortRule rule, AppSettings settings)
    {
        var errors = new RuleValidator().Validate(rule).Errors
            .Concat(new RuleTargetValidator().Validate(rule, settings.TargetFolders).Errors)
            .Select(error => error.Message).ToList();
        if (rule.Id == Guid.Empty) errors.Add("Die Regel-ID darf nicht leer sein.");
        if (errors.Count > 0) throw new RuleChangeRejectedException(string.Join(Environment.NewLine, errors));
    }

    private static void RequireUnchangedRule(SortRule original, AppSettings settings)
    {
        var current = settings.Rules.FirstOrDefault(rule => rule.Id == original.Id);
        if (current is null)
            throw new RuleChangeRejectedException("Die Regel wurde inzwischen entfernt. Bitte die Übersicht neu öffnen.");
        if (current != original)
            throw new RuleChangeRejectedException("Die Regel wurde inzwischen geändert. Bitte neu laden; dein Entwurf bleibt erhalten.");
    }

    private async Task<SettingsSaveResult> ChangeRuleAsync(Func<AppSettings, AppSettings> update, string successMessage)
    {
        if (IsSavingSettings)
            return new(false, "Eine andere Änderung wird gerade gespeichert. Bitte anschließend erneut versuchen.");
        IsSavingSettings = true;
        try
        {
            if (settingsService?.Current is null)
            {
                SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
                return new(false, SettingsStatus);
            }
            var result = await settingsService.UpdateAsync(update);
            SettingsStatus = result.IsSuccess ? successMessage : result.ErrorMessage ?? "Die Regeländerung konnte nicht gespeichert werden.";
            return result;
        }
        catch (RuleChangeRejectedException exception)
        {
            SettingsStatus = exception.Message;
            return new(false, exception.Message);
        }
        finally
        {
            IsSavingSettings = false;
            RefreshRuleTargetWarnings();
        }
    }

    private sealed class RuleChangeRejectedException(string message) : Exception(message);
}
