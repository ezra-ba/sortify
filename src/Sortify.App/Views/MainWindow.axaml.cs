using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
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
    private Avalonia.Point _dragOffset;

    public MainWindow()
    {
        InitializeComponent();
        if (OperatingSystem.IsWindows())
        {
            Win32Properties.AddWindowStylesCallback(this, static (style, exStyle) =>
                (style | 0x00010000u | 0x00020000u | 0x00040000u, exStyle));
        }
        AddHandler(InputElement.PointerPressedEvent, TargetFolderPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerMovedEvent, TargetFolderPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerReleasedEvent, TargetFolderPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerEnteredEvent, ButtonPointerEntered, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerExitedEvent, ButtonPointerExited, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerPressedEvent, ButtonPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(InputElement.PointerReleasedEvent, ButtonPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private static readonly IBrush Accent = Brushes.White;
    private static readonly IBrush AccentHover = new SolidColorBrush(Color.Parse("#F7F3FF"));
    private static readonly IBrush AccentPressed = new SolidColorBrush(Color.Parse("#EEE8FF"));
    private static readonly IBrush SoftAccent = new SolidColorBrush(Color.Parse("#F7F3FF"));
    private static readonly IBrush SoftAccentPressed = new SolidColorBrush(Color.Parse("#EEE8FF"));
    private static readonly IBrush Surface = new SolidColorBrush(Color.Parse("#FFFFFF"));

    private static Button? GetButton(object? source)
    {
        var visual = source as Visual;
        while (visual is not null)
        {
            if (visual is Button button) return button;
            visual = visual.GetVisualParent();
        }

        return null;
    }

    private static void ButtonPointerEntered(object? sender, PointerEventArgs e)
    {
        if (GetButton(e.Source) is not { } button) return;
        if (button.Classes.Contains("window-button"))
        {
            button.Background = button.Classes.Contains("close")
                ? SoftAccent
                : SoftAccent;
            return;
        }
        button.Background = button.Classes.Contains("primary") ? AccentHover : SoftAccent;
    }

    private static void ButtonPointerExited(object? sender, PointerEventArgs e)
    {
        if (GetButton(e.Source) is not { } button) return;
        button.RenderTransform = new ScaleTransform(1, 1);
        if (button.Classes.Contains("window-button"))
        {
            button.Background = Brushes.Transparent;
            return;
        }
        button.Background = button.Classes.Contains("primary")
            ? Accent
            : button.Classes.Contains("target-folder") && button.Classes.Contains("selected")
                ? SoftAccent
                : Surface;
    }

    private static void ButtonPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (GetButton(e.Source) is not { } button) return;
        button.RenderTransform = new ScaleTransform(0.97, 0.97);
        if (button.Classes.Contains("window-button"))
        {
            button.Background = button.Classes.Contains("close")
                ? new SolidColorBrush(Color.Parse("#F9D8D8"))
                : SoftAccentPressed;
            return;
        }
        button.Background = button.Classes.Contains("primary") ? AccentPressed : SoftAccentPressed;
    }

    private static void ButtonPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var button = GetButton(e.Source) ?? e.Pointer.Captured as Button;
        if (button is null) return;
        button.RenderTransform = new ScaleTransform(1, 1);
        if (button.Classes.Contains("window-button"))
        {
            button.Background = Brushes.Transparent;
            return;
        }
        button.Background = button.Classes.Contains("target-folder") && button.Classes.Contains("selected")
            ? SoftAccent
            : Brushes.White;
    }

    private void OpenFileSuggestion(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
            new FileSuggestionWindow(new FileSuggestionViewModel(viewModel.TargetFolders)).Show(this);
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void WindowButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string action })
            return;

        switch (action)
        {
            case "minimize":
                WindowState = WindowState.Minimized;
                break;
            case "maximize":
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                break;
            case "close":
                Close();
                break;
        }
    }

    private static void CloseButton_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.Background = new SolidColorBrush(Color.Parse("#FDECEC"));
            button.Foreground = new SolidColorBrush(Color.Parse("#1C1C1E"));
        }
    }

    private static void CloseButton_PointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.Background = Brushes.Transparent;
            button.Foreground = new SolidColorBrush(Color.Parse("#6E6E73"));
        }
    }

    private static void CloseButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Background = SoftAccentPressed;
            button.Foreground = new SolidColorBrush(Color.Parse("#1C1C1E"));
        }
    }

    private static void CloseButton_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        CloseButton_PointerExited(sender, e);
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

    private void TargetFolderPointerPressed(object? sender, PointerPressedEventArgs e)
    {

        if (DataContext is MainViewModel { IsSavingSettings: true })
        {
            return;
        }

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
            button.Background = button.Classes.Contains("selected")
                ? new SolidColorBrush(Color.Parse("#F7F3FF"))
                : Brushes.White;
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
            button.Background = button.Classes.Contains("selected")
                ? new SolidColorBrush(Color.Parse("#F7F3FF"))
                : Brushes.White;
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
