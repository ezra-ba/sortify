using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sortify.App.ViewModels;

public partial class FileSuggestionViewModel : ViewModelBase
{
    public ObservableCollection<string> TargetFolders { get; } =
    [
        "Dokumente",
        "Bilder",
        "Archiv"
    ];

    [ObservableProperty]
    public partial string DetectedFileName { get; set; } = "rechnung_2026-10-07.pdf";

    [ObservableProperty]
    public partial string? SelectedTargetFolder { get; set; } = "Dokumente";

    [ObservableProperty]
    public partial string NewFileName { get; set; } = "Rechnung_2026-10-07.pdf";
}
