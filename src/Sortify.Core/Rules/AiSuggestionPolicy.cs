using Sortify.Core.Models;

namespace Sortify.Core.Rules;

public static class AiSuggestionPolicy
{
    public static bool ShouldRequest(AppSettings settings, RuleEvaluationResult result) =>
        settings.AiMetadataConsent && settings.TargetFolders.Count > 0
        && result.Diagnostics.Count == 0
        && result.Status is not (RuleEvaluationStatus.Ignored or RuleEvaluationStatus.Failed)
        && (settings.AiMode == AiSuggestionMode.Supplement
            || settings.AiMode == AiSuggestionMode.Fallback && !HasLocalMatch(result));

    public static bool HasLocalMatch(RuleEvaluationResult result) =>
        result.Status == RuleEvaluationStatus.Matched && result.Candidates.Count == 1;
}
