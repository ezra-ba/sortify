using System;

namespace Sortify.Core.Rules;

public static class PortableNameValidator
{
    private static readonly HashSet<string> ReservedNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "CONIN$", "CONOUT$",
            "COM1", "COM2", "COM3", "COM4", "COM5",
            "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5",
            "LPT6", "LPT7", "LPT8", "LPT9",
            "COM¹", "COM²", "COM³",
            "LPT¹", "LPT²", "LPT³"
        };

    public static bool IsValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            name is "." or ".." ||
            name.StartsWith(' ') ||
            name.EndsWith('.') ||
            name.EndsWith(' '))
        {
            return false;
        }

        foreach (var character in name)
        {
            if (IsInvalidCharacter(character))
            {
                return false;
            }
        }

        var baseName = name.Split('.')[0].TrimEnd(' ');

        return !ReservedNames.Contains(baseName);
    }

    public static string SanitizeFileStem(string stem)
    {
        var leadingSpaces = stem.Length - stem.TrimStart(' ').Length;
        var end = stem.TrimEnd(' ').Length;
        return string.Create(stem.Length, (stem, leadingSpaces, end), (characters, state) =>
        {
            for (var i = 0; i < characters.Length; i++)
            {
                var character = state.stem[i];
                characters[i] = IsInvalidCharacter(character) || i < state.leadingSpaces || i >= state.end
                    ? '-'
                    : character;
            }
        });
    }

    private static bool IsInvalidCharacter(char character) => character < 32 || "<>:\"/\\|?*".Contains(character);
}
