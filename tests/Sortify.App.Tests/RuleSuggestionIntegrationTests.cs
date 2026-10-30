using System.Collections.ObjectModel;
using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Core.Services;

namespace Sortify.App.Tests;

public sealed class RuleSuggestionIntegrationTests
{
    [Theory]
    [InlineData("matched")]
    [InlineData("ambiguous")]
    [InlineData("no-match")]
    [InlineData("no-default")]
    public async Task ReadyFileUsesRulesAndStillRequiresConfirmation(string scenario)
    {
        var first = new TargetFolder(Guid.NewGuid(), Path.Combine(Path.GetTempPath(), "default"), "Default");
        var second = new TargetFolder(Guid.NewGuid(), Path.Combine(Path.GetTempPath(), "invoices"), "Invoices");
        SortRule Rule(TargetFolder target) => new(
            Guid.NewGuid(), target.Nickname, "^invoice", new[] { ".pdf" }, null,
            "", "sorted_{name}{ext}", 1, true) { TargetFolderId = target.Id };
        var rules = scenario == "matched" ? new[] { Rule(second) }
            : scenario == "ambiguous" ? new[] { Rule(first), Rule(second) }
            : Array.Empty<SortRule>();
        var settings = new AppSettings(Path.GetTempPath(), null, false, rules)
        {
            TargetFolders = new[] { first, second },
            DefaultTargetFolderId = scenario == "no-default" ? null : first.Id
        };
        var cards = new ObservableCollection<TargetFolderViewModel>
        {
            new(first.Id, first.Nickname, first.Path, true),
            new(second.Id, second.Nickname, second.Path)
        };
        var source = Path.Combine(Path.GetTempPath(), "invoice.pdf");
        var service = new MoveService(source);
        var model = new FileSuggestionViewModel(cards, source, service,
            new RuleEngine(new RuleValidator(), new RuleTargetValidator()), () => settings);

        await model.InitializeFileOperationAsync();

        Assert.Equal(0, service.MoveCount);
        if (scenario == "matched")
        {
            Assert.Equal(second.Id, model.SelectedTargetFolder?.Id);
            Assert.Equal("sorted_invoice", model.NewFileName);
            Assert.Contains("Invoices", model.SuggestionExplanation);
        }
        else if (scenario == "no-default")
        {
            Assert.Null(model.SelectedTargetFolder);
            Assert.False(model.CanConfirmMove);
            return;
        }
        else
        {
            Assert.Equal(first.Id, model.SelectedTargetFolder?.Id);
            Assert.Equal("invoice", model.NewFileName);
            Assert.Contains("Standardordner", model.SuggestionExplanation);
        }

        // A manual target/name change must remain authoritative, including a readiness retry.
        model.SelectedTargetFolder = cards[1];
        model.NewFileName = "manual";
        await model.InitializeFileOperationAsync();
        Assert.Equal("manual", model.NewFileName);
        Assert.True(await model.ConfirmMoveAsync());
        Assert.Equal(1, service.MoveCount);
        Assert.Equal(Path.Combine(second.Path, "manual.pdf"), service.Request?.TargetPath);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LateAiReplyPreservesManualEditsAndNeverMovesFile(bool cancel)
    {
        var target = new TargetFolder(Guid.NewGuid(), Path.GetTempPath(), "Schule");
        var settings = new AppSettings(Path.GetTempPath(), null, false, []) {
            TargetFolders = [target], DefaultTargetFolderId = target.Id,
            AiMode = AiSuggestionMode.Fallback, AiMetadataConsent = true };
        var cards = new ObservableCollection<TargetFolderViewModel> { new(target.Id, target.Nickname, target.Path, true) };
        var source = Path.Combine(Path.GetTempPath(), "invoice.pdf");
        var move = new MoveService(source);
        var ai = new PendingAiService();
        var model = new FileSuggestionViewModel(cards, source, move,
            new RuleEngine(new RuleValidator(), new RuleTargetValidator()), () => settings, ai);
        var pending = model.InitializeFileOperationAsync();
        Assert.True(model.IsAiBusy);
        model.NewFileName = "manual";
        if (cancel) model.CancelAiSuggestion();
        ai.Completion.SetResult(new(new(target.Id, "AI.pdf", "Testvorschlag", []), "Verfügbar"));
        await pending;
        Assert.Equal("manual", model.NewFileName);
        Assert.Equal(0, move.MoveCount);
        Assert.Equal(!cancel, model.HasAiAlternative);
        if (!cancel)
        {
            model.AdoptAiSuggestion();
            Assert.Equal("AI", model.NewFileName);
            Assert.Equal(0, move.MoveCount);
        }
    }

    private sealed class PendingAiService : IAiSuggestionService
    {
        public TaskCompletionSource<AiSuggestionResult> Completion { get; } = new();
        public Task<AiSuggestionResult> SuggestAsync(AiSuggestionRequest request, CancellationToken cancellationToken = default)
            => Completion.Task;
    }

    private sealed class MoveService(string source) : IFileMoveService
    {
        public int MoveCount { get; private set; }
        public FileMoveRequest? Request { get; private set; }
        public Task<FileSourceSnapshotResult> InspectSourceAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(new FileSourceSnapshotResult(new(source, 1, DateTimeOffset.UnixEpoch), null));
        public Task<FileMoveResult> MoveAsync(FileMoveRequest request, CancellationToken cancellationToken = default)
        {
            MoveCount++;
            Request = request;
            return Task.FromResult(new FileMoveResult(FileMoveStatus.Succeeded, null));
        }
    }
}
