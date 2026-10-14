using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sortify.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<TargetFolderViewModel> TargetFolders { get; } =
    [
        new("Dokumente", @"C:\Users\Manuel\Dokumente", true),
        new("Bilder", @"D:\Archiv\Bilder"),
        new("Archiv", @"D:\Archiv\Alt")
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedTargetFolder))]
    [NotifyCanExecuteChangedFor(nameof(SetDefaultTargetFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveTargetFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(BeginRenameTargetFolderCommand))]
    public partial TargetFolderViewModel? SelectedTargetFolder { get; set; }

    [ObservableProperty]
    public partial string WatchedFolder { get; set; } = "Noch kein Ordner ausgewählt";

    public bool HasSelectedTargetFolder => SelectedTargetFolder is not null;

    [RelayCommand]
    private void SelectTargetFolder(TargetFolderViewModel folder) => SetSelectedTargetFolder(folder);

    private void SetSelectedTargetFolder(TargetFolderViewModel? folder)
    {
        if (SelectedTargetFolder is not null) SelectedTargetFolder.IsSelected = false;
        SelectedTargetFolder = folder;
        if (SelectedTargetFolder is not null) SelectedTargetFolder.IsSelected = true;
    }

    public void MoveTargetFolder(TargetFolderViewModel source, TargetFolderViewModel target)
    {
        var sourceIndex = TargetFolders.IndexOf(source);
        var targetIndex = TargetFolders.IndexOf(target);
        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex) return;

        TargetFolders.Move(sourceIndex, targetIndex);
        SetSelectedTargetFolder(source);
    }

    public void AddTargetFolder(string path)
    {
        var existing = TargetFolders.FirstOrDefault(folder =>
            string.Equals(folder.Path, path, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            SetSelectedTargetFolder(existing);
            return;
        }

        var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var folder = new TargetFolderViewModel(string.IsNullOrEmpty(name) ? path : name, path);
        TargetFolders.Add(folder);
        SetSelectedTargetFolder(folder);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTargetFolder))]
    private void SetDefaultTargetFolder()
    {
        foreach (var folder in TargetFolders) folder.IsDefault = folder == SelectedTargetFolder;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTargetFolder))]
    private void RemoveTargetFolder()
    {
        var folder = SelectedTargetFolder;
        if (folder is null) return;
        var index = TargetFolders.IndexOf(folder);
        TargetFolders.Remove(folder);
        SetSelectedTargetFolder(TargetFolders.ElementAtOrDefault(Math.Min(index, TargetFolders.Count - 1)));
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTargetFolder))]
    private void BeginRenameTargetFolder()
    {
        if (SelectedTargetFolder is not null) SelectedTargetFolder.IsEditingNickname = true;
    }
}

public partial class TargetFolderViewModel : ObservableObject
{
    public TargetFolderViewModel(string nickname, string path, bool isDefault = false)
    {
        Nickname = nickname; Path = path; IsDefault = isDefault;
    }

    public string Path { get; }

    [ObservableProperty] public partial string Nickname { get; set; }
    [ObservableProperty] public partial bool IsDefault { get; set; }
    [ObservableProperty] public partial bool IsSelected { get; set; }
    [ObservableProperty] public partial bool IsEditingNickname { get; set; }

    [RelayCommand]
    private void SaveNickname()
    {
        Nickname = Nickname.Trim();
        if (Nickname.Length == 0) Nickname = System.IO.Path.GetFileName(Path.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        IsEditingNickname = false;
    }

    [RelayCommand] private void CancelRename() => IsEditingNickname = false;
}
