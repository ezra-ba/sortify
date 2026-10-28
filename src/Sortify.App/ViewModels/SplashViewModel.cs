using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sortify.App.ViewModels;

public partial class SplashViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Status { get; set; } = "Sortify wird gestartet …";

    public string Version { get; } = "v" + (
        typeof(SplashViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0]
        ?? typeof(SplashViewModel).Assembly.GetName().Version?.ToString(3)
        ?? "unbekannt");
}
