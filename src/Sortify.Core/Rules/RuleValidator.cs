using Sortify.Core.Models;
using Sortify.Core.Services;
using System.Text.RegularExpressions;

namespace Sortify.Core.Rules;

public sealed class RuleValidator : IRuleValidator
{
    private const int MaxPatternLength = 2048;
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    public RuleValidationResult Validate(SortRule rule)
    {
        var errors = new List<RuleValidationError>();

        if (string.IsNullOrWhiteSpace(rule.Name))
        {
            errors.Add(new(
                nameof(SortRule.Name),
                "Der Regelname darf nicht leer sein."));
        }

        if (rule.AllowedExtensions.Count == 0)
        {
            errors.Add(new(
                nameof(SortRule.AllowedExtensions),
                "Mindestens eine Dateiendung muss angegeben werden."));
        }

        foreach (var extension in rule.AllowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(extension) ||
                !extension.StartsWith('.') ||
                FileCategoryMap.GetCategory(extension) is null)
            {
                errors.Add(new(
                    nameof(SortRule.AllowedExtensions),
                    $"Die Dateiendung '{extension}' ist nicht zugelassen."));
            }
        }

        if (rule.Priority is < 0 or > 1000)
        {
            errors.Add(new(
                nameof(SortRule.Priority),
                "Die Priorität muss zwischen 0 und 1000 liegen."));
        }

        if (rule.NameConditions is not null)
        {
            var compiled = SimpleRuleCompiler.Compile(rule.NameConditions);
            errors.AddRange(compiled.Errors);
            if (compiled.IsValid && !string.Equals(compiled.Pattern, rule.FileNamePattern, StringComparison.Ordinal))
                errors.Add(new(nameof(SortRule.NameConditions),
                    "Namensbedingungen und gespeichertes Suchmuster stimmen nicht überein."));
        }

        if (string.IsNullOrEmpty(rule.FileNamePattern))
        {
            errors.Add(new(
                nameof(SortRule.FileNamePattern),
                "Ein Dateinamenmuster muss angegeben werden."));
        }
        else
        {
            ValidateRegex(
                rule.FileNamePattern,
                nameof(SortRule.FileNamePattern),
                errors);
        }

        if (rule.RelativePathPattern is not null)
        {
            ValidateRegex(
                rule.RelativePathPattern,
                nameof(SortRule.RelativePathPattern),
                errors);
        }

        // Bei Regeln mit Target-ID entscheidet ausschließlich die Zielreferenz.
        // Das alte Feld bleibt zur verlustfreien Übernahme erhalten.
        if (rule.TargetFolderId is null)
        {
            ValidateTargetDirectory(rule.TargetDirectory, errors);
        }
        ValidateRenamePattern(rule.RenamePattern, errors);

        return new RuleValidationResult(errors.AsReadOnly());
    }

    private static void ValidateRenamePattern(
        string? pattern,
        List<RuleValidationError> errors)
    {
        // Kein Schema bedeutet: Originalname behalten.
        if (string.IsNullOrEmpty(pattern))
        {
            return;
        }

        const string extensionPlaceholder = "{ext}";

        var firstExtensionIndex = pattern.IndexOf(
            extensionPlaceholder,
            StringComparison.Ordinal);

        if (!pattern.EndsWith(
                extensionPlaceholder,
                StringComparison.Ordinal) ||
            firstExtensionIndex != pattern.Length - extensionPlaceholder.Length)
        {
            errors.Add(new(
                nameof(SortRule.RenamePattern),
                "Das Schema muss genau einmal '{ext}' am Ende enthalten."));

            return;
        }

        var remainingText = pattern
            .Replace("{name}", "", StringComparison.Ordinal)
            .Replace("{ext}", "", StringComparison.Ordinal);

        if (remainingText.Contains('{') || remainingText.Contains('}'))
        {
            errors.Add(new(
                nameof(SortRule.RenamePattern),
                "Nur die Platzhalter '{name}' und '{ext}' sind erlaubt."));

            return;
        }

        // Beispielwerte prüfen die grundsätzliche Form des Schemas.
        var exampleName = pattern
            .Replace("{name}", "Beispiel", StringComparison.Ordinal)
            .Replace("{ext}", ".pdf", StringComparison.Ordinal);

        if (!PortableNameValidator.IsValid(exampleName) ||
            pattern == extensionPlaceholder)
        {
            errors.Add(new(
                nameof(SortRule.RenamePattern),
                "Das Schema muss einen gültigen Dateinamen mit einem " +
                "Namen vor der Endung erzeugen."));
        }
    }

    private static void ValidateRegex(
        string pattern,
        string fieldName,
        List<RuleValidationError> errors)
    {
        if (pattern.Length > MaxPatternLength)
        {
            errors.Add(new(
                fieldName,
                $"Das Regex-Muster darf maximal {MaxPatternLength} Zeichen enthalten."));

            return;
        }

        try
        {
            _ = new Regex(
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                RegexTimeout);
        }
        catch (ArgumentException)
        {
            errors.Add(new(
                fieldName,
                "Das Regex-Muster ist syntaktisch ungültig."));
        }
    }

    private static void ValidateTargetDirectory(
        string targetDirectory,
        List<RuleValidationError> errors)
    {
        if (targetDirectory.Length == 0)
        {
            return;
        }

        var segments = targetDirectory.Split('/');

        if (segments.Any(segment =>
            !PortableNameValidator.IsValid(segment)))
        {
            errors.Add(new(
                nameof(SortRule.TargetDirectory),
                "Das Ziel muss aus gültigen relativen Ordnernamen bestehen, " +
                "getrennt durch '/'. Absolute Pfade, reservierte Namen " +
                "und ungültige Zeichen sind nicht erlaubt."));
        }
    }

}