using Avalonia.Controls;
using Sortify.App.ViewModels;

namespace Sortify.App.Views;

public partial class FileSuggestionWindow : Window
{
    public FileSuggestionWindow()
    {
        InitializeComponent();
    }

    public FileSuggestionWindow(FileSuggestionViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
