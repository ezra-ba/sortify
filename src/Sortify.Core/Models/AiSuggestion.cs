namespace Sortify.Core.Models;

public sealed record AiSuggestionRequest(
    FileDescriptor File, AppSettings Settings, RuleEvaluationResult LocalResult);

public sealed record AiFileSuggestion(
    Guid TargetFolderId, string FileName, string Reason, IReadOnlyList<Guid> AppliedRuleIds);

public sealed record AiSuggestionResult(AiFileSuggestion? Suggestion, string Message);
