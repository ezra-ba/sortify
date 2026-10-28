using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Logging;
using System.Collections.Concurrent;

namespace Sortify.Infrastructure.Monitoring;

public sealed class FolderMonitor(IActivityLog? log = null) : IDisposable
{
    private FileSystemWatcher? watcher;

    public void Start(string directory, Action<string> onFile, Action<string> onError)
    {
        try { StartCore(directory, onFile, onError); }
        catch (Exception)
        {
            Dispose();
            log.Record(LogLevel.Error, "Monitor.StartFailed", "Ordnerüberwachung konnte nicht gestartet werden.");
            throw;
        }
    }

    private void StartCore(string directory, Action<string> onFile, Action<string> onError)
    {
        Dispose();
        var current = new FileSystemWatcher(directory) { NotifyFilter = NotifyFilters.FileName };
        var seen = new ConcurrentDictionary<string, byte>(
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        void Report(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (ReferenceEquals(watcher, current) && File.Exists(path)
                && extension is not (".crdownload" or ".part" or ".partial" or ".tmp")
                && !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal)
                && seen.TryAdd(path, 0))
            {
                log.Record(LogLevel.Information, "Monitor.FileDetected", "Neue Datei im Überwachungsordner erkannt.");
                onFile(path);
            }
        }

        current.Created += (_, args) => Report(args.FullPath);
        current.Renamed += (_, args) => Report(args.FullPath);
        current.Deleted += (sender, args) => seen.TryRemove(args.FullPath, out _);
        current.Error += (_, _) =>
        {
            if (ReferenceEquals(watcher, current))
            {
                log.Record(LogLevel.Error, "Monitor.Interrupted", "Ordnerüberwachung unterbrochen.");
                onError("Ordnerueberwachung unterbrochen. Bitte den Ordner erneut auswaehlen.");
            }
        };
        watcher = current;
        current.EnableRaisingEvents = true;
        log.Record(LogLevel.Information, "Monitor.Started", "Ordnerüberwachung gestartet.");
    }

    public void Dispose()
    {
        var previous = watcher;
        watcher = null;
        previous?.Dispose();
        if (previous is not null) log.Record(LogLevel.Information, "Monitor.Stopped", "Ordnerüberwachung beendet.");
    }
}
