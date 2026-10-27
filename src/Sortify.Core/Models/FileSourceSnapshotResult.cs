namespace Sortify.Core.Models;

public sealed record FileSourceSnapshotResult(
    FileSourceSnapshot? Snapshot,
    string? ErrorMessage);