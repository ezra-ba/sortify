using Avalonia.Controls;

namespace Sortify.App.Services;

public sealed class TrayMenu : IDisposable
{
    private readonly TrayIcon icon;
    private readonly DesktopSession session;
    private readonly NativeMenuItem pause = new("Überwachung pausieren");
    private readonly NativeMenuItem closeToTray = new("Beim Schließen im Tray weiterlaufen")
        { ToggleType = MenuItemToggleType.CheckBox, IsEnabled = false };
    private readonly NativeMenuItem login = new("Bei Windows-Anmeldung starten")
        { ToggleType = MenuItemToggleType.CheckBox, IsEnabled = false };

    public TrayMenu(TrayIcon icon, DesktopSession session)
    {
        this.icon = icon;
        this.session = session;
        var open = new NativeMenuItem("Sortify öffnen");
        open.Click += (_, _) => session.ShowMainWindow();
        pause.Click += (_, _) => session.TogglePause();
        closeToTray.Click += async (_, _) => await session.ToggleCloseToTrayAsync();
        login.Click += (_, _) => session.ToggleLoginStartup();
        var exit = new NativeMenuItem("Beenden");
        exit.Click += (_, _) => session.Exit();
        icon.Menu = new NativeMenu
        {
            Items = { open, pause, new NativeMenuItemSeparator(), closeToTray, login,
                new NativeMenuItemSeparator(), exit }
        };
        icon.Clicked += OnClicked;
        session.StateChanged += Refresh;
        Refresh();
    }

    private void OnClicked(object? sender, EventArgs args) => session.ShowMainWindow();

    private void Refresh()
    {
        pause.Header = session.IsPaused ? "Überwachung fortsetzen" : "Überwachung pausieren";
        closeToTray.IsChecked = session.CloseToTray;
        closeToTray.IsEnabled = session.CanSavePreferences && session.TrayAvailable;
        login.IsEnabled = session.LoginStartupSupported && session.IsReady;
        pause.IsEnabled = session.IsReady;
        login.IsChecked = session.LoginStartupEnabled;
        icon.ToolTipText = session.IsPaused ? "Sortify - pausiert" : "Sortify";
    }

    public void Dispose()
    {
        session.StateChanged -= Refresh;
        icon.Clicked -= OnClicked;
        icon.Dispose();
    }
}
