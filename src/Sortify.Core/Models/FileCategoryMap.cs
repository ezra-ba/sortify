namespace Sortify.Core.Models;

public static class FileCategoryMap
{
    private static readonly Dictionary<string, string> Categories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "Dokumente",
            [".docx"] = "Dokumente",
            [".odt"] = "Dokumente",
            [".rtf"] = "Dokumente",
            [".txt"] = "Dokumente",

            [".xlsx"] = "Tabellen",
            [".ods"] = "Tabellen",
            [".csv"] = "Tabellen",
            [".tsv"] = "Tabellen",

            [".pptx"] = "Präsentationen",
            [".odp"] = "Präsentationen",

            [".jpg"] = "Bilder",
            [".jpeg"] = "Bilder",
            [".png"] = "Bilder",
            [".gif"] = "Bilder",
            [".webp"] = "Bilder",
            [".bmp"] = "Bilder",
            [".tif"] = "Bilder",
            [".tiff"] = "Bilder",

            [".mp3"] = "Audio",
            [".wav"] = "Audio",
            [".flac"] = "Audio",
            [".aac"] = "Audio",
            [".ogg"] = "Audio",
            [".m4a"] = "Audio",

            [".mp4"] = "Videos",
            [".mkv"] = "Videos",
            [".mov"] = "Videos",
            [".avi"] = "Videos",
            [".webm"] = "Videos",

            [".epub"] = "E-Books",
        };

    public static string? GetCategory(string extension)
    {
        return Categories.TryGetValue(extension, out var category)
            ? category
            : null;
    }
}