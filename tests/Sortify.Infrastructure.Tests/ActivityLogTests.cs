using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Core.Rules;
using Sortify.Infrastructure.FileSystem;
using Sortify.Infrastructure.Logging;
using Sortify.Infrastructure.Monitoring;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Tests;

public sealed class ActivityLogTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "sortify-log-" + Guid.NewGuid());
    private string Logs => Path.Combine(root, "logs");

    [Fact]
    public async Task ConcurrentInstancesPersistCompleteEntriesAndNewestFirstReadSurvivesRestart()
    {
        var first = new JsonActivityLog(Logs);
        Assert.Empty(first.ReadRecent().Entries);
        Assert.False(Directory.Exists(Logs));
        var second = new JsonActivityLog(Logs);
        await Task.WhenAll(Enumerable.Range(0, 100).Select(index => Task.Run(() =>
            Assert.True((index % 2 == 0 ? first : second).TryWrite(LogLevel.Information, "Test." + index, "Line\nwith ünicode")))));
        var result = new JsonActivityLog(Logs).ReadRecent(1000);
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Entries.Count);
        Assert.Equal(100, result.Entries.Select(entry => entry.EventName).Distinct().Count());
        Assert.All(result.Entries, entry => Assert.Equal(TimeSpan.Zero, entry.TimestampUtc.Offset));
        Assert.True(first.TryWrite(LogLevel.Warning, "Last", "Last message"));
        Assert.Equal("Last", Assert.Single(second.ReadRecent(1).Entries).EventName);
        using var timestampDocument = System.Text.Json.JsonDocument.Parse(File.ReadLines(Path.Combine(Logs, "activity.jsonl")).Last());
        Assert.Equal("Warning", timestampDocument.RootElement.GetProperty("Level").GetString());
    }

    [Fact]
    public void RotationBoundsStorageAndReadsAcrossArchivesInWriteOrder()
    {
        var log = new JsonActivityLog(Logs, maxFileBytes: 4096, archiveCount: 2);
        for (var index = 0; index < 50; index++)
            Assert.True(log.TryWrite(LogLevel.Information, "Event." + index, new string('x', 1000)));
        var files = Directory.GetFiles(Logs);
        Assert.Equal(3, files.Length);
        Assert.All(files, path => Assert.InRange(new FileInfo(path).Length, 1, 4096));
        var recent = log.ReadRecent(1000);
        Assert.True(recent.IsSuccess);
        var numbers = recent.Entries.Select(entry => int.Parse(entry.EventName.Split('.')[1])).ToArray();
        Assert.Equal(49, numbers[0]);
        Assert.Equal(numbers.OrderDescending(), numbers);
        Assert.True(numbers.Length > File.ReadAllLines(Path.Combine(Logs, "activity.jsonl")).Length);
        Assert.DoesNotContain(0, numbers);
    }

    [Fact]
    public void DamagedLinesAreReportedButValidEntriesRemainReadableAndInputCannotInjectLines()
    {
        var log = new JsonActivityLog(Logs);
        Assert.True(log.TryWrite(LogLevel.Error, "Test", "first\nsecond"));
        var path = Path.Combine(Logs, "activity.jsonl");
        Assert.Single(File.ReadAllLines(path));
        File.AppendAllText(path, "{broken\n{}\n");
        var result = log.ReadRecent();
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.SkippedEntries);
        Assert.Equal("first\nsecond", Assert.Single(result.Entries).Message);
        Assert.False(log.TryWrite(LogLevel.Information, "", "message"));
        Assert.False(log.TryWrite(LogLevel.Information, "Test", new string('x', 2001)));
        Assert.Throws<ArgumentOutOfRangeException>(() => log.ReadRecent(1001));
    }

    [Fact]
    public void UnavailableStorageReturnsFailuresAndCanRecover()
    {
        Directory.CreateDirectory(root);
        File.WriteAllText(Logs, "blocked");
        var log = new JsonActivityLog(Logs);
        Assert.False(log.TryWrite(LogLevel.Error, "Test", "Message"));
        Assert.False(log.ReadRecent().IsSuccess);
        File.Delete(Logs);
        Assert.True(log.TryWrite(LogLevel.Information, "Recovered", "Message"));
        File.WriteAllText(Path.Combine(Logs, "activity.jsonl"), new string('x', 1_048_577));
        Assert.False(log.ReadRecent().IsSuccess);
    }

    [Fact]
    public async Task FileMoveLogsOnlyOutcomesWithFileAndTarget()
    {
        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "private-invoice.txt");
        var target = Path.Combine(root, "sorted.txt");
        File.WriteAllText(source, "content");
        var info = new FileInfo(source);
        var log = new JsonActivityLog(Logs);
        var service = new LoggedFileMoveService(new FileMoveService(), log);
        var request = new FileMoveRequest(source, target, info.Length, new DateTimeOffset(info.LastWriteTimeUtc), false);
        Assert.Equal(FileMoveStatus.NotConfirmed, (await service.MoveAsync(request)).Status);
        Assert.True(File.Exists(source));
        Assert.Equal(FileMoveStatus.Succeeded, (await service.MoveAsync(request with { IsConfirmed = true })).Status);
        Assert.Equal("content", File.ReadAllText(target));
        Assert.False(File.Exists(source));
        var entries = log.ReadRecent().Entries;
        Assert.Equal(new[] { "FileMove.Succeeded", "FileMove.NotConfirmed" }, entries.Select(entry => entry.EventName));
        Assert.Contains("private-invoice.txt", entries[0].Message);
        Assert.Contains(root, entries[0].Message);
        Assert.Contains("sorted.txt", entries[0].Message);
        Assert.Contains("private-invoice.txt", entries[1].Message);
        service.RecordDismissal(source);
        Assert.Equal(2, log.ReadRecent().Entries.Count);
        var inspection = new LoggedFileMoveService(new FileMoveService(
            TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(5), TimeSpan.FromSeconds(1)), log);
        Assert.NotNull((await inspection.InspectSourceAsync(target)).Snapshot);
        Assert.Equal(2, log.ReadRecent().Entries.Count);
    }

    [Fact]
    public async Task ThrowingLoggerDoesNotChangeSuccessfulMoveOrCancellation()
    {
        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "source.txt");
        File.WriteAllText(source, "safe");
        var info = new FileInfo(source);
        var service = new LoggedFileMoveService(new FileMoveService(), new ThrowingLog());
        var request = new FileMoveRequest(source, Path.Combine(root, "target.txt"), info.Length,
            new DateTimeOffset(info.LastWriteTimeUtc), true);
        Assert.Equal(FileMoveStatus.Succeeded, (await service.MoveAsync(request)).Status);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.MoveAsync(request, cancellation.Token));
    }

    [Fact]
    public async Task SettingsRemainSilentOnSuccessAndFailure()
    {
        Directory.CreateDirectory(root);
        var log = new JsonActivityLog(Logs);
        var service = new LoggedSettingsStore(new JsonSettingsStore(Path.Combine(root, "settings.json"),
            new SettingsValidator(new RuleValidator())), log);
        Assert.Equal(SettingsLoadStatus.NotFound, (await service.LoadAsync()).Status);
        Assert.True((await service.SaveAsync(new AppSettings(null, null, false, []))).IsSuccess);
        Assert.Equal(SettingsLoadStatus.Loaded, (await service.LoadAsync()).Status);
        File.WriteAllText(Path.Combine(root, "settings.json"), "{broken");
        Assert.Equal(SettingsLoadStatus.Failed, (await service.LoadAsync()).Status);
        Assert.Empty(log.ReadRecent().Entries);
        File.Delete(Path.Combine(root, "settings.json"));
        Directory.CreateDirectory(Path.Combine(root, "settings.json"));
        Assert.False((await service.SaveAsync(new AppSettings(null, null, false, []))).IsSuccess);
        Assert.Empty(log.ReadRecent().Entries);
        Directory.Delete(Path.Combine(root, "settings.json"));
        var throwing = new LoggedSettingsStore(new JsonSettingsStore(Path.Combine(root, "settings.json"),
            new SettingsValidator(new RuleValidator())), new ThrowingLog());
        Assert.True((await throwing.SaveAsync(new AppSettings(null, null, false, []))).IsSuccess);
    }

    [Fact]
    public async Task MonitorRemainsSilentIncludingFailedStart()
    {
        Directory.CreateDirectory(root);
        var log = new JsonActivityLog(Logs);
        using var monitor = new FolderMonitor(log);
        var detected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        monitor.Start(root, _ => detected.TrySetResult(), _ => { });
        File.WriteAllText(Path.Combine(root, "new.pdf"), "content");
        await detected.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Dispose();
        Assert.Empty(log.ReadRecent().Entries);
        Assert.Throws<ArgumentException>(() => monitor.Start(Path.Combine(root, "missing"), _ => { }, _ => { }));
        Assert.Empty(log.ReadRecent().Entries);
    }

    [Fact]
    public void RuleEvaluationRemainsSilentAndPreservesResult()
    {
        var log = new JsonActivityLog(Logs);
        var engine = new RuleEngine(new RuleValidator(), new RuleTargetValidator());
        var file = new FileDescriptor(root, "private-name", ".pdf", "private-name.pdf", 1, DateTimeOffset.UnixEpoch);
        var expected = engine.Evaluate(file, [], []);
        var result = new LoggedRuleEngine(engine, log).Evaluate(file, [], []);
        Assert.Equal(expected.Status, result.Status);
        Assert.Equal(expected.Candidates, result.Candidates);
        Assert.Empty(log.ReadRecent().Entries);
        Assert.Equal(expected.Status, new LoggedRuleEngine(engine, new ThrowingLog()).Evaluate(file, [], []).Status);
    }

    [Fact]
    public void DismissalWithoutMoveIsRecordedOnceWithFileName()
    {
        var log = new JsonActivityLog(Logs);
        var service = new LoggedFileMoveService(new FileMoveService(), log);
        var source = Path.Combine(root, "not-moved.pdf");
        service.RecordDismissal(source);
        service.RecordDismissal(source);
        var entry = Assert.Single(log.ReadRecent().Entries);
        Assert.Equal("FileMove.Cancelled", entry.EventName);
        Assert.Contains("not-moved.pdf", entry.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    private sealed class ThrowingLog : IActivityLog
    {
        public bool TryWrite(LogLevel level, string eventName, string message) => throw new IOException("Log failure");
        public LogReadResult ReadRecent(int limit = 100) => throw new IOException("Log failure");
    }
}
