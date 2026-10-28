using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using Sortify.App.Services;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Rules;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;
using Sortify.Infrastructure.Startup;
using Sortify.Infrastructure.Logging;
using Sortify.Infrastructure.FileSystem;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.App;

public partial class App : Application
{
    private DesktopSession? session;
    private TrayMenu? trayMenu;
    private ApplicationStartup? startup;
    private bool activationRequested;

    public void ShowMainWindow()
    {
        activationRequested = true;
        session?.ShowMainWindow();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            startup = new ApplicationStartup();
            var background = desktop.Args?.Contains("--background", StringComparer.OrdinalIgnoreCase) == true;
            desktop.Exit += (_, _) =>
            {
                startup.Dispose();
                session?.Dispose();
                trayMenu?.Dispose();
                TrayIcon.SetIcons(this, null);
            };
            Dispatcher.UIThread.Post(async () => await startup.RunAsync(background,
                (reportStatus, token) => InitializeDesktopAsync(desktop, background, reportStatus, token),
                _ => ShowStartupFailure(desktop),
                () => desktop.Shutdown()));
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeDesktopAsync(IClassicDesktopStyleApplicationLifetime desktop,
        bool background, Action<string> reportStatus, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        reportStatus("Oberfläche wird vorbereitet …");
        MainViewModel viewModel;
        IActivityLog log = new JsonActivityLog(JsonActivityLog.GetDefaultDirectory());
        ISettingsService? settingsService = null;

        try
        {
            var store = new LoggedSettingsStore(new JsonSettingsStore(
                SettingsStoragePath.GetDefaultFilePath(),
                new SettingsValidator(new RuleValidator())), log);
            settingsService = new SettingsService(store);
            viewModel = new MainViewModel(settingsService,
                new WindowsLoginStartup(Environment.ProcessPath));
        }
        catch (InvalidOperationException exception)
        {
            viewModel = new MainViewModel { SettingsStatus = exception.Message };
        }

        var moves = (IFileMoveService)new LoggedFileMoveService(new FileMoveService(), log);
        var engine = (IRuleEngine)new LoggedRuleEngine(
            new RuleEngine(new RuleValidator(), new RuleTargetValidator()), log);
        var monitor = new FolderMonitor(log);
        var window = new MainWindow(log) { DataContext = viewModel };

        // Keep unsupported tray environments accessible instead of hiding the only window.
        var trayAvailable = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();
        TrayIcon? icon = null;
        if (trayAvailable)
        {
            using var stream = AssetLoader.Open(new Uri("avares://Sortify.App/Assets/Branding/sortify-icon.png"));
            icon = new TrayIcon { Icon = new WindowIcon(stream), IsVisible = true };
            TrayIcon.SetIcons(this, new TrayIcons { icon });
            trayAvailable = icon.NativeMenuExporter is not null;
            if (!trayAvailable)
            {
                TrayIcon.SetIcons(this, null);
                icon.Dispose();
                icon = null;
            }
        }
        session = new DesktopSession(viewModel, window, settingsService,
            new WindowsLoginStartup(Environment.ProcessPath), () => desktop.Shutdown(), trayAvailable,
            moves, engine, monitor);
        if (icon is not null)
            trayMenu = new TrayMenu(icon, session);

        window.Opened += (_, _) => desktop.MainWindow ??= window;
        if (activationRequested)
            session.ShowMainWindow();
        await session.InitializeAsync(background, reportStatus, cancellationToken);
    }

    private void ShowStartupFailure(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (session is not null)
        {
            session.ShowStartupFailure();
            return;
        }

        var close = new Button { Content = "Beenden" };
        close.Click += (_, _) => desktop.Shutdown();
        var errorWindow = new Window
        {
            Title = "Sortify – Startfehler",
            Width = 480,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Sortify konnte nicht gestartet werden. Bitte die Anwendung beenden und erneut starten.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    },
                    close
                }
            }
        };
        errorWindow.Closed += (_, _) => desktop.Shutdown();
        desktop.MainWindow = errorWindow;
        errorWindow.Show();
    }
}
