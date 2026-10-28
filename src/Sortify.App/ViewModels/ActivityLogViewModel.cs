using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.App.ViewModels;

public sealed record ActivityLogItem(DateTimeOffset TimestampUtc, string Level, string EventName, string Message);

public partial class ActivityLogViewModel(IActivityLog log) : ViewModelBase
{
    public ObservableCollection<ActivityLogItem> Entries { get; } = [];

    [ObservableProperty] public partial bool IsLoading { get; private set; }
    [ObservableProperty] public partial string StatusMessage { get; private set; } = "";
    [ObservableProperty] public partial string? ReadError { get; private set; }
    [ObservableProperty] public partial int SkippedEntries { get; private set; }

    public bool HasEntries => Entries.Count > 0;
    public bool HasNoEntries => !HasEntries;
    public bool HasReadError => !string.IsNullOrWhiteSpace(ReadError);
    public bool HasSkippedEntries => SkippedEntries > 0;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        StatusMessage = "Aktivitätsprotokoll wird geladen …";
        ReadError = null;
        SkippedEntries = 0;
        OnPropertyChanged(nameof(HasReadError));
        OnPropertyChanged(nameof(HasSkippedEntries));
        try
        {
            var result = await Task.Run(() => log.ReadRecent(100));
            Entries.Clear();
            foreach (var entry in result.Entries)
                Entries.Add(new ActivityLogItem(entry.TimestampUtc, FormatLevel(entry.Level), entry.EventName, entry.Message));
            ReadError = result.ErrorMessage is null ? null : "Das Protokoll konnte nicht vollständig gelesen werden.";
            SkippedEntries = result.SkippedEntries;
            StatusMessage = Entries.Count == 0 ? "Noch keine Aktivitäten protokolliert." : $"Letzte {Entries.Count}";
            OnPropertyChanged(nameof(HasEntries));
            OnPropertyChanged(nameof(HasNoEntries));
            OnPropertyChanged(nameof(HasReadError));
            OnPropertyChanged(nameof(HasSkippedEntries));
        }
        finally { IsLoading = false; }
    }

    private static string FormatLevel(LogLevel level) => level switch
    {
        LogLevel.Warning => "Warnung",
        LogLevel.Error => "Fehler",
        _ => "Information"
    };
}
