using System.Collections.ObjectModel;
using Sortify.App.ViewModels;
using Sortify.Core.Models;

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
        var file = new FileDescriptor(Path.Combine(Path.GetTempPath(), name), stem, extension, name, 1, DateTimeOffset.UtcNow);
        var model = new FileSuggestionViewModel(new ObservableCollection<TargetFolderViewModel> { target },
            file, new FileSuggestionProposal(name, id, "Detected file"));
        Assert.Equal(name, model.DetectedFileName);
        Assert.Equal(stem, model.NewFileName);
        Assert.Equal(extension, model.FileExtension);
        Assert.Same(target, model.SelectedTargetFolder);
        Assert.Equal(Path.Combine(target.Path, name), model.FilePathPreview);
        Assert.Equal("Detected file", model.SuggestionExplanation);
        Assert.True(model.IsFileNameValid);
    }

    [Fact]
    public void PopupWithoutTargetDoesNotInventDestination()
    {
        var file = new FileDescriptor("/incoming/test.pdf", "test", ".pdf", "test.pdf", 1, DateTimeOffset.UtcNow);
        var model = new FileSuggestionViewModel([], file,
            new FileSuggestionProposal("test.pdf", null, "Choose target"));
        Assert.Null(model.SelectedTargetFolder);
        Assert.Empty(model.FilePathPreview);
        model.NewFileName = "../escape";
        Assert.False(model.IsFileNameValid);
    }
}
