using Sortify.Core.Services;

namespace Sortify.Core.Tests;

public sealed class DesktopLifetimeControllerTests
{
    [Fact]
    public void TrayLossRestoresAccessAndExplicitCloseSchedulesOneExit()
    {
        var host = new Host();
        var lifetime = new DesktopLifetimeController(host, true);
        lifetime.Start(background: true);
        Assert.True(lifetime.IsInBackground);
        Assert.True(lifetime.RequestClose(closeToTray: true));
        Assert.Equal(1, host.Hides);
        Assert.Equal(0, host.Exits);
        lifetime.SetTrayAvailable(false);
        Assert.False(lifetime.IsInBackground);
        Assert.Equal(1, host.Shows);
        Assert.False(lifetime.RequestClose(closeToTray: true));
        Assert.False(lifetime.RequestClose(closeToTray: true));
        Assert.Equal(1, host.Exits);
        lifetime.SetTrayAvailable(true);
        lifetime.Show();
        Assert.Equal(1, host.Shows);
    }

    [Fact]
    public void UnavailableTrayDoesNotStartHiddenAndCanRecoverForLaterClose()
    {
        var host = new Host();
        var lifetime = new DesktopLifetimeController(host, false);
        lifetime.Start(background: true);
        Assert.False(lifetime.IsInBackground);
        Assert.Equal(1, host.Shows);
        lifetime.SetTrayAvailable(true);
        Assert.True(lifetime.RequestClose(closeToTray: true));
        Assert.True(lifetime.IsInBackground);
        lifetime.Show();
        Assert.False(lifetime.IsInBackground);
        Assert.False(lifetime.RequestClose(closeToTray: false));
        Assert.Equal(1, host.Exits);
    }

    [Fact]
    public void SystemShutdownIsNeverCancelledOrScheduledAgain()
    {
        var host = new Host();
        var lifetime = new DesktopLifetimeController(host, true);
        lifetime.Start(background: true);
        Assert.False(lifetime.RequestClose(closeToTray: true, systemShutdown: true));
        lifetime.SetTrayAvailable(false);
        lifetime.Show();
        Assert.True(lifetime.IsExitRequested);
        Assert.False(lifetime.IsInBackground);
        Assert.Equal(0, host.Shows);
        Assert.Equal(0, host.Exits);
    }

    private sealed class Host : IDesktopLifetimeHost
    {
        public int Shows { get; private set; }
        public int Hides { get; private set; }
        public int Exits { get; private set; }
        public void ShowWindow() => Shows++;
        public void HideWindow() => Hides++;
        public void RequestShutdown() => Exits++;
    }
}
