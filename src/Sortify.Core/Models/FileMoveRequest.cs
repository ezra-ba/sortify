namespace Sortify.Core.Models;

public sealed record FileMoveRequest(
    string SourcePath,
    string TargetPath,
    long ExpectedLength,
    DateTimeOffset ExpectedLastWriteTimeUtc,
    bool IsConfirmed);