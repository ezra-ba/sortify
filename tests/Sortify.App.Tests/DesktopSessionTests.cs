using Avalonia.Controls;
using Avalonia.Headless;
using Sortify.App.Services;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

[CollectionDefinition("Avalonia", DisableParallelization = true)]
public sealed class AvaloniaCollection;

[Collection("Avalonia")]
public sealed class DesktopSessionTests
{
    [Fact]
    public async Task BackgroundStartupMonitorsWithoutOpeningMainWindowAndSupportsPause()
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var directory = Path.Combine(Path.GetTempPath(), "sortify-tray-" + Guid.NewGuid());
            Directory.CreateDirectory(directory);
            var store = new MemoryStore(new AppSettings(directory, null, false, [])
            {
                StartMinimized = true
            });
            var settings = new SettingsService(store);
            var viewModel = new MainViewModel(settings);
            var window = new Window();
            using var session = new DesktopSession(viewModel, window, settings, new LoginStartup(), () => { }, true);
            FileSuggestionWindow? popup = null;
            using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((opened, _) =>
            {
                if (opened is FileSuggestionWindow suggestion)
                    popup = suggestion;
            });
            try
            {
                await session.InitializeAsync(background: true);
                Assert.False(window.IsVisible);
                Assert.Equal(directory, viewModel.CurrentSettings!.WatchedFolderPath);
                await File.WriteAllTextAsync(Path.Combine(directory, "first.pdf"), "unchanged");
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                while (popup is null)
                    await Task.Delay(25, timeout.Token);
                Assert.True(popup.IsVisible);
                Assert.Null(popup.Owner);
                Assert.False(window.IsVisible);
                session.TogglePause();
                Assert.True(session.IsPaused);
                Assert.False(popup.IsVisible);
                popup = null;
                await File.WriteAllTextAsync(Path.Combine(directory, "paused.pdf"), "unchanged");
                await Task.Delay(200);
                Assert.Null(popup);
                session.TogglePause();
                await File.WriteAllTextAsync(Path.Combine(directory, "resumed.pdf"), "unchanged");
                while (popup is null)
                    await Task.Delay(25, timeout.Token);
                session.Exit();
                Assert.False(popup.IsVisible);
            }
            finally
            {
                session.Dispose();
                window.Close();
                Directory.Delete(directory, true);
            }
            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task CloseHidesWindowAndPreferenceCanRestoreNormalExit()
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var store = new MemoryStore(new AppSettings(null, null, false, []));
            var settings = new SettingsService(store);
            var window = new Window();
            var exits = 0;
            using var session = new DesktopSession(new MainViewModel(settings), window, settings,
                new LoginStartup(), () => exits++, true);
            try
            {
                await session.InitializeAsync(background: false);
                window.Close();
                Assert.False(window.IsVisible);
                Assert.Equal(0, exits);
                session.ShowMainWindow();
                Assert.True(window.IsVisible);
                await session.ToggleCloseToTrayAsync();
                Assert.False(store.Current!.CloseToTray);
                window.Close();
                await Task.Delay(25);
                Assert.Equal(1, exits);
            }
            finally
            {
                session.Dispose();
                window.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task MissingTrayOrConfigurationNeverLeavesUserWithHiddenApp(bool trayAvailable, bool loadFails)
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var store = new MemoryStore(new AppSettings(Path.GetTempPath(), null, false, [])) { FailLoad = loadFails };
            var settings = new SettingsService(store);
            var window = new Window();
            var exits = 0;
            using var session = new DesktopSession(new MainViewModel(settings), window, settings,
                new LoginStartup(), () => exits++, trayAvailable);
            try
            {
                await session.InitializeAsync(background: true);
                Assert.True(window.IsVisible);
                session.Exit();
                Assert.Equal(1, exits);
            }
            finally
            {
                session.Dispose();
                window.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task PreferenceFailuresDoNotChangeSavedStateAndAreVisible()
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var store = new MemoryStore(new AppSettings(null, null, false, [])) { FailSave = true };
            var settings = new SettingsService(store);
            var viewModel = new MainViewModel(settings);
            var window = new Window();
            var login = new LoginStartup();
            using var session = new DesktopSession(viewModel, window, settings, login, () => { }, true);
            try
            {
                await session.InitializeAsync(background: false);
                await session.ToggleCloseToTrayAsync();
                Assert.True(session.CloseToTray);
                Assert.Equal("Save failed", viewModel.SettingsStatus);
                session.ToggleLoginStartup();
                Assert.True(session.LoginStartupEnabled);
                session.ToggleLoginStartup();
                Assert.False(session.LoginStartupEnabled);
                login.Fail = true;
                window.Hide();
                session.ToggleLoginStartup();
                Assert.False(session.LoginStartupEnabled);
                Assert.True(window.IsVisible);
                Assert.Contains("Autostart", viewModel.SettingsStatus);
            }
            finally
            {
                session.Dispose();
                window.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task UnavailableWatchedFolderShowsStartupErrorInsteadOfHiding()
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var missing = Path.Combine(Path.GetTempPath(), "sortify-missing-" + Guid.NewGuid());
            var settings = new SettingsService(new MemoryStore(new AppSettings(missing, null, false, [])));
            var viewModel = new MainViewModel(settings);
            var window = new Window();
            using var session = new DesktopSession(viewModel, window, settings, new LoginStartup(), () => { }, true);
            try
            {
                await session.InitializeAsync(background: true);
                Assert.True(window.IsVisible);
                Assert.Contains("konnte nicht starten", viewModel.MonitoringStatus);
            }
            finally
            {
                session.Dispose();
                window.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    private sealed class LoginStartup : ILoginStartup
    {
        public bool IsSupported => true;
        public bool IsEnabled { get; private set; }
        public bool Fail { get; set; }
        public void SetEnabled(bool enabled)
        {
            if (Fail)
                throw new UnauthorizedAccessException("Denied");
            IsEnabled = enabled;
        }
    }

    private sealed class MemoryStore(AppSettings? settings) : ISettingsStore
    {
        public AppSettings? Current { get; private set; } = settings;
        public bool FailLoad { get; init; }
        public bool FailSave { get; init; }
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(FailLoad
                ? new SettingsLoadResult(SettingsLoadStatus.Failed, null, "Load failed")
                : new SettingsLoadResult(SettingsLoadStatus.Loaded, Current, null));

        public Task<SettingsSaveResult> SaveAsync(AppSettings value, CancellationToken cancellationToken = default)
        {
            if (FailSave)
                return Task.FromResult(new SettingsSaveResult(false, "Save failed"));
            Current = value;
            return Task.FromResult(new SettingsSaveResult(true, null));
        }
    }
}
