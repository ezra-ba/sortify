using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.App.ViewModels;

public partial class FileSuggestionViewModel : ViewModelBase
{
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

    public FileSuggestionViewModel(
        ObservableCollection<TargetFolderViewModel> targetFolders,
        FileDescriptor file,
        FileSuggestionProposal proposal) : this(targetFolders)
    {
        SourcePath = file.SourcePath;
        DetectedFileName = Path.GetFileName(file.SourcePath);
        FileExtension = file.Extension;
        NewFileName = Path.GetFileNameWithoutExtension(proposal.FileName);
        SelectedTargetFolder = TargetFolders.FirstOrDefault(t => t.Id == proposal.TargetFolderId);
        SuggestionExplanation = proposal.Explanation;
    }

    public string? SourcePath { get; }

    public string SuggestionExplanation { get; } = "Manuelle Popup-Vorschau";

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

    public bool IsFileNameValid => PortableNameValidator.IsValid(NewFileName + FileExtension)
        && NewFileName.Length > 0 && NewFileName.Length + FileExtension.Length <= 255;

    public bool IsValidationVisible => !IsFileNameValid;

    public string ValidationMessage => IsFileNameValid
        ? string.Empty
        : "Bitte einen gueltigen Dateinamen mit maximal 255 Zeichen inklusive Endung eingeben.";

    public string FilePathPreview => SelectedTargetFolder is null || string.IsNullOrWhiteSpace(NewFileName)
        ? string.Empty
        : Path.Combine(SelectedTargetFolder.Path, NewFileName + FileExtension);
}
