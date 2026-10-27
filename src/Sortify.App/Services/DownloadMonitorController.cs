using Avalonia.Threading;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Infrastructure.Monitoring;
using Sortify.Core.Services;
using Sortify.Infrastructure.FileSystem;

namespace Sortify.App.Services;

public sealed class DownloadMonitorController(MainViewModel viewModel) : IDisposable
{
    private readonly IFileMoveService fileMoveService =
    new FileMoveService();
    private readonly FolderMonitor monitor = new();
    private readonly List<FileSuggestionWindow> popups = [];
    private int session;
    private bool disposed;

    public bool IsMonitoring { get; private set; }

    public void Restart(string? directory)
    {
        if (disposed)
            return;

        var currentSession = ++session;
        IsMonitoring = false;
        monitor.Dispose();
        foreach (var popup in popups.ToArray())
            popup.Close();

        if (string.IsNullOrWhiteSpace(directory))
        {
            viewModel.MonitoringStatus = "Bitte einen Ueberwachungsordner auswaehlen.";
            return;
        }

        try
        {
            monitor.Start(directory, path => Dispatcher.UIThread.Post(() =>
            {
                if (disposed || session != currentSession)
                    return;
                var popup = new FileSuggestionWindow(
                    new FileSuggestionViewModel(
                        viewModel.TargetFolders,
                            path,
                            fileMoveService))
                {
                    Topmost = true
                };
                popups.Add(popup);
                popup.Closed += (_, _) => popups.Remove(popup);
                popup.Show();
            }), error => Dispatcher.UIThread.Post(() =>
            {
                if (!disposed && session == currentSession)
                {
                    monitor.Dispose();
                    IsMonitoring = false;
                    viewModel.MonitoringStatus = error;
                }
            }));
            viewModel.MonitoringStatus = "Ordnerueberwachung aktiv.";
            IsMonitoring = true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            monitor.Dispose();
            viewModel.MonitoringStatus = "Ordnerueberwachung konnte nicht starten: " + exception.Message;
        }
    }

    public void Dispose()
    {
        disposed = true;
        IsMonitoring = false;
        session++;
        monitor.Dispose();
        foreach (var popup in popups.ToArray())
            popup.Close();
    }
}
