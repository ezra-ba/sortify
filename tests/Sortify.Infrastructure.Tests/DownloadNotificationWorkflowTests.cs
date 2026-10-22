using System.Runtime.CompilerServices;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.Infrastructure.Tests;

public sealed class DownloadNotificationWorkflowTests
{
    [Fact]
    public async Task RealFileFlowsThroughProviderIntoPresenterWithoutBeingChanged()
    {
        var directory = Path.Combine(Path.GetTempPath(), "sortify-flow-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            var id = Guid.NewGuid();
            var settings = new AppSettings(directory, null, false, [])
            {
                TargetFolders = [new TargetFolder(id, Path.Combine(directory, "target"), "School")],
                DefaultTargetFolderId = id
            };
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var presenter = new CapturingPresenter();
            var provider = new CapturingProvider();
            var workflow = new DownloadNotificationWorkflow(
                new FolderMonitor(TimeSpan.FromMilliseconds(25), TimeSpan.FromMilliseconds(100)),
                provider, presenter, () => settings);
            var running = workflow.RunAsync(directory, timeout.Token);
            var path = Path.Combine(directory, "questions.DOCX");
            await File.WriteAllTextAsync(path, "unchanged");

            var shown = await presenter.Shown.Task.WaitAsync(timeout.Token);
            Assert.Equal(path, shown.File.SourcePath);
            Assert.Equal("questions.DOCX", shown.Proposal.FileName);
            Assert.Equal(id, shown.Proposal.TargetFolderId);
            Assert.Equal("questions.DOCX", provider.Request!.FileName);
            Assert.Equal(".DOCX", provider.Request.Extension);
            Assert.Equal("School", Assert.Single(provider.Request.Targets).Name);
            Assert.Equal("unchanged", await File.ReadAllTextAsync(path));
            timeout.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("failure")]
    [InlineData("invalid-name")]
    [InlineData("unknown-target")]
    [InlineData("changed-extension")]
    [InlineData("timeout")]
    public async Task ProviderProblemsFallBackToOriginalNameAndConfiguredDefault(string mode)
    {
        var file = new FileDescriptor("/incoming/invoice.pdf", "invoice", ".pdf", "invoice.pdf", 1, DateTimeOffset.UtcNow);
        var id = Guid.NewGuid();
        var settings = new AppSettings(null, null, false, [])
        {
            TargetFolders = [new TargetFolder(id, "/target", "Archive")],
            DefaultTargetFolderId = id
        };
        var presenter = new CapturingPresenter();
        await new DownloadNotificationWorkflow(new SingleFileMonitor(file),
            new CapturingProvider(mode), presenter, () => settings,
            TimeSpan.FromMilliseconds(100)).RunAsync("/incoming");
        var shown = await presenter.Shown.Task;
        Assert.Equal("invoice.pdf", shown.Proposal.FileName);
        Assert.Equal(id, shown.Proposal.TargetFolderId);
    }

    [Fact]
    public async Task MissingTargetStillShowsOriginalFileWithNoSelectedDestination()
    {
        var presenter = new CapturingPresenter();
        var file = new FileDescriptor("/incoming/image.png", "image", ".png", "image.png", 1, DateTimeOffset.UtcNow);
        await new DownloadNotificationWorkflow(new SingleFileMonitor(file),
            new LocalFileSuggestionProvider(), presenter, () => null).RunAsync("/incoming");
        var shown = await presenter.Shown.Task;
        Assert.Null(shown.Proposal.TargetFolderId);
        Assert.Empty(shown.Targets);
        Assert.Equal("image.png", shown.Proposal.FileName);
    }

    private sealed class SingleFileMonitor(FileDescriptor file) : IFileMonitor
    {
        public async IAsyncEnumerable<FileDescriptor> WatchAsync(
            string directory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    private sealed class CapturingProvider(string? mode = null) : IFileSuggestionProvider
    {
        public FileSuggestionRequest? Request { get; private set; }

        public Task<FileSuggestionProposal> CreateAsync(
            FileSuggestionRequest request, CancellationToken cancellationToken = default)
        {
            Request = request;
            if (mode == "timeout")
                return new TaskCompletionSource<FileSuggestionProposal>().Task;
            if (mode == "failure")
                throw new IOException("Provider unavailable");
            return Task.FromResult(new FileSuggestionProposal(
                mode == "invalid-name" ? "../outside.pdf"
                    : mode == "changed-extension" ? "invoice.exe" : request.FileName,
                mode == "unknown-target" ? Guid.NewGuid() : request.DefaultTargetFolderId,
                "Example proposal"));
        }
    }

    private sealed class CapturingPresenter : IDownloadPresenter
    {
        public TaskCompletionSource<(FileDescriptor File, FileSuggestionProposal Proposal, IReadOnlyList<TargetFolder> Targets)> Shown { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task ShowAsync(FileDescriptor file, FileSuggestionProposal proposal,
            IReadOnlyList<TargetFolder> targets, CancellationToken cancellationToken = default)
        {
            Shown.TrySetResult((file, proposal, targets));
            return Task.CompletedTask;
        }
    }
}
