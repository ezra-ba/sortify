using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

public sealed class LoginStartupSettingsTests
{
    [Fact]
    public async Task UnsupportedAutoStartIsNotSavedAsEnabledButOtherPreferencesCanStillBeSaved()
    {
        var store = new Store();
        var model = new MainViewModel(new SettingsService(store), new UnsupportedStartup());
        await model.InitializeSettingsAsync();
        var result = await model.SaveGeneralSettingsAsync(true, true, true);
        Assert.False(result.IsSuccess);
        Assert.Contains("Autostart", model.SettingsStatus);
        Assert.Equal(0, store.Saves);
        Assert.False(model.CurrentSettings!.AutoStart);
        Assert.True((await model.SaveGeneralSettingsAsync(false, false, false)).IsSuccess);
        Assert.Equal(1, store.Saves);
    }

    private sealed class UnsupportedStartup : ILoginStartup
    {
        public bool IsSupported => false;
        public bool IsEnabled => false;
        public void SetEnabled(bool enabled) => throw new InvalidOperationException("Must not be called");
    }

    private sealed class Store : ISettingsStore
    {
        public int Saves { get; private set; }
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsLoadResult(SettingsLoadStatus.Loaded, new AppSettings(null, null, false, []), null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            Saves++;
            return Task.FromResult(new SettingsSaveResult(true, null));
        }
    }
}
