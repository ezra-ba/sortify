namespace Sortify.Core.Models;

public sealed record FileMoveResult(
    FileMoveStatus Status,
    string? ErrorMessage);