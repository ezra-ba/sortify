namespace Sortify.Core.Models;

public sealed record SortSuggestion(
    string SourcePath,
    string TargetPath,
    string RuleName);
