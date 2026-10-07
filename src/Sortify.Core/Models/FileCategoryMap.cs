namespace Sortify.Core.Models;

public static class FileCategoryMap
{
    private static readonly Dictionary<string, string> Categories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "Documents",
            [".docx"] = "Documents",
            [".odt"] = "Documents",
            [".rtf"] = "Documents",
            [".txt"] = "Documents",

            [".xlsx"] = "Spreadsheets",
            [".ods"] = "Spreadsheets",
            [".csv"] = "Spreadsheets",
            [".tsv"] = "Spreadsheets",

            [".pptx"] = "Presentations",
            [".odp"] = "Presentations",

            [".jpg"] = "Images",
            [".jpeg"] = "Images",
            [".png"] = "Images",
            [".gif"] = "Images",
            [".webp"] = "Images",
            [".bmp"] = "Images",
            [".tif"] = "Images",
            [".tiff"] = "Images",

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