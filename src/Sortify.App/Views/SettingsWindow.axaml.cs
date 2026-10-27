using Avalonia.Controls;
using Sortify.App.ViewModels;
using Avalonia.Interactivity;
using System.Runtime.InteropServices;

namespace Sortify.App.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Opened += ApplyNativeTitleBarColor;
        Opened += (_, _) => EnsureRuleEditor();
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

    private void EnsureRuleEditor()
    {
        if (RulesEditor.DataContext is not RuleEditorViewModel && DataContext is MainViewModel main)
            RulesEditor.DataContext = new RuleEditorViewModel(main);
    }

    public void OpenRulesForTarget(TargetFolderViewModel? target)
    {
        EnsureRuleEditor();
        if (RulesEditor.DataContext is RuleEditorViewModel editor)
            editor.SelectedTarget = target;
        SelectSection(RulesButton, new RoutedEventArgs());
    }

    private void SelectSection(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string section }) return;

        foreach (var button in new[] { GeneralButton, WatchedButton, RulesButton, AppearanceButton })
            button.Classes.Set("selected", ReferenceEquals(button, sender));

        var rulesSelected = section == "Sortierregeln";
        RulesEditor.IsVisible = rulesSelected;
        PlaceholderPanel.IsVisible = !rulesSelected;
        if (rulesSelected)
        {
            EnsureRuleEditor();
            if (RulesEditor.DataContext is RuleEditorViewModel editor) editor.RefreshRules();
        }
        SectionTitle.Text = section;
        SectionDescription.Text = section switch
        {
            "Überwachungsordner" => "Lege fest, welcher Ordner auf neue Dateien überwacht wird.",
            "Sortierregeln" => "Verwalte die Regeln, nach denen Dateien vorgeschlagen werden.",
            "Darstellung" => "Passe die Darstellung der Anwendung an.",
            _ => "Grundlegende Einstellungen für Sortify."
        };
    }

}
