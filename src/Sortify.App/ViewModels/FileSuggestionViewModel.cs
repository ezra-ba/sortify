using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sortify.App.ViewModels;

public partial class FileSuggestionViewModel : ViewModelBase
{
    private static readonly Regex ValidFileNamePattern = new("^[\\p{L}\\p{N}_-](?:[\\p{L}\\p{N}_- ]{0,253}[\\p{L}\\p{N}_-])?$", RegexOptions.CultureInvariant);

    public ObservableCollection<TargetFolderViewModel> TargetFolders { get; }

    public FileSuggestionViewModel()
        : this([])
    {
    }

    public FileSuggestionViewModel(ObservableCollection<TargetFolderViewModel> targetFolders)
    {
        TargetFolders = targetFolders;
        SelectedTargetFolder = TargetFolders.FirstOrDefault(folder => folder.IsDefault)
            ?? TargetFolders.FirstOrDefault();
    }

    [ObservableProperty]
    public partial string DetectedFileName { get; set; } = "rechnung_2026-10-07.pdf";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilePathPreview))]
    public partial TargetFolderViewModel? SelectedTargetFolder { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilePathPreview))]
    [NotifyPropertyChangedFor(nameof(IsFileNameValid))]
    [NotifyPropertyChangedFor(nameof(IsValidationVisible))]
    [NotifyPropertyChangedFor(nameof(ValidationMessage))]
    public partial string NewFileName { get; set; } = "Rechnung_2026-10-07";

    public string FileExtension { get; } = ".pdf";

    [ObservableProperty]
    public partial bool HasValidationError { get; set; }

    public bool IsFileNameValid => ValidFileNamePattern.IsMatch(NewFileName.Normalize());

    public bool IsValidationVisible => !IsFileNameValid;

    public string ValidationMessage => IsFileNameValid
        ? string.Empty
        : "Erlaubt sind maximal 255 Unicode-Buchstaben, Zahlen, Leerzeichen sowie _ und -. Führende oder abschließende Leerzeichen und Sonderzeichen sind nicht erlaubt.";

    public string FilePathPreview => SelectedTargetFolder is null || string.IsNullOrWhiteSpace(NewFileName)
        ? string.Empty
        : Path.Combine(SelectedTargetFolder.Path, NewFileName + FileExtension);
}
