using Sortify.Core.Models;

namespace Sortify.Core.Services;

public interface IFileMonitor
{
    IAsyncEnumerable<FileDescriptor> WatchAsync(
        string directory,
        CancellationToken cancellationToken = default);
}
