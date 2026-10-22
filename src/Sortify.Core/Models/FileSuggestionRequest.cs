namespace Sortify.Core.Models;

// Provider input deliberately contains no absolute paths or document contents.
public sealed record FileSuggestionRequest(
    string FileName,
    string Extension,
    IReadOnlyList<SuggestionTarget> Targets,
    Guid? DefaultTargetFolderId);

public sealed record SuggestionTarget(Guid Id, string Name);
