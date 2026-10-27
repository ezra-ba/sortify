using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls.Shapes;

namespace Sortify.App.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Opened += ApplyNativeTitleBarColor;
    }

    private void ApplyNativeTitleBarColor(object? sender, EventArgs e)
    {
        if (!OperatingSystem.IsWindows()) return;

        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero) return;

        // DWM expects COLORREF values in 0x00BBGGRR format.
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

    private void SelectSection(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string section }) return;

        foreach (var button in new[] { GeneralButton, WatchedButton, RulesButton, AppearanceButton })
            button.Classes.Set("selected", ReferenceEquals(button, sender));

        SectionTitle.Text = section;
        var appearanceSelected = section == "Darstellung";
        AppearanceCard.IsVisible = appearanceSelected;
        PlaceholderCard.IsVisible = !appearanceSelected;
    }

    private void DarkModeChanged(object? sender, RoutedEventArgs e)
    {
        AnimateDarkModeKnob();
        if (Application.Current is not { } application)
        {
            return;
        }

        var isDark = DarkModeToggle.IsChecked == true;
        application.RequestedThemeVariant = isDark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;

    }

    private void AnimateDarkModeKnob()
    {
        var knob = this.GetVisualDescendants().OfType<Ellipse>().FirstOrDefault(x => x.Name == "ToggleKnob");
        if (knob is null) return;

        var start = DarkModeToggle.IsChecked == true ? 2d : 26d;
        var end = DarkModeToggle.IsChecked == true ? 26d : 2d;
        var stopwatch = Stopwatch.StartNew();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += (_, _) =>
        {
            var t = Math.Clamp(stopwatch.Elapsed.TotalMilliseconds / 260d, 0d, 1d);
            var eased = t < 0.72d
                ? t / 0.72d
                : 1d + (t - 0.72d) / 0.28d * 0.08d;
            var progress = 1d - Math.Pow(1d - Math.Min(eased, 1.08d), 3d);
            Canvas.SetLeft(knob, start + (end - start) * progress);
            var stretch = t < 0.5d ? 1d + t * 0.44d : 1.22d - (t - 0.5d) * 0.44d;
            knob.RenderTransform = new ScaleTransform(stretch, 1d / Math.Sqrt(stretch));
            if (t >= 1d)
            {
                Canvas.SetLeft(knob, end);
                knob.RenderTransform = new ScaleTransform(1d, 1d);
                timer.Stop();
            }
        };
        timer.Start();
    }


}
