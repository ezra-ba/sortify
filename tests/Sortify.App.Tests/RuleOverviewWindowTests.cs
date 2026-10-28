using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

[Collection("Avalonia")]
public sealed class RuleOverviewWindowTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    public async Task OverviewEmptyStatePlusAndEditAreSeparateAndThemed(bool dark, int ruleCount)
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var target = new TargetFolder(Guid.NewGuid(), Path.GetTempPath(), "Dokumente");
            var rules = Enumerable.Range(1, ruleCount).Select(index => new SortRule(
                Guid.NewGuid(), "Regel " + index, "^rechnung", [".pdf"], null, "", null, 0, index != 3)
                { TargetFolderId = target.Id }).ToArray();
            var main = new MainViewModel(new SettingsService(new Store(new AppSettings(null, null, false, rules)
                { TargetFolders = [target], DarkMode = dark })));
            await main.InitializeSettingsAsync();
            var window = new SettingsWindow { DataContext = main, Width = 1100, Height = 900 };
            try
            {
                window.Show();
                Layout();
                var actions = window.FindControl<Grid>("RuleActions")!;
                Assert.False(actions.IsVisible);
                Click(window, window.FindControl<Button>("RulesButton")!);
                var view = window.FindControl<RuleEditorView>("RulesEditor")!;
                var editor = Assert.IsType<RuleEditorViewModel>(view.DataContext);
                var form = view.FindControl<Border>("RuleEditorForm")!;
                var overview = view.FindControl<StackPanel>("RuleOverviewPanel")!;
                var empty = view.FindControl<Border>("EmptyRulesPanel")!;
                var plus = window.FindControl<Button>("NewRuleButton")!;
                Assert.True(actions.IsVisible);
                Assert.True(plus.IsVisible);
                Assert.NotNull(plus.Command);
                Assert.False(form.IsVisible);
                Assert.Equal(ruleCount == 0, empty.IsVisible);
                Assert.Equal(ruleCount > 0, overview.IsVisible);
                var title = window.FindControl<TextBlock>("SectionTitle")!;
                Assert.True(plus.TranslatePoint(default, window)!.Value.X > title.TranslatePoint(default, window)!.Value.X);
                Capture(window, $"/tmp/sortify-rules-{(ruleCount == 0 ? "empty" : "list")}-{(dark ? "dark" : "light")}.png");

                if (ruleCount == 0)
                {
                    Click(window, view.FindControl<Button>("EmptyCreateRuleButton")!);
                    Assert.True(form.IsVisible);
                    Assert.False(plus.IsVisible);
                    Click(window, view.FindControl<Button>("BackToRulesButton")!);
                    Assert.True(empty.IsVisible);
                }
                Click(window, plus);
                Assert.True(form.IsVisible);
                Assert.False(overview.IsVisible);
                Assert.False(empty.IsVisible);
                Assert.False(plus.IsVisible);
                Assert.False(view.FindControl<CheckBox>("RuleActiveCheckBox")!.IsVisible);
                Assert.True(view.FindControl<TextBox>("RuleNameInput")!.IsFocused);
                view.FindControl<TextBox>("RuleNameInput")!.Text = "Neue Testregel";
                view.FindControl<ComboBox>("RuleTargetInput")!.SelectedItem = main.TargetFolders[0];
                editor.NameConditions[0].Text = "test";
                Layout();
                await Assert.IsAssignableFrom<IAsyncRelayCommand>(view.FindControl<Button>("CreateRuleButton")!.Command).ExecuteAsync(null);
                Layout();
                Assert.False(form.IsVisible);
                Assert.True(overview.IsVisible);
                Assert.True(plus.IsVisible);
                var saved = main.CurrentSettings!.Rules.Single(rule => rule.Name == "Neue Testregel");
                var edit = view.GetVisualDescendants().OfType<Button>().Single(button =>
                    Equals(button.Content, "Bearbeiten") && button.CommandParameter is RuleListItem item && item.Id == saved.Id);
                edit.BringIntoView();
                Layout();
                Click(window, edit);
                Assert.True(form.IsVisible);
                Assert.False(overview.IsVisible);
                Assert.True(editor.IsEditing);
                Assert.True(view.FindControl<CheckBox>("RuleActiveCheckBox")!.IsVisible);
                Assert.Equal(saved.Name, view.FindControl<TextBox>("RuleNameInput")!.Text);
                if (ruleCount > 0) Capture(window, $"/tmp/sortify-rules-edit-{(dark ? "dark" : "light")}.png");
                Click(window, view.FindControl<Button>("BackToRulesButton")!);
                Assert.True(overview.IsVisible);
                Assert.False(form.IsVisible);
                Assert.True(plus.IsVisible);
                Click(window, window.FindControl<Button>("AppearanceButton")!);
                Assert.False(actions.IsVisible);
                Click(window, window.FindControl<Button>("RulesButton")!);
                Assert.True(overview.IsVisible);
                Assert.False(form.IsVisible);
            }
            finally { window.Close(); }
            return true;
        }, CancellationToken.None);
    }

    private static void Layout()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
    }
    private static void Click(Window window, Button button)
    {
        var position = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
        window.MouseDown(position, MouseButton.Left);
        window.MouseUp(position, MouseButton.Left);
        Layout();
    }
    private static void Capture(Window window, string path)
    {
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        frame.Save(path, Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
    }
    private sealed class Store(AppSettings settings) : ISettingsStore
    {
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsLoadResult(SettingsLoadStatus.Loaded, settings, null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings updated, CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsSaveResult(true, null));
    }
}
