namespace Sortify.Core.Models;

public sealed record RuleEvaluationResult(
    RuleEvaluationStatus Status,
    IReadOnlyList<RuleTargetCandidate> Candidates,
    IReadOnlyList<RuleEvaluationDiagnostic> Diagnostics);