using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Monitoring;

public sealed class FolderMonitor : IFileMonitor
{
    private readonly TimeSpan pollInterval;
    private readonly TimeSpan stabilityPeriod;

    public FolderMonitor(TimeSpan? pollInterval = null, TimeSpan? stabilityPeriod = null)
    {
        this.pollInterval = pollInterval ?? TimeSpan.FromMilliseconds(250);
        this.stabilityPeriod = stabilityPeriod ?? TimeSpan.FromSeconds(1);
        if (this.pollInterval <= TimeSpan.Zero || this.stabilityPeriod <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(pollInterval));
    }

    public async IAsyncEnumerable<FileDescriptor> WatchAsync(
        string directory,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(directory);
        var comparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var known = new ConcurrentDictionary<string, byte>(comparer);
        var pending = new ConcurrentDictionary<string, Candidate>(comparer);
        foreach (var path in Directory.EnumerateFiles(root))
            known.TryAdd(path, 0);

        void Enqueue(string path)
        {
            if (!known.ContainsKey(path) && !IsTemporary(path))
                pending.TryAdd(path, new Candidate());
        }

        using var watcher = new FileSystemWatcher(root)
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
        };
        var errors = new ConcurrentQueue<Exception>();
        watcher.Created += (_, args) => Enqueue(args.FullPath);
        watcher.Renamed += (sender, args) =>
        {
            known.TryRemove(args.OldFullPath, out _);
            pending.TryRemove(args.OldFullPath, out _);
            Enqueue(args.FullPath);
        };
        watcher.Deleted += (sender, args) =>
        {
            known.TryRemove(args.FullPath, out _);
            pending.TryRemove(args.FullPath, out _);
        };
        watcher.Error += (_, args) => errors.Enqueue(args.GetException());
        watcher.EnableRaisingEvents = true;

        // Catch files created between the initial inventory and enabling events.
        foreach (var path in Directory.EnumerateFiles(root))
            Enqueue(path);

        using var timer = new PeriodicTimer(pollInterval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (errors.TryDequeue(out var error))
                throw new IOException("Die Ordnerueberwachung wurde unterbrochen. Bitte den Ordner erneut auswaehlen.", error);

            foreach (var (path, candidate) in pending.ToArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                FileDescriptor? file = null;
                try
                {
                    if (!File.Exists(path))
                    {
                        pending.TryRemove(path, out _);
                        continue;
                    }

                    // Do not follow links out of the selected directory.
                    if ((File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                    {
                        pending.TryRemove(path, out _);
                        continue;
                    }

                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                    var length = stream.Length;
                    var modified = File.GetLastWriteTimeUtc(path);
                    if (candidate.Size != length || candidate.Modified != modified)
                    {
                        candidate.Size = length;
                        candidate.Modified = modified;
                        candidate.StableSince = Stopwatch.GetTimestamp();
                    }
                    else if (Stopwatch.GetElapsedTime(candidate.StableSince) >= stabilityPeriod)
                    {
                        file = new FileDescriptor(path, Path.GetFileNameWithoutExtension(path),
                            Path.GetExtension(path), Path.GetRelativePath(root, path).Replace('\\', '/'),
                            length, new DateTimeOffset(modified));
                    }
                }
                catch (IOException)
                {
                    candidate.StableSince = Stopwatch.GetTimestamp();
                }
                catch (UnauthorizedAccessException)
                {
                    candidate.StableSince = Stopwatch.GetTimestamp();
                }

                if (file is not null &&
                    pending.TryRemove(new KeyValuePair<string, Candidate>(path, candidate)))
                {
                    known.TryAdd(path, 0);
                    yield return file;
                }
            }
        }
    }

    private static bool IsTemporary(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".crdownload", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".part", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".partial", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".tmp", StringComparison.OrdinalIgnoreCase)
            || Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal);
    }

    private sealed class Candidate
    {
        public long Size { get; set; } = -1;
        public DateTime Modified { get; set; }
        public long StableSince { get; set; } = Stopwatch.GetTimestamp();
    }
}
