using Sortify.Core.Models;

namespace Sortify.Core.Tests;

public sealed class FileCategoryMapTests
{
    [Theory]
    [InlineData(".pdf", "Documents")]
    [InlineData(".docx", "Documents")]
    [InlineData(".odt", "Documents")]
    [InlineData(".rtf", "Documents")]
    [InlineData(".txt", "Documents")]
    [InlineData(".xlsx", "Spreadsheets")]
    [InlineData(".ods", "Spreadsheets")]
    [InlineData(".csv", "Spreadsheets")]
    [InlineData(".tsv", "Spreadsheets")]
    [InlineData(".pptx", "Presentations")]
    [InlineData(".odp", "Presentations")]
    [InlineData(".jpg", "Images")]
    [InlineData(".jpeg", "Images")]
    [InlineData(".png", "Images")]
    [InlineData(".gif", "Images")]
    [InlineData(".webp", "Images")]
    [InlineData(".bmp", "Images")]
    [InlineData(".tif", "Images")]
    [InlineData(".tiff", "Images")]
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
    [InlineData(".PDF", "Documents")]
    [InlineData(".JpG", "Images")]
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