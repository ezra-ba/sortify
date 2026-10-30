using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Logging;

public sealed class LoggedFileMoveService(IFileMoveService inner, IActivityLog log) : IFileMoveService
{
    public async Task<FileMoveResult> MoveAsync(FileMoveRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await inner.MoveAsync(request, cancellationToken);
            log.Record(result.Status == FileMoveStatus.Succeeded ? LogLevel.Information : LogLevel.Warning,
                "FileMove." + result.Status, "Dateivorgang abgeschlossen: " + result.Status);
            return result;
        }
        catch (OperationCanceledException)
        {
            log.Record(LogLevel.Information, "FileMove.Cancelled", "Dateivorgang abgebrochen.");
            throw;
        }
        catch (Exception)
        {
            log.Record(LogLevel.Error, "FileMove.Exception", "Unerwarteter Fehler beim Dateivorgang.");
            throw;
        }
    }

    public async Task<FileSourceSnapshotResult> InspectSourceAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await inner.InspectSourceAsync(sourcePath, cancellationToken);
            log.Record(result.Snapshot is null ? LogLevel.Warning : LogLevel.Information,
                result.Snapshot is null ? "FileSource.Unavailable" : "FileSource.Ready",
                result.Snapshot is null ? "Datei noch nicht bereit zum Einsortieren." : "Datei bereit zum Einsortieren.");
            return result;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception)
        {
            log.Record(LogLevel.Error, "FileSource.Exception", "Unerwarteter Fehler bei der Dateiprüfung.");
            throw;
        }
    }
}
