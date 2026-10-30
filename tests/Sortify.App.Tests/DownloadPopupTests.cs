using Sortify.Infrastructure.Logging;
using Sortify.Infrastructure.FileSystem;
using Sortify.Infrastructure.Monitoring;
using Sortify.Core.Rules;
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

[Collection("Avalonia")]
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
            var log = new JsonActivityLog(Path.Combine(root, "logs"));
            using var controller = new DownloadMonitorController(model, new FileMoveService(),
                new RuleEngine(new RuleValidator(), new RuleTargetValidator()), new FolderMonitor(log), log);
            FileSuggestionWindow? openedPopup = null;
            using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
            {
                if (window is FileSuggestionWindow popup)
                    openedPopup = popup;
            });
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                controller.Restart(root);
                owner.WindowState = WindowState.Minimized;
                var file = Path.Combine(root, "questions.final.DOCX");
                await File.WriteAllTextAsync(file, "unchanged");
                while (openedPopup is null)
                    await Task.Delay(25, timeout.Token);

                var popup = openedPopup;
                Assert.Null(popup.Owner);
                Assert.True(popup.IsVisible);
                Assert.True(popup.Topmost);
                Assert.Empty(owner.OwnedWindows);
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

                Assert.Empty(log.ReadRecent().Entries);
                popup.Close();
                Assert.False(popup.IsVisible);
                var entry = Assert.Single(log.ReadRecent().Entries);
                Assert.Equal("FileMove.Cancelled", entry.EventName);
                Assert.Contains("questions.final.DOCX", entry.Message);
                controller.Restart(null);
                Assert.Single(log.ReadRecent().Entries);
                Assert.False(popup.IsVisible);
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
