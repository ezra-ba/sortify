using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Persistence;
using Sortify.App.Services;
using Sortify.Core.Services;
using Sortify.Infrastructure.Startup;

namespace Sortify.App;

public partial class App : Application
{
    private DesktopSession? session;
    private TrayMenu? trayMenu;
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
            MainViewModel viewModel;
            ISettingsService? settingsService = null;

            try
            {
                var store = new JsonSettingsStore(
                    SettingsStoragePath.GetDefaultFilePath(),
                    new SettingsValidator(new RuleValidator()));

                settingsService = new SettingsService(store);

                viewModel = new MainViewModel(settingsService);
            }
            catch (InvalidOperationException exception)
            {
                viewModel = new MainViewModel
                {
                    SettingsStatus = exception.Message
                };
            }

            var window = new MainWindow
            {
                DataContext = viewModel
            };

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
                new WindowsLoginStartup(Environment.ProcessPath), () => desktop.Shutdown(), trayAvailable);
            if (icon is not null)
            {
                trayMenu = new TrayMenu(icon, session);
            }
            window.Opened += (_, _) => desktop.MainWindow ??= window;
            desktop.Exit += (_, _) =>
            {
                session.Dispose();
                trayMenu?.Dispose();
                TrayIcon.SetIcons(this, null);
            };
            if (activationRequested)
                session.ShowMainWindow();
            Dispatcher.UIThread.Post(async () => await session.InitializeAsync(
                desktop.Args?.Contains("--background", StringComparer.OrdinalIgnoreCase) == true));
        }

        base.OnFrameworkInitializationCompleted();
    }
}
