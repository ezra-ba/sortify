using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface ISettingsStore
{
    Task<SettingsLoadResult> LoadAsync(
        CancellationToken cancellationToken = default);

    Task<SettingsSaveResult> SaveAsync(
        AppSettings settings,
        CancellationToken cancellationToken = default);
}