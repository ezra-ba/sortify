using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sortify.Core.Models;
using Sortify.Core.Services;


namespace Sortify.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsSavingSettings { get; set; }

    private async Task RunSettingsChangeAsync(Func<Task> operation)
    {
        if (IsSavingSettings)
        {
            return;
        }

        IsSavingSettings = true;

        try
        {
            await operation();
        }
        finally
        {
            IsSavingSettings = false;
        }
    }

    public Task SaveTargetFolderOrderAsync()
    {
        return RunSettingsChangeAsync(
            SaveTargetFolderOrderCoreAsync);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTargetFolder))]
    private Task SetDefaultTargetFolderAsync()
    {
        return RunSettingsChangeAsync(
            SetDefaultTargetFolderCoreAsync);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTargetFolder))]
    private Task RemoveTargetFolderAsync()
    {
        return RunSettingsChangeAsync(
            RemoveTargetFolderCoreAsync);
    }

    [RelayCommand]
    private Task SaveTargetNicknameAsync(TargetFolderViewModel folder)
    {
        return RunSettingsChangeAsync(
            () => SaveTargetNicknameCoreAsync(folder));
    }



    public string DefaultTargetFolderButtonText =>
    SelectedTargetFolder?.IsDefault == true
        ? "Standard aufheben"
        : "Als Standard festlegen";
    private readonly ISettingsService? settingsService;

    public AppSettings? CurrentSettings => settingsService?.Current;


    public event Action? MonitoringFolderConfigured;

    [ObservableProperty]
    public partial string MonitoringStatus { get; set; } = "";

    public MainViewModel()
    {
        // Für die Vorschau im Avalonia-Designer.
    }

    public MainViewModel(ISettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(settingsService);
        this.settingsService = settingsService;
    }

    [ObservableProperty]
    public partial string SettingsStatus { get; set; } =
        "Einstellungen werden geladen.";

    public async Task InitializeSettingsAsync()
    {
        if (settingsService is null)
        {
            return;
        }

        var result = await settingsService.InitializeAsync();

        WatchedFolder = settingsService.Current?.WatchedFolderPath
            ?? "Noch kein Ordner ausgewählt";

        if (settingsService.Current is { } settings)
        {
            RestoreTargetFolders(settings);
        }

        SettingsStatus = result.Status switch
        {
            SettingsLoadStatus.Loaded => "Einstellungen geladen.",
            SettingsLoadStatus.NotFound => "Bitte den ersten Ordner einrichten.",
            _ => result.ErrorMessage
                ?? "Die Einstellungen konnten nicht geladen werden."
        };
    }
    private void RestoreTargetFolders(AppSettings settings)
    {
        SetSelectedTargetFolder(null);
        TargetFolders.Clear();

        foreach (var target in settings.TargetFolders)
        {
            TargetFolders.Add(new TargetFolderViewModel(
                target.Id,
                target.Nickname,
                target.Path,
                target.Id == settings.DefaultTargetFolderId));
        }
    }

    public Task SaveWatchedFolderAsync(string path)
    {
        return RunSettingsChangeAsync(
            () => SaveWatchedFolderCoreAsync(path));
    }

    private async Task SaveWatchedFolderCoreAsync(string path)
    {
        if (settingsService is null)
        {
            SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
            return;
        }

        var result = await settingsService.UpdateAsync(current => current with
        {
            WatchedFolderPath = path
        });

        if (!result.IsSuccess)
        {
            SettingsStatus = result.ErrorMessage
                ?? "Der Ordner konnte nicht gespeichert werden.";
            return;
        }

        WatchedFolder = settingsService.Current!.WatchedFolderPath!;
        SettingsStatus = "Überwachungsordner gespeichert.";
        MonitoringFolderConfigured?.Invoke();
    }
    private async Task<SettingsSaveResult> SaveRulesCoreAsync(
        IReadOnlyList<SortRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        if (settingsService is null)
        {
            var failure = new SettingsSaveResult(
                false,
                "Die Einstellungen sind nicht verfügbar.");

            SettingsStatus = failure.ErrorMessage!;
            return failure;
        }

        // Die übergebene Liste für diesen Speichervorgang übernehmen.
        var rulesSnapshot = rules
            .Select(rule => rule with
            {
                AllowedExtensions = rule.AllowedExtensions.ToArray()
            })
            .ToArray();

        var result = await settingsService.UpdateAsync(current => current with
        {
            Rules = rulesSnapshot
        });

        SettingsStatus = result.IsSuccess
            ? "Sortierregeln gespeichert."
            : result.ErrorMessage
                ?? "Die Sortierregeln konnten nicht gespeichert werden.";

        return result;
    }
    public ObservableCollection<TargetFolderViewModel> TargetFolders { get; } =
        [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedTargetFolder))]
    [NotifyPropertyChangedFor(nameof(DefaultTargetFolderButtonText))]
    [NotifyCanExecuteChangedFor(nameof(SetDefaultTargetFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveTargetFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(BeginRenameTargetFolderCommand))]
    public partial TargetFolderViewModel? SelectedTargetFolder { get; set; }

    [ObservableProperty]
    public partial string WatchedFolder { get; set; } = "Noch kein Ordner ausgewählt";

    public bool HasSelectedTargetFolder => SelectedTargetFolder is not null;

    [RelayCommand]
    public void SelectTargetFolder(TargetFolderViewModel folder) => SetSelectedTargetFolder(folder);

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
    private async Task SaveTargetFolderOrderCoreAsync()
    {
        if (settingsService is null)
        {
            SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
            return;
        }

        var orderedIds = TargetFolders
            .Select(folder => folder.Id)
            .ToArray();

        var targetsMatch = false;

        var result = await settingsService.UpdateAsync(current =>
        {
            var storedTargets = current.TargetFolders
                .ToDictionary(target => target.Id);

            targetsMatch =
                orderedIds.Length == storedTargets.Count &&
                orderedIds.Distinct().Count() == storedTargets.Count &&
                orderedIds.All(id => storedTargets.ContainsKey(id));

            if (!targetsMatch)
            {
                return current;
            }

            return current with
            {
                TargetFolders = orderedIds
                    .Select(id => storedTargets[id])
                    .ToArray()
            };
        });

        if (!result.IsSuccess || !targetsMatch)
        {
            if (settingsService.Current is { } savedSettings)
            {
                RestoreSavedTargetOrder(savedSettings);
            }

            SettingsStatus = !result.IsSuccess
                ? result.ErrorMessage
                    ?? "Die Reihenfolge konnte nicht gespeichert werden."
                : "Die Target-Liste hat sich geändert. Bitte erneut sortieren.";

            return;
        }

        SettingsStatus = "Target-Reihenfolge gespeichert.";
    }

    private void RestoreSavedTargetOrder(AppSettings settings)
    {
        var cardsById = TargetFolders.ToDictionary(folder => folder.Id);

        var sameTargets =
            cardsById.Count == settings.TargetFolders.Count &&
            settings.TargetFolders.All(
                target => cardsById.ContainsKey(target.Id));

        if (!sameTargets)
        {
            RestoreTargetFolders(settings);
            return;
        }

        // Vorhandene Karten samt Auswahl und Namensentwürfen behalten.
        for (var index = 0; index < settings.TargetFolders.Count; index++)
        {
            var card = cardsById[settings.TargetFolders[index].Id];
            var currentIndex = TargetFolders.IndexOf(card);

            if (currentIndex != index)
            {
                TargetFolders.Move(currentIndex, index);
            }
        }
    }

    public Task AddTargetFolderAsync(string path)
    {
        return RunSettingsChangeAsync(
            () => AddTargetFolderCoreAsync(path));
    }

    private async Task AddTargetFolderCoreAsync(string path)
    {
        if (settingsService is null)
        {
            SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
            return;
        }

        var name = Path.GetFileName(
            Path.TrimEndingDirectorySeparator(path));

        var proposedTarget = new TargetFolder(
            Guid.NewGuid(),
            path,
            string.IsNullOrWhiteSpace(name) ? path : name);

        TargetFolder? savedTarget = null;
        var alreadyExists = false;

        var result = await settingsService.UpdateAsync(current =>
        {
            // Prüfung innerhalb der Sperre: Auch ein unmittelbar zuvor
            // hinzugefügter Ordner wird berücksichtigt.
            var existing = current.TargetFolders.FirstOrDefault(
                target => string.Equals(
                    target.Path,
                    path,
                    StringComparison.Ordinal));

            if (existing is not null)
            {
                alreadyExists = true;
                savedTarget = existing;
                return current;
            }

            savedTarget = proposedTarget;

            return current with
            {
                TargetFolders = current.TargetFolders
                    .Append(proposedTarget)
                    .ToArray()
            };
        });

        if (!result.IsSuccess)
        {
            SettingsStatus = result.ErrorMessage
                ?? "Der Target-Ordner konnte nicht gespeichert werden.";
            return;
        }

        if (savedTarget is null)
        {
            SettingsStatus = "Der gespeicherte Target-Ordner fehlt.";
            return;
        }

        var card = TargetFolders.FirstOrDefault(
            folder => folder.Id == savedTarget.Id);

        if (card is null)
        {
            card = new TargetFolderViewModel(
                savedTarget.Id,
                savedTarget.Nickname,
                savedTarget.Path,
                savedTarget.Id ==
                    settingsService.Current!.DefaultTargetFolderId);

            TargetFolders.Add(card);
        }

        SetSelectedTargetFolder(card);

        SettingsStatus = alreadyExists
            ? "Dieser Target-Ordner ist bereits vorhanden."
            : "Target-Ordner gespeichert.";
    }
    private async Task SetDefaultTargetFolderCoreAsync()
    {
        var selected = SelectedTargetFolder;

        if (selected is null)
        {
            return;
        }

        if (settingsService is null)
        {
            SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
            return;
        }

        var targetExists = true;

        var result = await settingsService.UpdateAsync(current =>
        {
            if (!current.TargetFolders.Any(target => target.Id == selected.Id))
            {
                targetExists = false;
                return current;
            }

            return current with
            {
                DefaultTargetFolderId =
                    current.DefaultTargetFolderId == selected.Id
                        ? null
                        : selected.Id
            };
        });

        if (!result.IsSuccess)
        {
            SettingsStatus = result.ErrorMessage
                ?? "Das Standardziel konnte nicht gespeichert werden.";
            return;
        }

        var defaultId = settingsService.Current!.DefaultTargetFolderId;

        foreach (var folder in TargetFolders)
        {
            folder.IsDefault = folder.Id == defaultId;
        }

        OnPropertyChanged(nameof(DefaultTargetFolderButtonText));

        if (!targetExists)
        {
            SettingsStatus = "Der ausgewählte Target-Ordner ist nicht gespeichert.";
            return;
        }

        SettingsStatus = defaultId is null
            ? "Standardziel aufgehoben."
            : "Standardziel gespeichert.";
    }
    private async Task RemoveTargetFolderCoreAsync()
    {
        var selected = SelectedTargetFolder;

        if (selected is null)
        {
            return;
        }

        if (settingsService is null)
        {
            SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
            return;
        }

        var result = await settingsService.UpdateAsync(current => current with
        {
            TargetFolders = current.TargetFolders
                .Where(target => target.Id != selected.Id)
                .ToArray(),

            DefaultTargetFolderId =
                current.DefaultTargetFolderId == selected.Id
                    ? null
                    : current.DefaultTargetFolderId
        });

        if (!result.IsSuccess)
        {
            SettingsStatus = result.ErrorMessage
                ?? "Der Target-Ordner konnte nicht entfernt werden.";
            return;
        }

        var previousIndex = TargetFolders.IndexOf(selected);

        SetSelectedTargetFolder(null);
        TargetFolders.Remove(selected);

        if (TargetFolders.Count > 0)
        {
            var nextIndex = Math.Clamp(
                previousIndex,
                0,
                TargetFolders.Count - 1);

            SetSelectedTargetFolder(TargetFolders[nextIndex]);
        }

        OnPropertyChanged(nameof(DefaultTargetFolderButtonText));
        SettingsStatus = "Target-Ordner aus der Konfiguration entfernt.";
    }

    private async Task SaveTargetNicknameCoreAsync(TargetFolderViewModel folder)
    {
        if (settingsService is null)
        {
            SettingsStatus = "Die Einstellungen sind nicht verfügbar.";
            return;
        }

        var nickname = folder.DraftNickname.Trim();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            SettingsStatus = "Der Spitzname darf nicht leer sein.";
            return;
        }

        var targetExists = false;

        var result = await settingsService.UpdateAsync(current =>
        {
            targetExists = current.TargetFolders.Any(
                target => target.Id == folder.Id);

            if (!targetExists)
            {
                return current;
            }

            return current with
            {
                TargetFolders = current.TargetFolders
                    .Select(target => target.Id == folder.Id
                        ? target with { Nickname = nickname }
                        : target)
                    .ToArray()
            };
        });

        if (!result.IsSuccess)
        {
            SettingsStatus = result.ErrorMessage
                ?? "Der Spitzname konnte nicht gespeichert werden.";
            return;
        }

        if (!targetExists)
        {
            SettingsStatus = "Der Target-Ordner ist nicht mehr gespeichert.";
            return;
        }

        folder.Nickname = nickname;
        folder.IsEditingNickname = false;
        SettingsStatus = "Spitzname gespeichert.";
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTargetFolder))]
    private void BeginRenameTargetFolder()
    {
        if (SelectedTargetFolder is not null) SelectedTargetFolder.IsEditingNickname = true;
    }

    public async Task<SettingsSaveResult> SaveRulesAsync(
        IReadOnlyList<SortRule> rules)
    {
        if (IsSavingSettings)
        {
            return new SettingsSaveResult(
                false,
                "Eine andere Änderung wird gerade gespeichert. " +
                "Bitte anschließend erneut versuchen.");
        }

        IsSavingSettings = true;

        try
        {
            return await SaveRulesCoreAsync(rules);
        }
        finally
        {
            IsSavingSettings = false;
        }
    }

}

public partial class TargetFolderViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string DraftNickname { get; set; } = "";

    partial void OnIsEditingNicknameChanged(bool value)
    {
        if (value)
        {
            DraftNickname = Nickname;
        }
    }
    public TargetFolderViewModel(
        string nickname,
        string path,
        bool isDefault = false)
        : this(Guid.NewGuid(), nickname, path, isDefault)
    {
    }

    public TargetFolderViewModel(
        Guid id,
        string nickname,
        string path,
        bool isDefault = false)
    {
        Id = id;
        Nickname = nickname;
        Path = path;
        IsDefault = isDefault;
    }

    public Guid Id { get; }

    public string Path { get; }

    [ObservableProperty] public partial string Nickname { get; set; }
    [ObservableProperty] public partial bool IsDefault { get; set; }
    [ObservableProperty] public partial bool IsSelected { get; set; }
    [ObservableProperty] public partial bool IsEditingNickname { get; set; }
    [ObservableProperty] public partial bool IsDragging { get; set; }

    [RelayCommand] private void CancelRename() => IsEditingNickname = false;
}
