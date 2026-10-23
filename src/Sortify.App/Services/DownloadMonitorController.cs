using Avalonia.Controls;
using Avalonia.Threading;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.App.Services;

public sealed class DownloadMonitorController(Window owner, MainViewModel viewModel) : IDisposable
{
    private readonly FolderMonitor monitor = new();
    private readonly List<FileSuggestionWindow> popups = [];
    private int session;
    private bool disposed;

    public void Restart(string? directory)
    {
        if (disposed)
            return;

        var currentSession = ++session;
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
                    new FileSuggestionViewModel(viewModel.TargetFolders, path));
                popups.Add(popup);
                popup.Closed += (_, _) => popups.Remove(popup);
                popup.Show(owner);
            }), error => Dispatcher.UIThread.Post(() =>
            {
                if (!disposed && session == currentSession)
                {
                    monitor.Dispose();
                    viewModel.MonitoringStatus = error;
                }
            }));
            viewModel.MonitoringStatus = "Ordnerueberwachung aktiv.";
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
        session++;
        monitor.Dispose();
        foreach (var popup in popups.ToArray())
            popup.Close();
    }
}
