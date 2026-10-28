namespace Sortify.Core.Models;

public sealed record RuleMatchReason(
    Guid RuleId,
    string RuleName,
    string Extension,
    IReadOnlyList<FileNameCondition>? NameConditions);
