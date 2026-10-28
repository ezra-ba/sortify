using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.App.Services;

internal sealed class NullActivityLog : IActivityLog
{
    public bool TryWrite(LogLevel level, string eventName, string message) => true;
    public LogReadResult ReadRecent(int limit = 100) => new([], null);
}
