namespace Sortify.Infrastructure.Persistence;

public static class SettingsStoragePath
{
    public static string GetDefaultFilePath()
    {
        var baseDirectory = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(baseDirectory) ||
            !Path.IsPathFullyQualified(baseDirectory))
        {
            throw new InvalidOperationException(
                "Der lokale Benutzerordner für Einstellungen konnte " +
                "nicht ermittelt werden.");
        }

        return Path.Combine(baseDirectory, "Sortify", "settings.json");
    }
}