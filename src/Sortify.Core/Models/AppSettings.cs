namespace Sortify.Core.Models;

public sealed record AppSettings(
    string? WatchedFolderPath,
    string? ArchiveRootPath,
    bool IncludeSubdirectories,
    IReadOnlyList<SortRule> Rules);