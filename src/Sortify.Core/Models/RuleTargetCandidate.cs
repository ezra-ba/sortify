namespace Sortify.Core.Models;

public sealed record RuleTargetCandidate(
    string Category,
    string TargetDirectory,
    string FileName,
    IReadOnlyList<RuleReference> Rules)
{
    public IReadOnlyList<RuleMatchReason> Reasons { get; init; } = Array.Empty<RuleMatchReason>();

    public Guid? TargetFolderId { get; init; }
}