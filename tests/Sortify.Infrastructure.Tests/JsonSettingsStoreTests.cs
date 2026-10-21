using System.Text.Json;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Tests;

public sealed class JsonSettingsStoreTests : IDisposable
{
    private readonly string directoryPath = Path.Combine(
        Path.GetTempPath(),
        "SortifyTests",
        Guid.NewGuid().ToString("N"));

    private readonly string filePath;

    public JsonSettingsStoreTests()
    {
        Directory.CreateDirectory(directoryPath);
        filePath = Path.Combine(directoryPath, "settings.json");
    }

    private JsonSettingsStore CreateStore() => new(
        filePath,
        new SettingsValidator(new RuleValidator()));

    private AppSettings CreateSettings() => new(
        Path.Combine(directoryPath, "Inbox"),
        Path.Combine(directoryPath, "Archive"),
        true,
        [
            new SortRule(
                Guid.NewGuid(),
                "Rechnungen",
                "^rechnung",
                [".pdf"],
                "^Schule/",
                "Rechnungen",
                "Archiv_{name}{ext}",
                200,
                true)
        ]);

    private Task WriteDocumentAsync(
        AppSettings settings,
        int version = 1)
    {
        var json = JsonSerializer.Serialize(new
        {
            Version = version,
            Settings = settings
        });

        return File.WriteAllTextAsync(filePath, json);
    }

    [Fact]
    public async Task LoadReturnsNotFoundForMissingFile()
    {
        var result = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.NotFound, result.Status);
        Assert.Null(result.Settings);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task LoadReturnsNotFoundForMissingDirectory()
    {
        var store = new JsonSettingsStore(
            Path.Combine(directoryPath, "Missing", "settings.json"),
            new SettingsValidator(new RuleValidator()));

        var result = await store.LoadAsync();

        Assert.Equal(SettingsLoadStatus.NotFound, result.Status);
        Assert.Null(result.Settings);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task LoadPreservesAllSettingsAndRuleFields()
    {
        var expected = CreateSettings();
        await WriteDocumentAsync(expected);

        var result = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.Loaded, result.Status);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.Settings);

        var actual = result.Settings;

        Assert.Equal(expected.WatchedFolderPath, actual.WatchedFolderPath);
        Assert.Equal(expected.ArchiveRootPath, actual.ArchiveRootPath);
        Assert.Equal(
            expected.IncludeSubdirectories,
            actual.IncludeSubdirectories);

        var expectedRule = Assert.Single(expected.Rules);
        var actualRule = Assert.Single(actual.Rules);

        Assert.Equal(expectedRule.Id, actualRule.Id);
        Assert.Equal(expectedRule.Name, actualRule.Name);
        Assert.Equal(expectedRule.FileNamePattern, actualRule.FileNamePattern);
        Assert.Equal(
            expectedRule.AllowedExtensions.ToArray(),
            actualRule.AllowedExtensions.ToArray());
        Assert.Equal(
            expectedRule.RelativePathPattern,
            actualRule.RelativePathPattern);
        Assert.Equal(expectedRule.TargetDirectory, actualRule.TargetDirectory);
        Assert.Equal(expectedRule.RenamePattern, actualRule.RenamePattern);
        Assert.Equal(expectedRule.Priority, actualRule.Priority);
        Assert.Equal(expectedRule.IsEnabled, actualRule.IsEnabled);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"Version\":1,\"Settings\":null}")]
    [InlineData("{\"Version\":1,\"Settings\":{}}")]
    public async Task LoadRejectsInvalidDocumentWithoutChangingFile(
        string json)
    {
        await File.WriteAllTextAsync(filePath, json);

        var result = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.Failed, result.Status);
        Assert.Null(result.Settings);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(json, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task LoadRejectsUnsupportedVersion()
    {
        await WriteDocumentAsync(CreateSettings(), version: 99);

        var result = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.Failed, result.Status);
        Assert.Null(result.Settings);
        Assert.Contains("Formatversion", result.ErrorMessage!);
    }

    [Fact]
    public async Task LoadRejectsInvalidRule()
    {
        var settings = CreateSettings();
        settings = settings with
        {
            Rules = [settings.Rules[0] with { Priority = -1 }]
        };

        await WriteDocumentAsync(settings);

        var result = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.Failed, result.Status);
        Assert.Null(result.Settings);
        Assert.Contains(nameof(SortRule.Priority), result.ErrorMessage!);
    }

    [Fact]
    public async Task LoadPropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateStore().LoadAsync(cancellation.Token));
    }

    public void Dispose()
    {
        Directory.Delete(directoryPath, recursive: true);
    }
    [Fact]
    public async Task SaveCreatesSettingsReadableByNewStore()
    {
        var expected = CreateSettings();

        var saved = await CreateStore().SaveAsync(expected);

        Assert.True(saved.IsSuccess);
        Assert.Null(saved.ErrorMessage);

        var loaded = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.Loaded, loaded.Status);
        Assert.NotNull(loaded.Settings);
        Assert.Null(loaded.ErrorMessage);

        AssertSettingsEqual(expected, loaded.Settings);
    }

    [Fact]
    public async Task SaveRejectsInvalidSettingsWithoutChangingExistingFile()
    {
        var original = CreateSettings();
        var initialSave = await CreateStore().SaveAsync(original);
        Assert.True(initialSave.IsSuccess);

        var originalJson = await File.ReadAllTextAsync(filePath);

        var invalid = original with
        {
            Rules = [original.Rules[0] with { Priority = -1 }]
        };

        var result = await CreateStore().SaveAsync(invalid);

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(originalJson, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task SaveReplacesExistingSettingsAndRemovesTemporaryFile()
    {
        var initialSave = await CreateStore().SaveAsync(CreateSettings());
        Assert.True(initialSave.IsSuccess);

        var updated = CreateSettings() with
        {
            IncludeSubdirectories = false
        };

        var result = await CreateStore().SaveAsync(updated);
        var loaded = await CreateStore().LoadAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(SettingsLoadStatus.Loaded, loaded.Status);
        Assert.NotNull(loaded.Settings);
        Assert.False(loaded.Settings.IncludeSubdirectories);
        Assert.Equal(
            updated.Rules[0].Id,
            Assert.Single(loaded.Settings.Rules).Id);

        Assert.Equal(
            filePath,
            Assert.Single(Directory.GetFiles(directoryPath)));
    }
    [Fact]
    public async Task SaveCreatesMissingDirectory()
    {
        var nestedDirectory = Path.Combine(directoryPath, "New", "Config");
        var nestedFile = Path.Combine(nestedDirectory, "settings.json");

        var store = new JsonSettingsStore(
            nestedFile,
            new SettingsValidator(new RuleValidator()));

        var saved = await store.SaveAsync(CreateSettings());
        var loaded = await store.LoadAsync();

        Assert.True(saved.IsSuccess);
        Assert.True(File.Exists(nestedFile));
        Assert.Equal(SettingsLoadStatus.Loaded, loaded.Status);
        Assert.NotNull(loaded.Settings);
    }

    [Fact]
    public async Task SavePropagatesCancellationWithoutChangingExistingFile()
    {
        var initialSave = await CreateStore().SaveAsync(CreateSettings());
        Assert.True(initialSave.IsSuccess);

        var originalJson = await File.ReadAllTextAsync(filePath);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateStore().SaveAsync(
                CreateSettings(),
                cancellation.Token));

        Assert.Equal(originalJson, await File.ReadAllTextAsync(filePath));
        Assert.Equal(
            filePath,
            Assert.Single(Directory.GetFiles(directoryPath)));
    }

    [Fact]
    public async Task SaveReportsFailureWhenParentPathIsAFile()
    {
        var blockingFile = Path.Combine(directoryPath, "blocked");
        await File.WriteAllTextAsync(blockingFile, "Original");

        var store = new JsonSettingsStore(
            Path.Combine(blockingFile, "settings.json"),
            new SettingsValidator(new RuleValidator()));

        var result = await store.SaveAsync(CreateSettings());

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal("Original", await File.ReadAllTextAsync(blockingFile));
    }

    [Fact]
    public async Task SaveCleansTemporaryFileWhenDestinationIsADirectory()
    {
        var destinationDirectory = Path.Combine(
            directoryPath,
            "settings-target");

        Directory.CreateDirectory(destinationDirectory);

        var markerFile = Path.Combine(destinationDirectory, "keep.txt");
        await File.WriteAllTextAsync(markerFile, "Original");

        var store = new JsonSettingsStore(
            destinationDirectory,
            new SettingsValidator(new RuleValidator()));

        var result = await store.SaveAsync(CreateSettings());

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.True(Directory.Exists(destinationDirectory));
        Assert.Equal("Original", await File.ReadAllTextAsync(markerFile));

        // Die temporäre Datei lag direkt im Testverzeichnis.
        Assert.Empty(Directory.GetFiles(directoryPath));
    }

    [Fact]
    public async Task SaveAndLoadUnconfiguredSettings()
    {
        var settings = new AppSettings(
            null,
            null,
            false,
            Array.Empty<SortRule>());

        var saved = await CreateStore().SaveAsync(settings);
        var loaded = await CreateStore().LoadAsync();

        Assert.True(saved.IsSuccess);
        Assert.Equal(SettingsLoadStatus.Loaded, loaded.Status);
        Assert.NotNull(loaded.Settings);
        Assert.Null(loaded.Settings.WatchedFolderPath);
        Assert.Null(loaded.Settings.ArchiveRootPath);
        Assert.Empty(loaded.Settings.Rules);
    }
    [Fact]
    public async Task NewServiceLoadsPreviouslySavedSettingsAndAllRules()
    {
        var settings = CreateSettings();

        var expected = settings with
        {
            Rules =
            [
                settings.Rules[0],
            settings.Rules[0] with
            {
                Id = Guid.NewGuid(),
                Name = "Deaktivierte Dokumentregel",
                FileNamePattern = ".*",
                AllowedExtensions = [".pdf", ".docx"],
                RelativePathPattern = null,
                TargetDirectory = "",
                RenamePattern = null,
                Priority = 0,
                IsEnabled = false
            }
            ]
        };

        var firstService = new SettingsService(CreateStore());

        var initialLoad = await firstService.InitializeAsync();
        Assert.Equal(SettingsLoadStatus.NotFound, initialLoad.Status);

        var saved = await firstService.SaveAsync(expected);
        Assert.True(saved.IsSuccess);

        // Neue Dienst- und Store-Instanz: kein bisheriger Speicherzustand.
        var secondService = new SettingsService(CreateStore());

        var loaded = await secondService.InitializeAsync();

        Assert.Equal(SettingsLoadStatus.Loaded, loaded.Status);
        Assert.Null(loaded.ErrorMessage);
        Assert.NotNull(secondService.Current);

        AssertSettingsEqual(expected, secondService.Current);
    }
    [Fact]
    public async Task SaveAndLoadPreservesTargetsOrderAndDefault()
    {
        var first = new TargetFolder(
            Guid.NewGuid(),
            Path.Combine(directoryPath, "Documents"),
            "Dokumente");

        var second = new TargetFolder(
            Guid.NewGuid(),
            Path.Combine(directoryPath, "School"),
            "Schule");

        var expected = CreateSettings() with
        {
            TargetFolders = [second, first],
            DefaultTargetFolderId = first.Id
        };

        var saved = await CreateStore().SaveAsync(expected);

        Assert.True(saved.IsSuccess);
        Assert.Null(saved.ErrorMessage);

        using var document = JsonDocument.Parse(
            await File.ReadAllTextAsync(filePath));

        Assert.Equal(
            2,
            document.RootElement.GetProperty("Version").GetInt32());

        var service = new SettingsService(CreateStore());
        var loaded = await service.InitializeAsync();

        Assert.Equal(SettingsLoadStatus.Loaded, loaded.Status);
        Assert.Null(loaded.ErrorMessage);
        Assert.NotNull(service.Current);

        AssertSettingsEqual(expected, service.Current);
    }
    [Fact]
    public async Task LoadSupportsVersionOneWithoutChangingFile()
    {
        var expected = CreateSettings();

        // Tatsächliches altes Format ohne Target-Eigenschaften.
        var json = JsonSerializer.Serialize(new
        {
            Version = 1,
            Settings = new
            {
                expected.WatchedFolderPath,
                expected.ArchiveRootPath,
                expected.IncludeSubdirectories,
                expected.Rules
            }
        });

        await File.WriteAllTextAsync(filePath, json);

        var result = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.Loaded, result.Status);
        Assert.NotNull(result.Settings);
        Assert.Null(result.ErrorMessage);

        AssertSettingsEqual(expected, result.Settings);
        Assert.Empty(result.Settings.TargetFolders);
        Assert.Null(result.Settings.DefaultTargetFolderId);

        // Laden allein migriert die Datei nicht auf der Festplatte.
        Assert.Equal(json, await File.ReadAllTextAsync(filePath));

        var saved = await CreateStore().SaveAsync(result.Settings);

        Assert.True(saved.IsSuccess);

        using var document = JsonDocument.Parse(
            await File.ReadAllTextAsync(filePath));

        Assert.Equal(
            2,
            document.RootElement.GetProperty("Version").GetInt32());

        var reloaded = await CreateStore().LoadAsync();

        Assert.Equal(SettingsLoadStatus.Loaded, reloaded.Status);
        Assert.NotNull(reloaded.Settings);
        AssertSettingsEqual(expected, reloaded.Settings);
    }
    private static void AssertSettingsEqual(
        AppSettings expected,
        AppSettings actual)
    {
        Assert.Equal(expected.WatchedFolderPath, actual.WatchedFolderPath);
        Assert.Equal(expected.ArchiveRootPath, actual.ArchiveRootPath);
        Assert.Equal(
            expected.IncludeSubdirectories,
            actual.IncludeSubdirectories);

        Assert.Equal(expected.Rules.Count, actual.Rules.Count);

        Assert.Equal(expected.DefaultTargetFolderId, actual.DefaultTargetFolderId);

        Assert.Equal(expected.TargetFolders.ToArray(),
        actual.TargetFolders.ToArray());

        for (var index = 0; index < expected.Rules.Count; index++)
        {
            var expectedRule = expected.Rules[index];
            var actualRule = actual.Rules[index];

            Assert.Equal(expectedRule.Id, actualRule.Id);
            Assert.Equal(expectedRule.Name, actualRule.Name);
            Assert.Equal(
                expectedRule.FileNamePattern,
                actualRule.FileNamePattern);
            Assert.Equal(
                expectedRule.AllowedExtensions.ToArray(),
                actualRule.AllowedExtensions.ToArray());
            Assert.Equal(
                expectedRule.RelativePathPattern,
                actualRule.RelativePathPattern);
            Assert.Equal(
                expectedRule.TargetDirectory,
                actualRule.TargetDirectory);
            Assert.Equal(
                expectedRule.RenamePattern,
                actualRule.RenamePattern);
            Assert.Equal(expectedRule.Priority, actualRule.Priority);
            Assert.Equal(expectedRule.IsEnabled, actualRule.IsEnabled);
        }
    }
}