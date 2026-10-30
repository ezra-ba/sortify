namespace Sortify.Core.Models;

public sealed record AppSettings(
    string? WatchedFolderPath,
    string? ArchiveRootPath,
    bool IncludeSubdirectories,
    IReadOnlyList<SortRule> Rules)
{
    public IReadOnlyList<TargetFolder> TargetFolders { get; init; } =
        Array.Empty<TargetFolder>();

    public Guid? DefaultTargetFolderId { get; init; }

    public bool CloseToTray { get; init; } = true;

    public bool DarkMode { get; init; }

    public bool AutoStart { get; init; }

    public bool StartMinimized { get; init; }

    public AiSuggestionMode AiMode { get; init; }

    public bool AiMetadataConsent { get; init; }

}
