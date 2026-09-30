using CommunityToolkit.Mvvm.ComponentModel;

namespace Sortify.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ProjectStatus { get; set; } = "Bereit für die erste Konfiguration";

    [ObservableProperty]
    public partial string WatchedFolder { get; set; } = "Noch kein Ordner ausgewählt";
}
