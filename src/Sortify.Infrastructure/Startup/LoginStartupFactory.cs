using Sortify.Core.Services;

namespace Sortify.Infrastructure.Startup;

public static class LoginStartupFactory
{
    public static ILoginStartup Create(string? executablePath)
    {
        if (OperatingSystem.IsWindows()) return new WindowsLoginStartup(executablePath);
        if (OperatingSystem.IsLinux()) return new LinuxLoginStartup(executablePath);
        return new UnsupportedLoginStartup();
    }

    private sealed class UnsupportedLoginStartup : ILoginStartup
    {
        public bool IsSupported => false;
        public bool IsEnabled => false;
        public void SetEnabled(bool enabled) => throw new PlatformNotSupportedException(
            "Autostart ist auf diesem Betriebssystem noch nicht implementiert.");
    }
}
