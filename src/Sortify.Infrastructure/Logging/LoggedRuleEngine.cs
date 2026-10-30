using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Logging;

// Compatibility adapter: rule evaluation no longer belongs to the activity log.
public sealed class LoggedRuleEngine : IRuleEngine
{
    private readonly IRuleEngine inner;
    public LoggedRuleEngine(IRuleEngine inner, IActivityLog log) => this.inner = inner;
    public RuleEvaluationResult Evaluate(FileDescriptor file, IReadOnlyCollection<SortRule> rules, IReadOnlyCollection<TargetFolder> targets)
        => inner.Evaluate(file, rules, targets);
}
