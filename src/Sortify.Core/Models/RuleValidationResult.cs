using System;

namespace Sortify.Core.Models;

public sealed record RuleValidationResult(IReadOnlyList<RuleValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
