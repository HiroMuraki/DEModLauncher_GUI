using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DEModLauncher_GUI.ViewModel;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// AdvancedSettingWindows.xaml 的交互逻辑
    /// </summary>
    public partial class AdvancedSettingWindow : Window {
        public AdvancedSettingWindow() {
            InitializeComponent();
        }

        private void SelectGameDirectory_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = DOOMEternal.GameDirectory;
            fbd.Description = "选择游戏文件夹";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                DOOMEternal.GameDirectory = fbd.SelectedPath;
            }
        }
        private void OpenGameDirectory_Click(object sender, RoutedEventArgs e) {
            DOOMEternal.OpenGameDirectory();
        }

        private void ExportModPacks_Click(object sender, RoutedEventArgs e) {
            DEModManager.GetInstance().ExportModPacks("test.zip");
        }

        private void ClearUnusedImageFiles_Click(object sender, RoutedEventArgs e) {
            DEModManager.GetInstance().ClearUnusedImageFiles();
        }

        private void ClearUnusedModFiles_Click(object sender, RoutedEventArgs e) {
            DEModManager.GetInstance().ClearUnusedModFile();
        }
    }
}
