using System.Diagnostics;
using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.FileSystem;

public sealed class FileMoveService : IFileMoveService
{
    private readonly TimeSpan stabilityWindow;
    private readonly TimeSpan pollInterval;
    private readonly TimeSpan availabilityTimeout;

    public FileMoveService(
        TimeSpan? stabilityWindow = null,
        TimeSpan? pollInterval = null,
        TimeSpan? availabilityTimeout = null)
    {
        this.stabilityWindow = stabilityWindow ?? TimeSpan.FromSeconds(2);
        this.pollInterval = pollInterval ?? TimeSpan.FromMilliseconds(250);
        this.availabilityTimeout = availabilityTimeout ?? TimeSpan.FromSeconds(30);

        if (this.stabilityWindow <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(stabilityWindow));
        if (this.pollInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(pollInterval));
        if (this.availabilityTimeout <= this.stabilityWindow)
            throw new ArgumentOutOfRangeException(nameof(availabilityTimeout));
    }

    public Task<FileSourceSnapshotResult> InspectSourceAsync(
        string sourcePath,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (string.IsNullOrWhiteSpace(sourcePath) ||
                    !Path.IsPathFullyQualified(sourcePath))
                {
                    return new FileSourceSnapshotResult(
                        null, "Der Quellpfad ist ungültig.");
                }

                var fullPath = Path.GetFullPath(sourcePath);
                var totalWait = Stopwatch.StartNew();
                var stableFor = new Stopwatch();
                FileSourceSnapshot? previous = null;

                while (totalWait.Elapsed < availabilityTimeout)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var source = new FileInfo(fullPath);
                        if (!source.Exists)
                        {
                            previous = null;
                            stableFor.Reset();
                        }
                        else
                        {
                            if (HasReparsePoint(fullPath))
                            {
                                return new FileSourceSnapshotResult(
                                    null, "Symbolische Links werden nicht unterstützt.");
                            }

                            FileSourceSnapshot candidate;
                            using (var stream = new FileStream(
                                fullPath, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                            {
                                if (stream.Length > 0)
                                    _ = stream.ReadByte();

                                source.Refresh();
                                candidate = new FileSourceSnapshot(
                                    fullPath,
                                    stream.Length,
                                    new DateTimeOffset(source.LastWriteTimeUtc));
                            }

                            if (candidate != previous)
                            {
                                previous = candidate;
                                stableFor.Restart();
                            }
                            else if (stableFor.Elapsed >= stabilityWindow)
                            {
                                return new FileSourceSnapshotResult(candidate, null);
                            }
                        }
                    }
                    catch (IOException)
                    {
                        // Copying, final renames and temporary sharing locks
                        // can make the file unavailable between samples.
                        previous = null;
                        stableFor.Reset();
                    }

                    var remaining = availabilityTimeout - totalWait.Elapsed;
                    if (remaining <= TimeSpan.Zero)
                        break;

                    await Task.Delay(
                        remaining < pollInterval ? remaining : pollInterval,
                        cancellationToken);
                }

                return new FileSourceSnapshotResult(
                    null,
                    "Die Datei ist noch nicht stabil lesbar. " +
                    "Bitte warten und „Datei erneut prüfen“ wählen.");
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException or
                ArgumentException or
                NotSupportedException)
            {
                return new FileSourceSnapshotResult(
                    null, "Die Quelldatei konnte nicht geprüft werden.");
            }
        }, cancellationToken);
    }

    public Task<FileMoveResult> MoveAsync(
        FileMoveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Task.Run(
            () => Move(request, cancellationToken),
            cancellationToken);
    }

    private static FileMoveResult Move(
        FileMoveRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!request.IsConfirmed)
        {
            return new(
                FileMoveStatus.NotConfirmed,
                "Der Vorgang wurde nicht bestätigt.");
        }

        if (string.IsNullOrWhiteSpace(request.SourcePath) ||
            string.IsNullOrWhiteSpace(request.TargetPath) ||
            !Path.IsPathFullyQualified(request.SourcePath) ||
            !Path.IsPathFullyQualified(request.TargetPath) ||
            request.ExpectedLength < 0)
        {
            return new(
                FileMoveStatus.InvalidRequest,
                "Quelle und Ziel müssen vollständige gültige Pfade sein.");
        }

        string? targetPath = null;

        try
        {
            var sourcePath = Path.GetFullPath(request.SourcePath);
            targetPath = Path.GetFullPath(request.TargetPath);

            // Exakt identische Pfade sind keine Verschiebeoperation.
            if (string.Equals(
                    sourcePath,
                    targetPath,
                    StringComparison.Ordinal))
            {
                return new(
                    FileMoveStatus.InvalidRequest,
                    "Quelle und Ziel sind identisch.");
            }

            var source = new FileInfo(sourcePath);

            if (!source.Exists)
            {
                return new(
                    FileMoveStatus.SourceNotFound,
                    "Die Quelldatei ist nicht mehr verfügbar.");
            }

            var targetDirectory = Path.GetDirectoryName(targetPath);

            if (string.IsNullOrEmpty(targetDirectory) ||
                !Directory.Exists(targetDirectory))
            {
                return new(
                    FileMoveStatus.InvalidRequest,
                    "Der Zielordner ist nicht verfügbar.");
            }

            if (HasReparsePoint(sourcePath) ||
                HasReparsePoint(targetDirectory))
            {
                return new(
                    FileMoveStatus.InvalidRequest,
                    "Symbolische Links und Ordnerverknüpfungen werden " +
                    "für diese Dateioperation nicht unterstützt.");
            }

            if (File.Exists(targetPath) || Directory.Exists(targetPath))
            {
                return new(
                    FileMoveStatus.TargetAlreadyExists,
                    "Das Ziel existiert bereits. Es wurde nichts überschrieben.");
            }

            source.Refresh();

            if (!source.Exists)
            {
                return new(
                    FileMoveStatus.SourceNotFound,
                    "Die Quelldatei ist nicht mehr verfügbar.");
            }

            if (source.Length != request.ExpectedLength ||
                source.LastWriteTimeUtc !=
                    request.ExpectedLastWriteTimeUtc.UtcDateTime)
            {
                return new(
                    FileMoveStatus.SourceChanged,
                    "Die Quelldatei wurde seit der Vorschau verändert. " +
                    "Bitte einen neuen Vorschlag erstellen.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Auch eine zwischenzeitlich entstandene Zieldatei
            // darf nicht überschrieben werden.
            File.Move(sourcePath, targetPath, overwrite: false);

            return new(FileMoveStatus.Succeeded, null);
        }
        catch (FileNotFoundException)
        {
            return new(
                FileMoveStatus.SourceNotFound,
                "Die Quelldatei ist nicht mehr verfügbar.");
        }
        catch (UnauthorizedAccessException)
        {
            return new(
                FileMoveStatus.Failed,
                "Keine Berechtigung zum Verschieben der Datei.");
        }
        catch (IOException)
        {
            if (targetPath is not null &&
                (File.Exists(targetPath) || Directory.Exists(targetPath)))
            {
                return new(
                    FileMoveStatus.TargetAlreadyExists,
                    "Das Ziel existiert bereits. Es wurde nichts überschrieben.");
            }

            return new(
                FileMoveStatus.Failed,
                "Die Datei konnte nicht verschoben werden. " +
                "Bitte Quelle und Ziel prüfen.");
        }
        catch (ArgumentException)
        {
            return new(
                FileMoveStatus.InvalidRequest,
                "Quelle oder Ziel enthält einen ungültigen Pfad.");
        }
        catch (NotSupportedException)
        {
            return new(
                FileMoveStatus.InvalidRequest,
                "Das angegebene Pfadformat wird nicht unterstützt.");
        }
    }

    private static bool HasReparsePoint(string path)
    {
        string? current = path;

        while (!string.IsNullOrEmpty(current))
        {
            if ((File.GetAttributes(current) &
                 FileAttributes.ReparsePoint) != 0)
            {
                return true;
            }

            current = Path.GetDirectoryName(current);
        }

        return false;
    }
}