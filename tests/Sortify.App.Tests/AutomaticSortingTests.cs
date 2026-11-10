using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Sortify.App.Services;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Infrastructure.FileSystem;
using Sortify.Infrastructure.Logging;
using Sortify.Infrastructure.Monitoring;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

[Collection("Avalonia")]
public sealed class AutomaticSortingTests
{
    [Theory]
    [InlineData("rule")]
    [InlineData("default")]
    [InlineData("no-default")]
    [InlineData("disabled")]
    [InlineData("collision")]
    [InlineData("ambiguous")]
    [InlineData("unsupported")]
    [InlineData("watched-target")]
    public async Task NewFileIsMovedAutomaticallyOnlyWithAnEligibleTarget(string scenario)
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "sortify-auto-" + Guid.NewGuid());
            var watched = Directory.CreateDirectory(Path.Combine(root, "watched")).FullName;
            var fallback = new TargetFolder(Guid.NewGuid(), Directory.CreateDirectory(Path.Combine(root, "default")).FullName, "Standard");
            var ruleTarget = new TargetFolder(Guid.NewGuid(), Directory.CreateDirectory(Path.Combine(root, "rule")).FullName, "Regelziel");
            if (scenario == "watched-target") ruleTarget = ruleTarget with { Path = watched };
            SortRule Rule(TargetFolder target) => new(Guid.NewGuid(), target.Nickname, "^invoice", [".pdf"], null,
                "", "sorted_{name}{ext}", 1, true) { TargetFolderId = target.Id };
            SortRule[] rules = scenario switch
            {
                "rule" or "collision" or "watched-target" => [Rule(ruleTarget)],
                "ambiguous" => [Rule(ruleTarget), Rule(fallback)],
                _ => []
            };
            var log = new JsonActivityLog(Path.Combine(root, "logs"));
            var store = new JsonSettingsStore(Path.Combine(root, "settings.json"), new SettingsValidator(new RuleValidator()));
            var settings = new AppSettings(watched, null, false, rules)
            {
                TargetFolders = [fallback, ruleTarget],
                DefaultTargetFolderId = scenario == "no-default" ? null : fallback.Id,
                AutoSort = scenario != "disabled"
            };
            FileSuggestionWindow? popup = null;
            using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
            {
                if (window is FileSuggestionWindow suggestion) popup = suggestion;
            });
            try
            {
                Assert.True((await store.SaveAsync(settings)).IsSuccess);
                var main = new MainViewModel(new SettingsService(store));
                await main.InitializeSettingsAsync();
                using var controller = new DownloadMonitorController(main,
                    new FileMoveService(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5)),
                    new RuleEngine(new RuleValidator(), new RuleTargetValidator()), new FolderMonitor(), log);
                var fileName = scenario == "unsupported" ? "invoice.zip" : "invoice.pdf";
                var source = Path.Combine(watched, fileName);
                var target = scenario is "rule" or "collision" or "watched-target" ? Path.Combine(ruleTarget.Path, "sorted_invoice.pdf") : Path.Combine(fallback.Path, fileName);
                if (scenario == "collision") await File.WriteAllTextAsync(target, "existing");
                controller.Restart(watched);
                await File.WriteAllTextAsync(source, "source");
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                if (scenario == "rule")
                {
                    while (log.ReadRecent().Entries.Count == 0 && popup is null)
                        await Task.Delay(20, timeout.Token);
                    Assert.Null(popup);
                    Assert.Equal("FileMove.Succeeded", Assert.Single(log.ReadRecent().Entries).EventName);
                    Assert.False(File.Exists(source));
                    Assert.Equal("source", await File.ReadAllTextAsync(target));
                }
                else
                {
                    while (popup is null) await Task.Delay(20, timeout.Token);
                    Assert.Equal("source", await File.ReadAllTextAsync(source));
                    var suggestion = Assert.IsType<FileSuggestionViewModel>(popup.DataContext);
                    if (scenario == "no-default")
                        Assert.Contains("Kein Standardordner", suggestion.SuggestionExplanation);
                    if (scenario == "watched-target")
                        Assert.Contains("Zielordner ist der Überwachungsordner", suggestion.SuggestionExplanation);
                    if (scenario == "collision")
                    {
                        Assert.Equal("existing", await File.ReadAllTextAsync(target));
                        Assert.Equal("FileMove.TargetAlreadyExists", Assert.Single(log.ReadRecent().Entries).EventName);
                    }
                    else
                    {
                        Assert.Empty(log.ReadRecent().Entries);
                        if (scenario == "default")
                        {
                            Assert.Equal(fallback.Id, suggestion.SelectedTargetFolder?.Id);
                            Assert.False(File.Exists(target));
                            Assert.True(await suggestion.ConfirmMoveAsync());
                            Assert.False(File.Exists(source));
                            Assert.Equal("source", await File.ReadAllTextAsync(target));
                        }
                    }
                }
            }
            finally
            {
                popup?.Close();
                Directory.Delete(root, true);
            }
            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task GeneralSettingsTogglePersistsOptInAndOptOut()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "sortify-auto-setting-" + Guid.NewGuid());
            var store = new JsonSettingsStore(Path.Combine(root, "settings.json"), new SettingsValidator(new RuleValidator()));
            var model = new MainViewModel(new SettingsService(store));
            await model.InitializeSettingsAsync();
            var window = new SettingsWindow { DataContext = model };
            try
            {
                window.Show();
                var toggle = window.FindControl<ToggleButton>("AutoSortToggle")!;
                Assert.False(toggle.IsChecked == true);
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                foreach (var enabled in new[] { true, false })
                {
                    toggle.IsChecked = enabled;
                    while (!toggle.IsEnabled) await Task.Delay(20, timeout.Token);
                    Assert.Equal(enabled, (await store.LoadAsync()).Settings?.AutoSort);
                }
            }
            finally
            {
                window.Close();
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
            return true;
        }, CancellationToken.None);
    }
}
