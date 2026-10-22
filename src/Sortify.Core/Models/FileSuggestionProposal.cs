namespace Sortify.Core.Models;

public sealed record FileSuggestionProposal(
    string FileName,
    Guid? TargetFolderId,
    string Explanation);
