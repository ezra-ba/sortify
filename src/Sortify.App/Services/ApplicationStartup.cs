using System.Diagnostics;
using Avalonia.Threading;
using Sortify.App.ViewModels;
using Sortify.App.Views;

namespace Sortify.App.Services;

public sealed class ApplicationStartup : IDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private SplashWindow? splash;
    private bool closingSplash;
    private bool started;
    private bool disposed;

    public async Task RunAsync(bool background,
        Func<Action<string>, CancellationToken, Task> initialize,
        Action<Exception> showError,
        Action cancelStartup)
    {
        if (disposed)
            return;
        if (started)
            throw new InvalidOperationException("Der Programmstart wurde bereits ausgeführt.");
        started = true;
        var token = cancellation.Token;

        try
        {
            if (!background)
            {
                splash = new SplashWindow();
                splash.Closed += (_, _) =>
                {
                    if (!closingSplash && !disposed)
                    {
                        Dispose();
                        cancelStartup();
                    }
                };
                splash.Show();
                // Give rendering priority before constructing the main window.
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            }
            token.ThrowIfCancellationRequested();
            await initialize(ReportStatus, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            Trace.TraceError("Sortify startup failed: {0}", error);
            CloseSplash();
            if (!disposed)
                showError(error);
        }
        finally
        {
            CloseSplash();
        }
    }

    private void ReportStatus(string status)
    {
        if (!disposed && splash?.DataContext is SplashViewModel model)
            model.Status = status;
    }

    private void CloseSplash()
    {
        closingSplash = true;
        splash?.Close();
        splash = null;
    }

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        cancellation.Cancel();
        CloseSplash();
        cancellation.Dispose();
    }
}
