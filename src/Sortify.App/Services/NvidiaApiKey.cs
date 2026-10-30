namespace Sortify.App.Services;

internal static class NvidiaApiKey
{
    public static string? Read()
    {
        var processValue = Environment.GetEnvironmentVariable("NVIDIA_API_KEY");
        if (!string.IsNullOrWhiteSpace(processValue)) return processValue;
        return OperatingSystem.IsWindows()
            ? Environment.GetEnvironmentVariable("NVIDIA_API_KEY", EnvironmentVariableTarget.User)
            : null;
    }
}
