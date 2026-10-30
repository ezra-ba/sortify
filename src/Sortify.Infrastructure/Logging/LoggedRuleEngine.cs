using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Logging;

public sealed class LoggedRuleEngine(IRuleEngine inner, IActivityLog log) : IRuleEngine
{
    public RuleEvaluationResult Evaluate(FileDescriptor file, IReadOnlyCollection<SortRule> rules,
        IReadOnlyCollection<TargetFolder> targets)
    {
        try
        {
            var result = inner.Evaluate(file, rules, targets);
            var level = result.Status switch
            {
                RuleEvaluationStatus.Failed => LogLevel.Error,
                RuleEvaluationStatus.Ambiguous => LogLevel.Warning,
                _ => LogLevel.Information
            };
            log.Record(level, "Rules." + result.Status, "Regelauswertung abgeschlossen: " + result.Status);
            return result;
        }
        catch (Exception)
        {
            log.Record(LogLevel.Error, "Rules.Exception", "Unerwarteter Fehler bei der Regelauswertung.");
            throw;
        }
    }
}
