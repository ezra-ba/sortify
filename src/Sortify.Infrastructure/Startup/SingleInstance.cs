using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace Sortify.Infrastructure.Startup;

public sealed class SingleInstance : IDisposable
{
    private readonly Mutex mutex;
    private readonly string pipeName;
    private readonly CancellationTokenSource cancellation = new();
    private Task? listener;
    private bool disposed;

    public bool IsPrimary { get; }

    public SingleInstance(string? name = null)
    {
        var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user)))[..16];
        pipeName = name ?? $"sortify-{id}";
        mutex = new Mutex(false, pipeName, out var created);
        IsPrimary = created;
    }

    public void Listen(Action showWindow)
    {
        if (!IsPrimary || listener is not null)
            throw new InvalidOperationException("Nur die erste Instanz darf auf Aktivierungen warten.");
        listener = Task.Run(() => ListenAsync(showWindow));
    }

    private async Task ListenAsync(Action showWindow)
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await server.WaitForConnectionAsync(cancellation.Token);
                var message = new byte[1];
                if (await server.ReadAsync(message, cancellation.Token) == 1 && message[0] == 1)
                    showWindow();
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (IOException)
            {
                if (!cancellation.IsCancellationRequested)
                    await Task.Delay(100, cancellation.Token).ConfigureAwait(false);
            }
        }
    }

    public async Task NotifyAsync(bool showWindow)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        await client.ConnectAsync(timeout.Token);
        await client.WriteAsync(new byte[] { showWindow ? (byte)1 : (byte)0 }, timeout.Token);
    }

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        cancellation.Cancel();
        try { listener?.GetAwaiter().GetResult(); }
        catch (OperationCanceledException) { }
        cancellation.Dispose();
        mutex.Dispose();
    }
}
