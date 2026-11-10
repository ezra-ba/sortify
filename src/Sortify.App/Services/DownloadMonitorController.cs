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
    IRuleEngine ruleEngine, FolderMonitor monitor, IActivityLog? log = null, ISystemNotificationService? notifications = null) : IDisposable
{
    // Compatibility constructor for existing UI tests and design-time callers.
    public DownloadMonitorController(MainViewModel viewModel)
        : this(viewModel, new FileMoveService(),
            new RuleEngine(new RuleValidator(), new RuleTargetValidator()), new FolderMonitor())
    {
    }

    private readonly List<FileSuggestionWindow> popups = [];
    private CancellationTokenSource? monitoringCancellation;
    private bool disposed;

    public bool IsMonitoring { get; private set; }

    public void Restart(string? directory)
    {
        if (disposed)
            return;

        StopMonitoring();
        monitoringCancellation = new CancellationTokenSource();
        var cancellationToken = monitoringCancellation.Token;
        IsMonitoring = false;
        if (string.IsNullOrWhiteSpace(directory))
        {
            viewModel.MonitoringStatus = "Bitte einen Ueberwachungsordner auswaehlen.";
            return;
        }

        try
        {
            monitor.Start(directory, path => Dispatcher.UIThread.Post(async () =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var activity = log is null ? null : new LoggedFileMoveService(fileMoveService, log);
                var suggestion = new FileSuggestionViewModel(viewModel.TargetFolders, path,
                    activity ?? fileMoveService, ruleEngine, () => viewModel.CurrentSettings);
                var prepareAutomatically = viewModel.CurrentSettings?.AutoSort == true;
                if (prepareAutomatically)
                {
                    try
                    {
                        await suggestion.InitializeFileOperationAsync(cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        var destination = await suggestion.TryAutoSortAsync(cancellationToken);
                        if (destination is not null)
                        {
                            if (notifications is not null)
                            {
                                var message = $"{DateTimeOffset.Now:HH:mm} Uhr · {Path.GetFileName(destination)}\nZiel: {Path.GetDirectoryName(destination)}";
                                await notifications.ShowAsync("Sortify · Datei verschoben", message);
                            }
                            return;
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                    return;
                var popup = new FileSuggestionWindow(suggestion, initializeOnOpen: !prepareAutomatically)
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
                if (!cancellationToken.IsCancellationRequested)
                {
                    monitoringCancellation?.Cancel();
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
        StopMonitoring();
    }

    private void StopMonitoring()
    {
        monitoringCancellation?.Cancel();
        monitoringCancellation?.Dispose();
        monitoringCancellation = null;
        monitor.Dispose();
        foreach (var popup in popups.ToArray())
            popup.Close();
    }
}
