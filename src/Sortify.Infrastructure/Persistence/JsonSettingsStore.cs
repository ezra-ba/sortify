using System.Text.Json;
using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Persistence;

public sealed class JsonSettingsStore : ISettingsStore
{
    private const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        RespectRequiredConstructorParameters = true
    };

    private readonly string filePath;
    private readonly SettingsValidator validator;

    public JsonSettingsStore(
        string filePath,
        SettingsValidator validator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(validator);

        this.filePath = Path.GetFullPath(filePath);
        this.validator = validator;
    }

    public async Task<SettingsLoadResult> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                options: FileOptions.Asynchronous);

            var document =
                await JsonSerializer.DeserializeAsync<SettingsDocument>(
                    stream,
                    JsonOptions,
                    cancellationToken);

            if (document is null)
            {
                return Failed("Die Einstellungsdatei enthält keine Konfiguration.");
            }

            if (document.Version != CurrentVersion)
            {
                return Failed(
                    $"Die Einstellungsdatei verwendet die nicht unterstützte " +
                    $"Formatversion {document.Version}.");
            }

            if (document.Settings is null)
            {
                return Failed("Der Einstellungsbereich fehlt.");
            }

            var errors = validator.Validate(document.Settings);

            if (errors.Count > 0)
            {
                return Failed(
                    "Die gespeicherten Einstellungen sind ungültig:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, errors));
            }

            return new SettingsLoadResult(
                SettingsLoadStatus.Loaded,
                document.Settings,
                null);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (DirectoryNotFoundException)
        {
            return NotFound();
        }
        catch (JsonException)
        {
            return Failed(
                "Die Einstellungsdatei enthält ungültige oder unvollständige JSON-Daten.");
        }
        catch (UnauthorizedAccessException)
        {
            return Failed(
                "Keine Berechtigung zum Lesen der Einstellungsdatei.");
        }
        catch (IOException)
        {
            return Failed(
                "Die Einstellungsdatei konnte nicht gelesen werden.");
        }
    }

    public async Task<SettingsSaveResult> SaveAsync(
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        var errors = validator.Validate(settings);

        if (errors.Count > 0)
        {
            return new SettingsSaveResult(
                false,
                "Die Einstellungen sind ungültig:" +
                Environment.NewLine +
                string.Join(Environment.NewLine, errors));
        }

        var directory = Path.GetDirectoryName(filePath)!;
        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(directory);

            var document = new SettingsDocument(CurrentVersion, settings);

            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    document,
                    JsonOptions,
                    cancellationToken);

                await stream.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Erst die vollständig geschriebene Datei übernehmen.
            File.Move(temporaryPath, filePath, overwrite: true);

            return new SettingsSaveResult(true, null);
        }
        catch (UnauthorizedAccessException)
        {
            return new SettingsSaveResult(
                false,
                "Keine Berechtigung zum Speichern der Einstellungen.");
        }
        catch (IOException)
        {
            return new SettingsSaveResult(
                false,
                "Die Einstellungen konnten nicht gespeichert werden.");
        }
        finally
        {
            // Aufräumfehler dürfen das eigentliche Ergebnis nicht verdecken.
            try
            {
                File.Delete(temporaryPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static SettingsLoadResult NotFound() => new(
        SettingsLoadStatus.NotFound,
        null,
        null);

    private static SettingsLoadResult Failed(string message) => new(
        SettingsLoadStatus.Failed,
        null,
        message);
}