namespace Sortify.App.ViewModels;

public sealed record TargetRuleTagViewModel(
    Guid TargetId,
    Guid? RuleId,
    string Name,
    bool IsDuplicate,
    bool IsDisabled,
    string ToolTip)
{
    public bool IsMissing => RuleId is null;
    public string AccessibleName => IsMissing ? "Neue Regel für diesen Target-Ordner erstellen"
        : $"Regel {Name} bearbeiten" + (IsDuplicate ? "; gleiche Bedingungen in mehreren Zielen" : "")
            + (IsDisabled ? "; deaktiviert" : "");

    public static TargetRuleTagViewModel Missing(Guid targetId) => new(
        targetId, null, "Neue Regel erstellen", false, false,
        "Diesem Target-Ordner ist keine Regel zugeordnet. Klicken, um eine Regel anzulegen.");
}
