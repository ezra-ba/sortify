using System.Text;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Startup;

public sealed class LinuxLoginStartup : ILoginStartup
{
    private const string Marker = "X-Sortify-Managed=true";
    private readonly string? executablePath;
    private readonly string registrationPath;

    public LinuxLoginStartup(string? executablePath, string? configurationDirectory = null)
    {
        this.executablePath = executablePath;
        var directory = configurationDirectory ?? ResolveConfigurationDirectory(
            Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        if (!Path.IsPathFullyQualified(directory)) throw new ArgumentException("Ein absoluter Konfigurationspfad ist erforderlich.");
        registrationPath = Path.Combine(directory, "autostart", "sortify.desktop");
    }

    public static string ResolveConfigurationDirectory(string? configured, string home)
    {
        if (!string.IsNullOrWhiteSpace(configured) && Path.IsPathFullyQualified(configured)) return configured;
        if (!Path.IsPathFullyQualified(home)) throw new InvalidOperationException("Der Benutzerordner konnte nicht ermittelt werden.");
        return Path.Combine(home, ".config");
    }

    public bool IsSupported
    {
        get
        {
            if (!OperatingSystem.IsLinux() || string.IsNullOrWhiteSpace(executablePath) ||
                !IsValidPath(executablePath) || !File.Exists(executablePath) ||
                Path.GetFileNameWithoutExtension(executablePath).Equals("dotnet", StringComparison.OrdinalIgnoreCase)) return false;
            var mode = File.GetUnixFileMode(executablePath);
            return (mode & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
        }
    }

    public bool IsEnabled => IsSupported && ReadRegistration() == BuildEntry(executablePath!, true);

    public void SetEnabled(bool enabled)
    {
        if (!OperatingSystem.IsLinux() || (enabled && !IsSupported))
            throw new PlatformNotSupportedException("Autostart benötigt eine ausführbare Sortify-App unter Linux; der dotnet-Host wird nicht unterstützt.");
        _ = ReadRegistration();
        // Keep a disabled user entry to override any system-wide entry with the same name.
        var content = enabled ? BuildEntry(executablePath!, true)
            : "[Desktop Entry]\nType=Application\nName=Sortify\n" + Marker + "\nHidden=true\n";
        var directory = Path.GetDirectoryName(registrationPath)!;
        Directory.CreateDirectory(directory);
        var temporary = Path.Combine(directory, $".sortify-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporary, content, new UTF8Encoding(false));
            _ = ReadRegistration();
            File.Move(temporary, registrationPath, overwrite: true);
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private string? ReadRegistration()
    {
        if (Directory.Exists(registrationPath)) throw new IOException("Der Autostartpfad ist ein Ordner.");
        var info = new FileInfo(registrationPath);
        if (info.LinkTarget is not null) throw new IOException("Autostart-Verknüpfungen werden nicht überschrieben.");
        try
        {
            if (info.Exists && info.Length > 65_536) throw new IOException("Die Autostartdatei ist unerwartet groß.");
            var content = File.ReadAllText(registrationPath);
            if (!content.Split('\n').Contains(Marker, StringComparer.Ordinal))
                throw new IOException("Die vorhandene Autostartdatei wird nicht von Sortify verwaltet.");
            return content;
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private static bool IsValidPath(string path) => Path.IsPathFullyQualified(path)
        && !path.Any(char.IsControl) && !path.Contains('=') && !path.Contains('%');

    public static string BuildEntry(string executablePath, bool enabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        if (!IsValidPath(executablePath))
            throw new ArgumentException("Ein absoluter Programmpfad ohne Steuerzeichen, Gleichheits- oder Prozentzeichen ist erforderlich.", nameof(executablePath));
        var quoted = new StringBuilder();
        foreach (var character in executablePath)
        {
            if (character is '\\' or '"' or '`' or '$') quoted.Append('\\');
            quoted.Append(character);
        }
        var argument = quoted.ToString().Replace("\\", "\\\\");
        return string.Join('\n', "[Desktop Entry]", "Type=Application", "Name=Sortify", Marker,
            $"Exec=\"{argument}\" --background", "Terminal=false", "Hidden=" + (enabled ? "false" : "true"), "");
    }
}
