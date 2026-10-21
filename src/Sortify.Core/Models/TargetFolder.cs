namespace Sortify.Core.Models;

public sealed record TargetFolder(
    Guid Id,
    string Path,
    string Nickname);