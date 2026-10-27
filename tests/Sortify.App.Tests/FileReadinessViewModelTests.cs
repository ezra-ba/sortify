using System.Collections.ObjectModel;
using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.App.Tests;

public sealed class FileReadinessViewModelTests
{
    private static string SourcePath => Path.Combine(Path.GetTempPath(), "document.docx");

    private static FileSuggestionViewModel CreateModel(IFileMoveService service) => new(
        new ObservableCollection<TargetFolderViewModel>
        {
            new(Guid.NewGuid(), "Documents", Path.GetTempPath())
        }, SourcePath, service);

    [Fact]
    public async Task WaitingDisablesConfirmationButAllowsCancellation()
    {
        var service = new TestMoveService
        {
            Inspection = async cancellation =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellation);
                throw new InvalidOperationException("Unreachable");
            }
        };
        var model = CreateModel(service);
        var preparation = model.InitializeFileOperationAsync();

        Assert.True(model.IsBusy);
        Assert.True(model.IsPreparingSource);
        Assert.True(model.CanCancel);
        Assert.False(model.CanConfirmMove);
        Assert.False(model.CanEdit);
        Assert.False(await model.ConfirmMoveAsync());
        Assert.Empty(service.Requests);

        model.CancelSourceInspection();
        await preparation.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(model.IsBusy);
        Assert.False(model.IsPreparingSource);
        Assert.True(model.CanRefreshSource);
        Assert.Empty(service.Requests);
    }

    [Fact]
    public async Task ChangedSourceRequiresRefreshAndAnotherExplicitConfirmation()
    {
        var samples = 0;
        var service = new TestMoveService
        {
            Inspection = _ => Task.FromResult(new FileSourceSnapshotResult(
                new FileSourceSnapshot(SourcePath, ++samples, DateTimeOffset.UnixEpoch), null)),
            MoveResult = request => new FileMoveResult(
                request.ExpectedLength == 1 ? FileMoveStatus.SourceChanged : FileMoveStatus.Succeeded,
                request.ExpectedLength == 1 ? "Datei verändert." : null)
        };
        var model = CreateModel(service);

        await model.InitializeFileOperationAsync();
        Assert.True(model.CanConfirmMove);
        Assert.Empty(service.Requests);

        Assert.False(await model.ConfirmMoveAsync());
        Assert.False(model.CanConfirmMove);
        Assert.True(model.CanRefreshSource);
        Assert.Equal(1, samples);
        Assert.Equal(1, Assert.Single(service.Requests).ExpectedLength);

        await model.InitializeFileOperationAsync();
        Assert.Equal(2, samples);
        Assert.Single(service.Requests);
        Assert.True(model.CanConfirmMove);
        Assert.False(model.CanRefreshSource);

        Assert.True(await model.ConfirmMoveAsync());
        Assert.Equal(2, service.Requests.Count);
        Assert.Equal(2, service.Requests[1].ExpectedLength);
        Assert.Equal(model.FilePathPreview, service.Requests[1].TargetPath);
        Assert.All(service.Requests, request => Assert.True(request.IsConfirmed));
        Assert.True(model.IsMoveCompleted);
        Assert.False(model.CanConfirmMove);
    }

    [Fact]
    public async Task TimeoutAllowsRetryWithoutMovingAnything()
    {
        var service = new TestMoveService
        {
            Inspection = _ => Task.FromResult(new FileSourceSnapshotResult(
                null, "Die Datei ist noch nicht stabil lesbar."))
        };
        var model = CreateModel(service);

        await model.InitializeFileOperationAsync();

        Assert.False(model.IsBusy);
        Assert.True(model.CanRefreshSource);
        Assert.False(model.CanConfirmMove);
        Assert.Contains("stabil", model.OperationMessage);
        Assert.Empty(service.Requests);
    }

    private sealed class TestMoveService : IFileMoveService
    {
        public required Func<CancellationToken, Task<FileSourceSnapshotResult>> Inspection { get; init; }
        public Func<FileMoveRequest, FileMoveResult> MoveResult { get; init; } =
            _ => new(FileMoveStatus.Succeeded, null);
        public List<FileMoveRequest> Requests { get; } = [];

        public Task<FileSourceSnapshotResult> InspectSourceAsync(
            string sourcePath, CancellationToken cancellationToken = default) => Inspection(cancellationToken);

        public Task<FileMoveResult> MoveAsync(
            FileMoveRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(MoveResult(request));
        }
    }
}
