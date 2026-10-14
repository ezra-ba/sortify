using Sortify.Core.Models;

namespace Sortify.Core.Tests;

public sealed class FileCategoryMapTests
{
    [Theory]
    [InlineData(".pdf", "Dokumente")]
    [InlineData(".docx", "Dokumente")]
    [InlineData(".odt", "Dokumente")]
    [InlineData(".rtf", "Dokumente")]
    [InlineData(".txt", "Dokumente")]
    [InlineData(".xlsx", "Tabellen")]
    [InlineData(".ods", "Tabellen")]
    [InlineData(".csv", "Tabellen")]
    [InlineData(".tsv", "Tabellen")]
    [InlineData(".pptx", "Präsentationen")]
    [InlineData(".odp", "Präsentationen")]
    [InlineData(".jpg", "Bilder")]
    [InlineData(".jpeg", "Bilder")]
    [InlineData(".png", "Bilder")]
    [InlineData(".gif", "Bilder")]
    [InlineData(".webp", "Bilder")]
    [InlineData(".bmp", "Bilder")]
    [InlineData(".tif", "Bilder")]
    [InlineData(".tiff", "Bilder")]
    [InlineData(".mp3", "Audio")]
    [InlineData(".wav", "Audio")]
    [InlineData(".flac", "Audio")]
    [InlineData(".aac", "Audio")]
    [InlineData(".ogg", "Audio")]
    [InlineData(".m4a", "Audio")]
    [InlineData(".mp4", "Videos")]
    [InlineData(".mkv", "Videos")]
    [InlineData(".mov", "Videos")]
    [InlineData(".avi", "Videos")]
    [InlineData(".webm", "Videos")]
    [InlineData(".epub", "E-Books")]
    [InlineData(".PDF", "Dokumente")]
    [InlineData(".JpG", "Bilder")]
    public void GetCategoryReturnsExpectedCategory(
        string extension,
        string expectedCategory)
    {
        Assert.Equal(
            expectedCategory,
            FileCategoryMap.GetCategory(extension));
    }

    [Theory]
    [InlineData("")]
    [InlineData(".exe")]
    [InlineData(".zip")]
    [InlineData(".winmd")]
    [InlineData(".sh")]
    [InlineData(".docm")]
    [InlineData(".xlsm")]
    [InlineData(".pptm")]
    [InlineData(".unknown")]
    public void GetCategoryRejectsUnsupportedExtensions(string extension)
    {
        Assert.Null(FileCategoryMap.GetCategory(extension));
    }
}