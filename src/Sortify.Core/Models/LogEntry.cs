namespace Sortify.Core.Models;

public sealed record LogEntry(DateTimeOffset TimestampUtc, LogLevel Level, string EventName, string Message);
