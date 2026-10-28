namespace Sortify.Core.Models;

public enum FileNameConditionKind
{
    Contains,
    StartsWith,
    EndsWith,
    AnyName
}

public sealed record FileNameCondition(FileNameConditionKind Kind, string Text);
