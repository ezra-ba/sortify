using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Styling;
using Sortify.App.Services;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

[Collection("Avalonia")]
public sealed class SplashStartupTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task StartupShowsSplashOnlyForForegroundAndInitializesOnce(bool background, bool activate)
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var store = new ControlledStore();
            var settings = new SettingsService(store);
            var model = new MainViewModel(settings);
            using var startup = new ApplicationStartup();
            SplashWindow? splash = null;
            Window? main = null;
            DesktopSession? desktopSession = null;
            var statuses = new List<string>();
            using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
            {
                if (window is SplashWindow candidate)
                    splash = candidate;
            });
            try
            {
                var run = startup.RunAsync(background, (progress, token) =>
                {
                    Assert.Equal(!background, splash?.IsVisible == true);
                    main = new Window();
                    desktopSession = new DesktopSession(model, main, settings, new LoginStartup(), () => { }, true);
                    if (activate)
                        desktopSession.ShowMainWindow();
                    return desktopSession.InitializeAsync(background, status =>
                    {
                        statuses.Add(status);
                        progress(status);
                    }, token);
                }, error => throw new InvalidOperationException("Unexpected startup failure", error), () => { });
                await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.NotNull(main);
                Assert.False(main.IsVisible);
                if (!background)
                {
                    var splashModel = Assert.IsType<SplashViewModel>(splash!.DataContext);
                    Assert.Equal("Einstellungen werden geladen …", splashModel.Status);
                    Assert.StartsWith("v", splashModel.Version);
                    Assert.DoesNotContain("+", splashModel.Version);
                    Assert.False(splash.ShowInTaskbar);
                    Assert.False(splash.Topmost);
                    AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                    await Task.Delay(200);
                    Capture(splash, "splashscreen-light.png");
                    splash.RequestedThemeVariant = ThemeVariant.Dark;
                    Capture(splash, "splashscreen-dark.png");
                }
                store.Complete();
                await run;
                Assert.Equal(!background || activate, main.IsVisible);
                Assert.False(splash?.IsVisible == true);
                Assert.True(desktopSession!.IsReady);
                Assert.Equal(1, store.LoadCalls);
                Assert.Equal(new[]
                {
                    "Einstellungen werden geladen …",
                    "Ordnerüberwachung wird gestartet …",
                    "Autostart-Einstellungen werden geprüft …"
                }, statuses);
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    startup.RunAsync(background, (_, _) => Task.CompletedTask, _ => { }, () => { }));
            }
            finally
            {
                store.Complete();
                startup.Dispose();
                desktopSession?.Dispose();
                main?.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClosingOrDisposingDuringLoadCancelsAndNeverShowsMainWindow(bool closeSplash)
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var store = new ControlledStore();
            var settings = new SettingsService(store);
            var model = new MainViewModel(settings);
            var main = new Window();
            using var desktopSession = new DesktopSession(model, main, settings, new LoginStartup(), () => { }, true);
            using var startup = new ApplicationStartup();
            SplashWindow? splash = null;
            var cancelled = 0;
            using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
            {
                if (window is SplashWindow candidate)
                    splash = candidate;
            });
            try
            {
                var run = startup.RunAsync(false,
                    (progress, token) => desktopSession.InitializeAsync(false, progress, token),
                    _ => Assert.Fail("Cancellation must not be shown as an error"),
                    () => { cancelled++; desktopSession.Dispose(); });
                await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.NotNull(splash);
                if (closeSplash)
                    splash.Close();
                else
                    startup.Dispose();
                Assert.True(store.Token.IsCancellationRequested);
                Assert.False(splash.IsVisible);
                // Deliberately finish a store that ignores cancellation: no late UI or watcher may start.
                store.Complete();
                await run;
                Assert.False(main.IsVisible);
                Assert.False(desktopSession.IsReady);
                Assert.Empty(model.MonitoringStatus);
                Assert.Equal(closeSplash ? 1 : 0, cancelled);
            }
            finally
            {
                store.Complete();
                startup.Dispose();
                desktopSession.Dispose();
                main.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task UnexpectedFailureClosesSplashBeforeDisplayingError()
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            using var startup = new ApplicationStartup();
            var main = new Window();
            var model = new MainViewModel();
            using var desktopSession = new DesktopSession(model, main, null, new LoginStartup(), () => { }, true);
            SplashWindow? splash = null;
            using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
            {
                if (window is SplashWindow candidate)
                    splash = candidate;
            });
            try
            {
                await startup.RunAsync(false, (_, _) => throw new IOException("Test failure"), error =>
                {
                    Assert.IsType<IOException>(error);
                    Assert.False(splash!.IsVisible);
                    desktopSession.ShowStartupFailure();
                }, () => { });
                Assert.True(main.IsVisible);
                Assert.Contains("nicht vollständig gestartet", model.SettingsStatus);
            }
            finally
            {
                desktopSession.Dispose();
                main.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task FailedSettingsLoadClosesSplashAndShowsConfigurationError()
    {
        using var headless = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await headless.Dispatch(async () =>
        {
            var store = new ControlledStore();
            var settings = new SettingsService(store);
            var model = new MainViewModel(settings);
            var main = new Window();
            using var desktopSession = new DesktopSession(model, main, settings, new LoginStartup(), () => { }, true);
            using var startup = new ApplicationStartup();
            SplashWindow? splash = null;
            using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
            {
                if (window is SplashWindow candidate)
                    splash = candidate;
            });
            try
            {
                var run = startup.RunAsync(false,
                    (progress, token) => desktopSession.InitializeAsync(false, progress, token),
                    _ => Assert.Fail("A settings load failure is already handled by the settings service"), () => { });
                await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                store.Complete(failed: true);
                await run;
                Assert.False(splash!.IsVisible);
                Assert.True(main.IsVisible);
                Assert.Equal("Einstellungen nicht lesbar.", model.SettingsStatus);
            }
            finally
            {
                store.Complete();
                desktopSession.Dispose();
                main.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    private static void Capture(SplashWindow window, string name)
    {
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        Assert.Equal(480, frame.PixelSize.Width);
        Assert.Equal(320, frame.PixelSize.Height);
        frame.Save(Path.Combine(AppContext.BaseDirectory, name),
            Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
    }

    private sealed class LoginStartup : ILoginStartup
    {
        public bool IsSupported => false;
        public bool IsEnabled => false;
        public void SetEnabled(bool enabled) => throw new PlatformNotSupportedException();
    }

    private sealed class ControlledStore : ISettingsStore
    {
        private readonly TaskCompletionSource<SettingsLoadResult> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int LoadCalls { get; private set; }
        public CancellationToken Token { get; private set; }
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
        {
            LoadCalls++;
            Token = cancellationToken;
            Started.TrySetResult();
            return result.Task;
        }
        public void Complete(bool failed = false) => result.TrySetResult(failed
            ? new SettingsLoadResult(SettingsLoadStatus.Failed, null, "Einstellungen nicht lesbar.")
            : new SettingsLoadResult(SettingsLoadStatus.Loaded, new AppSettings(Path.GetTempPath(), null, false, []), null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SettingsSaveResult(true, null));
    }
}
