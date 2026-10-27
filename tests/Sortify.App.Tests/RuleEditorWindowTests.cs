using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

[Collection("Avalonia")]
public sealed class RuleEditorWindowTests
{
    [Fact]
    public async Task RuleCreatedThroughBoundControlsSurvivesReloadAndProducesConfirmedProposal()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "SortifyRuleUi-" + Guid.NewGuid());
            Directory.CreateDirectory(root);
            var path = Path.Combine(root, "settings.json");
            JsonSettingsStore Store() => new(path, new SettingsValidator(new RuleValidator()));
            var target = new TargetFolder(Guid.NewGuid(), Path.Combine(root, "invoices"), "Rechnungen");
            var legacy = new SortRule(Guid.NewGuid(), "Altregel", "^legacy", [".pdf"], null, "", null, 10, true);
            var initial = new AppSettings(root, null, false, [legacy]) { TargetFolders = [target] };
            Assert.True((await Store().SaveAsync(initial)).IsSuccess);
            var main = new MainViewModel(new SettingsService(Store()));
            await main.InitializeSettingsAsync();
            var loadedLegacy = main.CurrentSettings!.Rules[0];
            var window = new SettingsWindow { DataContext = main, Height = 900 };
            try
            {
                window.OpenRulesForTarget(main.TargetFolders[0]);
                window.Show();
                var view = window.FindControl<RuleEditorView>("RulesEditor")!;
                var editor = Assert.IsType<RuleEditorViewModel>(view.DataContext);
                Assert.Same(main.TargetFolders[0], editor.SelectedTarget);
                view.FindControl<TextBox>("RuleNameInput")!.Text = "Rechnungen";
                view.FindControl<TextBox>("RulePatternInput")!.Text = "[";
                Dispatcher.UIThread.RunJobs();
                var button = view.FindControl<Button>("CreateRuleButton")!;
                var command = Assert.IsAssignableFrom<IAsyncRelayCommand>(button.Command);
                await command.ExecuteAsync(null);
                Assert.Single(main.CurrentSettings!.Rules);
                Assert.Contains("ungültig", view.FindControl<TextBlock>("RuleResultMessage")!.Text);
                view.FindControl<TextBox>("RulePatternInput")!.Text = "^rechnung";
                view.FindControl<TextBox>("RuleExtensionsInput")!.Text = "PDF, .pdf";
                view.FindControl<TextBox>("RuleRenameInput")!.Text = "sortiert_{name}{ext}";
                Dispatcher.UIThread.RunJobs();
                await command.ExecuteAsync(null);
                Assert.Equal(2, editor.ExistingRules.Count);
                Assert.Equal("#15803D", editor.MessageColor);
                Assert.Equal(".PDF", Assert.Single(main.CurrentSettings!.Rules[1].AllowedExtensions));
                Assert.True(main.HasRuleTargetWarnings);
                editor.SelectedExistingRule = editor.ExistingRules.First(item => item.Id == legacy.Id);
                editor.RepairTarget = main.TargetFolders[0];
                await editor.AssignTargetCommand.ExecuteAsync(null);
                Assert.False(main.HasRuleTargetWarnings);
                Assert.Equal(loadedLegacy with { TargetFolderId = target.Id }, main.CurrentSettings.Rules[0]);

                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                using var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
                frame.Save("/tmp/sortify-rule-editor-preview.png", Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                window.Close();

                // A fresh service and model read the file; no shared in-memory rule state.
                var reloaded = new MainViewModel(new SettingsService(Store()));
                await reloaded.InitializeSettingsAsync();
                Assert.Equal(2, reloaded.CurrentSettings!.Rules.Count);
                var saved = reloaded.CurrentSettings.Rules.Single(rule => rule.Name == "Rechnungen");
                Assert.Equal(target.Id, saved.TargetFolderId);
                var source = Path.Combine(root, "rechnung_2026.PDF");
                var moves = new Moves(source);
                var popup = new FileSuggestionViewModel(reloaded.TargetFolders, source, moves,
                    new RuleEngine(new RuleValidator(), new RuleTargetValidator()), () => reloaded.CurrentSettings);
                await popup.InitializeFileOperationAsync();
                Assert.Equal(target.Id, popup.SelectedTargetFolder?.Id);
                Assert.Equal("sortiert_rechnung_2026", popup.NewFileName);
                Assert.Contains("Rechnungen", popup.SuggestionExplanation);
                Assert.Equal(0, moves.Count);
                Assert.True(await popup.ConfirmMoveAsync());
                Assert.Equal(1, moves.Count);
                Assert.Equal(Path.Combine(target.Path, "sortiert_rechnung_2026.PDF"), moves.Request?.TargetPath);
            }
            finally
            {
                window.Close();
                Directory.Delete(root, true);
            }
            return true;
        }, CancellationToken.None);
    }

    private sealed class Moves(string source) : IFileMoveService
    {
        public int Count { get; private set; }
        public FileMoveRequest? Request { get; private set; }
        public Task<FileSourceSnapshotResult> InspectSourceAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(new FileSourceSnapshotResult(new(source, 4, DateTimeOffset.UnixEpoch), null));
        public Task<FileMoveResult> MoveAsync(FileMoveRequest request, CancellationToken cancellationToken = default)
        {
            Count++;
            Request = request;
            return Task.FromResult(new FileMoveResult(FileMoveStatus.Succeeded, null));
        }
    }
}
