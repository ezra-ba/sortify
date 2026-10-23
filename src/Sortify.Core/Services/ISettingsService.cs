using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface ISettingsService
{
    AppSettings? Current { get; }

    Task<SettingsLoadResult> InitializeAsync(
        CancellationToken cancellationToken = default);

    Task<SettingsSaveResult> SaveAsync(
        AppSettings settings,
        CancellationToken cancellationToken = default);

    Task<SettingsSaveResult> UpdateAsync(
        Func<AppSettings, AppSettings> update,
        CancellationToken cancellationToken = default);
}