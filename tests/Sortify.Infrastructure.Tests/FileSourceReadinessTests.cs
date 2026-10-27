using System.Diagnostics;
using Sortify.Infrastructure.FileSystem;

namespace Sortify.Infrastructure.Tests;

public sealed class FileSourceReadinessTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(), "SortifyReadiness", Guid.NewGuid().ToString("N"));

    public FileSourceReadinessTests() => Directory.CreateDirectory(directory);

    private static FileMoveService CreateService() => new(
        TimeSpan.FromMilliseconds(100),
        TimeSpan.FromMilliseconds(20),
        TimeSpan.FromSeconds(2));

    [Fact]
    public async Task StableReadableFileProducesSnapshotAfterStabilityWindow()
    {
        var path = Path.Combine(directory, "document.docx");
        await File.WriteAllTextAsync(path, "Finished document");
        var elapsed = Stopwatch.StartNew();

        var result = await CreateService().InspectSourceAsync(path);

        Assert.NotNull(result.Snapshot);
        Assert.Null(result.ErrorMessage);
        Assert.True(elapsed.Elapsed >= TimeSpan.FromMilliseconds(100));
        var file = new FileInfo(path);
        Assert.Equal(file.Length, result.Snapshot.Length);
        Assert.Equal(file.LastWriteTimeUtc, result.Snapshot.LastWriteTimeUtc.UtcDateTime);
        Assert.Equal("Finished document", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task LockedCopyWaitsForReleaseAndFinalMetadata()
    {
        var path = Path.Combine(directory, "copy.pptx");
        Task<Sortify.Core.Models.FileSourceSnapshotResult> inspection;
        await using (var writer = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await writer.WriteAsync(new byte[] { 1, 2, 3 });
            await writer.FlushAsync();
            inspection = CreateService().InspectSourceAsync(path);

            // Deliberately keep the writer locked longer than the stability window.
            await Task.Delay(250);
            Assert.False(inspection.IsCompleted);
            await writer.WriteAsync(new byte[] { 4, 5, 6, 7 });
            await writer.FlushAsync();
        }

        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(-10));
        var result = await inspection.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.NotNull(result.Snapshot);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(7, result.Snapshot.Length);
        Assert.Equal(
            File.GetLastWriteTimeUtc(path),
            result.Snapshot.LastWriteTimeUtc.UtcDateTime);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task UnavailableFileReturnsBoundedRetryMessage()
    {
        var service = new FileMoveService(
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(10),
            TimeSpan.FromMilliseconds(150));

        var result = await service.InspectSourceAsync(
            Path.Combine(directory, "missing.docx")).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Null(result.Snapshot);
        Assert.Contains("erneut prüfen", result.ErrorMessage!);
    }

    [Fact]
    public async Task WaitingInspectionCanBeCancelled()
    {
        using var cancellation = new CancellationTokenSource();
        var inspection = CreateService().InspectSourceAsync(
            Path.Combine(directory, "missing.pptx"), cancellation.Token);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => inspection.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    public void Dispose() => Directory.Delete(directory, recursive: true);
}
