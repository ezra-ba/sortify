using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Tests;

public sealed class SettingsServiceTests
{
    private static AppSettings CreateSettings(bool recursive = false) =>
        new(null, null, recursive, Array.Empty<SortRule>());

    [Fact]
    public async Task SaveBeforeInitializationIsRejected()
    {
        var store = new TestSettingsStore();
        var service = new SettingsService(store);

        var result = await service.SaveAsync(CreateSettings());

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Null(service.Current);
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public async Task InitializeLoadsSettingsOnlyOnce()
    {
        var settings = CreateSettings();
        var store = new TestSettingsStore
        {
            LoadResult = new(
                SettingsLoadStatus.Loaded,
                settings,
                null)
        };
        var service = new SettingsService(store);

        var first = await service.InitializeAsync();
        var second = await service.InitializeAsync();

        Assert.Equal(SettingsLoadStatus.Loaded, first.Status);
        Assert.Same(first, second);
        Assert.Same(settings, service.Current);
        Assert.Equal(1, store.LoadCalls);
    }

    [Fact]
    public async Task MissingFileAllowsFirstSave()
    {
        var store = new TestSettingsStore();
        var service = new SettingsService(store);

        var loaded = await service.InitializeAsync();

        Assert.Equal(SettingsLoadStatus.NotFound, loaded.Status);
        Assert.NotNull(service.Current);
        Assert.Null(service.Current.WatchedFolderPath);
        Assert.Null(service.Current.ArchiveRootPath);
        Assert.Empty(service.Current.Rules);

        var settings = CreateSettings(recursive: true);
        var saved = await service.SaveAsync(settings);

        Assert.True(saved.IsSuccess);
        Assert.Same(settings, service.Current);
        Assert.Same(settings, store.LastSaved);
        Assert.Equal(1, store.SaveCalls);
    }

    [Fact]
    public async Task LoadFailureBlocksSaving()
    {
        var store = new TestSettingsStore
        {
            LoadResult = new(
                SettingsLoadStatus.Failed,
                null,
                "Ungültige Einstellungsdatei.")
        };
        var service = new SettingsService(store);

        var loaded = await service.InitializeAsync();
        var saved = await service.SaveAsync(CreateSettings());

        Assert.Equal(SettingsLoadStatus.Failed, loaded.Status);
        Assert.False(saved.IsSuccess);
        Assert.Null(service.Current);
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public async Task LoadedStatusWithoutSettingsIsRejected()
    {
        var store = new TestSettingsStore
        {
            LoadResult = new(
                SettingsLoadStatus.Loaded,
                null,
                null)
        };
        var service = new SettingsService(store);

        var result = await service.InitializeAsync();

        Assert.Equal(SettingsLoadStatus.Failed, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Null(service.Current);

        var saved = await service.SaveAsync(CreateSettings());

        Assert.False(saved.IsSuccess);
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public async Task FailedSaveKeepsPreviousSettings()
    {
        var previous = CreateSettings();
        var store = new TestSettingsStore
        {
            LoadResult = new(
                SettingsLoadStatus.Loaded,
                previous,
                null),
            SaveResult = new(false, "Speichern fehlgeschlagen.")
        };
        var service = new SettingsService(store);
        await service.InitializeAsync();

        var result = await service.SaveAsync(
            CreateSettings(recursive: true));

        Assert.False(result.IsSuccess);
        Assert.Equal("Speichern fehlgeschlagen.", result.ErrorMessage);
        Assert.Same(previous, service.Current);
        Assert.Equal(1, store.SaveCalls);
    }

    [Fact]
    public async Task SuccessfulSaveUpdatesCurrentSettings()
    {
        var previous = CreateSettings();
        var updated = CreateSettings(recursive: true);

        var store = new TestSettingsStore
        {
            LoadResult = new(
                SettingsLoadStatus.Loaded,
                previous,
                null)
        };
        var service = new SettingsService(store);
        await service.InitializeAsync();

        var result = await service.SaveAsync(updated);

        Assert.True(result.IsSuccess);
        Assert.Same(updated, service.Current);
        Assert.Same(updated, store.LastSaved);
    }

    [Fact]
    public async Task CancelledSaveKeepsPreviousSettings()
    {
        var previous = CreateSettings();
        var store = new TestSettingsStore
        {
            LoadResult = new(
                SettingsLoadStatus.Loaded,
                previous,
                null)
        };
        var service = new SettingsService(store);
        await service.InitializeAsync();

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.SaveAsync(
                CreateSettings(recursive: true),
                cancellation.Token));

        Assert.Same(previous, service.Current);
        Assert.Equal(0, store.SaveCalls);

        // Der Dienst muss nach dem Abbruch weiterhin nutzbar sein.
        var result = await service.SaveAsync(CreateSettings());
        Assert.True(result.IsSuccess);
    }

    private sealed class TestSettingsStore : ISettingsStore
    {
        public SettingsLoadResult LoadResult { get; init; } =
            new(SettingsLoadStatus.NotFound, null, null);

        public SettingsSaveResult SaveResult { get; init; } =
            new(true, null);

        public int LoadCalls { get; private set; }

        public int SaveCalls { get; private set; }

        public AppSettings? LastSaved { get; private set; }

        public Task<SettingsLoadResult> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCalls++;

            return Task.FromResult(LoadResult);
        }

        public Task<SettingsSaveResult> SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveCalls++;
            LastSaved = settings;

            return Task.FromResult(SaveResult);
        }
    }
}