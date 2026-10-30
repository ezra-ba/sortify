using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Sortify.Core.Rules;
using Sortify.App.Services;
using Sortify.Core.Models;
using Sortify.Core.Services;

namespace Sortify.App.ViewModels;

public partial class FileSuggestionViewModel : ViewModelBase
{
    private readonly IFileMoveService? fileMoveService;
    private readonly IRuleEngine? ruleEngine;
    private readonly Func<AppSettings?>? settingsProvider;
    private readonly IAiSuggestionService? aiService;
    private AiSuggestionRequest? aiRequest;
    private CancellationTokenSource? aiCancellation;
    private int editRevision;
    private int aiRevision;
    private bool suggestionEvaluated;
    private FileSourceSnapshot? sourceSnapshot;
    private CancellationTokenSource? inspectionCancellation;

    [ObservableProperty]
    public partial bool IsAiBusy { get; set; }

    [ObservableProperty]
    public partial string AiStatus { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAiAlternative))]
    [NotifyPropertyChangedFor(nameof(AiAlternativeSummary))]
    public partial AiFileSuggestion? AiAlternative { get; set; }

    public bool HasAiAlternative => AiAlternative is not null && CanEdit;

    public string AiAlternativeSummary => AiAlternative is { } suggestion
        ? $"{suggestion.FileName} → {TargetFolders.FirstOrDefault(folder => folder.Id == suggestion.TargetFolderId)?.Nickname}"
        : "";

    partial void OnSelectedTargetFolderChanged(TargetFolderViewModel? value) => editRevision++;
    partial void OnNewFileNameChanged(string value) => editRevision++;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirmMove))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(CanRefreshSource))]
    [NotifyPropertyChangedFor(nameof(HasAiAlternative))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirmMove))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanRefreshSource))]
    [NotifyPropertyChangedFor(nameof(HasAiAlternative))]
    public partial bool IsMoveCompleted { get; set; }

    [ObservableProperty]
    public partial string OperationMessage { get; set; } = "";

    [ObservableProperty]
    public partial string OperationMessageColor { get; set; } = "#6E6E73";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    public partial bool IsPreparingSource { get; set; }

    public bool CanEdit => !IsBusy && !IsMoveCompleted;

    public bool CanCancel => !IsBusy || IsPreparingSource;

    public bool CanRefreshSource => CanEdit &&
        sourceSnapshot is null && fileMoveService is not null && SourcePath is not null;

    public bool CanConfirmMove =>
        CanEdit &&
        sourceSnapshot is not null &&
        IsFileNameValid &&
        SelectedTargetFolder is not null &&
        TargetFolders.Contains(SelectedTargetFolder);
    public ObservableCollection<TargetFolderViewModel> TargetFolders { get; }

    public FileSuggestionViewModel()
        : this([])
    {
    }

    public FileSuggestionViewModel(ObservableCollection<TargetFolderViewModel> targetFolders)
    {
        TargetFolders = targetFolders;
        SelectedTargetFolder = TargetFolders.FirstOrDefault(folder => folder.IsDefault)
            ?? TargetFolders.FirstOrDefault();
    }

    public FileSuggestionViewModel(
        ObservableCollection<TargetFolderViewModel> targetFolders,
        string path,
        IFileMoveService? fileMoveService = null,
        IRuleEngine? ruleEngine = null,
        Func<AppSettings?>? settingsProvider = null,
        IAiSuggestionService? aiService = null) : this(targetFolders)
    {
        this.fileMoveService = fileMoveService;
        this.ruleEngine = ruleEngine;
        this.settingsProvider = settingsProvider;
        this.aiService = aiService;
        SourcePath = path;
        DetectedFileName = Path.GetFileName(path);
        FileExtension = Path.GetExtension(path);
        NewFileName = Path.GetFileNameWithoutExtension(path);
        SuggestionExplanation = "Neue Datei erkannt.";
    }

    public string? SourcePath { get; }

    [ObservableProperty]
    public partial string SuggestionExplanation { get; set; } = "Manuelle Popup-Vorschau";

    [ObservableProperty]
    public partial string DetectedFileName { get; set; } = "rechnung_2026-10-07.pdf";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilePathPreview))]
    [NotifyPropertyChangedFor(nameof(CanConfirmMove))]
    public partial TargetFolderViewModel? SelectedTargetFolder { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilePathPreview))]
    [NotifyPropertyChangedFor(nameof(IsFileNameValid))]
    [NotifyPropertyChangedFor(nameof(IsValidationVisible))]
    [NotifyPropertyChangedFor(nameof(ValidationMessage))]
    [NotifyPropertyChangedFor(nameof(CanConfirmMove))]
    public partial string NewFileName { get; set; } = "Rechnung_2026-10-07";

    public string FileExtension { get; } = ".pdf";

    [ObservableProperty]
    public partial bool HasValidationError { get; set; }

    public bool IsFileNameValid => PortableNameValidator.IsValid(NewFileName + FileExtension)
        && NewFileName.Length > 0
        && !NewFileName.StartsWith(' ')
        && !NewFileName.EndsWith(' ')
        && NewFileName.Length + FileExtension.Length <= 255;

    public bool IsValidationVisible => !IsFileNameValid;

    public string ValidationMessage => IsFileNameValid
        ? string.Empty
        : "Ungültiger Dateiname";

    public string FilePathPreview => SelectedTargetFolder is null || string.IsNullOrWhiteSpace(NewFileName)
        ? string.Empty
        : Path.Combine(SelectedTargetFolder.Path, NewFileName + FileExtension);
    public async Task InitializeFileOperationAsync()
    {
        if (IsBusy || IsMoveCompleted || sourceSnapshot is not null)
            return;

        if (fileMoveService is null || SourcePath is null)
        {
            OperationMessage = "Manuelle Vorschau ohne Dateioperation.";
            return;
        }

        using var cancellation = new CancellationTokenSource();
        inspectionCancellation = cancellation;
        IsPreparingSource = true;
        IsBusy = true;
        OperationMessage = "Warte, bis die Datei stabil lesbar ist …";
        OperationMessageColor = "#6E6E73";

        try
        {
            var result = await fileMoveService.InspectSourceAsync(
                SourcePath, cancellation.Token);

            sourceSnapshot = result.Snapshot;
            if (sourceSnapshot is not null && !suggestionEvaluated && ruleEngine is not null)
            {
                ApplyRuleSuggestion(sourceSnapshot);
                suggestionEvaluated = true;
            }
            OperationMessage = result.ErrorMessage ?? "Datei bereit zum Einsortieren.";
            OperationMessageColor = result.Snapshot is not null
                ? "#15803D"
                : "#B42318";
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            OperationMessage = "Dateiprüfung abgebrochen.";
            OperationMessageColor = "#6E6E73";
        }
        finally
        {
            inspectionCancellation = null;
            IsPreparingSource = false;
            IsBusy = false;
            OnPropertyChanged(nameof(CanConfirmMove));
            OnPropertyChanged(nameof(CanRefreshSource));
        }

        if (aiRequest is not null && !cancellation.IsCancellationRequested)
            await LoadAiSuggestionAsync(aiRequest);
    }

    private void ApplyRuleSuggestion(FileSourceSnapshot snapshot)
    {
        var settings = settingsProvider?.Invoke();
        SelectedTargetFolder = null;
        if (settings is null)
        {
            SuggestionExplanation = "Keine Regeln verfügbar. Bitte einen Zielordner auswählen.";
            return;
        }

        var relativePath = string.IsNullOrWhiteSpace(settings.WatchedFolderPath)
            ? Path.GetFileName(snapshot.SourcePath)
            : Path.GetRelativePath(settings.WatchedFolderPath, snapshot.SourcePath);
        var file = new FileDescriptor(
            snapshot.SourcePath,
            Path.GetFileNameWithoutExtension(snapshot.SourcePath),
            Path.GetExtension(snapshot.SourcePath),
            relativePath,
            snapshot.Length,
            snapshot.LastWriteTimeUtc);
        var result = ruleEngine!.Evaluate(file, settings.Rules, settings.TargetFolders);
        aiRequest = new AiSuggestionRequest(file, settings, result);
        if (result.Status == RuleEvaluationStatus.Matched && result.Candidates.Count == 1)
        {
            var candidate = result.Candidates[0];
            var target = TargetFolders.FirstOrDefault(folder => folder.Id == candidate.TargetFolderId);
            // The popup currently keeps the source extension fixed.
            if (target is not null && PortableNameValidator.IsValid(candidate.FileName) &&
                string.Equals(Path.GetExtension(candidate.FileName), FileExtension, StringComparison.Ordinal))
            {
                SelectedTargetFolder = target;
                NewFileName = Path.GetFileNameWithoutExtension(candidate.FileName);
                SuggestionExplanation = candidate.Reasons.Count > 0
                    ? string.Join(Environment.NewLine, candidate.Reasons.Select(RuleExplanationFormatter.Describe))
                    : "Passende Regel: " + string.Join(", ", candidate.Rules.Select(rule => rule.Name));
                return;
            }
        }

        if (result.Diagnostics.Count > 0 ||
            result.Status is RuleEvaluationStatus.Ignored or RuleEvaluationStatus.Failed)
        {
            SuggestionExplanation = "Kein automatischer Vorschlag möglich. Bitte Ziel und Dateinamen manuell prüfen.";
            return;
        }

        SelectedTargetFolder = TargetFolders.FirstOrDefault(folder =>
            folder.Id == settings.DefaultTargetFolderId);
        var reason = result.Status == RuleEvaluationStatus.Ambiguous
            ? "Mehrere Regelvorschläge möglich."
            : "Keine eindeutige passende Regel gefunden.";
        SuggestionExplanation = reason + (SelectedTargetFolder is null
            ? " Bitte einen Zielordner auswählen."
            : " Der Standardordner wird vorgeschlagen.");
    }

    public void CancelSourceInspection()
    {
        inspectionCancellation?.Cancel();
        CancelAiSuggestion();
    }

    public void CancelAiSuggestion()
    {
        aiRevision++;
        aiCancellation?.Cancel();
        AiAlternative = null;
        AiStatus = "";
        IsAiBusy = false;
    }

    private async Task LoadAiSuggestionAsync(AiSuggestionRequest request)
    {
        if (aiService is null || !AiSuggestionPolicy.ShouldRequest(request.Settings, request.LocalResult))
            return;
        var revision = ++aiRevision;
        var originalEditRevision = editRevision;
        using var cancellation = new CancellationTokenSource();
        aiCancellation = cancellation;
        IsAiBusy = true;
        AiStatus = "KI-Vorschlag wird angefragt …";
        try
        {
            var result = await aiService.SuggestAsync(request, cancellation.Token);
            var current = settingsProvider?.Invoke();
            if (cancellation.IsCancellationRequested || revision != aiRevision || IsMoveCompleted || current is null
                || current.AiMode != request.Settings.AiMode || !current.AiMetadataConsent
                || !ReferenceEquals(current.Rules, request.Settings.Rules)
                || !ReferenceEquals(current.TargetFolders, request.Settings.TargetFolders)
                || current.DefaultTargetFolderId != request.Settings.DefaultTargetFolderId)
                return;
            AiStatus = result.Message;
            if (result.Suggestion is not { } suggestion
                || !TargetFolders.Any(folder => folder.Id == suggestion.TargetFolderId))
                return;
            AiAlternative = suggestion;
            if (!AiSuggestionPolicy.HasLocalMatch(request.LocalResult) && originalEditRevision == editRevision)
                AdoptAiSuggestion();
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            if (revision == aiRevision)
                AiStatus = "Der KI-Vorschlag ist nicht verfügbar. Der lokale Vorschlag bleibt erhalten.";
        }
        finally
        {
            if (ReferenceEquals(aiCancellation, cancellation))
                aiCancellation = null;
            if (revision == aiRevision)
                IsAiBusy = false;
        }
    }

    public void AdoptAiSuggestion()
    {
        if (!CanEdit || AiAlternative is not { } suggestion)
            return;
        var target = TargetFolders.FirstOrDefault(folder => folder.Id == suggestion.TargetFolderId);
        if (target is null)
            return;
        SelectedTargetFolder = target;
        NewFileName = Path.GetFileNameWithoutExtension(suggestion.FileName);
        SuggestionExplanation = "KI-Vorschlag: " + suggestion.Reason;
        AiAlternative = null;
        AiStatus = "KI-Vorschlag übernommen. Die Datei wird erst nach Bestätigung geändert.";
    }

    public async Task<bool> ConfirmMoveAsync()
    {
        if (!CanConfirmMove ||
            fileMoveService is null ||
            sourceSnapshot is null)
        {
            return false;
        }

        // Genau die beim Klick angezeigten Werte übernehmen.
        CancelAiSuggestion();
        var request = new FileMoveRequest(
            sourceSnapshot.SourcePath,
            FilePathPreview,
            sourceSnapshot.Length,
            sourceSnapshot.LastWriteTimeUtc,
            IsConfirmed: true);

        IsBusy = true;

        try
        {
            var result = await fileMoveService.MoveAsync(request);

            if (result.Status == FileMoveStatus.Succeeded)
            {
                IsMoveCompleted = true;
                OperationMessage = "Datei erfolgreich verschoben.";
                OperationMessageColor = "#15803D";
                return true;
            }

            OperationMessage = result.ErrorMessage
                ?? "Die Datei konnte nicht verschoben werden.";
            OperationMessageColor = "#B42318";

            if (result.Status is FileMoveStatus.SourceChanged
                or FileMoveStatus.SourceNotFound)
            {
                // Keine stillschweigende neue Bestätigung für veränderte Daten.
                sourceSnapshot = null;
                suggestionEvaluated = false;
                aiRequest = null;
                OperationMessage +=
                    " Bitte „Datei erneut prüfen“ wählen und anschließend erneut bestätigen.";

                OnPropertyChanged(nameof(CanConfirmMove));
                OnPropertyChanged(nameof(CanRefreshSource));
            }

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
