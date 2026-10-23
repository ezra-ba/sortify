using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IRuleEngine
{
    RuleEvaluationResult Evaluate(
        FileDescriptor file,
        IReadOnlyCollection<SortRule> rules);
}
