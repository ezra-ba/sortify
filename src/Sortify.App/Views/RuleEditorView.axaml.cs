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
        if (e.PropertyName != nameof(RuleEditorViewModel.IsEditing) ||
            sender is not RuleEditorViewModel { IsEditing: true } editor)
            return;

        // Wait for bindings and layout after loading the selected rule.
        Dispatcher.UIThread.Post(() =>
        {
            if (!ReferenceEquals(DataContext, editor) || !editor.IsEditing || TopLevel.GetTopLevel(this) is null)
                return;

            if (editor.CanEditRuleFields) RuleNameInput.Focus();
            else RuleActiveCheckBox.Focus();
            var scroll = this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
            if (scroll?.Content is Visual content &&
                RuleEditorForm.TranslatePoint(default, content) is { } position)
            {
                scroll.Offset = new Vector(scroll.Offset.X, Math.Max(0, position.Y));
            }
            else
            {
                RuleEditorForm.BringIntoView();
            }
        }, DispatcherPriority.Loaded);
    }
}
