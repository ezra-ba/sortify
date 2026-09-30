namespace Sortify.Core.Models;

public sealed record SortRule(
    Guid Id,
    string Name,
    string FileNamePattern,
    string TargetDirectory,
    string? RenamePattern,
    int Priority,
    bool IsEnabled);
