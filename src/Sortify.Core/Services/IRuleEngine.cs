using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IRuleEngine
{
    SortSuggestion? CreateSuggestion(
        string filePath,
        IReadOnlyCollection<SortRule> rules);
}
