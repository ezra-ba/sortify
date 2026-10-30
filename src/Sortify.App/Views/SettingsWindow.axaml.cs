using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Sortify.App.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Media;
using System.Runtime.InteropServices;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.App.Services;

namespace Sortify.App.Views;

public partial class SettingsWindow : Window
{
    private bool loadingAiSettings;
    private readonly ActivityLogViewModel activityLog;

    public SettingsWindow() : this(null)
    {
    }

    public SettingsWindow(IActivityLog? log = null)
    {
        activityLog = new ActivityLogViewModel(log ?? new NullActivityLog());
        InitializeComponent();
        ActivityLogPanel.DataContext = activityLog;
        Opened += ApplyNativeTitleBarColor;
        Opened += (_, _) =>
        {
            EnsureRuleEditor();
            if (DataContext is MainViewModel main)
            {
                DarkModeToggle.IsChecked = main.CurrentSettings?.DarkMode == true;
                AutoStartToggle.IsChecked = main.CurrentSettings?.AutoStart == true;
                StartMinimizedToggle.IsChecked = main.CurrentSettings?.StartMinimized == true;
                StartMinimizedToggle.IsEnabled = AutoStartToggle.IsChecked == true;
                StartMinimizedCard.Opacity = AutoStartToggle.IsChecked == true ? 1d : 0.45d;
                CloseToTrayToggle.IsChecked = main.CurrentSettings?.CloseToTray != false;
                LoadAiSettings(main);
            }
            _ = activityLog.LoadAsync();
        };
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
        SelectSection(RulesButton, new RoutedEventArgs());
        if (RulesEditor.DataContext is RuleEditorViewModel editor)
            editor.BeginCreateForTarget(target);
    }

    public bool OpenRuleForEditing(Guid ruleId)
    {
        SelectSection(RulesButton, new RoutedEventArgs());
        return RulesEditor.DataContext is RuleEditorViewModel editor && editor.OpenRuleForEditing(ruleId);
    }

    private void SelectSection(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string section }) return;

        foreach (var button in new[] { GeneralButton, WatchedButton, RulesButton, AppearanceButton, AiButton, ActivityLogButton })
            button.Classes.Set("selected", ReferenceEquals(button, sender));

        var rulesSelected = section == "Sortierregeln";
        RulesEditor.IsVisible = rulesSelected;
        RuleActions.IsVisible = rulesSelected;
        var generalSelected = section == "Allgemein";
        GeneralSettingsPanel.IsVisible = generalSelected;
        AiSettingsPanel.IsVisible = section == "KI-Unterstützung";
        if (AiSettingsPanel.IsVisible && DataContext is MainViewModel main)
            LoadAiSettings(main);
        if (rulesSelected)
        {
            EnsureRuleEditor();
            if (RulesEditor.DataContext is RuleEditorViewModel editor) editor.ShowOverviewCommand.Execute(null);
        }
        SectionTitle.Text = section;
        var appearanceSelected = section == "Darstellung";
        AppearanceCard.IsVisible = appearanceSelected;
        ActivityLogPanel.IsVisible = section == "Aktivitätsprotokoll";
    }

    private async void DarkModeChanged(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not { } application)
        {
            return;
        }

        var isDark = DarkModeToggle.IsChecked == true;
        application.RequestedThemeVariant = isDark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;

        if (DataContext is MainViewModel main)
            await main.SaveDarkModeAsync(isDark);

    }

    private async void GeneralSettingChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel main || !IsInitialized)
            return;

        StartMinimizedToggle.IsEnabled = AutoStartToggle.IsChecked == true;
        StartMinimizedCard.Opacity = AutoStartToggle.IsChecked == true ? 1d : 0.45d;
        if (StartMinimizedToggle.IsChecked != true && AutoStartToggle.IsChecked != true)
            StartMinimizedToggle.IsChecked = false;

        await main.SaveGeneralSettingsAsync(
            AutoStartToggle.IsChecked == true,
            StartMinimizedToggle.IsChecked == true,
            CloseToTrayToggle.IsChecked == true);
    }

    private void LoadAiSettings(MainViewModel main)
    {
        loadingAiSettings = true;
        try
        {
            AiModeSelector.SelectedIndex = (int)(main.CurrentSettings?.AiMode ?? AiSuggestionMode.Disabled);
            AiConsentCheck.IsChecked = main.CurrentSettings?.AiMetadataConsent == true;
            AiKeyStatus.Text = string.IsNullOrWhiteSpace(Sortify.App.Services.NvidiaApiKey.Read())
                ? "API-Schlüssel fehlt: NVIDIA_API_KEY als Benutzervariable setzen."
                : "API-Schlüssel über NVIDIA_API_KEY verfügbar.";
        }
        finally { loadingAiSettings = false; }
    }

    private async void AiModeChanged(object? sender, SelectionChangedEventArgs e) => await SaveAiSettingsAsync();
    private async void AiConsentChanged(object? sender, RoutedEventArgs e) => await SaveAiSettingsAsync();

    private async Task SaveAiSettingsAsync()
    {
        if (loadingAiSettings || !IsInitialized || !IsVisible || DataContext is not MainViewModel main
            || AiModeSelector.SelectedIndex < 0)
            return;
        AiSettingsPanel.IsEnabled = false;
        try
        {
            var result = await main.SaveAiSettingsAsync((AiSuggestionMode)AiModeSelector.SelectedIndex,
                AiConsentCheck.IsChecked == true);
            AiSaveStatus.Text = result.IsSuccess ? "Gespeichert." : result.ErrorMessage;
            if (!result.IsSuccess)
                LoadAiSettings(main);
        }
        finally { AiSettingsPanel.IsEnabled = true; }
    }

}
