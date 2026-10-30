using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Persistence;

public sealed class SettingsValidator
{
    private readonly IRuleValidator ruleValidator;

    public SettingsValidator(IRuleValidator ruleValidator)
    {
        this.ruleValidator = ruleValidator;
    }

    public IReadOnlyList<string> Validate(AppSettings settings)
    {
        var errors = new List<string>();

        if (!Enum.IsDefined(settings.AiMode))
            errors.Add("Der KI-Modus ist ungültig.");

        ValidateFolderPath(
            settings.WatchedFolderPath,
            "Eingangsordner",
            errors);

        ValidateFolderPath(
            settings.ArchiveRootPath,
            "Archivordner",
            errors);

        if (settings.TargetFolders is null)
        {
            errors.Add("Die Target-Ordnerliste fehlt.");
        }
        else
        {
            var targetIds = new HashSet<Guid>();

            foreach (var target in settings.TargetFolders)
            {
                if (target is null)
                {
                    errors.Add(
                        "Die Target-Ordnerliste enthält einen leeren Eintrag.");
                    continue;
                }

                if (target.Id == Guid.Empty || !targetIds.Add(target.Id))
                {
                    errors.Add(
                        "Target-Ordner-IDs müssen eindeutig und dürfen nicht leer sein.");
                }

                if (target.Path is null)
                {
                    errors.Add("Der Pfad eines Target-Ordners fehlt.");
                }
                else
                {
                    ValidateFolderPath(
                        target.Path,
                        "Target-Ordner",
                        errors);
                }

                if (string.IsNullOrWhiteSpace(target.Nickname))
                {
                    errors.Add(
                        "Der Spitzname eines Target-Ordners darf nicht leer sein.");
                }
            }

            if (settings.DefaultTargetFolderId is { } defaultId &&
                !targetIds.Contains(defaultId))
            {
                errors.Add(
                    "Das Standardziel muss auf einen vorhandenen Target-Ordner verweisen.");
            }
        }

        if (settings.Rules is null)
        {
            errors.Add("Die Regelliste fehlt.");
            return errors.AsReadOnly();
        }

        var ruleIds = new HashSet<Guid>();

        foreach (var rule in settings.Rules)
        {
            if (rule is null)
            {
                errors.Add("Die Regelliste enthält einen leeren Eintrag.");
                continue;
            }

            if (rule.Id == Guid.Empty || !ruleIds.Add(rule.Id))
            {
                errors.Add(
                    "Regel-IDs müssen eindeutig und dürfen nicht leer sein.");
            }

            // JSON kann trotz nicht-nullbarer C#-Typen null enthalten.
            if (rule.AllowedExtensions is null ||
                rule.TargetDirectory is null)
            {
                errors.Add(
                    $"Bei Regel '{rule.Name}' fehlen erforderliche Felder.");
                continue;
            }

            var result = ruleValidator.Validate(rule);

            foreach (var error in result.Errors)
            {
                errors.Add(
                    $"Regel '{rule.Name}', Feld '{error.FieldName}': " +
                    error.Message);
            }
        }

        return errors.AsReadOnly();
    }

    private static void ValidateFolderPath(
        string? path,
        string description,
        List<string> errors)
    {
        // Nicht eingerichtet ist ein erlaubter Zustand.
        if (path is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(path) ||
            path.Contains('\0') ||
            !Path.IsPathFullyQualified(path))
        {
            errors.Add(
                $"{description}: Ein vollständiger absoluter Pfad ist erforderlich.");
        }
    }
}
