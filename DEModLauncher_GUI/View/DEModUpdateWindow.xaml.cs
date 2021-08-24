using System.Windows;
using System.Windows.Input;
using DEModLauncher_GUI.ViewModel;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// DEModListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModUpdateWindow : Window {
        public DEModManager ModManager { get; }

        public DEModUpdateWindow() {
            ModManager = DEModManager.GetInstance();
            InitializeComponent();
        }

        private void Window_Close(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void UpdateMod_Click(object sender, RoutedEventArgs e) {
            ModManager.UpdateResource((DEModResource)(sender as FrameworkElement).Tag);
        }
    }
}
