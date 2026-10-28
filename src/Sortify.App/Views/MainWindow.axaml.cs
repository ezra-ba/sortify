using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Sortify.App.ViewModels;
using System.Runtime.InteropServices;

namespace Sortify.App.Views;

public partial class MainWindow : Window
{
    private TargetFolderViewModel? _draggedTargetFolder;
    private Avalonia.Point _dragStart;
    private bool _hasDragged;
    private Button? _pressedTargetButton;
    private Avalonia.Point _dragOffset;

    public MainWindow()
    {
        InitializeComponent();
        Opened += ApplyNativeTitleBarColor;
        if (OperatingSystem.IsWindows())
        {
            Win32Properties.AddWindowStylesCallback(this, static (style, exStyle) =>
                (style | 0x00010000u | 0x00020000u | 0x00040000u, exStyle));
        }
        AddHandler(InputElement.PointerPressedEvent, TargetFolderPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerMovedEvent, TargetFolderPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerReleasedEvent, TargetFolderPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void ApplyNativeTitleBarColor(object? sender, EventArgs e)
    {
        if (!OperatingSystem.IsWindows()) return;

        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero) return;

        var white = 0x00FFFFFF;
        var darkText = 0x001C1C1C;
        DwmSetWindowAttribute(handle, 35, ref white, sizeof(int));
        DwmSetWindowAttribute(handle, 36, ref darkText, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int value,
        int valueSize);

    private void OpenFileSuggestion(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
            new FileSuggestionWindow(new FileSuggestionViewModel(viewModel.TargetFolders)).Show(this);
    }

    private void OpenTargetRules(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel { SelectedTargetFolder: { } target }) return;
        var settings = new SettingsWindow { DataContext = DataContext };
        settings.OpenRulesForTarget(target);
        settings.Show(this);
    }

    private void OpenRuleTag(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: TargetRuleTagViewModel tag } ||
            DataContext is not MainViewModel { IsSavingSettings: false } main)
            return;
        var target = main.TargetFolders.FirstOrDefault(folder => folder.Id == tag.TargetId);
        if (target is null) return;
        main.SelectTargetFolder(target);
        var settings = new SettingsWindow { DataContext = main };
        settings.Show(this);
        if (tag.RuleId is { } ruleId) settings.OpenRuleForEditing(ruleId);
        else settings.OpenRulesForTarget(target);
        e.Handled = true;
    }

    private void OpenSettings(object? sender, RoutedEventArgs e)
    {
        new SettingsWindow { DataContext = DataContext }.Show(this);
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

    private async void AddTargetFolder(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Target-Ordner auswählen"
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
                "Bitte einen lokalen Target-Ordner auswählen.";
            return;
        }

        await viewModel.AddTargetFolderAsync(path);
    }

    private static bool IsRuleTagSource(object? source)
    {
        for (var visual = source as Visual; visual is not null; visual = visual.GetVisualParent())
            if (visual is Button button && button.Classes.Contains("rule-tag")) return true;
        return false;
    }

    private void TargetFolderPointerPressed(object? sender, PointerPressedEventArgs e)
    {

        if (DataContext is MainViewModel { IsSavingSettings: true })
        {
            return;
        }

        if (IsRuleTagSource(e.Source)) return;

        if (e.Source is Button button && button.Classes.Contains("nickname-action"))
            return;

        var position = e.GetPosition(this);
        var folder = FindTargetFolderAt(position);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && folder is not null && !folder.IsEditingNickname)
        {
            if (DataContext is MainViewModel viewModel)
                viewModel.SelectTargetFolder(folder);

            _pressedTargetButton = FindTargetButtonAt(position);
            if (_pressedTargetButton?.TranslatePoint(new Point(), this) is Point buttonTopLeft)
                _dragOffset = position - buttonTopLeft;
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

        if (DataContext is MainViewModel { IsSavingSettings: true })
        {
            return;
        }

        if (IsRuleTagSource(e.Source)) return;

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

    private async void TargetFolderPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {

        if (IsRuleTagSource(e.Source)) return;

        if (e.Source is Button button &&
            button.Classes.Contains("nickname-action"))
        {
            return;
        }

        var shouldSaveOrder =
            _hasDragged && _draggedTargetFolder is not null;

        e.Pointer.Capture(null);
        ClearPressedButton();
        ResetDrag();

        if (shouldSaveOrder && DataContext is MainViewModel viewModel)
        {
            await viewModel.SaveTargetFolderOrderAsync();
        }

        Dispatcher.UIThread.Post(ResetTargetFolderVisualStates);
    }

    private void ResetTargetFolderVisualStates()
    {
        foreach (var button in this.GetVisualDescendants().OfType<Button>()
                     .Where(button => button.Classes.Contains("target-folder")))
        {
            button.RenderTransform = new ScaleTransform(1, 1);
        }
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
        if (_pressedTargetButton is { } button)
        {
            button.RenderTransform = new ScaleTransform(1, 1);
        }

        _pressedTargetButton?.Classes.Remove("press-feedback");
        _pressedTargetButton = null;
    }

    private void ShowDragPreview(TargetFolderViewModel folder, Avalonia.Point position)
    {
        if (_pressedTargetButton is { Bounds.Width: > 0 } sourceButton)
            DragPreview.Width = sourceButton.Bounds.Width;

        var canvasPosition = position;
        if (DragPreview.Parent is Visual canvas &&
            this.TranslatePoint(position, canvas) is Point translatedPosition)
        {
            canvasPosition = translatedPosition;
        }

        DragPreviewNickname.Text = folder.Nickname;
        DragPreviewPath.Text = folder.Path;
        DragPreviewDefault.IsVisible = folder.IsDefault;
        DragPreview.IsVisible = true;
        Canvas.SetLeft(DragPreview, canvasPosition.X - _dragOffset.X);
        Canvas.SetTop(DragPreview, canvasPosition.Y - _dragOffset.Y);
    }

    private void ResetDrag()
    {
        if (_draggedTargetFolder is not null)
            _draggedTargetFolder.IsDragging = false;

        ClearPressedButton();
        _draggedTargetFolder = null;
        _dragOffset = new Point();
        _hasDragged = false;
        DragPreview.IsVisible = false;
    }
}
