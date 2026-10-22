using Avalonia.Controls;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Sortify.App.ViewModels;
using System.Runtime.InteropServices;

namespace Sortify.App.Views;

public partial class FileSuggestionWindow : Window
{
    private CancellationTokenSource? _shakeCancellation;

    public FileSuggestionWindow()
    {
        InitializeComponent();
        Opened += RemoveWindowsChrome;
    }

    private void RemoveWindowsChrome(object? sender, EventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            PositionBottomRight();
            return;
        }

        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero)
            return;

        var style = GetWindowLongPtr(handle, WindowStyleIndex).ToInt64();
        style &= ~(WindowCaption | WindowThickFrame | WindowMinimizeBox | WindowMaximizeBox | WindowSystemMenu);
        SetWindowLongPtr(handle, WindowStyleIndex, new IntPtr(style));
        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, FrameChanged | NoMove | NoSize | NoZOrder);
        PositionBottomRight();
    }

    private void PositionBottomRight()
    {
        var screen = Screens.Primary;
        if (screen is null)
            return;

        var scaling = screen.Scaling;
        var windowWidth = (int)Math.Ceiling(ClientSize.Width * scaling);
        var windowHeight = (int)Math.Ceiling(ClientSize.Height * scaling);
        var screenMargin = (int)Math.Ceiling(8 * scaling);

        Position = new PixelPoint(
            screen.WorkingArea.Right - windowWidth - screenMargin,
            screen.WorkingArea.Bottom - windowHeight - screenMargin);
    }

    private static readonly IntPtr WindowStyleIndex = new(-16);
    private const long WindowCaption = 0x00C00000;
    private const long WindowThickFrame = 0x00040000;
    private const long WindowMinimizeBox = 0x00020000;
    private const long WindowMaximizeBox = 0x00010000;
    private const long WindowSystemMenu = 0x00080000;
    private const uint FrameChanged = 0x0020;
    private const uint NoMove = 0x0002;
    private const uint NoSize = 0x0001;
    private const uint NoZOrder = 0x0004;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr windowHandle, IntPtr index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr windowHandle, IntPtr index, IntPtr newStyle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    public FileSuggestionWindow(FileSuggestionViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void FileNameTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        if (DataContext is FileSuggestionViewModel current && current.IsFileNameValid)
        {
            if (DataContext is FileSuggestionViewModel viewModel)
                viewModel.HasValidationError = false;
            return;
        }

        ShowValidationError();
    }

    private void ShowValidationError()
    {
        if (DataContext is FileSuggestionViewModel viewModel)
            viewModel.HasValidationError = true;
        ShakeFileNameTextBox();
    }

    private async void ShakeFileNameTextBox()
    {
        _shakeCancellation?.Cancel();
        _shakeCancellation?.Dispose();
        _shakeCancellation = new CancellationTokenSource();
        var cancellationToken = _shakeCancellation.Token;

        try
        {
            const int frameCount = 18;
            const double amplitude = 6;
            const int frameDurationMilliseconds = 14;

            for (var frame = 0; frame < frameCount; frame++)
            {
                var progress = (double)frame / (frameCount - 1);
                var offset = Math.Sin(progress * Math.PI * 4) * amplitude * (1 - progress);
                FileNameTextBox.RenderTransform = new TranslateTransform(offset, 0);
                await Task.Delay(frameDurationMilliseconds, cancellationToken);
            }

            FileNameTextBox.RenderTransform = new TranslateTransform(0, 0);
        }
        catch (OperationCanceledException)
        {
            // A new invalid input starts a fresh shake.
        }
    }
}
