using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Tests;

public sealed class SettingsStoragePathTests
{
    [Fact]
    public void GetDefaultFilePathUsesLocalApplicationData()
    {
        var baseDirectory = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        var result = SettingsStoragePath.GetDefaultFilePath();

        Assert.True(Path.IsPathFullyQualified(result));
        Assert.Equal(
            Path.Combine(baseDirectory, "Sortify", "settings.json"),
            result);
    }
}