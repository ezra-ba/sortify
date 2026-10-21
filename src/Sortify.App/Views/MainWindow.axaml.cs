using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Sortify.App.ViewModels;

namespace Sortify.App.Views;

public partial class MainWindow : Window
{
    private TargetFolderViewModel? _draggedTargetFolder;
    private Avalonia.Point _dragStart;
    private bool _hasDragged;
    private Button? _pressedTargetButton;

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(InputElement.PointerPressedEvent, TargetFolderPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerMovedEvent, TargetFolderPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerReleasedEvent, TargetFolderPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void OpenFileSuggestion(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new FileSuggestionWindow(new FileSuggestionViewModel()).Show(this);
    }

    private async void SelectWatchedFolder(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Überwachungs-Ordner auswählen"
            });

        if (folders.Count == 0 ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var path = folders[0].TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(path))
        {
            viewModel.SettingsStatus =
                "Bitte einen lokalen Ordner auswählen.";
            return;
        }

        await viewModel.SaveWatchedFolderAsync(path);
    }

    private async void AddTargetFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Target-Ordner auswählen"
        });
        if (folders.Count > 0 && DataContext is MainViewModel viewModel)
            viewModel.AddTargetFolder(folders[0].Path.LocalPath);
    }

    private void TargetFolderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Button button && button.Classes.Contains("nickname-action"))
            return;

        var position = e.GetPosition(this);
        var folder = FindTargetFolderAt(position);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && folder is not null && !folder.IsEditingNickname)
        {
            if (DataContext is MainViewModel viewModel)
                viewModel.SelectTargetFolder(folder);

            _pressedTargetButton = FindTargetButtonAt(position);
            _pressedTargetButton?.Classes.Add("press-feedback");
            e.Pointer.Capture(this);
            _draggedTargetFolder = folder;
            _dragStart = e.GetPosition(this);
            _hasDragged = false;
            if (e.ClickCount == 2)
            {
                folder.IsEditingNickname = true;
                ResetDrag();
            }
        }
    }

    private void TargetFolderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Source is Button button && button.Classes.Contains("nickname-action"))
            return;

        if (_draggedTargetFolder is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        var position = e.GetPosition(this);
        var deltaX = position.X - _dragStart.X;
        var deltaY = position.Y - _dragStart.Y;
        if (!_hasDragged && (deltaX * deltaX) + (deltaY * deltaY) < 36) return;
        _hasDragged = true;
        _draggedTargetFolder.IsDragging = true;

        ShowDragPreview(_draggedTargetFolder, position);

        var target = FindTargetFolderAt(position);
        var targetCard = FindTargetFolderCardAt(position);
        if (target is not null && targetCard is not null && DataContext is MainViewModel viewModel && target != _draggedTargetFolder)
        {
            var sourceIndex = viewModel.TargetFolders.IndexOf(_draggedTargetFolder);
            var targetIndex = viewModel.TargetFolders.IndexOf(target);
            var targetTopLeft = targetCard.TranslatePoint(new Point(), this);
            if (targetTopLeft is null)
                return;

            var targetMiddle = targetTopLeft.Value.Y + (targetCard.Bounds.Height / 2);
            var movingDown = targetIndex > sourceIndex;
            var crossedTargetMiddle = movingDown ? position.Y >= targetMiddle : position.Y <= targetMiddle;

            if (crossedTargetMiddle)
                viewModel.MoveTargetFolder(_draggedTargetFolder, target);
        }
    }

    private TargetFolderViewModel? FindTargetFolderAt(Avalonia.Point position)
    {
        var visual = this.InputHitTest(position) as Visual;
        while (visual is not null)
        {
            if (visual.DataContext is TargetFolderViewModel folder)
                return folder;
            visual = visual.GetVisualParent();
        }

        return null;
    }

    private Border? FindTargetFolderCardAt(Avalonia.Point position)
    {
        var visual = this.InputHitTest(position) as Visual;
        while (visual is not null)
        {
            if (visual is Border border && border.Classes.Contains("target-folder-card"))
                return border;
            visual = visual.GetVisualParent();
        }

        return null;
    }

    private void TargetFolderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is Button button && button.Classes.Contains("nickname-action"))
            return;

        e.Pointer.Capture(null);
        ClearPressedButton();
        ResetDrag();
    }

    private Button? FindTargetButtonAt(Avalonia.Point position)
    {
        var visual = this.InputHitTest(position) as Visual;
        while (visual is not null)
        {
            if (visual is Button button && button.Classes.Contains("target-folder"))
                return button;
            visual = visual.GetVisualParent();
        }

        return null;
    }

    private void ClearPressedButton()
    {
        _pressedTargetButton?.Classes.Remove("press-feedback");
        _pressedTargetButton = null;
    }

    private void ShowDragPreview(TargetFolderViewModel folder, Avalonia.Point position)
    {
        DragPreviewNickname.Text = folder.Nickname;
        DragPreviewPath.Text = folder.Path;
        DragPreviewDefault.IsVisible = folder.IsDefault;
        DragPreview.IsVisible = true;
        Canvas.SetLeft(DragPreview, position.X + 12);
        Canvas.SetTop(DragPreview, position.Y + 12);
    }

    private void ResetDrag()
    {
        if (_draggedTargetFolder is not null)
            _draggedTargetFolder.IsDragging = false;

        ClearPressedButton();
        _draggedTargetFolder = null;
        _hasDragged = false;
        DragPreview.IsVisible = false;
    }
}
