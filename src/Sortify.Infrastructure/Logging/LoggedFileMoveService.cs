using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Logging;

// One instance per suggestion: dismissing it after an already logged attempt must not duplicate the outcome.
public sealed class LoggedFileMoveService(IFileMoveService inner, IActivityLog log) : IFileMoveService
{
    private bool hasRecordedAttempt;
    private bool dismissalRecorded;

    public void RecordDismissal(string sourcePath)
    {
        if (hasRecordedAttempt || dismissalRecorded) return;
        dismissalRecorded = true;
        log.Record(LogLevel.Information, "FileMove.Cancelled", $"Datei „{FileName(sourcePath)}“ nicht verschoben: Vorschlag abgebrochen.");
    }

    public async Task<FileMoveResult> MoveAsync(FileMoveRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        hasRecordedAttempt = true;
        try
        {
            var result = await inner.MoveAsync(request, cancellationToken);
            if (result.Status == FileMoveStatus.Succeeded)
                log.Record(LogLevel.Information, "FileMove.Succeeded",
                    $"Datei „{FileName(request.SourcePath)}“ nach „{TargetDirectory(request.TargetPath)}“ verschoben (Zieldatei: „{FileName(request.TargetPath)}“).");
            else
                log.Record(LogLevel.Warning, "FileMove." + result.Status,
                    $"Datei „{FileName(request.SourcePath)}“ nicht verschoben: {Reason(result.Status)}.");
            return result;
        }
        catch (OperationCanceledException)
        {
            log.Record(LogLevel.Information, "FileMove.Cancelled", $"Datei „{FileName(request.SourcePath)}“ nicht verschoben: Vorgang abgebrochen.");
            throw;
        }
        catch (Exception)
        {
            log.Record(LogLevel.Error, "FileMove.Exception", $"Datei „{FileName(request.SourcePath)}“ nicht verschoben: unerwarteter Fehler.");
            throw;
        }
    }

    public Task<FileSourceSnapshotResult> InspectSourceAsync(string sourcePath, CancellationToken cancellationToken = default)
        => inner.InspectSourceAsync(sourcePath, cancellationToken);

    private static string FileName(string path)
    {
        try { return Display(Path.GetFileName(path)); }
        catch (ArgumentException) { return "unbekannte Datei"; }
    }

    private static string TargetDirectory(string path)
    {
        try { return Display(Path.GetDirectoryName(path) ?? "unbekannter Zielordner"); }
        catch (ArgumentException) { return "unbekannter Zielordner"; }
    }

    private static string Display(string? value)
    {
        var clean = new string((value ?? "unbekannt").Select(character => char.IsControl(character) ? ' ' : character).ToArray());
        return clean.Length > 550 ? clean[..547] + "…" : clean;
    }

    private static string Reason(FileMoveStatus status) => status switch
    {
        FileMoveStatus.NotConfirmed => "nicht bestätigt",
        FileMoveStatus.SourceNotFound => "Quelldatei nicht mehr vorhanden",
        FileMoveStatus.SourceChanged => "Quelldatei wurde verändert",
        FileMoveStatus.TargetAlreadyExists => "Zieldatei existiert bereits",
        FileMoveStatus.InvalidRequest => "Quelle oder Ziel ungültig",
        _ => "Verschieben fehlgeschlagen"
    };
}
