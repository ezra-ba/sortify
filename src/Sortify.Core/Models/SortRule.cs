namespace Sortify.Core.Models;

public sealed record SortRule(
    Guid Id,
    string Name,
    string FileNamePattern,
    IReadOnlyCollection<string> AllowedExtensions,
    string? RelativePathPattern,
    string TargetDirectory,
    string? RenamePattern,
    int Priority,
    bool IsEnabled)
{
    public IReadOnlyList<FileNameCondition>? NameConditions { get; init; }

    public Guid? TargetFolderId { get; init; }
}