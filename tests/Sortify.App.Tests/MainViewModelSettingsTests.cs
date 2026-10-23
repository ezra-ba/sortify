using Sortify.App.ViewModels;
using Sortify.Core.Models;
using Sortify.Core.Services;
using Sortify.Infrastructure.Persistence;

namespace Sortify.App.Tests;

public sealed class MainViewModelSettingsTests
{
    private static TargetFolder CreateTarget() => new(
        Guid.NewGuid(),
        Path.Combine(Path.GetTempPath(), "Sortify-Documents"),
        "Dokumente");

    private static AppSettings CreateSettings(TargetFolder target) =>
        new(null, null, false, Array.Empty<SortRule>())
        {
            TargetFolders = [target],
            DefaultTargetFolderId = target.Id
        };

    [Fact]
    public async Task FailedAddDoesNotChangeCardsAndReleasesBusyState()
    {
        var target = CreateTarget();
        var store = new FailingSettingsStore(CreateSettings(target));
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        await viewModel.AddTargetFolderAsync(
            Path.Combine(Path.GetTempPath(), "Sortify-NewTarget"));

        Assert.Equal(target.Id, Assert.Single(viewModel.TargetFolders).Id);
        Assert.Equal("Test-Speicherfehler.", viewModel.SettingsStatus);
        Assert.False(viewModel.IsSavingSettings);

        Assert.NotNull(service.Current);
        Assert.Equal(target.Id, Assert.Single(service.Current.TargetFolders).Id);
    }

    [Fact]
    public async Task FailedRemoveKeepsCardAndDefault()
    {
        var target = CreateTarget();
        var store = new FailingSettingsStore(CreateSettings(target));
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        var card = Assert.Single(viewModel.TargetFolders);
        viewModel.SelectTargetFolder(card);

        await viewModel.RemoveTargetFolderCommand.ExecuteAsync(null);

        Assert.Same(card, Assert.Single(viewModel.TargetFolders));
        Assert.True(card.IsDefault);
        Assert.Same(card, viewModel.SelectedTargetFolder);
        Assert.Equal("Test-Speicherfehler.", viewModel.SettingsStatus);
        Assert.False(viewModel.IsSavingSettings);

        Assert.NotNull(service.Current);
        Assert.Equal(target.Id, service.Current.DefaultTargetFolderId);
    }

    [Fact]
    public async Task FailedNicknameSaveKeepsSavedNameAndDraft()
    {
        var target = CreateTarget();
        var store = new FailingSettingsStore(CreateSettings(target));
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        var card = Assert.Single(viewModel.TargetFolders);
        card.IsEditingNickname = true;
        card.DraftNickname = "Neuer Name";

        await viewModel.SaveTargetNicknameCommand.ExecuteAsync(card);

        Assert.Equal("Dokumente", card.Nickname);
        Assert.Equal("Neuer Name", card.DraftNickname);
        Assert.True(card.IsEditingNickname);
        Assert.Equal("Test-Speicherfehler.", viewModel.SettingsStatus);
        Assert.False(viewModel.IsSavingSettings);

        Assert.NotNull(service.Current);
        Assert.Equal(
            "Dokumente",
            Assert.Single(service.Current.TargetFolders).Nickname);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedDefaultChangeKeepsPreviousMarking(
        bool selectCurrentDefault)
    {
        var first = CreateTarget();
        var second = new TargetFolder(
            Guid.NewGuid(),
            Path.Combine(Path.GetTempPath(), "Sortify-Second"),
            "Weiteres Ziel");

        var settings = CreateSettings(first) with
        {
            TargetFolders = [first, second]
        };

        var store = new FailingSettingsStore(settings);
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        viewModel.SelectTargetFolder(
            viewModel.TargetFolders[selectCurrentDefault ? 0 : 1]);

        await viewModel.SetDefaultTargetFolderCommand.ExecuteAsync(null);

        Assert.True(viewModel.TargetFolders[0].IsDefault);
        Assert.False(viewModel.TargetFolders[1].IsDefault);
        Assert.Equal("Test-Speicherfehler.", viewModel.SettingsStatus);
        Assert.False(viewModel.IsSavingSettings);

        Assert.NotNull(service.Current);
        Assert.Equal(first.Id, service.Current.DefaultTargetFolderId);
    }

    [Fact]
    public async Task FailedReorderRestoresOrderAndKeepsNicknameDraft()
    {
        var first = CreateTarget();
        var second = new TargetFolder(
            Guid.NewGuid(),
            Path.Combine(Path.GetTempPath(), "Sortify-Second"),
            "Weiteres Ziel");

        var settings = CreateSettings(first) with
        {
            TargetFolders = [first, second]
        };

        var store = new FailingSettingsStore(settings);
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        var firstCard = viewModel.TargetFolders[0];
        var secondCard = viewModel.TargetFolders[1];

        firstCard.IsEditingNickname = true;
        firstCard.DraftNickname = "Noch nicht gespeichert";

        viewModel.MoveTargetFolder(firstCard, secondCard);

        Assert.Same(firstCard, viewModel.TargetFolders[1]);

        await viewModel.SaveTargetFolderOrderAsync();

        Assert.Same(firstCard, viewModel.TargetFolders[0]);
        Assert.Same(secondCard, viewModel.TargetFolders[1]);
        Assert.Equal("Dokumente", firstCard.Nickname);
        Assert.Equal("Noch nicht gespeichert", firstCard.DraftNickname);
        Assert.True(firstCard.IsEditingNickname);
        Assert.True(firstCard.IsDefault);

        Assert.Equal("Test-Speicherfehler.", viewModel.SettingsStatus);
        Assert.False(viewModel.IsSavingSettings);

        Assert.NotNull(service.Current);
        Assert.Equal(
            new[] { first.Id, second.Id },
            service.Current.TargetFolders.Select(target => target.Id).ToArray());
    }

    private sealed class WaitingSettingsStore : ISettingsStore
    {
        private readonly AppSettings settings;

        public WaitingSettingsStore(AppSettings settings)
        {
            this.settings = settings;
        }

        public TaskCompletionSource<bool> SaveStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<SettingsSaveResult> CompleteSave { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int SaveCalls { get; private set; }

        public Task<SettingsLoadResult> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new SettingsLoadResult(
                SettingsLoadStatus.Loaded,
                settings,
                null));
        }

        public async Task<SettingsSaveResult> SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SaveCalls++;
            SaveStarted.TrySetResult(true);

            return await CompleteSave.Task.WaitAsync(cancellationToken);
        }
    }

    private sealed class FailingSettingsStore : ISettingsStore
    {
        private readonly AppSettings settings;

        public FailingSettingsStore(AppSettings settings)
        {
            this.settings = settings;
        }

        public Task<SettingsLoadResult> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new SettingsLoadResult(
                SettingsLoadStatus.Loaded,
                settings,
                null));
        }

        public Task<SettingsSaveResult> SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new SettingsSaveResult(
                false,
                "Test-Speicherfehler."));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BusyStateBlocksAnotherChangeUntilSaveCompletes(
        bool saveSucceeds)
    {
        var target = CreateTarget();
        var store = new WaitingSettingsStore(CreateSettings(target));
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        var pendingSave = viewModel.AddTargetFolderAsync(
            Path.Combine(Path.GetTempPath(), "Sortify-NewTarget"));

        try
        {
            await store.SaveStarted.Task.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.True(viewModel.IsSavingSettings);
            Assert.Equal(target.Id, Assert.Single(viewModel.TargetFolders).Id);

            // Währenddessen darf kein zweiter Speichervorgang starten.
            var blocked = await viewModel.SaveRulesAsync(
                Array.Empty<SortRule>());

            Assert.False(blocked.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(blocked.ErrorMessage));
            Assert.Equal(1, store.SaveCalls);
            Assert.True(viewModel.IsSavingSettings);
        }
        finally
        {
            store.CompleteSave.TrySetResult(new SettingsSaveResult(
                saveSucceeds,
                saveSucceeds ? null : "Test-Speicherfehler."));
        }

        await pendingSave.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(viewModel.IsSavingSettings);
        Assert.Equal(
            saveSucceeds ? 2 : 1,
            viewModel.TargetFolders.Count);

        Assert.NotNull(service.Current);
        Assert.Equal(
            saveSucceeds ? 2 : 1,
            service.Current.TargetFolders.Count);

        if (!saveSucceeds)
        {
            Assert.Equal("Test-Speicherfehler.", viewModel.SettingsStatus);
        }
    }

    [Fact]
    public async Task UnexpectedSaveExceptionReleasesBusyState()
    {
        var target = CreateTarget();
        var store = new WaitingSettingsStore(CreateSettings(target));
        var service = new SettingsService(store);
        var viewModel = new MainViewModel(service);

        await viewModel.InitializeSettingsAsync();

        var pendingSave = viewModel.AddTargetFolderAsync(
            Path.Combine(Path.GetTempPath(), "Sortify-NewTarget"));

        try
        {
            await store.SaveStarted.Task.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.True(viewModel.IsSavingSettings);
        }
        finally
        {
            store.CompleteSave.TrySetException(
                new InvalidOperationException("Absichtlicher Testfehler."));
        }

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => pendingSave.WaitAsync(TimeSpan.FromSeconds(5)));

        Assert.False(viewModel.IsSavingSettings);
        Assert.Equal(target.Id, Assert.Single(viewModel.TargetFolders).Id);

        Assert.NotNull(service.Current);
        Assert.Equal(target.Id, Assert.Single(service.Current.TargetFolders).Id);
    }

}