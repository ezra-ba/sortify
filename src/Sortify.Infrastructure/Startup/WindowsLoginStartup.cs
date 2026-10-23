using Microsoft.Win32;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Startup;

public sealed class WindowsLoginStartup(string? executablePath) : ILoginStartup
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Sortify";

    public bool IsSupported => OperatingSystem.IsWindows()
        && !string.IsNullOrWhiteSpace(executablePath)
        && !Path.GetFileNameWithoutExtension(executablePath).Equals("dotnet", StringComparison.OrdinalIgnoreCase);

    public bool IsEnabled
    {
        get
        {
            if (!OperatingSystem.IsWindows() || !IsSupported)
                return false;
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return string.Equals(key?.GetValue(ValueName) as string,
                BuildCommand(executablePath!), StringComparison.OrdinalIgnoreCase);
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (!OperatingSystem.IsWindows() || !IsSupported)
            throw new PlatformNotSupportedException("Autostart ist nur mit einer Windows-App ausführbar.");

        using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
        if (enabled)
            key.SetValue(ValueName, BuildCommand(executablePath!), RegistryValueKind.String);
        else
            key.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    public static string BuildCommand(string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        if (!Path.IsPathFullyQualified(executablePath) || executablePath.Contains('"'))
            throw new ArgumentException("Ein absoluter Programmpfad ohne Anführungszeichen ist erforderlich.", nameof(executablePath));
        var command = $"\"{executablePath}\" --background";
        if (command.Length > 260)
            throw new ArgumentException("Der Autostart-Befehl darf maximal 260 Zeichen enthalten.", nameof(executablePath));
        return command;
    }
}
