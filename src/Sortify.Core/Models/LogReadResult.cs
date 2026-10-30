namespace Sortify.Core.Models;

public sealed record LogReadResult(IReadOnlyList<LogEntry> Entries, string? ErrorMessage, int SkippedEntries = 0)
{
    public bool IsSuccess => ErrorMessage is null;
}
