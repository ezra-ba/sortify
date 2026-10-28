using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

[Collection("Avalonia")]
public sealed class RuleActivationWindowTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CheckboxLocksFieldsAndSavesOnlyStatusWithInactiveListAndTargetIndicators(bool dark)
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var root = Path.Combine(Path.GetTempPath(), "SortifyActive-" + Guid.NewGuid());
            Directory.CreateDirectory(root);
            JsonSettingsStore Store() => new(Path.Combine(root, "settings.json"), new SettingsValidator(new RuleValidator()));
            var target = new TargetFolder(Guid.NewGuid(), root, "Invoices");
            var otherTarget = new TargetFolder(Guid.NewGuid(), Path.Combine(root, "other"), "Other");
            var original = new SortRule(Guid.NewGuid(), "Rechnungen", "^rechnung", [".pdf"], null, "", "{name}{ext}", 5, true)
                { TargetFolderId = target.Id };
            var other = original with { Id = Guid.NewGuid(), Name = "Other rule", TargetFolderId = otherTarget.Id };
            var settings = new AppSettings(root, null, false, [original, other])
                { TargetFolders = [target, otherTarget], DarkMode = dark };
            Assert.True((await Store().SaveAsync(settings)).IsSuccess);
            var main = new MainViewModel(new SettingsService(Store()));
            await main.InitializeSettingsAsync();
            var window = new SettingsWindow { DataContext = main, Height = 1150, Width = 1000 };
            try
            {
                window.Show();
                Assert.True(window.OpenRuleForEditing(original.Id));
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                var view = window.FindControl<RuleEditorView>("RulesEditor")!;
                var editor = Assert.IsType<RuleEditorViewModel>(view.DataContext);
                var checkbox = view.FindControl<CheckBox>("RuleActiveCheckBox")!;
                var fields = view.FindControl<StackPanel>("RuleFieldsPanel")!;
                var name = view.FindControl<TextBox>("RuleNameInput")!;
                var pattern = view.FindControl<TextBox>("RulePatternInput")!;
                var save = view.FindControl<Button>("CreateRuleButton")!;
                Assert.Same(checkbox, Assert.IsType<StackPanel>(checkbox.Parent).Children[0]);
                Assert.True(checkbox.IsVisible);
                Assert.True(checkbox.IsChecked);
                Assert.True(fields.IsEffectivelyEnabled);
                Assert.All(main.TargetFolders, folder => Assert.True(folder.HasDuplicateRules));

                pattern.Text = "["; // An unsaved invalid draft must not prevent turning a rule off.
                checkbox.IsChecked = false;
                Dispatcher.UIThread.RunJobs();
                Assert.False(editor.IsRuleActive);
                Assert.False(fields.IsEffectivelyEnabled);
                Assert.False(name.IsEffectivelyEnabled);
                Assert.False(pattern.IsEffectivelyEnabled);
                Assert.True(checkbox.IsEffectivelyEnabled);
                Assert.True(save.IsEffectivelyEnabled);
                Assert.True(main.CurrentSettings!.Rules[0].IsEnabled); // No auto-save on checkbox change.
                await Assert.IsAssignableFrom<IAsyncRelayCommand>(save.Command).ExecuteAsync(null);
                Assert.False(main.CurrentSettings.Rules[0].IsEnabled);
                Assert.Equal(original.FileNamePattern, main.CurrentSettings.Rules[0].FileNamePattern);
                Assert.Equal(original.Name, main.CurrentSettings.Rules[0].Name);
                Assert.Equal(original.RenamePattern, main.CurrentSettings.Rules[0].RenamePattern);
                Assert.True(editor.ExistingRules.Single(item => item.Id == original.Id).IsInactive);
                Assert.True(Assert.Single(main.TargetFolders[0].RuleTags).IsDisabled);
                Assert.All(main.TargetFolders, folder => Assert.False(folder.HasDuplicateRules));
                var reloaded = new SettingsService(Store());
                await reloaded.InitializeAsync();
                Assert.False(reloaded.Current!.Rules[0].IsEnabled);

                Assert.True(window.OpenRuleForEditing(original.Id));
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Assert.False(checkbox.IsChecked);
                Assert.True(checkbox.IsFocused);
                Assert.False(fields.IsEffectivelyEnabled);
                var inactiveLabel = view.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "Inaktiv" && text.DataContext is RuleListItem item && item.Id == original.Id);
                Assert.True(Assert.IsType<Border>(inactiveLabel.Parent).IsVisible);
                using (var frame = window.CaptureRenderedFrame())
                {
                    Assert.NotNull(frame);
                    frame.Save(dark ? "/tmp/sortify-inactive-rule-dark.png" : "/tmp/sortify-inactive-rule-light.png",
                        Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                }

                checkbox.IsChecked = true;
                Dispatcher.UIThread.RunJobs();
                Assert.True(fields.IsEffectivelyEnabled);
                Assert.True(name.IsEffectivelyEnabled);
                await Assert.IsAssignableFrom<IAsyncRelayCommand>(save.Command).ExecuteAsync(null);
                Assert.True(main.CurrentSettings.Rules[0].IsEnabled);
                Assert.False(editor.ExistingRules.Single(item => item.Id == original.Id).IsInactive);
                Assert.All(main.TargetFolders, folder => Assert.True(folder.HasDuplicateRules));
                Assert.False(checkbox.IsVisible);
                Assert.True(editor.IsRuleActive); // New-rule defaults restored after saving.
            }
            finally
            {
                window.Close();
                Directory.Delete(root, true);
            }
            return true;
        }, CancellationToken.None);
    }
}
