using Sortify.Core.Models;

namespace Sortify.App.ViewModels;

public partial class MainViewModel
{
    public event Action? AiConfigurationChanged;

    public void NotifyAiInstructionsChanged() => AiConfigurationChanged?.Invoke();

    public async Task<SettingsSaveResult> SaveAiSettingsAsync(AiSuggestionMode mode, bool consent,
        bool shareTargetTrees, bool shareContextDirectory, string? contextDirectoryPath)
    {
        if (settingsService is null || !Enum.IsDefined(mode))
            return new(false, "KI-Einstellungen sind nicht verfügbar oder ungültig.");
        contextDirectoryPath = string.IsNullOrWhiteSpace(contextDirectoryPath)
            ? null : contextDirectoryPath.Trim();
        if (contextDirectoryPath is not null && !Path.IsPathFullyQualified(contextDirectoryPath))
            return new(false, "Bitte einen vollständigen Pfad für den KI-Kontextordner angeben.");
        if (shareContextDirectory && (contextDirectoryPath is null || !Directory.Exists(contextDirectoryPath)))
            return new(false, "Der ausgewählte KI-Kontextordner ist nicht erreichbar.");
        var result = await settingsService.UpdateAsync(current => current with
        {
            AiMode = mode,
            AiMetadataConsent = consent,
            AiShareTargetTrees = shareTargetTrees,
            AiShareContextDirectory = shareContextDirectory,
            AiContextDirectoryPath = contextDirectoryPath
        });
        SettingsStatus = result.IsSuccess ? "KI-Einstellungen gespeichert." : result.ErrorMessage ?? "Speichern fehlgeschlagen.";
        if (result.IsSuccess)
            AiConfigurationChanged?.Invoke();
        return result;
    }
}
