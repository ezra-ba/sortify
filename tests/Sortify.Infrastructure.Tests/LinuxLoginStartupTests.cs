using System.Diagnostics;
using Sortify.Infrastructure.Startup;

namespace Sortify.Infrastructure.Tests;

public sealed class LinuxLoginStartupTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "sortify-startup-" + Guid.NewGuid());
    private string Registration => Path.Combine(root, "autostart", "sortify.desktop");

    [Fact]
    public void RegistrationCanBeEnabledDisabledAndUpdatedWithoutTouchingOtherEntries()
    {
        if (!OperatingSystem.IsLinux()) return;
        Directory.CreateDirectory(root);
        var executable = CreateExecutable("Sortify App");
        var service = new LinuxLoginStartup(executable, root);
        Assert.True(service.IsSupported);
        Assert.False(service.IsEnabled);
        service.SetEnabled(true);
        Assert.True(new LinuxLoginStartup(executable, root).IsEnabled);
        Assert.Contains(" --background\n", File.ReadAllText(Registration));
        var other = Path.Combine(root, "autostart", "other.desktop");
        File.WriteAllText(other, "unchanged");
        service.SetEnabled(false);
        Assert.False(service.IsEnabled);
        Assert.Contains("Hidden=true", File.ReadAllText(Registration));
        Assert.Equal("unchanged", File.ReadAllText(other));
        service.SetEnabled(true);
        Assert.True(service.IsEnabled);
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(Registration)!, "*.tmp"));
    }

    [Fact]
    public void ForeignRegistrationAndSymbolicLinkArePreserved()
    {
        if (!OperatingSystem.IsLinux()) return;
        var executable = CreateExecutable("Sortify");
        var service = new LinuxLoginStartup(executable, root);
        Directory.CreateDirectory(Path.GetDirectoryName(Registration)!);
        File.WriteAllText(Registration, "foreign");
        Assert.Throws<IOException>(() => service.SetEnabled(true));
        Assert.Equal("foreign", File.ReadAllText(Registration));
        File.Delete(Registration);
        var target = Path.Combine(root, "foreign.txt");
        File.WriteAllText(target, "preserve");
        File.CreateSymbolicLink(Registration, target);
        Assert.Throws<IOException>(() => service.SetEnabled(false));
        Assert.Equal("preserve", File.ReadAllText(target));
    }

    [Fact]
    public void MissingNonExecutableAndDotnetHostAreNotRegistered()
    {
        if (!OperatingSystem.IsLinux()) return;
        var missing = new LinuxLoginStartup(Path.Combine(root, "missing"), root);
        Assert.False(missing.IsSupported);
        Assert.Throws<PlatformNotSupportedException>(() => missing.SetEnabled(true));
        var host = new LinuxLoginStartup(CreateExecutable("dotnet"), root);
        Assert.False(host.IsSupported);
        var path = CreateExecutable("Sortify");
        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        Assert.False(new LinuxLoginStartup(path, root).IsSupported);
        Assert.False(File.Exists(Registration));
    }

    [Fact]
    public void XdgPathsAndUnsafeExecutableNamesAreHandledExplicitly()
    {
        Assert.Equal(root, LinuxLoginStartup.ResolveConfigurationDirectory(root, Path.GetTempPath()));
        Assert.Equal(Path.Combine(Path.GetTempPath(), ".config"),
            LinuxLoginStartup.ResolveConfigurationDirectory("relative", Path.GetTempPath()));
        foreach (var name in new[] { "app\nInjected", "app=other", "app%f" })
            Assert.Throws<ArgumentException>(() => LinuxLoginStartup.BuildEntry(Path.Combine(root, name)));
        Assert.Throws<ArgumentException>(() => LinuxLoginStartup.BuildEntry("relative"));
    }

    [Fact]
    public void FactorySelectsTheCurrentOperatingSystem()
    {
        var service = LoginStartupFactory.Create(null);
        if (OperatingSystem.IsLinux()) Assert.IsType<LinuxLoginStartup>(service);
        else if (OperatingSystem.IsWindows()) Assert.IsType<WindowsLoginStartup>(service);
        else Assert.False(service.IsSupported);
    }

    [Fact]
    public async Task NativeLinuxLauncherReceivesExactExecutableAndBackgroundArgumentWhenAvailable()
    {
        if (!OperatingSystem.IsLinux() || !File.Exists("/usr/bin/gio") || !File.Exists("/usr/bin/desktop-file-validate")) return;
        var executable = CreateExecutable("Sortify ü & $ ' \" \\ ` App");
        var service = new LinuxLoginStartup(executable, root);
        service.SetEnabled(true);
        using (var validator = Process.Start(new ProcessStartInfo("/usr/bin/desktop-file-validate")
            { ArgumentList = { Registration }, RedirectStandardError = true, RedirectStandardOutput = true })!)
        {
            await validator.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(validator.ExitCode == 0, await validator.StandardOutput.ReadToEndAsync() + await validator.StandardError.ReadToEndAsync());
        }
        using var process = Process.Start(new ProcessStartInfo("/usr/bin/gio")
            { ArgumentList = { "launch", Registration }, RedirectStandardError = true, RedirectStandardOutput = true })!;
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(process.ExitCode == 0, await process.StandardError.ReadToEndAsync());
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!File.Exists(executable + ".args")) await Task.Delay(20, timeout.Token);
        Assert.Equal("--background", File.ReadAllText(executable + ".args"));
    }

    private string CreateExecutable(string name)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, name);
        File.WriteAllText(path, "#!/bin/sh\nprintf '%s' \"$1\" > \"$0.args\"\n");
        if (OperatingSystem.IsLinux()) File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
