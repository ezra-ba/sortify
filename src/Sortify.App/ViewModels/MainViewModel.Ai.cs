using Sortify.Core.Models;

namespace Sortify.App.ViewModels;

public partial class MainViewModel
{
    public event Action? AiConfigurationChanged;

    public async Task<SettingsSaveResult> SaveAiSettingsAsync(AiSuggestionMode mode, bool consent)
    {
        if (settingsService is null || !Enum.IsDefined(mode))
            return new(false, "KI-Einstellungen sind nicht verfügbar oder ungültig.");
        var result = await settingsService.UpdateAsync(current => current with
        {
            AiMode = mode,
            AiMetadataConsent = consent
        });
        SettingsStatus = result.IsSuccess ? "KI-Einstellungen gespeichert." : result.ErrorMessage ?? "Speichern fehlgeschlagen.";
        if (result.IsSuccess)
            AiConfigurationChanged?.Invoke();
        return result;
    }
}
