using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Sortify.App.ViewModels;

namespace Sortify.App.Views;

public partial class RuleEditorView : UserControl
{
    private RuleEditorViewModel? observedEditor;

    public RuleEditorView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ObserveEditor();
        AttachedToVisualTree += (_, _) => ObserveEditor();
        DetachedFromVisualTree += (_, _) => StopObserving();
    }

    private void ObserveEditor()
    {
        StopObserving();
        observedEditor = DataContext as RuleEditorViewModel;
        if (observedEditor is not null)
            observedEditor.PropertyChanged += EditorPropertyChanged;
    }

    private void StopObserving()
    {
        if (observedEditor is not null)
            observedEditor.PropertyChanged -= EditorPropertyChanged;
        observedEditor = null;
    }

    private void EditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not RuleEditorViewModel editor ||
            e.PropertyName is not (nameof(RuleEditorViewModel.IsEditing) or nameof(RuleEditorViewModel.IsFormVisible)))
            return;
        var showForm = editor.IsFormVisible;

        // Run after the overview/form visibility and bindings have updated their layout.
        Dispatcher.UIThread.Post(() =>
        {
            if (!ReferenceEquals(DataContext, editor) || showForm != editor.IsFormVisible || TopLevel.GetTopLevel(this) is null)
                return;
            var scroll = this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
            if (!showForm)
            {
                if (scroll is not null) scroll.Offset = new Vector(scroll.Offset.X, 0);
                if (editor.ShowEmptyState) EmptyCreateRuleButton.Focus();
                else if (TopLevel.GetTopLevel(this) is Window window) window.FindControl<Button>("NewRuleButton")?.Focus();
                return;
            }

            if (editor.CanEditRuleFields) RuleNameInput.Focus();
            else RuleActiveCheckBox.Focus();
            if (scroll is not null) scroll.Offset = new Vector(scroll.Offset.X, 0);
            else RuleEditorForm.BringIntoView();
        }, DispatcherPriority.Loaded);
    }
}
