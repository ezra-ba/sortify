using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Core.Rules;

public sealed class RuleTargetValidator : IRuleTargetValidator
{
    public RuleValidationResult Validate(
        SortRule rule,
        IReadOnlyCollection<TargetFolder> targets)
    {
        var errors = new List<RuleValidationError>();

        if (rule.TargetFolderId is null)
        {
            errors.Add(new(
                nameof(SortRule.TargetFolderId),
                "Der Regel muss ein Target-Ordner zugeordnet werden."));
        }
        else if (rule.TargetFolderId == Guid.Empty)
        {
            errors.Add(new(
                nameof(SortRule.TargetFolderId),
                "Die Target-ID darf nicht leer sein."));
        }
        else if (!targets.Any(target =>
                     target.Id == rule.TargetFolderId))
        {
            errors.Add(new(
                nameof(SortRule.TargetFolderId),
                "Der zugeordnete Target-Ordner ist nicht vorhanden."));
        }

        return new RuleValidationResult(errors.AsReadOnly());
    }
}