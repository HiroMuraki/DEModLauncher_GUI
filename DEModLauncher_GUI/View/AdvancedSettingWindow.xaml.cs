using DEModLauncher_GUI.ViewModel;
using System;
using System.Windows;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// AdvancedSettingWindows.xaml 的交互逻辑
    /// </summary>
    public partial class AdvancedSettingWindow : Window {
        public AdvancedSettingWindow() {
            InitializeComponent();
            GameDirectory.Text = DOOMEternal.GameDirectory;
            GameDirectory.ToolTip = DOOMEternal.GameDirectory;
        }

        //private void SelectGameDirectory_Click(object sender, RoutedEventArgs e) {
        //    System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        //    fbd.SelectedPath = DOOMEternal.GameDirectory;
        //    fbd.Description = "选择游戏文件夹";
        //    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
        //        DOOMEternal.GameDirectory = fbd.SelectedPath;
        //        GameDirectory.Text = DOOMEternal.GameDirectory;
        //        GameDirectory.ToolTip = DOOMEternal.GameDirectory;
        //    }
        //}
        private void OpenGameDirectory_Click(object sender, RoutedEventArgs e) {
            try {
                DOOMEternal.OpenGameDirectory();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "无法打开文件夹", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExportModPacks_Click(object sender, RoutedEventArgs e) {
            try {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.InitialDirectory = DOOMEternal.GameDirectory;
                sfd.FileName = $@"ModPacks.zip";
                sfd.Filter = "ZIP压缩包|*.zip";
                sfd.Title = "选择导出的文件";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    DEModManager.ExportModPacks(sfd.FileName);
                    MessageBox.Show("模组包导出完成");
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组包导出错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClearUnusedImageFiles_Click(object sender, RoutedEventArgs e) {
            try {
                var removedFiles = DEModManager.GetInstance().ClearUnusedImageFiles();
                string outputInf = "清理完成，以下文件被移除:\n" + string.Join('\n', removedFiles);
                InformationWindow.Show(outputInf, "清理完成", this);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "文件清理出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClearUnusedModFiles_Click(object sender, RoutedEventArgs e) {
            try {
                var removedFiles = DEModManager.GetInstance().ClearUnusedModFile();
                string outputInf = "清理完成，以下文件被移除:\n" + string.Join('\n', removedFiles);
                InformationWindow.Show(outputInf, "清理完成", this);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "文件清理出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
