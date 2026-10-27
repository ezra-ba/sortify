namespace Sortify.Core.Models;

public sealed record FileSourceSnapshot(
    string SourcePath,
    long Length,
    DateTimeOffset LastWriteTimeUtc);