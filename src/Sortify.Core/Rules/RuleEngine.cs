using Sortify.Core.Models;
using Sortify.Core.Services;
using System.Text.RegularExpressions;
using System.Text;

namespace Sortify.Core.Rules;

public sealed class RuleEngine : IRuleEngine
{
    private static string ApplyRenamePattern(
    string pattern,
    FileDescriptor file)
    {
        var result = new StringBuilder();

        for (var index = 0; index < pattern.Length;)
        {
            if (pattern.AsSpan(index).StartsWith(
                    "{name}".AsSpan(),
                    StringComparison.Ordinal))
            {
                result.Append(file.FileNameWithoutExtension);
                index += "{name}".Length;
            }
            else if (pattern.AsSpan(index).StartsWith(
                         "{ext}".AsSpan(),
                         StringComparison.Ordinal))
            {
                result.Append(file.Extension);
                index += "{ext}".Length;
            }
            else
            {
                result.Append(pattern[index]);
                index++;
            }
        }

        return result.ToString();
    }
    private static readonly TimeSpan RegexTimeout =
    TimeSpan.FromMilliseconds(100);

    private static bool Matches(string pattern, string value)
    {
        return Regex.IsMatch(
            value,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            RegexTimeout);
    }
    private readonly IRuleValidator validator;

    public RuleEngine(IRuleValidator validator)
    {
        this.validator = validator;
    }
    public RuleEvaluationResult Evaluate(
        FileDescriptor file,
        IReadOnlyCollection<SortRule> rules)
    {
        var category = FileCategoryMap.GetCategory(file.Extension);

        if (category is null)
        {
            return new RuleEvaluationResult(
                RuleEvaluationStatus.Ignored,
                Array.Empty<RuleTargetCandidate>(),
                Array.Empty<RuleEvaluationDiagnostic>());
        }

        var diagnostics = new List<RuleEvaluationDiagnostic>();
        var validRules = new List<SortRule>();

        foreach (var rule in rules)
        {
            if (!rule.IsEnabled)
            {
                continue;
            }

            var validation = validator.Validate(rule);

            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    diagnostics.Add(new RuleEvaluationDiagnostic(
                        rule.Id,
                        "InvalidRule",
                        error.FieldName,
                        error.Message));
                }

                continue;
            }

            validRules.Add(rule);
        }

        var matchingRules = new List<SortRule>();

        foreach (var rule in validRules)
        {
            if (!rule.AllowedExtensions.Contains(
                    file.Extension,
                    StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var fieldName = nameof(SortRule.FileNamePattern);

            try
            {
                if (!Matches(
                        rule.FileNamePattern,
                        file.FileNameWithoutExtension))
                {
                    continue;
                }

                fieldName = nameof(SortRule.RelativePathPattern);

                if (rule.RelativePathPattern is not null &&
                    !Matches(rule.RelativePathPattern, file.RelativePath))
                {
                    continue;
                }

                matchingRules.Add(rule);
            }
            catch (RegexMatchTimeoutException)
            {
                diagnostics.Add(new RuleEvaluationDiagnostic(
                    rule.Id,
                    "RegexTimeout",
                    fieldName,
                    "Das Zeitlimit für den Regex-Abgleich wurde überschritten."));
            }
        }

        if (diagnostics.Any(diagnostic => diagnostic.Code == "RegexTimeout"))
        {
            return new RuleEvaluationResult(
                RuleEvaluationStatus.Failed,
                Array.Empty<RuleTargetCandidate>(),
                diagnostics.AsReadOnly());
        }

        if (matchingRules.Count > 0)
        {
            var highestPriority = matchingRules.Max(rule => rule.Priority);

            var preferredRules = matchingRules
                .Where(rule => rule.Priority == highestPriority)
                .ToList();
            var originalFileName =
                file.FileNameWithoutExtension + file.Extension;

            var proposedCandidates = new List<RuleTargetCandidate>();

            foreach (var rule in preferredRules)
            {
                var fileName = originalFileName;

                if (!string.IsNullOrEmpty(rule.RenamePattern))
                {
                    fileName = ApplyRenamePattern(rule.RenamePattern, file);
                }

                if (!PortableNameValidator.IsValid(fileName))
                {
                    diagnostics.Add(new RuleEvaluationDiagnostic(
                        rule.Id,
                        "InvalidFileName",
                        nameof(SortRule.RenamePattern),
                        "Der erzeugte Dateiname ist nicht zulässig."));

                    continue;
                }

                proposedCandidates.Add(new RuleTargetCandidate(
                    category,
                    rule.TargetDirectory,
                    fileName,
                    new[] { new RuleReference(rule.Id, rule.Name) }));
            }

            if (diagnostics.Any(diagnostic =>
                diagnostic.Code == "InvalidFileName"))
            {
                return new RuleEvaluationResult(
                    RuleEvaluationStatus.Failed,
                    Array.Empty<RuleTargetCandidate>(),
                    diagnostics.AsReadOnly());
            }

            var candidates = proposedCandidates
                .GroupBy(candidate => (
                    candidate.TargetDirectory,
                    candidate.FileName))
                .Select(group => new RuleTargetCandidate(
                    category,
                    group.Key.TargetDirectory,
                    group.Key.FileName,
                    group.SelectMany(candidate => candidate.Rules).ToArray()))
                .ToArray();

            return new RuleEvaluationResult(
                candidates.Length == 1
                    ? RuleEvaluationStatus.Matched
                    : RuleEvaluationStatus.Ambiguous,
                candidates,
                diagnostics.AsReadOnly());
        }

        var fallbackFileName =
            file.FileNameWithoutExtension + file.Extension;

        if (!PortableNameValidator.IsValid(fallbackFileName))
        {
            diagnostics.Add(new RuleEvaluationDiagnostic(
                null,
                "InvalidFileName",
                null,
                "Der ursprüngliche Dateiname ist für das Archiv nicht zulässig."));

            return new RuleEvaluationResult(
                RuleEvaluationStatus.Failed,
                Array.Empty<RuleTargetCandidate>(),
                diagnostics.AsReadOnly());
        }

        var candidate = new RuleTargetCandidate(
            category,
            "",
            fallbackFileName,
            Array.Empty<RuleReference>());

        return new RuleEvaluationResult(
            RuleEvaluationStatus.CategoryFallback,
            new[] { candidate },
            diagnostics.AsReadOnly());
    }
}