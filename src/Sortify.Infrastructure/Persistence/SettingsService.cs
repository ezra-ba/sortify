using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Persistence;

public sealed class SettingsService : ISettingsService
{
    private readonly ISettingsStore store;
    private readonly SemaphoreSlim gate = new(1, 1);

    private SettingsLoadResult? initializationResult;

    public AppSettings? Current { get; private set; }

    public SettingsService(ISettingsStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        this.store = store;
    }

    public async Task<SettingsLoadResult> InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (initializationResult is not null)
            {
                return initializationResult;
            }

            var result = await store.LoadAsync(cancellationToken);

            switch (result.Status)
            {
                case SettingsLoadStatus.Loaded:
                    if (result.Settings is null)
                    {
                        result = new SettingsLoadResult(
                            SettingsLoadStatus.Failed,
                            null,
                            "Die geladenen Einstellungen fehlen.");
                    }
                    else
                    {
                        Current = result.Settings;
                    }

                    break;

                case SettingsLoadStatus.NotFound:
                    Current = new AppSettings(
                        null,
                        null,
                        false,
                        Array.Empty<SortRule>());
                    break;

                case SettingsLoadStatus.Failed:
                    Current = null;
                    break;

                default:
                    result = new SettingsLoadResult(
                        SettingsLoadStatus.Failed,
                        null,
                        "Der Ladestatus der Einstellungen ist unbekannt.");
                    break;
            }

            initializationResult = result;
            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<SettingsSaveResult> SaveAsync(
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await gate.WaitAsync(cancellationToken);

        try
        {
            if (initializationResult is null)
            {
                return new SettingsSaveResult(
                    false,
                    "Die Einstellungen wurden noch nicht initialisiert.");
            }

            if (Current is null)
            {
                return new SettingsSaveResult(
                    false,
                    "Speichern ist nach einem Ladefehler gesperrt. " +
                    "Bitte zuerst die Einstellungsdatei prüfen.");
            }

            var result = await store.SaveAsync(
                settings,
                cancellationToken);

            if (result.IsSuccess)
            {
                Current = settings;
            }

            return result;
        }
        finally
        {
            gate.Release();
        }
    }
}