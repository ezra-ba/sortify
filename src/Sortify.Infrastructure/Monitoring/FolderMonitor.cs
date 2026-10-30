using Sortify.Core.Services;
using System.Collections.Concurrent;

namespace Sortify.Infrastructure.Monitoring;

public sealed class FolderMonitor : IDisposable
{
    // Retained for callers that already inject a logger; monitoring is intentionally silent.
    public FolderMonitor(IActivityLog? log = null) { }
    private FileSystemWatcher? watcher;

    public void Start(string directory, Action<string> onFile, Action<string> onError)
    {
        try { StartCore(directory, onFile, onError); }
        catch (Exception)
        {
            Dispose();
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
                onError("Ordnerueberwachung unterbrochen. Bitte den Ordner erneut auswaehlen.");
            }
        };
        watcher = current;
        current.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        var previous = watcher;
        watcher = null;
        previous?.Dispose();
    }
}
