using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Logging;

// Compatibility adapter: settings operations no longer belong to the activity log.
public sealed class LoggedSettingsStore : ISettingsStore
{
    private readonly ISettingsStore inner;
    public LoggedSettingsStore(ISettingsStore inner, IActivityLog log) => this.inner = inner;
    public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default) => inner.LoadAsync(cancellationToken);
    public Task<SettingsSaveResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => inner.SaveAsync(settings, cancellationToken);
}
