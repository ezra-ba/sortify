using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Threading;
using Sortify.App.ViewModels;
using Sortify.App.Views;
using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.App.Services;

public sealed class AvaloniaDownloadPresenter(Window owner) : IDownloadPresenter
{
    public async Task ShowAsync(
        FileDescriptor file,
        FileSuggestionProposal proposal,
        IReadOnlyList<TargetFolder> targets,
        CancellationToken cancellationToken = default)
    {
        await Dispatcher.UIThread.InvokeAsync(
            () => ShowCoreAsync(file, proposal, targets, cancellationToken));
    }

    private async Task ShowCoreAsync(
        FileDescriptor file,
        FileSuggestionProposal proposal,
        IReadOnlyList<TargetFolder> targets,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cards = new ObservableCollection<TargetFolderViewModel>(
            targets.Select(t => new TargetFolderViewModel(
                t.Id, t.Nickname, t.Path, t.Id == proposal.TargetFolderId)));
        var viewModel = new FileSuggestionViewModel(cards, file, proposal);
        var popup = new FileSuggestionWindow(viewModel);
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        popup.Closed += (_, _) => closed.TrySetResult();
        popup.Show(owner);
        using var registration = cancellationToken.Register(
            () => Dispatcher.UIThread.Post(() => popup.Close()));
        await closed.Task;
        cancellationToken.ThrowIfCancellationRequested();
    }
}
