using Sortify.Core.Models;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.Infrastructure.Tests;

public sealed class FolderMonitorTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "sortify-monitor-" + Guid.NewGuid());
    private readonly FolderMonitor monitor = new(
        TimeSpan.FromMilliseconds(25), TimeSpan.FromMilliseconds(150));

    public FolderMonitorTests() => Directory.CreateDirectory(directory);

    [Fact]
    public async Task DetectsNewFileButNotExistingFilesAndPreservesBytes()
    {
        await File.WriteAllTextAsync(Path.Combine(directory, "existing.pdf"), "old");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var reader = monitor.WatchAsync(directory, timeout.Token).GetAsyncEnumerator();
        var next = reader.MoveNextAsync().AsTask();
        var path = Path.Combine(directory, "new.file.DOCX");
        await File.WriteAllTextAsync(path, "content");

        Assert.True(await next);
        var file = reader.Current;
        Assert.Equal(path, file.SourcePath);
        Assert.Equal("new.file", file.FileNameWithoutExtension);
        Assert.Equal(".DOCX", file.Extension);
        Assert.Equal("new.file.DOCX", file.RelativePath);
        Assert.Equal(7, file.Size);
        Assert.Equal("content", await File.ReadAllTextAsync(path));
    }

    [Theory]
    [InlineData(".crdownload")]
    [InlineData(".part")]
    [InlineData(".partial")]
    [InlineData(".tmp")]
    public async Task WaitsForTemporaryDownloadToBeRenamed(string suffix)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var reader = monitor.WatchAsync(directory, timeout.Token).GetAsyncEnumerator();
        var next = reader.MoveNextAsync().AsTask();
        var temporary = Path.Combine(directory, "download.pdf" + suffix);
        await File.WriteAllTextAsync(temporary, "download");
        await Task.Delay(400, timeout.Token);
        Assert.False(next.IsCompleted);
        var final = Path.Combine(directory, "download.pdf");
        File.Move(temporary, final);
        Assert.True(await next);
        Assert.Equal(final, reader.Current.SourcePath);
    }

    [Fact]
    public async Task WaitsForExclusiveWriterAndStableMetadata()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var reader = monitor.WatchAsync(directory, timeout.Token).GetAsyncEnumerator();
        var next = reader.MoveNextAsync().AsTask();
        var path = Path.Combine(directory, "stream.txt");
        await using (var writer = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await writer.WriteAsync(new byte[] { 1, 2 }, timeout.Token);
            await writer.FlushAsync(timeout.Token);
            await Task.Delay(400, timeout.Token);
            Assert.False(next.IsCompleted);
            await writer.WriteAsync(new byte[] { 3 }, timeout.Token);
        }

        Assert.True(await next);
        Assert.Equal(3, reader.Current.Size);
    }

    [Fact]
    public async Task DoesNotNotifyAgainForSameFileOrItsChanges()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var reader = monitor.WatchAsync(directory, timeout.Token).GetAsyncEnumerator();
        var next = reader.MoveNextAsync().AsTask();
        var first = Path.Combine(directory, "first.txt");
        await File.WriteAllTextAsync(first, "a");
        Assert.True(await next);
        Assert.Equal(first, reader.Current.SourcePath);

        var second = reader.MoveNextAsync().AsTask();
        await File.AppendAllTextAsync(first, "b");
        var other = Path.Combine(directory, "other.txt");
        await File.WriteAllTextAsync(other, "other");
        Assert.True(await second);
        Assert.Equal(other, reader.Current.SourcePath);
    }

    [Fact]
    public async Task CancellationStopsOldFolderBeforeStartingAnother()
    {
        using var firstSession = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using (var reader = monitor.WatchAsync(directory, firstSession.Token).GetAsyncEnumerator())
        {
            var next = reader.MoveNextAsync().AsTask();
            firstSession.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => next);
        }

        var other = Path.Combine(directory, "other");
        Directory.CreateDirectory(other);
        using var secondSession = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var secondReader = monitor.WatchAsync(other, secondSession.Token).GetAsyncEnumerator();
        var detected = secondReader.MoveNextAsync().AsTask();
        await File.WriteAllTextAsync(Path.Combine(directory, "old-folder.txt"), "old");
        var expected = Path.Combine(other, "new-folder.txt");
        await File.WriteAllTextAsync(expected, "new");
        Assert.True(await detected);
        Assert.Equal(expected, secondReader.Current.SourcePath);
    }

    public void Dispose() => Directory.Delete(directory, true);
}
