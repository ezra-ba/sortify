using Avalonia.Controls;
using Sortify.App.ViewModels;

namespace Sortify.App.Views;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        DataContext = new SplashViewModel();
    }
}
