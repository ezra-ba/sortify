using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sortify.Core.Models;

namespace Sortify.App.ViewModels;

public partial class FileNameConditionViewModel(Action<FileNameConditionViewModel> remove) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContains))]
    [NotifyPropertyChangedFor(nameof(IsStartsWith))]
    [NotifyPropertyChangedFor(nameof(IsEndsWith))]
    [NotifyPropertyChangedFor(nameof(IsAnyName))]
    [NotifyPropertyChangedFor(nameof(NeedsText))]
    public partial FileNameConditionKind Kind { get; set; }

    [ObservableProperty] public partial string Text { get; set; } = "";
    public bool IsContains => Kind == FileNameConditionKind.Contains;
    public bool IsStartsWith => Kind == FileNameConditionKind.StartsWith;
    public bool IsEndsWith => Kind == FileNameConditionKind.EndsWith;
    public bool IsAnyName => Kind == FileNameConditionKind.AnyName;
    public bool NeedsText => !IsAnyName;
    public FileNameCondition ToCondition() => new(Kind, IsAnyName ? "" : Text);
    [RelayCommand] private void Contains() => Kind = FileNameConditionKind.Contains;
    [RelayCommand] private void StartsWith() => Kind = FileNameConditionKind.StartsWith;
    [RelayCommand] private void EndsWith() => Kind = FileNameConditionKind.EndsWith;
    [RelayCommand] private void AnyName() => Kind = FileNameConditionKind.AnyName;
    [RelayCommand] private void Remove() => remove(this);
}
