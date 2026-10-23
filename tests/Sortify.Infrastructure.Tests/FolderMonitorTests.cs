using System.Threading.Channels;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.Infrastructure.Tests;

public sealed class FolderMonitorTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "sortify-monitor-" + Guid.NewGuid());
    private readonly FolderMonitor monitor = new();

    public FolderMonitorTests() => Directory.CreateDirectory(directory);

    [Fact]
    public async Task ReportsNewFileInsteadOfExistingFile()
    {
        await File.WriteAllTextAsync(Path.Combine(directory, "existing.pdf"), "old");
        var detected = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        monitor.Start(directory, path => detected.TrySetResult(path), _ => { });
        var expected = Path.Combine(directory, "new.DOCX");
        await File.WriteAllTextAsync(expected, "unchanged");

        Assert.Equal(expected, await detected.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal("unchanged", await File.ReadAllTextAsync(expected));
    }

    [Theory]
    [InlineData(".crdownload")]
    [InlineData(".part")]
    [InlineData(".partial")]
    [InlineData(".tmp")]
    public async Task IgnoresTemporaryNameAndReportsFinalRename(string suffix)
    {
        var detected = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        monitor.Start(directory, path => detected.TrySetResult(path), _ => { });
        var temporary = Path.Combine(directory, "download.pdf" + suffix);
        await File.WriteAllTextAsync(temporary, "content");
        await Task.Delay(150);
        Assert.False(detected.Task.IsCompleted);

        var final = Path.Combine(directory, "download.pdf");
        File.Move(temporary, final);
        Assert.Equal(final, await detected.Task.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task RestartReplacesOldFolder()
    {
        var paths = Channel.CreateUnbounded<string>();
        monitor.Start(directory, path => paths.Writer.TryWrite(path), _ => { });
        var other = Path.Combine(directory, "other");
        Directory.CreateDirectory(other);
        monitor.Start(other, path => paths.Writer.TryWrite(path), _ => { });
        await File.WriteAllTextAsync(Path.Combine(directory, "old.txt"), "old");
        var expected = Path.Combine(other, "new.txt");
        await File.WriteAllTextAsync(expected, "new");
        Assert.Equal(expected, await paths.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
    }

    public void Dispose()
    {
        monitor.Dispose();
        Directory.Delete(directory, true);
    }
}
