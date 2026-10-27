namespace Sortify.Core.Models;

public sealed record RuleTargetWarning(
    Guid RuleId,
    string RuleName,
    string Message);