namespace Sortify.Core.Services;

/// <summary>
/// Controls window lifetime without depending on a desktop toolkit or an OS name.
/// Call from the host's event thread. Tray availability means the host has a usable tray.
/// </summary>
public sealed class DesktopLifetimeController
{
    private readonly IDesktopLifetimeHost host;
    public bool TrayAvailable { get; private set; }
    public bool IsInBackground { get; private set; }
    public bool IsExitRequested { get; private set; }

    public DesktopLifetimeController(IDesktopLifetimeHost host, bool trayAvailable)
    {
        ArgumentNullException.ThrowIfNull(host);
        this.host = host;
        TrayAvailable = trayAvailable;
    }

    public void Start(bool background)
    {
        if (IsExitRequested) return;
        if (background && TrayAvailable) Hide();
        else Show();
    }

    /// <returns>True when the caller must cancel the window-close event.</returns>
    public bool RequestClose(bool closeToTray, bool systemShutdown = false)
    {
        if (systemShutdown)
        {
            Stop();
            return false;
        }
        if (IsExitRequested) return false;
        if (closeToTray && TrayAvailable)
        {
            Hide();
            return true;
        }
        IsExitRequested = true;
        host.RequestShutdown();
        return false;
    }

    public void SetTrayAvailable(bool available)
    {
        TrayAvailable = available;
        if (!available && IsInBackground && !IsExitRequested) Show();
    }

    public void Show()
    {
        if (IsExitRequested) return;
        host.ShowWindow();
        IsInBackground = false;
    }

    private void Hide()
    {
        if (IsInBackground) return;
        host.HideWindow();
        IsInBackground = true;
    }

    /// <summary>Mark shutdown/disposal without scheduling another shutdown.</summary>
    public void Stop()
    {
        IsExitRequested = true;
        IsInBackground = false;
    }
}
