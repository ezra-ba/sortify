namespace Sortify.Core.Models;

public sealed record RuleTargetCandidate(
    string Category,
    string TargetDirectory,
    string FileName,
    IReadOnlyList<RuleReference> Rules);