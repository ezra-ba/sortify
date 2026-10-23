using System.Collections.ObjectModel;
using Sortify.App.ViewModels;

namespace Sortify.App.Tests;

public sealed class FileSuggestionViewModelTests
{
    [Theory]
    [InlineData("01_questions.final.DOCX", "01_questions.final", ".DOCX")]
    [InlineData("picture.png", "picture", ".png")]
    [InlineData("README", "README", "")]
    public void PopupUsesDetectedFileInsteadOfDemoData(string name, string stem, string extension)
    {
        var id = Guid.NewGuid();
        var target = new TargetFolderViewModel(id, "School", Path.GetTempPath());
        var model = new FileSuggestionViewModel(new ObservableCollection<TargetFolderViewModel> { target },
            Path.Combine(Path.GetTempPath(), name));
        Assert.Equal(name, model.DetectedFileName);
        Assert.Equal(stem, model.NewFileName);
        Assert.Equal(extension, model.FileExtension);
        Assert.Same(target, model.SelectedTargetFolder);
        Assert.Equal(Path.Combine(target.Path, name), model.FilePathPreview);
        Assert.Equal("Neue Datei erkannt.", model.SuggestionExplanation);
        Assert.True(model.IsFileNameValid);
    }

    [Fact]
    public void PopupWithoutTargetDoesNotInventDestination()
    {
        var model = new FileSuggestionViewModel([], Path.Combine(Path.GetTempPath(), "test.pdf"));
        Assert.Null(model.SelectedTargetFolder);
        Assert.Empty(model.FilePathPreview);
        model.NewFileName = "../escape";
        Assert.False(model.IsFileNameValid);
    }

    [Theory]
    [InlineData(" name")]
    [InlineData("name ")]
    public void FileNameWithLeadingOrTrailingSpaceIsInvalid(string name)
    {
        var model = new FileSuggestionViewModel([], Path.Combine(Path.GetTempPath(), "test.pdf"))
        {
            NewFileName = name
        };

        Assert.False(model.IsFileNameValid);
    }
}
