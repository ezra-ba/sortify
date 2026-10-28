using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Logging;

public sealed class LoggedSettingsStore(ISettingsStore inner, IActivityLog log) : ISettingsStore
{
    public async Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await inner.LoadAsync(cancellationToken);
            log.Record(result.Status == SettingsLoadStatus.Failed ? LogLevel.Error : LogLevel.Information,
                "Settings.Load." + result.Status, "Einstellungen geladen: " + result.Status);
            return result;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception)
        {
            log.Record(LogLevel.Error, "Settings.Load.Exception", "Unerwarteter Fehler beim Laden der Einstellungen.");
            throw;
        }
    }

    public async Task<SettingsSaveResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await inner.SaveAsync(settings, cancellationToken);
            log.Record(result.IsSuccess ? LogLevel.Information : LogLevel.Error,
                result.IsSuccess ? "Settings.Save.Succeeded" : "Settings.Save.Failed",
                result.IsSuccess ? "Einstellungen gespeichert." : "Einstellungen konnten nicht gespeichert werden.");
            return result;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception)
        {
            log.Record(LogLevel.Error, "Settings.Save.Exception", "Unerwarteter Fehler beim Speichern der Einstellungen.");
            throw;
        }
    }
}
