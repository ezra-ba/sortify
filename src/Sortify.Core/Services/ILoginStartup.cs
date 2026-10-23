namespace Sortify.Core.Services;

public interface ILoginStartup
{
    bool IsSupported { get; }
    bool IsEnabled { get; }
    void SetEnabled(bool enabled);
}
