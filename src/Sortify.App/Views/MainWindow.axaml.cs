using Avalonia.Controls;
using Sortify.App.ViewModels;

namespace Sortify.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenFileSuggestion(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new FileSuggestionWindow(new FileSuggestionViewModel()).Show(this);
    }
}
