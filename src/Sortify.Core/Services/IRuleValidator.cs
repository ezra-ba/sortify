using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IRuleValidator
{
    RuleValidationResult Validate(SortRule rule);
}
