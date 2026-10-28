using Avalonia.Controls;
using Avalonia;
using Avalonia.Styling;
using Sortify.App.ViewModels;
using Sortify.Core.Services;

namespace Sortify.App.Services;

public sealed class DesktopSession : IDisposable
{
    private readonly MainViewModel viewModel;
    private readonly Window window;
    private readonly ISettingsService? settings;
    private readonly ILoginStartup loginStartup;
    private readonly Action shutdown;
    private readonly DownloadMonitorController monitor;
    private bool initialized;
    private bool showRequested;
    private bool disposed;

    public bool TrayAvailable { get; }
    public bool CloseToTray => settings?.Current?.CloseToTray ?? true;
    public bool IsPaused { get; private set; }
    public bool LoginStartupSupported => loginStartup.IsSupported;
    public bool LoginStartupEnabled { get; private set; }
    public bool CanSavePreferences => initialized && settings?.Current is not null;
    public bool IsReady => initialized && !disposed;
    public event Action? StateChanged;

    public DesktopSession(MainViewModel viewModel, Window window, ISettingsService? settings,
        ILoginStartup loginStartup, Action shutdown, bool trayAvailable)
    {
        this.viewModel = viewModel;
        this.window = window;
        this.settings = settings;
        this.loginStartup = loginStartup;
        this.shutdown = shutdown;
        TrayAvailable = trayAvailable;
        monitor = new DownloadMonitorController(viewModel);
        viewModel.MonitoringFolderConfigured += RestartMonitoring;
        window.Closing += OnClosing;
    }

    public async Task InitializeAsync(bool background)
    {
        await viewModel.InitializeSettingsAsync();
        if (disposed)
            return;
        if (settings?.Current is { } loadedSettings && Application.Current is { } application)
        {
            application.RequestedThemeVariant = loadedSettings.DarkMode
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }
        initialized = true;
        RestartMonitoring();
        try
        {
            LoginStartupEnabled = loginStartup.IsEnabled;
        }
        catch (Exception error) when (IsPreferenceError(error))
        {
            viewModel.SettingsStatus = "Autostart konnte nicht gelesen werden: " + error.Message;
            showRequested = true;
        }
        StateChanged?.Invoke();
        var startMinimized = settings?.Current?.StartMinimized == true;
        if (!background || !startMinimized || showRequested || !TrayAvailable || !monitor.IsMonitoring)
            ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        if (disposed)
            return;
        showRequested = true;
        if (!initialized)
            return;
        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
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
        if (disposed || args.CloseReason is WindowCloseReason.OSShutdown or WindowCloseReason.ApplicationShutdown)
            return;
        if (TrayAvailable && CloseToTray)
        {
            args.Cancel = true;
            window.Hide();
        }
        else
        {
            // Finish closing this window before requesting application shutdown.
            Avalonia.Threading.Dispatcher.UIThread.Post(Exit);
        }
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
        viewModel.MonitoringFolderConfigured -= RestartMonitoring;
        window.Closing -= OnClosing;
        monitor.Dispose();
    }
}
