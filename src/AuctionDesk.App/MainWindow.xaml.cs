using System.Windows;
using AuctionDesk.App.ViewModels;

namespace AuctionDesk.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Initialize();
        }
    }
}