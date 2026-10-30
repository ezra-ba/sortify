using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Logging;

internal static class ActivityLogExtensions
{
    // Even a replacement logger must never change an application's operation result.
    internal static void Record(this IActivityLog? log, LogLevel level, string name, string message)
    {
        try { log?.TryWrite(level, name, message); }
        catch (Exception) { /* Logging is best effort; never recursively log its own failure. */ }
    }
}
