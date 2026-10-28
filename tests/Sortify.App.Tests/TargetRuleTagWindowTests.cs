using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

[Collection("Avalonia")]
public sealed class TargetRuleTagWindowTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ThemedTagsNavigateToExactRuleWithoutDraggingAndMissingTagCreatesRule(bool dark)
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            var theme = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            Application.Current!.RequestedThemeVariant = theme;
            var targets = new[] { "INF", "Lieder (Testordner)", "FH-Hagenberg", "Videos", "Bilder" }
                .Select(name => new TargetFolder(Guid.NewGuid(), Path.Combine(Path.GetTempPath(), name), name)).ToArray();
            SortRule Rule(int target, string name, string pattern, string extension) => new(
                Guid.NewGuid(), name, pattern, [extension], null, "", null, 0, true) { TargetFolderId = targets[target].Id };
            var rules = new[] { Rule(0, "Rechnungen", "^rechnung", ".pdf"), Rule(0, "INF", "^inf", ".docx"),
                Rule(2, "Lieder", ".*", ".mp3"), Rule(3, "Musik", ".*", ".mp3") };
            var settings = new AppSettings(Path.GetTempPath(), null, false, rules)
                { TargetFolders = targets, DefaultTargetFolderId = targets[4].Id, DarkMode = dark };
            var main = new MainViewModel(new SettingsService(new Store(settings)));
            await main.InitializeSettingsAsync();
            main.SelectTargetFolder(main.TargetFolders[0]);
            var window = new MainWindow { DataContext = main, Width = 1500, Height = 950 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                var buttons = window.GetVisualDescendants().OfType<Button>().Where(button => button.Classes.Contains("rule-tag")).ToArray();
                Assert.Equal(6, buttons.Length);
                var normal = buttons.Single(button => button.DataContext is TargetRuleTagViewModel tag && tag.RuleId == rules[0].Id);
                var duplicate = buttons.Single(button => button.DataContext is TargetRuleTagViewModel tag && tag.RuleId == rules[2].Id);
                var missing = buttons.Single(button => button.DataContext is TargetRuleTagViewModel tag && tag.IsMissing && tag.TargetId == targets[1].Id);
                Assert.Empty(normal.GetVisualAncestors().OfType<Button>());
                var tagName = normal.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == rules[0].Name);
                var nameCenter = tagName.TranslatePoint(new Point(tagName.Bounds.Width / 2, tagName.Bounds.Height / 2), normal)!.Value;
                Assert.InRange(nameCenter.X - normal.Bounds.Width / 2, -0.5, 0.5);
                Assert.NotEqual(((ISolidColorBrush)normal.Background!).Color, ((ISolidColorBrush)duplicate.Background!).Color);
                Assert.NotEqual(((ISolidColorBrush)missing.Background!).Color, ((ISolidColorBrush)duplicate.Background!).Color);
                Assert.Equal(((ISolidColorBrush)normal.Background!).Color, ((ISolidColorBrush)missing.Background!).Color);
                Assert.Equal(36d, missing.Bounds.Width);
                Assert.Equal(missing.Bounds.Width, missing.Bounds.Height);
                Assert.Equal(new CornerRadius(12), missing.CornerRadius);
                Assert.Single(missing.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>(), path => path.IsVisible);
                Assert.Equal(theme, window.ActualThemeVariant);
                Assert.Equal(Color.Parse(dark ? "#382C54" : "#F0ECFF"), ((ISolidColorBrush)normal.Background!).Color);
                using (var frame = window.CaptureRenderedFrame())
                {
                    Assert.NotNull(frame);
                    frame.Save(dark ? "/tmp/sortify-target-tags-dark.png" : "/tmp/sortify-target-tags-light.png",
                        Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                }

                // Real pointer events exercise the card drag handlers as well as tag navigation.
                var position = normal.TranslatePoint(new Point(normal.Bounds.Width / 2, normal.Bounds.Height / 2), window)!.Value;
                window.MouseDown(position, MouseButton.Left);
                window.MouseUp(position, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                var opened = Assert.Single(window.OwnedWindows.OfType<SettingsWindow>());
                var view = opened.FindControl<RuleEditorView>("RulesEditor")!;
                var editor = Assert.IsType<RuleEditorViewModel>(view.DataContext);
                Assert.True(editor.IsEditing);
                Assert.Equal(rules[0].Name, editor.RuleName);
                Assert.Equal(rules[0].FileNamePattern, editor.FileNamePattern);
                Assert.Equal(targets[0].Id, editor.SelectedTarget?.Id);
                Assert.True(view.FindControl<TextBox>("RuleNameInput")!.IsFocused);
                Assert.All(main.TargetFolders, folder => Assert.False(folder.IsDragging));
                Assert.Equal(targets.Select(target => target.Id), main.TargetFolders.Select(folder => folder.Id));
                Assert.False(opened.FindControl<Border>("PlaceholderPanel")!.IsVisible);
                opened.Close();

                position = missing.TranslatePoint(new Point(missing.Bounds.Width / 2, missing.Bounds.Height / 2), window)!.Value;
                window.MouseDown(position, MouseButton.Left);
                window.MouseUp(position, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                opened = Assert.Single(window.OwnedWindows.OfType<SettingsWindow>());
                editor = Assert.IsType<RuleEditorViewModel>(opened.FindControl<RuleEditorView>("RulesEditor")!.DataContext);
                Assert.False(editor.IsEditing);
                Assert.True(editor.IsFormVisible);
                Assert.Equal(targets[1].Id, editor.SelectedTarget?.Id);
                opened.Close();

                // The card body still supports selection and drag ordering beside its tags.
                var sourceCard = window.GetVisualDescendants().OfType<Button>().Single(button =>
                    button.Classes.Contains("target-folder") && button.DataContext is TargetFolderViewModel folder && folder.Id == targets[0].Id);
                var targetCard = window.GetVisualDescendants().OfType<Button>().Single(button =>
                    button.Classes.Contains("target-folder") && button.DataContext is TargetFolderViewModel folder && folder.Id == targets[1].Id);
                var sourcePoint = sourceCard.TranslatePoint(new Point(60, sourceCard.Bounds.Height / 2), window)!.Value;
                var targetPoint = targetCard.TranslatePoint(new Point(60, targetCard.Bounds.Height / 2 + 12), window)!.Value;
                window.MouseDown(sourcePoint, MouseButton.Left);
                window.MouseMove(targetPoint, RawInputModifiers.LeftMouseButton);
                window.MouseUp(targetPoint, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(new[] { targets[1].Id, targets[0].Id, targets[2].Id, targets[3].Id, targets[4].Id },
                    main.TargetFolders.Select(folder => folder.Id));
                Assert.All(main.TargetFolders, folder => Assert.False(folder.IsDragging));
            }
            finally
            {
                foreach (var owned in window.OwnedWindows.ToArray()) owned.Close();
                window.Close();
            }
            return true;
        }, CancellationToken.None);
    }

    private sealed class Store(AppSettings settings) : ISettingsStore
    {
        public Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsLoadResult(SettingsLoadStatus.Loaded, settings, null));
        public Task<SettingsSaveResult> SaveAsync(AppSettings updated, CancellationToken cancellationToken = default)
            => Task.FromResult(new SettingsSaveResult(true, null));
    }
}
