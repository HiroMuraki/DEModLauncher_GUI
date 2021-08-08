using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// DEModListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModUpdateWindow : Window {
        public List<string> ModList {
            get { return (List<string>)GetValue(ModListProperty); }
            set { SetValue(ModListProperty, value); }
        }
        public static readonly DependencyProperty ModListProperty =
            DependencyProperty.Register(nameof(ModList), typeof(List<string>), typeof(DEModUpdateWindow), new PropertyMetadata(null));

        public DEModUpdateWindow() {
            ModList = ViewModel.DEModManager.GetInstance().GetUsedMods();
            InitializeComponent();
        }

        private void Window_Close(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void UpdateMod_Click(object sender, RoutedEventArgs e) {
            string oldModName = (string)(sender as FrameworkElement).Tag;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = DOOMEternal.ModPacksDirectory;
            ofd.Title = $"替换{oldModName}";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string newFileName = ofd.FileName;
                ViewModel.DEModManager.GetInstance().UpdateModFile(oldModName, newFileName);
                ModList = ViewModel.DEModManager.GetInstance().GetUsedMods();
            }
        }
    }
}
