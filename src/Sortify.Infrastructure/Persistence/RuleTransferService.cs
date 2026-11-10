using System.Text.Json;
using Sortify.Core.Models;
using Sortify.Core.Rules;

namespace Sortify.Infrastructure.Persistence;

public static class RuleTransferService
{
    private const int FormatVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task ExportAsync(string path, AppSettings settings)
    {
        var targets = settings.TargetFolders
            .Where(target => settings.Rules.Any(rule => rule.TargetFolderId == target.Id))
            .ToArray();
        if (settings.Rules.Any(rule => targets.All(target => target.Id != rule.TargetFolderId)))
            throw new InvalidDataException("Mindestens eine Regel hat keinen gültigen Zielordner.");

        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                await JsonSerializer.SerializeAsync(stream,
                    new RuleTransferDocument(FormatVersion, targets, settings.Rules), JsonOptions);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public static async Task<RuleTransferDocument> ReadAsync(string path)
    {
        if (new FileInfo(path).Length > 1_048_576)
            throw new InvalidDataException("Die Regeldatei ist zu groß.");
        await using var stream = File.OpenRead(path);
        var document = await JsonSerializer.DeserializeAsync<RuleTransferDocument>(stream, JsonOptions);
        if (document is not { Version: FormatVersion, Targets: not null, Rules: not null })
            throw new InvalidDataException("Die Regeldatei hat ein unbekanntes oder unvollständiges Format.");
        if (document.Targets.Any(target => target is null || target.Id == Guid.Empty ||
                string.IsNullOrWhiteSpace(target.Path) || string.IsNullOrWhiteSpace(target.Nickname)) ||
            document.Rules.Any(rule => rule is null || rule.Id == Guid.Empty ||
                rule.TargetFolderId is null || rule.AllowedExtensions is null || rule.TargetDirectory is null) ||
            document.Targets.Select(target => target.Id).Distinct().Count() != document.Targets.Count ||
            document.Rules.Select(rule => rule.Id).Distinct().Count() != document.Rules.Count)
            throw new InvalidDataException("Die Regeldatei enthält ungültige oder doppelte Einträge.");
        return document;
    }

    public static (AppSettings Settings, int Imported) Merge(AppSettings current, RuleTransferDocument document)
    {
        var targets = current.TargetFolders.ToList();
        var targetIds = new Dictionary<Guid, Guid>();
        foreach (var imported in document.Targets)
        {
            string path;
            try { path = Path.GetFullPath(imported.Path); }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
            {
                throw new InvalidDataException($"Ungültiger Zielpfad: {imported.Path}");
            }
            if (!Directory.Exists(path))
                throw new InvalidDataException($"Zielordner fehlt: {path}");
            var existing = targets.FirstOrDefault(target => PathEquals(target.Path, path));
            if (existing is null)
            {
                existing = new TargetFolder(Guid.NewGuid(), path, imported.Nickname);
                targets.Add(existing);
            }
            targetIds.Add(imported.Id, existing.Id);
        }

        var rules = current.Rules.ToList();
        var importedCount = 0;
        foreach (var imported in document.Rules)
        {
            if (imported.TargetFolderId is not { } oldTargetId || !targetIds.TryGetValue(oldTargetId, out var newTargetId))
                throw new InvalidDataException($"Für Regel „{imported.Name}“ fehlt der Zielordner.");
            var mapped = imported with { TargetFolderId = newTargetId,
                AllowedExtensions = imported.AllowedExtensions.ToArray(),
                NameConditions = imported.NameConditions?.ToArray() };
            var existing = rules.FirstOrDefault(rule => rule.Id == mapped.Id);
            if (existing is not null)
            {
                if (JsonSerializer.Serialize(existing) != JsonSerializer.Serialize(mapped))
                    throw new InvalidDataException($"Regel-ID bereits für eine andere Regel vergeben: {mapped.Name}");
                continue;
            }
            rules.Add(mapped);
            importedCount++;
        }

        var result = current with { TargetFolders = targets, Rules = rules };
        var errors = new SettingsValidator(new RuleValidator()).Validate(result).ToList();
        errors.AddRange(rules.SelectMany(rule => new RuleTargetValidator()
            .Validate(rule, targets).Errors.Select(error => error.Message)));
        if (errors.Count > 0) throw new InvalidDataException("Ungültige Regeldatei: " + errors[0]);
        return (result, importedCount);
    }

    private static bool PathEquals(string first, string second) =>
        string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(second),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}

public sealed record RuleTransferDocument(int Version, IReadOnlyList<TargetFolder> Targets,
    IReadOnlyList<SortRule> Rules);
