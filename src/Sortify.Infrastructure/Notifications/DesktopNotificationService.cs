using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Notifications;

public sealed class DesktopNotificationService : ISystemNotificationService
{
    private const string AppleScript = """
        on run argv
            display notification (item 2 of argv) with title (item 1 of argv)
        end run
        """;

    private const string WindowsScript = """
        $ErrorActionPreference = 'Stop'
        Add-Type -AssemblyName System.Windows.Forms, System.Drawing
        $icon = New-Object System.Windows.Forms.NotifyIcon
        try {
            $icon.Icon = [System.Drawing.SystemIcons]::Information
            $icon.Text = 'Sortify'
            $icon.Visible = $true
            $icon.ShowBalloonTip(5000, $env:SORTIFY_NOTIFICATION_TITLE, $env:SORTIFY_NOTIFICATION_BODY, 'Info')
            Start-Sleep -Seconds 6
        } finally {
            $icon.Dispose()
        }
        """;

    public async Task ShowAsync(string title, string message)
    {
        var command = CreateCommand(title, message);
        if (command is null) return;

        try
        {
            using var process = Process.Start(command);
            if (process is null) return;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(12));
            try
            {
                await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
                if (process.ExitCode != 0)
                    Trace.TraceWarning("Sortify: Systembenachrichtigung abgelehnt (Exitcode {0}).", process.ExitCode);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                if (!process.HasExited) process.Kill();
            }
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            // A missing utility, unavailable desktop session or failed notification cannot affect sorting.
            Trace.TraceWarning("Sortify: Systembenachrichtigung nicht verfügbar ({0}).", error.GetType().Name);
        }
    }

    private static ProcessStartInfo? CreateCommand(string title, string message)
    {
        if (OperatingSystem.IsLinux())
            return Command("notify-send", "--app-name=Sortify", "--expire-time=5000", "--", title, WebUtility.HtmlEncode(message));

        if (OperatingSystem.IsMacOS())
            return Command("/usr/bin/osascript", "-e", AppleScript, "--", title, message);

        if (!OperatingSystem.IsWindows()) return null;

        var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell", "v1.0", "powershell.exe");
        var command = Command(powershell, "-NoProfile", "-NonInteractive", "-EncodedCommand",
            Convert.ToBase64String(Encoding.Unicode.GetBytes(WindowsScript)));
        command.Environment["SORTIFY_NOTIFICATION_TITLE"] = title;
        command.Environment["SORTIFY_NOTIFICATION_BODY"] = message;
        return command;
    }

    private static ProcessStartInfo Command(string executable, params string[] arguments)
    {
        var command = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in arguments) command.ArgumentList.Add(argument);
        return command;
    }
}
