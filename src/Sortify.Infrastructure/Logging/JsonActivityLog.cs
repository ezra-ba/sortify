using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Logging;

public sealed class JsonActivityLog : IActivityLog
{
    private static readonly ConcurrentDictionary<string, object> Gates = new(
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private readonly string directory;
    private readonly long maxFileBytes;
    private readonly int archiveCount;
    private readonly TimeProvider clock;
    private readonly object gate;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        RespectRequiredConstructorParameters = true,
        Converters = { new JsonStringEnumConverter<LogLevel>() }
    };
    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    public JsonActivityLog(string directory, long maxFileBytes = 1_048_576, int archiveCount = 3,
        TimeProvider? clock = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        if (!Path.IsPathFullyQualified(directory)) throw new ArgumentException("An absolute log directory is required.", nameof(directory));
        if (maxFileBytes < 4096 || maxFileBytes > 16_777_216) throw new ArgumentOutOfRangeException(nameof(maxFileBytes));
        if (archiveCount < 1 || archiveCount > 10) throw new ArgumentOutOfRangeException(nameof(archiveCount));
        this.directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        this.maxFileBytes = maxFileBytes;
        this.archiveCount = archiveCount;
        this.clock = clock ?? TimeProvider.System;
        gate = Gates.GetOrAdd(this.directory, _ => new object());
    }

    public static string GetDefaultDirectory() => Path.Combine(
        Path.GetDirectoryName(SettingsStoragePath.GetDefaultFilePath())!, "logs");

    private string FilePath(int index) => Path.Combine(directory,
        index == 0 ? "activity.jsonl" : $"activity.{index}.jsonl");

    public bool TryWrite(LogLevel level, string eventName, string message)
    {
        if (!Enum.IsDefined(level) || string.IsNullOrWhiteSpace(eventName) || eventName.Length > 100 ||
            string.IsNullOrWhiteSpace(message) || message.Length > 2000) return false;
        try
        {
            var entry = new LogEntry(clock.GetUtcNow(), level, eventName, message);
            var bytes = Utf8.GetBytes(JsonSerializer.Serialize(entry, JsonOptions) + "\n");
            if (bytes.Length > maxFileBytes) return false;
            lock (gate)
            {
                Directory.CreateDirectory(directory);
                var current = FilePath(0);
                if (File.Exists(current) && new FileInfo(current).Length + bytes.Length > maxFileBytes)
                {
                    for (var index = archiveCount; index >= 1; index--)
                    {
                        var previous = FilePath(index - 1);
                        if (File.Exists(previous)) File.Move(previous, FilePath(index), overwrite: true);
                    }
                }
                using var stream = new FileStream(current, FileMode.Append, FileAccess.Write, FileShare.Read);
                stream.Write(bytes);
                stream.Flush();
            }
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }

    public LogReadResult ReadRecent(int limit = 100)
    {
        if (limit is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(limit));
        var entries = new Queue<LogEntry>();
        var skipped = 0;
        try
        {
            lock (gate)
            {
                if (File.Exists(directory)) return new([], "Der Protokollordner ist nicht verfügbar.");
                for (var index = archiveCount; index >= 0; index--)
                {
                    var path = FilePath(index);
                    if (!File.Exists(path)) continue;
                    // Refuse externally replaced oversized files rather than reading unbounded data.
                    if (new FileInfo(path).Length > maxFileBytes)
                        return new([], "Eine Log-Datei überschreitet die zulässige Größe.", skipped);
                    foreach (var line in File.ReadLines(path, Utf8))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        try
                        {
                            var entry = JsonSerializer.Deserialize<LogEntry>(line, JsonOptions);
                            if (entry is null || entry.TimestampUtc == default || !Enum.IsDefined(entry.Level) ||
                                string.IsNullOrWhiteSpace(entry.EventName) || string.IsNullOrWhiteSpace(entry.Message))
                            { skipped++; continue; }
                            entries.Enqueue(entry);
                            if (entries.Count > limit) entries.Dequeue();
                        }
                        catch (JsonException) { skipped++; }
                    }
                }
            }
            // Storage order breaks timestamp ties and tolerates system-clock corrections.
            return new(entries.Reverse().ToArray(), null, skipped);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new([], "Die Protokolldateien konnten nicht gelesen werden.", skipped);
        }
    }
}
