using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Persistence;
using Sortify.Infrastructure.Startup;

namespace Sortify.Infrastructure.Tests;

public sealed class StartupTests
{
    [Fact]
    public void LoginCommandQuotesPathAndStartsInBackground()
    {
        var executable = Path.Combine(Path.GetTempPath(), "Sortify App", "Sortify.exe");
        Assert.Equal($"\"{executable}\" --background", WindowsLoginStartup.BuildCommand(executable));
    }

    [Theory]
    [InlineData("Sortify.exe")]
    [InlineData("invalid\"path")]
    public void LoginCommandRejectsInvalidPath(string path) =>
        Assert.Throws<ArgumentException>(() => WindowsLoginStartup.BuildCommand(path));

    [Fact]
    public void LoginCommandRejectsOverlongCommand()
    {
        var path = Path.Combine(Path.GetTempPath(), new string('a', 260), "Sortify.exe");
        Assert.Throws<ArgumentException>(() => WindowsLoginStartup.BuildCommand(path));
    }

    [Fact]
    public void DotnetHostCannotBeRegisteredInsteadOfApplication()
    {
        var service = new WindowsLoginStartup(Path.Combine(Path.GetTempPath(), "dotnet.exe"));
        Assert.False(service.IsSupported);
        Assert.False(service.IsEnabled);
        Assert.Throws<PlatformNotSupportedException>(() => service.SetEnabled(true));
    }

    [Fact]
    public async Task SecondInstanceSignalsFirstInsteadOfOwningAnotherSession()
    {
        var name = "sortify-test-" + Guid.NewGuid().ToString("N");
        using var primary = new SingleInstance(name);
        using var secondary = new SingleInstance(name);
        var activated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.True(primary.IsPrimary);
        Assert.False(secondary.IsPrimary);
        primary.Listen(() => activated.TrySetResult());
        await secondary.NotifyAsync(showWindow: false);
        await Task.Delay(100);
        Assert.False(activated.Task.IsCompleted);
        await secondary.NotifyAsync(showWindow: true);
        await activated.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Throws<InvalidOperationException>(() => secondary.Listen(() => { }));
    }

    [Fact]
    public void ExitingReleasesInstanceName()
    {
        var name = "sortify-test-" + Guid.NewGuid().ToString("N");
        using (var primary = new SingleInstance(name))
        {
            primary.Listen(() => { });
            Assert.True(primary.IsPrimary);
        }
        using var replacement = new SingleInstance(name);
        Assert.True(replacement.IsPrimary);
    }

    [Fact]
    public async Task ClosePreferenceSurvivesReloadAndOldFilesUseDefault()
    {
        var directory = Path.Combine(Path.GetTempPath(), "sortify-startup-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "settings.json");
            var store = new JsonSettingsStore(path, new SettingsValidator(new RuleValidator()));
            var settings = new AppSettings(null, null, false, []) { CloseToTray = false };
            Assert.True((await store.SaveAsync(settings)).IsSuccess);
            Assert.False((await store.LoadAsync()).Settings!.CloseToTray);
            await File.WriteAllTextAsync(path,
                """{"Version":2,"Settings":{"WatchedFolderPath":null,"ArchiveRootPath":null,"IncludeSubdirectories":false,"Rules":[]}}""");
            Assert.True((await store.LoadAsync()).Settings!.CloseToTray);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
