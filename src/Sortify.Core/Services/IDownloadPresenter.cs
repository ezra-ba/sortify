using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IDownloadPresenter
{
    Task ShowAsync(
        FileDescriptor file,
        FileSuggestionProposal proposal,
        IReadOnlyList<TargetFolder> targets,
        CancellationToken cancellationToken = default);
}
