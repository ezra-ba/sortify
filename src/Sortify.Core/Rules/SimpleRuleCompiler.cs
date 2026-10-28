using System.Text;
using System.Text.RegularExpressions;
using Sortify.Core.Models;

namespace Sortify.Core.Rules;

public sealed record SimpleRuleCompilationResult(string? Pattern, IReadOnlyList<RuleValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public static class SimpleRuleCompiler
{
    public static SimpleRuleCompilationResult Compile(IReadOnlyList<FileNameCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        var errors = new List<RuleValidationError>();
        void Error(string message) => errors.Add(new(nameof(SortRule.NameConditions), message));
        if (conditions.Count is < 1 or > 20)
        {
            Error("Eine Regel benötigt zwischen 1 und 20 Namensbedingungen.");
            return new(null, errors);
        }

        var pattern = new StringBuilder(@"\A");
        foreach (var condition in conditions)
        {
            if (condition is null || !Enum.IsDefined(condition.Kind))
            {
                Error("Die Namensbedingung ist ungültig.");
                continue;
            }
            if (condition.Kind == FileNameConditionKind.AnyName)
            {
                if (conditions.Count != 1 || !string.IsNullOrEmpty(condition.Text))
                    Error("„Beliebiger Name“ muss allein und ohne Suchtext verwendet werden.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(condition.Text) || condition.Text.Length > 256)
            {
                Error("Der Suchtext muss 1 bis 256 Zeichen enthalten und darf nicht nur aus Leerzeichen bestehen.");
                continue;
            }
            var text = Regex.Escape(condition.Text);
            pattern.Append(condition.Kind switch
            {
                FileNameConditionKind.Contains => @"(?=[\s\S]*" + text + ")",
                FileNameConditionKind.StartsWith => "(?=" + text + ")",
                FileNameConditionKind.EndsWith => @"(?=[\s\S]*" + text + @"\z)",
                _ => throw new InvalidOperationException()
            });
        }
        pattern.Append(@"[\s\S]*\z");
        if (pattern.Length > 2048) Error("Die kombinierten Namensbedingungen sind zu lang.");
        return new(errors.Count == 0 ? pattern.ToString() : null, errors.AsReadOnly());
    }
}
