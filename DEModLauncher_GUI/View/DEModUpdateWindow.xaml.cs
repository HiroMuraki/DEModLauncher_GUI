using DEModLauncher_GUI.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace DEModLauncher_GUI.View;

public partial class DEModUpdateWindow : Window
{
    internal DEModManagerViewModel ModManager { get; } = DEModManagerViewModel.Instance;

    public DEModUpdateWindow()
    {
        InitializeComponent();
    }

    private void Window_Close(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Move(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DragMove();
        }
        catch
        {

        }
    }

    private void UpdateMod_Click(object sender, RoutedEventArgs e)
    {
        ModManager.TipToUpdateResource((DEModResourceViewModel)((FrameworkElement)sender).Tag);
    }
}
