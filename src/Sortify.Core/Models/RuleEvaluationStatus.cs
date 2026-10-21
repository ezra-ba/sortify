namespace Sortify.Core.Models;

public enum RuleEvaluationStatus
{
    Ignored,
    Matched,
    Ambiguous,
    CategoryFallback,
    Failed
}
