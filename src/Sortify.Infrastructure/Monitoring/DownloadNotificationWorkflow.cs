using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Monitoring;

public sealed class DownloadNotificationWorkflow(
    IFileMonitor monitor,
    IFileSuggestionProvider provider,
    IDownloadPresenter presenter,
    Func<AppSettings?> getSettings,
    TimeSpan? providerTimeout = null)
{
    public async Task RunAsync(string directory, CancellationToken cancellationToken = default)
    {
        await foreach (var file in monitor.WatchAsync(directory, cancellationToken))
        {
            var settings = getSettings();
            var targets = settings?.TargetFolders.ToArray() ?? [];
            var request = new FileSuggestionRequest(
                Path.GetFileName(file.SourcePath), file.Extension,
                targets.Select(t => new SuggestionTarget(t.Id, t.Nickname)).ToArray(),
                settings?.DefaultTargetFolderId);

            FileSuggestionProposal proposal;
            try
            {
                using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                requestTimeout.CancelAfter(providerTimeout ?? TimeSpan.FromSeconds(10));
                proposal = await provider.CreateAsync(request, requestTimeout.Token)
                    .WaitAsync(requestTimeout.Token);
                if (proposal is null || !PortableNameValidator.IsValid(proposal.FileName)
                    || !string.Equals(Path.GetExtension(proposal.FileName), file.Extension, StringComparison.OrdinalIgnoreCase)
                    || (proposal.TargetFolderId is { } id && !targets.Any(t => t.Id == id)))
                {
                    throw new InvalidOperationException("Ungueltiger Vorschlag.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                proposal = await new LocalFileSuggestionProvider().CreateAsync(request, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            // Closing one popup advances the queue; no file operation is performed.
            await presenter.ShowAsync(file, proposal, targets, cancellationToken);
        }
    }
}
