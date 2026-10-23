using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Sortify.App.Services;
using Sortify.App.ViewModels;
using Sortify.App.Views;

namespace Sortify.App.Tests;

public static class TestApplication
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<Sortify.App.App>()
        .UseSkia().WithInterFont()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public sealed class DownloadPopupTests
{
    [Fact]
    public async Task NewFileOpensPopupAndFolderChangeClosesIt()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "sortify-popup-" + Guid.NewGuid());
            Directory.CreateDirectory(root);
            var owner = new Window { Width = 600, Height = 400 };
            owner.Show();
            var model = new MainViewModel();
            model.TargetFolders.Add(new TargetFolderViewModel("School",
                Path.Combine(Path.GetPathRoot(root)!, "Sortify", "School"), true));
            using var controller = new DownloadMonitorController(owner, model);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                controller.Restart(root);
                var file = Path.Combine(root, "questions.final.DOCX");
                await File.WriteAllTextAsync(file, "unchanged");
                while (!owner.OwnedWindows.OfType<FileSuggestionWindow>().Any())
                    await Task.Delay(25, timeout.Token);

                var popup = Assert.Single(owner.OwnedWindows.OfType<FileSuggestionWindow>());
                var popupModel = Assert.IsType<FileSuggestionViewModel>(popup.DataContext);
                Assert.Equal("questions.final.DOCX", popupModel.DetectedFileName);
                Assert.Equal(".DOCX", popupModel.FileExtension);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                using (var frame = popup.CaptureRenderedFrame())
                {
                    Assert.NotNull(frame);
                    frame.Save(Path.Combine(AppContext.BaseDirectory, "download-popup.png"),
                        Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                }

                controller.Restart(null);
                Assert.Empty(owner.OwnedWindows);
                Assert.Equal("unchanged", await File.ReadAllTextAsync(file));
            }
            finally
            {
                controller.Dispose();
                owner.Close();
                Directory.Delete(root, true);
            }
            return true;
        }, CancellationToken.None);
    }
}
