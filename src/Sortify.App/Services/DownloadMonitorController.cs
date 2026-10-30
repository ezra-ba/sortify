using Sortify.Infrastructure.Logging;
using Avalonia.Threading;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Infrastructure.Monitoring;
using Sortify.Core.Services;
using Sortify.Core.Rules;
using Sortify.Infrastructure.FileSystem;

namespace Sortify.App.Services;

public sealed class DownloadMonitorController(MainViewModel viewModel, IFileMoveService fileMoveService,
    IRuleEngine ruleEngine, FolderMonitor monitor, IActivityLog? log = null) : IDisposable
{
    // Compatibility constructor for existing UI tests and design-time callers.
    public DownloadMonitorController(MainViewModel viewModel)
        : this(viewModel, new FileMoveService(),
            new RuleEngine(new RuleValidator(), new RuleTargetValidator()), new FolderMonitor())
    {
    }

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
                var activity = log is null ? null : new LoggedFileMoveService(fileMoveService, log);
                var popup = new FileSuggestionWindow(
                    new FileSuggestionViewModel(
                        viewModel.TargetFolders,
                            path,
                            activity ?? fileMoveService,
                            ruleEngine,
                            () => viewModel.CurrentSettings))
                {
                    Topmost = true
                };
                popups.Add(popup);
                popup.Closed += (_, _) =>
                {
                    activity?.RecordDismissal(path);
                    popups.Remove(popup);
                };
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
