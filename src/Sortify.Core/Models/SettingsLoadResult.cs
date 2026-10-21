namespace Sortify.Core.Models;

public sealed record SettingsLoadResult(
    SettingsLoadStatus Status,
    AppSettings? Settings,
    string? ErrorMessage);