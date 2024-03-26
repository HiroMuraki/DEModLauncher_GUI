using DEModLauncher_GUI.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DEModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.ViewModel.DEModPackViewModel>;

namespace DEModLauncher_GUI.View;

public partial class DEModPackSelectWindow : Window
{
    internal DEModPacks ModPackSelectors { get; } = new DEModPacks();

    internal IEnumerable<DEModPackViewModel> SelectedModPacks
    {
        get
        {
            foreach (DEModPackViewModel item in ModPackSelectors)
            {
                if (item.Status == Status.Enable)
                {
                    yield return item;
                }
            }
        }
    }

    internal DEModPackSelectWindow(IEnumerable<DEModPackViewModel> modPacks)
    {
        foreach (DEModPackViewModel modPack in modPacks)
        {
            ModPackSelectors.Add(new DEModPackViewModel().LoadFromModel(modPack.ConvertToModel()));
        }
        InitializeComponent();
    }

    private void ModPack_Toggle(object sender, MouseButtonEventArgs e)
    {
        ((DEModPackViewModel)((ModPack)sender).Tag).Toggle();
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Window_Close(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
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
}
