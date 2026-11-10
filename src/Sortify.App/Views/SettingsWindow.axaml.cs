using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Sortify.App.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using System.Runtime.InteropServices;
using Sortify.Core.Services;
using Sortify.App.Services;

namespace Sortify.App.Views;

public partial class SettingsWindow : Window
{
    private readonly ActivityLogViewModel activityLog;
    private bool updatingAutoSortToggle;
    private RuleEditorViewModel? observedRuleEditor;

    public SettingsWindow() : this(null)
    {
    }

    public SettingsWindow(IActivityLog? log = null)
    {
        activityLog = new ActivityLogViewModel(log ?? new NullActivityLog());
        InitializeComponent();
        ActivityLogPanel.DataContext = activityLog;
        Opened += ApplyNativeTitleBarColor;
        Opened += (_, _) => Dispatcher.UIThread.Post(() => Opacity = 1, DispatcherPriority.Render);
        Opened += (_, _) =>
        {
            EnsureRuleEditor();
            ObserveRuleEditor();
            if (DataContext is MainViewModel main)
            {
                UpdateAutoSortToggle(main.CurrentSettings?.AutoSort == true);
                DarkModeToggle.IsChecked = main.CurrentSettings?.DarkMode == true;
                AutoStartToggle.IsChecked = main.CurrentSettings?.AutoStart == true;
                StartMinimizedToggle.IsChecked = main.CurrentSettings?.StartMinimized == true;
                StartMinimizedToggle.IsEnabled = AutoStartToggle.IsChecked == true;
                StartMinimizedCard.Opacity = AutoStartToggle.IsChecked == true ? 1d : 0.45d;
                CloseToTrayToggle.IsChecked = main.CurrentSettings?.CloseToTray != false;
            }
            _ = activityLog.LoadAsync();
        };
    }

    private void ObserveRuleEditor()
    {
        if (RulesEditor.DataContext is not RuleEditorViewModel editor) return;
        if (ReferenceEquals(observedRuleEditor, editor)) return;
        if (observedRuleEditor is not null) observedRuleEditor.PropertyChanged -= RuleEditorPropertyChanged;
        observedRuleEditor = editor;
        editor.PropertyChanged += RuleEditorPropertyChanged;
    }

    private void RuleEditorPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not RuleEditorViewModel editor) return;
        if (e.PropertyName == nameof(RuleEditorViewModel.IsDeletePending))
        {
            if (editor.IsDeletePending)
            {
                DeleteConfirmationOverlay.IsVisible = true;
                DeleteConfirmationOverlay.Opacity = 0;
                Dispatcher.UIThread.Post(() => DeleteConfirmationOverlay.Opacity = 1, DispatcherPriority.Render);
            }
            else
            {
                DeleteConfirmationOverlay.Opacity = 0;
                _ = HideDeleteOverlayAfterFadeAsync(editor);
            }
            return;
        }
        if (e.PropertyName != nameof(RuleEditorViewModel.IsMessageVisible)) return;
        if (editor.IsMessageVisible)
        {
            RuleMessageBanner.IsVisible = true;
            ((TranslateTransform)RuleMessageBanner.RenderTransform!).Y = -28;
            Dispatcher.UIThread.Post(() => ((TranslateTransform)RuleMessageBanner.RenderTransform!).Y = 0, DispatcherPriority.Render);
        }
        else
        {
            ((TranslateTransform)RuleMessageBanner.RenderTransform!).Y = -28;
            _ = HideRuleMessageBannerAsync(editor);
        }
    }

    private async Task HideDeleteOverlayAfterFadeAsync(RuleEditorViewModel editor)
    {
        await Task.Delay(200);
        if (!editor.IsDeletePending) DeleteConfirmationOverlay.IsVisible = false;
    }

    private async Task HideRuleMessageBannerAsync(RuleEditorViewModel editor)
    {
        await Task.Delay(200);
        if (!editor.IsMessageVisible) RuleMessageBanner.IsVisible = false;
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

        foreach (var button in new[] { GeneralButton, WatchedButton, RulesButton, AppearanceButton, ActivityLogButton, StatisticsButton })
            button.Classes.Set("selected", ReferenceEquals(button, sender));

        var rulesSelected = section == "Sortierregeln";
        RulesEditor.IsVisible = rulesSelected;
        RuleActions.IsVisible = rulesSelected;
        var generalSelected = section == "Allgemein";
        GeneralSettingsPanel.IsVisible = generalSelected;
        if (rulesSelected)
        {
            EnsureRuleEditor();
            if (RulesEditor.DataContext is RuleEditorViewModel editor) editor.ShowOverviewCommand.Execute(null);
        }
        SectionTitle.Text = section;
        var appearanceSelected = section == "Darstellung";
        AppearanceCard.IsVisible = appearanceSelected;
        ActivityLogPanel.IsVisible = section == "Aktivitätsprotokoll";
        StatisticsPanel.IsVisible = section == "Download-Statistik";
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

    private void UpdateAutoSortToggle(bool enabled)
    {
        updatingAutoSortToggle = true;
        AutoSortToggle.IsChecked = enabled;
        updatingAutoSortToggle = false;
    }

    private async void AutoSortChanged(object? sender, RoutedEventArgs e)
    {
        if (updatingAutoSortToggle || !IsInitialized || DataContext is not MainViewModel main)
            return;

        AutoSortToggle.IsEnabled = false;
        AutoSortError.IsVisible = false;
        try
        {
            var result = await main.SaveAutoSortAsync(AutoSortToggle.IsChecked == true);
            if (!result.IsSuccess)
            {
                UpdateAutoSortToggle(main.CurrentSettings?.AutoSort == true);
                AutoSortError.Text = result.ErrorMessage ?? "Automatisches Sortieren konnte nicht gespeichert werden.";
                AutoSortError.IsVisible = true;
            }
        }
        finally
        {
            AutoSortToggle.IsEnabled = true;
        }
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

}
