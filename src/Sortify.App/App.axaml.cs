using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Persistence;
using Sortify.App.Services;

namespace Sortify.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainViewModel viewModel;

            try
            {
                var store = new JsonSettingsStore(
                    SettingsStoragePath.GetDefaultFilePath(),
                    new SettingsValidator(new RuleValidator()));

                var settingsService = new SettingsService(store);

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

            desktop.MainWindow = window;

            var controller = new DownloadMonitorController(viewModel);
            var initialized = false;
            viewModel.MonitoringFolderConfigured += () =>
            {
                if (initialized)
                    controller.Restart(viewModel.CurrentSettings?.WatchedFolderPath);
            };
            window.Closed += (_, _) => controller.Dispose();

            window.Opened += async (_, _) =>
            {
                await viewModel.InitializeSettingsAsync();
                initialized = true;
                controller.Restart(viewModel.CurrentSettings?.WatchedFolderPath);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
