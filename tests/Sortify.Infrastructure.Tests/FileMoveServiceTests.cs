using Sortify.Core.Models;
using Sortify.Infrastructure.FileSystem;

namespace Sortify.Infrastructure.Tests;

public sealed class FileMoveServiceTests : IDisposable
{
    private readonly FileMoveService service = new();

    private readonly string directory = Path.Combine(
        Path.GetTempPath(),
        "SortifyMoveTests",
        Guid.NewGuid().ToString("N"));

    public FileMoveServiceTests()
    {
        Directory.CreateDirectory(directory);
    }

    private async Task<FileMoveRequest> CreateRequestAsync()
    {
        var sourcePath = Path.Combine(directory, "original.txt");
        await File.WriteAllTextAsync(sourcePath, "Quelldatei");

        var source = new FileInfo(sourcePath);

        return new FileMoveRequest(
            sourcePath,
            Path.Combine(directory, "renamed.txt"),
            source.Length,
            new DateTimeOffset(source.LastWriteTimeUtc),
            true);
    }

    [Fact]
    public async Task MoveRenamesAndMovesConfirmedFile()
    {
        var request = await CreateRequestAsync();

        var targetDirectory = Path.Combine(directory, "Target");
        Directory.CreateDirectory(targetDirectory);

        request = request with
        {
            TargetPath = Path.Combine(targetDirectory, "renamed.txt")
        };

        var result = await service.MoveAsync(request);

        Assert.Equal(FileMoveStatus.Succeeded, result.Status);
        Assert.Null(result.ErrorMessage);
        Assert.False(File.Exists(request.SourcePath));
        Assert.Equal(
            "Quelldatei",
            await File.ReadAllTextAsync(request.TargetPath));
    }

    [Fact]
    public async Task MoveWithoutConfirmationChangesNothing()
    {
        var request = await CreateRequestAsync();
        request = request with { IsConfirmed = false };

        var result = await service.MoveAsync(request);

        Assert.Equal(FileMoveStatus.NotConfirmed, result.Status);
        Assert.Equal(
            "Quelldatei",
            await File.ReadAllTextAsync(request.SourcePath));
        Assert.False(File.Exists(request.TargetPath));
    }

    [Fact]
    public async Task MoveDoesNotOverwriteTargetCreatedAfterPreview()
    {
        var request = await CreateRequestAsync();

        // Die Zieldatei entsteht erst nach Erstellung des Auftrags.
        await File.WriteAllTextAsync(
            request.TargetPath,
            "Bestehende Zieldatei");

        var result = await service.MoveAsync(request);

        Assert.Equal(FileMoveStatus.TargetAlreadyExists, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));

        Assert.Equal(
            "Quelldatei",
            await File.ReadAllTextAsync(request.SourcePath));
        Assert.Equal(
            "Bestehende Zieldatei",
            await File.ReadAllTextAsync(request.TargetPath));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MoveRejectsChangedSource(bool changeTimestampOnly)
    {
        var request = await CreateRequestAsync();

        if (changeTimestampOnly)
        {
            File.SetLastWriteTimeUtc(
                request.SourcePath,
                request.ExpectedLastWriteTimeUtc.UtcDateTime.AddMinutes(1));
        }
        else
        {
            await File.AppendAllTextAsync(
                request.SourcePath,
                " – nachträglich verändert");
        }

        var contentBeforeMove =
            await File.ReadAllTextAsync(request.SourcePath);

        var result = await service.MoveAsync(request);

        Assert.Equal(FileMoveStatus.SourceChanged, result.Status);
        Assert.Equal(
            contentBeforeMove,
            await File.ReadAllTextAsync(request.SourcePath));
        Assert.False(File.Exists(request.TargetPath));
    }

    [Fact]
    public async Task MoveRejectsMissingTargetDirectory()
    {
        var request = await CreateRequestAsync();

        request = request with
        {
            TargetPath = Path.Combine(
                directory,
                "Missing",
                "renamed.txt")
        };

        var result = await service.MoveAsync(request);

        Assert.Equal(FileMoveStatus.InvalidRequest, result.Status);
        Assert.Equal(
            "Quelldatei",
            await File.ReadAllTextAsync(request.SourcePath));
        Assert.False(Directory.Exists(
            Path.GetDirectoryName(request.TargetPath)));
    }

    [Fact]
    public async Task MoveReportsMissingSource()
    {
        var request = await CreateRequestAsync();
        File.Delete(request.SourcePath);

        var result = await service.MoveAsync(request);

        Assert.Equal(FileMoveStatus.SourceNotFound, result.Status);
        Assert.False(File.Exists(request.TargetPath));
    }

    [Fact]
    public async Task MoveFailurePreservesSource()
    {
        var request = await CreateRequestAsync();

        // Überlanger einzelner Dateiname provoziert einen Dateisystemfehler.
        request = request with
        {
            TargetPath = Path.Combine(directory, new string('x', 300))
        };

        var result = await service.MoveAsync(request);

        Assert.Equal(FileMoveStatus.Failed, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(
            "Quelldatei",
            await File.ReadAllTextAsync(request.SourcePath));

        Assert.Equal(
            request.SourcePath,
            Assert.Single(Directory.GetFiles(directory)));
    }

    public void Dispose()
    {
        Directory.Delete(directory, recursive: true);
    }
}