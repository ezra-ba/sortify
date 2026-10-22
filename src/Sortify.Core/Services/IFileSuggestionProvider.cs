using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IFileSuggestionProvider
{
    Task<FileSuggestionProposal> CreateAsync(
        FileSuggestionRequest request,
        CancellationToken cancellationToken = default);
}
