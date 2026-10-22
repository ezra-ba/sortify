using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Monitoring;

public sealed class LocalFileSuggestionProvider : IFileSuggestionProvider
{
    public Task<FileSuggestionProposal> CreateAsync(
        FileSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var target = request.Targets.FirstOrDefault(t => t.Id == request.DefaultTargetFolderId);
        return Task.FromResult(new FileSuggestionProposal(request.FileName, target?.Id,
            target is null
                ? "Neue Datei erkannt. Bitte ein Ziel auswaehlen."
                : "Neue Datei erkannt. Das konfigurierte Standardziel ist vorausgewaehlt."));
    }
}
