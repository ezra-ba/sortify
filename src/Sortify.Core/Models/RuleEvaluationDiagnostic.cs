namespace Sortify.Core.Models;

public sealed record RuleEvaluationDiagnostic(
    Guid? RuleId,
    string Code,
    string? FieldName,
    string Message);