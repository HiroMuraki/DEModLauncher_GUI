using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// DEModListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModUpdateWindow : Window {
        public IModManager ModManager { get; }

        public DEModUpdateWindow() {
            ModManager = ViewModel.DEModManager.GetInstance();
            InitializeComponent();
        }

        private void Window_Close(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void UpdateMod_Click(object sender, RoutedEventArgs e) {
            ModManager.UpdateResource((IModResource)(sender as FrameworkElement).Tag);
        }
    }
}
