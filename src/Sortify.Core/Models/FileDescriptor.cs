namespace Sortify.Core.Models;

public sealed record FileDescriptor(
    string SourcePath,
    string FileNameWithoutExtension,
    string Extension,
    string RelativePath,
    long Size,
    DateTimeOffset LastWriteTimeUtc);