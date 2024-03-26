using DEModLauncher_GUI.ViewModel;
using System;
using System.Windows;

namespace DEModLauncher_GUI.View;

public partial class AdvancedSettingWindow : Window
{
    public AdvancedSettingWindow()
    {
        InitializeComponent();
        GameDirectory.Text = DOOMEternal.GameDirectory;
        GameDirectory.ToolTip = DOOMEternal.GameDirectory;
    }

    private void OpenGameDirectory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DOOMEternal.OpenGameDirectory();
        }
        catch (Exception exp)
        {
            MessageBox.Show(exp.Message, "无法打开文件夹", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportModPacks_Click(object sender, RoutedEventArgs e)
    {
        DEModManagerViewModelExtensions.TipToExportModPacks();
    }

    private void ClearUnusedImageFiles_Click(object sender, RoutedEventArgs e)
    {
        DEModManagerViewModel.Instance.TipToClearUnusedImageFiles();
    }

    private void ClearUnusedModFiles_Click(object sender, RoutedEventArgs e)
    {
        DEModManagerViewModel.Instance.TipToClearUnusedModFiles();
    }

    private void UpdateModFile_Click(object sender, RoutedEventArgs e)
    {
        new DEModUpdateWindow()
        {
            Owner = this
        }.ShowDialog();
    }

    private void ViewLauncherProfile_Click(object sender, RoutedEventArgs e)
    {
        DOOMEternal.OpenLauncherProfile();
    }

    private void ResetModLoader_Click(object sender, RoutedEventArgs e)
    {
        DEModManagerViewModelExtensions.TipToResetModLoader();
    }
}
