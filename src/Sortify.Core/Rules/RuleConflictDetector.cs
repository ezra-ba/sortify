using System.Text.Json;
using Sortify.Core.Models;

namespace Sortify.Core.Rules;

public sealed record RuleConflictGroup(IReadOnlyList<Guid> RuleIds, IReadOnlyList<Guid> TargetIds);

/// <summary>Finds identical active recognition definitions, not arbitrary regex overlaps.</summary>
public static class RuleConflictDetector
{
    public static IReadOnlyList<RuleConflictGroup> Find(
        IReadOnlyCollection<SortRule> rules, IReadOnlyCollection<TargetFolder> targets)
    {
        var validator = new RuleValidator();
        var targetValidator = new RuleTargetValidator();
        return rules.Where(rule => rule.IsEnabled && validator.Validate(rule).IsValid &&
                targetValidator.Validate(rule, targets).IsValid)
            .GroupBy(RecognitionKey)
            .Where(group => group.Select(rule => rule.TargetFolderId).Distinct().Count() > 1)
            .Select(group => new RuleConflictGroup(group.Select(rule => rule.Id).ToArray(),
                group.Select(rule => rule.TargetFolderId!.Value).Distinct().ToArray()))
            .ToArray();
    }

    private static string RecognitionKey(SortRule rule)
    {
        // Text is literal in simple conditions. Ordering and duplicate AND clauses do not change recognition.
        var conditions = rule.NameConditions?.Select(condition => condition with
            { Text = condition.Text.ToUpperInvariant() })
            .Distinct().OrderBy(condition => condition.Kind).ThenBy(condition => condition.Text, StringComparer.Ordinal)
            .ToArray();
        return JsonSerializer.Serialize(new
        {
            rule.Priority,
            Extensions = rule.AllowedExtensions.Select(extension => extension.ToUpperInvariant())
                .Distinct().OrderBy(extension => extension, StringComparer.Ordinal).ToArray(),
            rule.RelativePathPattern,
            Conditions = conditions,
            Regex = conditions is null ? rule.FileNamePattern : null
        });
    }
}
