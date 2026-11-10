using System.Text;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Ai;

public sealed class AiInstructionsStore
{
    public const int MaxCharacters = 4000;
    private readonly string path;

    public AiInstructionsStore(string path) => this.path = Path.GetFullPath(path);

    public static string GetDefaultPath() => Path.Combine(
        Path.GetDirectoryName(SettingsStoragePath.GetDefaultFilePath())!, "agents.md");

    public string Read()
    {
        if (!File.Exists(path)) return "";
        using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = new char[MaxCharacters + 1];
        var count = reader.ReadBlock(content, 0, content.Length);
        if (count > MaxCharacters) throw new InvalidDataException("Die Sortify-Anweisung ist zu lang.");
        return new string(content, 0, count);
    }

    public async Task SaveAsync(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length > MaxCharacters) throw new ArgumentException("Maximal 4000 Zeichen sind erlaubt.", nameof(text));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, text, new UTF8Encoding(false));
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
