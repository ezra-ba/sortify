using Avalonia;
using System;
using Avalonia.Threading;
using Sortify.Infrastructure.Startup;

namespace Sortify.App;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        using var instance = new SingleInstance();
        if (!instance.IsPrimary)
        {
            try
            {
                instance.NotifyAsync(!args.Contains("--background", StringComparer.OrdinalIgnoreCase))
                    .GetAwaiter().GetResult();
                return 0;
            }
            catch (Exception error) when (error is System.IO.IOException or OperationCanceledException)
            {
                Console.Error.WriteLine("Sortify laeuft bereits, konnte aber nicht aktiviert werden: " + error.Message);
                return 1;
            }
        }
        return BuildAvaloniaApp()
            .AfterSetup(_ => instance.Listen(() =>
                Dispatcher.UIThread.Post(() => (Application.Current as App)?.ShowMainWindow())))
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
