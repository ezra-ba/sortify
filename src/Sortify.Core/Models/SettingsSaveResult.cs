namespace Sortify.Core.Models;

public sealed record SettingsSaveResult(
    bool IsSuccess,
    string? ErrorMessage);