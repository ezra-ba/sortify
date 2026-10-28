using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.App.ViewModels;

public partial class MainViewModel
{
    /// <summary>
    /// Adds one validated rule without replacing existing rules or other settings.
    /// The UI supplies a new rule ID and an explicitly selected target ID.
    /// </summary>
    public async Task<SettingsSaveResult> AddRuleAsync(SortRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (IsSavingSettings)
        {
            return new(false,
                "Eine andere Änderung wird gerade gespeichert. Bitte anschließend erneut versuchen.");
        }

        IsSavingSettings = true;
        try
        {
            if (settingsService?.Current is null)
            {
                SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
                return new(false, SettingsStatus);
            }

            // Do not retain a mutable collection owned by the editor.
            var snapshot = rule with { AllowedExtensions = rule.AllowedExtensions.ToArray(),
                NameConditions = rule.NameConditions?.ToArray() };
            var result = await settingsService.UpdateAsync(current =>
            {
                // Validate against the latest state while the settings service holds its lock.
                var errors = new RuleValidator().Validate(snapshot).Errors
                    .Concat(new RuleTargetValidator().Validate(snapshot, current.TargetFolders).Errors)
                    .Select(error => error.Message)
                    .ToList();

                if (snapshot.Id == Guid.Empty)
                    errors.Add("Die Regel-ID darf nicht leer sein.");
                else if (current.Rules.Any(existing => existing.Id == snapshot.Id))
                    errors.Add("Eine Regel mit dieser ID ist bereits vorhanden.");

                if (errors.Count > 0)
                    throw new RuleCreationRejectedException(string.Join(Environment.NewLine, errors));

                return current with { Rules = current.Rules.Append(snapshot).ToArray() };
            });

            SettingsStatus = result.IsSuccess
                ? "Sortierregel angelegt."
                : result.ErrorMessage ?? "Die Sortierregel konnte nicht gespeichert werden.";
            return result;
        }
        catch (RuleCreationRejectedException exception)
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

    private sealed class RuleCreationRejectedException(string message) : Exception(message);
}
