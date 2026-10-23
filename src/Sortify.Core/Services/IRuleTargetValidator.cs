using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IRuleTargetValidator
{
    RuleValidationResult Validate(
        SortRule rule,
        IReadOnlyCollection<TargetFolder> targets);
}