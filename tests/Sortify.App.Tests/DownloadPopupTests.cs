using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Sortify.App.Services;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Infrastructure.Monitoring;

namespace Sortify.App.Tests;

public static class TestApplication
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<Sortify.App.App>()
        .UseSkia()
        .WithInterFont()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public sealed class DownloadPopupTests
{
    [Fact]
    public async Task NewDownloadsOpenRealPopupsInSequenceAndCancellationClosesThem()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        await session.Dispatch(async () =>
        {
            await RunScenarioAsync();
            return true;
        }, CancellationToken.None);
    }

    private static async Task RunScenarioAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "sortify-popup-" + Guid.NewGuid());
        Directory.CreateDirectory(root);
        var target = new TargetFolder(Guid.NewGuid(),
            Path.Combine(Path.GetPathRoot(root)!, "Sortify", "School"), "School");
        var settings = new AppSettings(root, null, false, [])
        {
            TargetFolders = [target], DefaultTargetFolderId = target.Id
        };
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var owner = new Window { Width = 600, Height = 400 };
        owner.Show();
        var workflow = new DownloadNotificationWorkflow(
            new FolderMonitor(TimeSpan.FromMilliseconds(25), TimeSpan.FromMilliseconds(100)),
            new LocalFileSuggestionProvider(), new AvaloniaDownloadPresenter(owner), () => settings);
        var running = workflow.RunAsync(root, timeout.Token);
        try
        {
            var first = Path.Combine(root, "questions.final.DOCX");
            await File.WriteAllTextAsync(first, "unchanged");
            var popup = await WaitForPopupAsync(owner, timeout.Token);
            var model = Assert.IsType<FileSuggestionViewModel>(popup.DataContext);
            Assert.Equal("questions.final.DOCX", model.DetectedFileName);
            Assert.Equal(".DOCX", model.FileExtension);
            Assert.Equal(Path.Combine(target.Path, "questions.final.DOCX"), model.FilePathPreview);

            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            using (var frame = popup.CaptureRenderedFrame())
            {
                Assert.NotNull(frame);
                frame.Save(Path.Combine(AppContext.BaseDirectory, "download-popup.png"),
                    Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
            }

            var second = Path.Combine(root, "picture.png");
            await File.WriteAllTextAsync(second, "second");
            Assert.Single(owner.OwnedWindows);
            popup.Close();
            var nextPopup = await WaitForPopupAsync(owner, timeout.Token);
            var nextModel = Assert.IsType<FileSuggestionViewModel>(nextPopup.DataContext);
            Assert.Equal("picture.png", nextModel.DetectedFileName);
            Assert.Equal(".png", nextModel.FileExtension);
            timeout.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);
            Assert.Empty(owner.OwnedWindows);
            Assert.Equal("unchanged", await File.ReadAllTextAsync(first));
            Assert.Equal("second", await File.ReadAllTextAsync(second));
        }
        finally
        {
            timeout.Cancel();
            try { await running; }
            catch (OperationCanceledException) { }
            owner.Close();
            Directory.Delete(root, true);
        }
    }

    private static async Task<FileSuggestionWindow> WaitForPopupAsync(Window owner, CancellationToken token)
    {
        while (true)
        {
            token.ThrowIfCancellationRequested();
            if (owner.OwnedWindows.OfType<FileSuggestionWindow>().FirstOrDefault() is { } popup)
                return popup;
            await Task.Delay(25, token);
        }
    }
}
