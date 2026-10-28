using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IActivityLog
{
    // Returns false when persistence fails. Does not throw for unavailable storage.
    bool TryWrite(LogLevel level, string eventName, string message);

    // Newest entries first. Reading never creates a directory or file.
    LogReadResult ReadRecent(int limit = 100);
}
