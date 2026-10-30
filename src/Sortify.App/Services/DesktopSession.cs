using Avalonia.Controls;
using Avalonia;
using Avalonia.Styling;
using Sortify.App.ViewModels;
using Sortify.Core.Services;
using Sortify.Core.Rules;
using Sortify.Infrastructure.FileSystem;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.App.Services;

public sealed class DesktopSession : IDisposable
{
    private readonly MainViewModel viewModel;
    private readonly Window window;
    private readonly DesktopLifetimeController lifetime;
    private readonly ISettingsService? settings;
    private readonly ILoginStartup loginStartup;
    private readonly Action shutdown;
    private readonly DownloadMonitorController monitor;
    private bool initialized;
    private bool showRequested;
    private bool disposed;

    public bool TrayAvailable => lifetime.TrayAvailable;
    public bool IsInBackground => lifetime.IsInBackground;
    public bool CloseToTray => settings?.Current?.CloseToTray ?? true;
    public bool IsPaused { get; private set; }
    public bool LoginStartupSupported => loginStartup.IsSupported;
    public bool LoginStartupEnabled { get; private set; }
    public bool CanSavePreferences => initialized && settings?.Current is not null;
    public bool IsReady => initialized && !disposed;
    public event Action? StateChanged;

    public DesktopSession(MainViewModel viewModel, Window window, ISettingsService? settings,
        ILoginStartup loginStartup, Action shutdown, bool trayAvailable,
        IFileMoveService? fileMoveService = null, IRuleEngine? ruleEngine = null, FolderMonitor? monitor = null, IActivityLog? log = null)
    {
        this.viewModel = viewModel;
        this.window = window;
        this.settings = settings;
        this.loginStartup = loginStartup;
        this.shutdown = shutdown;
        lifetime = new DesktopLifetimeController(
            new LifetimeHost(window, () => Avalonia.Threading.Dispatcher.UIThread.Post(Exit)), trayAvailable);
        this.monitor = new DownloadMonitorController(viewModel,
            fileMoveService ?? new FileMoveService(),
            ruleEngine ?? new RuleEngine(new RuleValidator(), new RuleTargetValidator()),
            monitor ?? new FolderMonitor(), log);
        viewModel.MonitoringFolderConfigured += RestartMonitoring;
        window.Closing += OnClosing;
    }

    public async Task InitializeAsync(bool background, Action<string>? reportStatus = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        reportStatus?.Invoke("Einstellungen werden geladen …");
        await viewModel.InitializeSettingsAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (disposed)
            return;
        if (settings?.Current is { } loadedSettings && Application.Current is { } application)
        {
            application.RequestedThemeVariant = loadedSettings.DarkMode
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }
        initialized = true;
        reportStatus?.Invoke("Ordnerüberwachung wird gestartet …");
        RestartMonitoring();
        try
        {
            reportStatus?.Invoke("Autostart-Einstellungen werden geprüft …");
            LoginStartupEnabled = loginStartup.IsEnabled;
        }
        catch (Exception error) when (IsPreferenceError(error))
        {
            viewModel.SettingsStatus = "Autostart konnte nicht gelesen werden: " + error.Message;
            showRequested = true;
        }
        var startMinimized = settings?.Current?.StartMinimized == true;
        lifetime.Start(background && startMinimized && !showRequested && monitor.IsMonitoring);
        StateChanged?.Invoke();
    }

    public void ShowStartupFailure()
    {
        if (disposed)
            return;
        monitor.Dispose();
        initialized = true;
        viewModel.SettingsStatus = "Sortify konnte nicht vollständig gestartet werden. Bitte die Anwendung beenden und erneut starten.";
        StateChanged?.Invoke();
        ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        if (disposed)
            return;
        showRequested = true;
        if (!initialized)
            return;
        lifetime.Show();
    }

    // The native tray adapter reports availability on the UI thread.
    // Losing a usable tray must restore a hidden window rather than orphan the process.
    public void SetTrayAvailability(bool available)
    {
        if (disposed) return;
        lifetime.SetTrayAvailable(available);
        StateChanged?.Invoke();
    }

    public void TogglePause()
    {
        if (!initialized || disposed)
            return;
        IsPaused = !IsPaused;
        RestartMonitoring();
        StateChanged?.Invoke();
    }

    private void RestartMonitoring()
    {
        if (!initialized || disposed)
            return;
        monitor.Restart(IsPaused ? null : settings?.Current?.WatchedFolderPath);
        if (IsPaused)
            viewModel.MonitoringStatus = "Ordnerüberwachung pausiert.";
    }

    public async Task ToggleCloseToTrayAsync()
    {
        if (!CanSavePreferences || disposed || settings is null)
            return;
        var result = await settings.UpdateAsync(current => current with { CloseToTray = !current.CloseToTray });
        if (disposed)
            return;
        viewModel.SettingsStatus = result.IsSuccess
            ? "Verhalten beim Schließen gespeichert."
            : result.ErrorMessage ?? "Einstellungen konnten nicht gespeichert werden.";
        StateChanged?.Invoke();
        if (!result.IsSuccess)
            ShowMainWindow();
    }

    public async Task ToggleLoginStartupAsync()
    {
        if (!initialized || disposed || !LoginStartupSupported)
            return;
        try
        {
            loginStartup.SetEnabled(!LoginStartupEnabled);
            LoginStartupEnabled = loginStartup.IsEnabled;
            if (settings is not null)
                await settings.UpdateAsync(current => current with { AutoStart = LoginStartupEnabled });
            viewModel.SettingsStatus = LoginStartupEnabled ? "Autostart aktiviert." : "Autostart deaktiviert.";
        }
        catch (Exception error) when (IsPreferenceError(error))
        {
            viewModel.SettingsStatus = "Autostart konnte nicht geändert werden: " + error.Message;
            ShowMainWindow();
        }
        StateChanged?.Invoke();
    }

    public void ToggleLoginStartup() => _ = ToggleLoginStartupAsync();

    private static bool IsPreferenceError(Exception error) => error is IOException
        or UnauthorizedAccessException or System.Security.SecurityException
        or ArgumentException or PlatformNotSupportedException;

    private void OnClosing(object? sender, WindowClosingEventArgs args)
    {
        if (disposed) return;
        args.Cancel = lifetime.RequestClose(CloseToTray,
            args.CloseReason is WindowCloseReason.OSShutdown or WindowCloseReason.ApplicationShutdown);
    }

    public void Exit()
    {
        if (disposed)
            return;
        Dispose();
        shutdown();
    }

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        lifetime.Stop();
        viewModel.MonitoringFolderConfigured -= RestartMonitoring;
        window.Closing -= OnClosing;
        monitor.Dispose();
    }

    private sealed class LifetimeHost(Window window, Action requestShutdown) : IDesktopLifetimeHost
    {
        public void ShowWindow()
        {
            window.Show();
            window.WindowState = WindowState.Normal;
            window.Activate();
        }

        public void HideWindow() => window.Hide();
        public void RequestShutdown() => requestShutdown();
    }

}
