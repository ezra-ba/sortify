namespace Sortify.Core.Models;

public sealed record SortRule(
    Guid Id,
    string Name,
    string FileNamePattern,
    IReadOnlyCollection<string> AllowedExtensions, //erlaubte File-Endungen
    string? RelativePathPattern, //Quellpfade als Regel
    string TargetDirectory,
    string? RenamePattern,
    int Priority,
    bool IsEnabled);