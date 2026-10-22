using Avalonia.Threading;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.App.Services;

public sealed class DownloadMonitorController(
    DownloadNotificationWorkflow workflow,
    Action<string> reportStatus) : IAsyncDisposable
{
    private CancellationTokenSource? session;
    private Task running = Task.CompletedTask;
    private bool disposed;

    public void Restart(string? directory)
    {
        if (disposed)
            return;
        try { session?.Cancel(); }
        catch (ObjectDisposedException) { }
        var previous = running;
        session = new CancellationTokenSource();
        var token = session.Token;
        var source = session;
        running = Task.Run(async () =>
        {
            try
            {
                await previous;
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(directory))
                {
                    Report("Bitte einen Ueberwachungsordner auswaehlen.", token);
                    return;
                }

                Report("Ordnerueberwachung aktiv.", token);
                await workflow.RunAsync(directory, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception exception)
            {
                Report("Ordnerueberwachung gestoppt: " + exception.Message, token);
            }
            finally
            {
                // Disposal belongs to this run; a newer session may already exist.
                source.Dispose();
            }
        });
    }

    private void Report(string text, CancellationToken token) =>
        Dispatcher.UIThread.Post(() =>
        {
            if (!token.IsCancellationRequested)
                reportStatus(text);
        });

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (session is not null)
        {
            // A completed run already disposed its token source.
            try { session.Cancel(); }
            catch (ObjectDisposedException) { }
        }
        await running;
    }
}
