using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IFileMoveService
{
    Task<FileMoveResult> MoveAsync(
        FileMoveRequest request,
        CancellationToken cancellationToken = default);
    Task<FileSourceSnapshotResult> InspectSourceAsync(
        string sourcePath,
        CancellationToken cancellationToken = default);
}