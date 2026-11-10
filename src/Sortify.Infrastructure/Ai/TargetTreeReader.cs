using Sortify.Core.Models;

namespace Sortify.Infrastructure.Ai;

public sealed record AiTargetTree(Guid TargetId, IReadOnlyList<string> RelativeEntries, bool Truncated);
public sealed record AiContextTree(IReadOnlyList<string> RelativeEntries, bool Truncated);

public static class TargetTreeReader
{
    private const int MaxDepth = 3;
    private const int MaxEntriesPerTarget = 120;
    private const int MaxEntriesTotal = 1200;
    private const int MaxCharactersTotal = 24_000;

    public static AiContextTree CaptureContextDirectory(string rootPath,
        string? fileName = null, CancellationToken cancellationToken = default)
    {
        var entries = new List<string>();
        var truncated = false;
        var inspected = 0;
        var pending = new Queue<(DirectoryInfo Directory, string Relative, int Depth)>();
        pending.Enqueue((new DirectoryInfo(rootPath), "", 0));
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (directory, relative, depth) = pending.Dequeue();
            try
            {
                if (!directory.Exists || directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    truncated = true;
                    continue;
                }
                foreach (var entry in directory.EnumerateFileSystemInfos())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (++inspected > 20_000)
                    {
                        truncated = true;
                        break;
                    }
                    try
                    {
                        if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint)
                            || entry.Attributes.HasFlag(FileAttributes.Hidden)
                            || entry.Attributes.HasFlag(FileAttributes.System))
                            continue;
                        var isDirectory = entry.Attributes.HasFlag(FileAttributes.Directory);
                        var name = relative + entry.Name + (isDirectory ? "/" : "");
                        entries.Add(name);
                        if (isDirectory)
                        {
                            if (depth + 1 < 16)
                                pending.Enqueue((new DirectoryInfo(entry.FullName), name, depth + 1));
                            else
                                truncated = true;
                        }
                    }
                    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                    {
                        truncated = true;
                    }
                }
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                truncated = true;
            }
        }
        var terms = (fileName ?? "").Split(['_', '-', ' ', '.', '(', ')'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(term => term.Length >= 3 && !int.TryParse(term, out _))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var ranked = entries.OrderByDescending(entry => terms.Count(term =>
                entry.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ThenBy(entry => entry.Count(c => c == '/'))
            .ThenBy(entry => entry, StringComparer.OrdinalIgnoreCase);
        var selected = new List<string>();
        var characters = 0;
        foreach (var entry in ranked)
        {
            if (selected.Count >= MaxEntriesTotal) { truncated = true; break; }
            if (characters + entry.Length > MaxCharactersTotal) { truncated = true; continue; }
            selected.Add(entry);
            characters += entry.Length;
        }
        return new AiContextTree(selected, truncated);
    }

    public static IReadOnlyList<AiTargetTree> Capture(IReadOnlyList<TargetFolder> targets,
        CancellationToken cancellationToken = default)
    {
        var trees = new List<AiTargetTree>();
        var totalEntries = 0;
        var totalCharacters = 0;
        foreach (var target in targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entries = new List<string>();
            var truncated = false;
            var pending = new Stack<(DirectoryInfo Directory, string Relative, int Depth)>();
            pending.Push((new DirectoryInfo(target.Path), "", 0));
            while (pending.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (directory, relative, depth) = pending.Pop();
                try
                {
                    if (!directory.Exists || directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        continue;
                    foreach (var entry in directory.EnumerateFileSystemInfos())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (entries.Count >= MaxEntriesPerTarget || totalEntries >= MaxEntriesTotal
                            || totalCharacters >= MaxCharactersTotal)
                        {
                            truncated = true;
                            break;
                        }
                        try
                        {
                            if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint)
                                || entry.Attributes.HasFlag(FileAttributes.Hidden)
                                || entry.Attributes.HasFlag(FileAttributes.System))
                                continue;
                            var isDirectory = entry.Attributes.HasFlag(FileAttributes.Directory);
                            var name = relative + entry.Name + (isDirectory ? "/" : "");
                            if (totalCharacters + name.Length > MaxCharactersTotal)
                            {
                                truncated = true;
                                break;
                            }
                            entries.Add(name);
                            totalEntries++;
                            totalCharacters += name.Length;
                            if (isDirectory)
                            {
                                if (depth + 1 < MaxDepth)
                                    pending.Push((new DirectoryInfo(entry.FullName), name, depth + 1));
                                else
                                    truncated = true;
                            }
                        }
                        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                        {
                            truncated = true;
                        }
                    }
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    truncated = true;
                }
            }
            trees.Add(new AiTargetTree(target.Id, entries, truncated));
        }
        return trees;
    }
}
