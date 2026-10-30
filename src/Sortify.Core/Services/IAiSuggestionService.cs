using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IAiSuggestionService
{
    Task<AiSuggestionResult> SuggestAsync(AiSuggestionRequest request, CancellationToken cancellationToken = default);
}
