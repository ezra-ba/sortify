using Sortify.Core.Models;

namespace Sortify.App.Services;

public static class RuleExplanationFormatter
{
    public static string Describe(FileNameCondition condition) => condition.Kind switch
    {
        FileNameConditionKind.Contains => $"Name enthält „{condition.Text}“",
        FileNameConditionKind.StartsWith => $"Name beginnt mit „{condition.Text}“",
        FileNameConditionKind.EndsWith => $"Name endet mit „{condition.Text}“",
        FileNameConditionKind.AnyName => "beliebiger Dateiname",
        _ => "ungültige Bedingung"
    };

    public static string Describe(RuleMatchReason reason)
    {
        var name = reason.NameConditions is null ? "Regel: " + reason.RuleName
            : string.Join(" und ", reason.NameConditions.Select(Describe));
        return $"{reason.RuleName}: {reason.Extension.TrimStart('.').ToUpperInvariant()}-Datei; {name}";
    }
}
