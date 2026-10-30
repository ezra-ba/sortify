namespace Sortify.Core.Services;

/// <summary>UI adapter used by the OS-independent lifetime controller.</summary>
public interface IDesktopLifetimeHost
{
    void ShowWindow();
    void HideWindow();

    /// <summary>Schedule application shutdown after the current close event completes.</summary>
    void RequestShutdown();
}
