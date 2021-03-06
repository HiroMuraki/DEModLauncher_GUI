using DEModLauncher_GUI.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// DEModListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModUpdateWindow : Window {
        public DEModManager ModManager { get; } = DEModManager.GetInstance();

        public DEModUpdateWindow() {
            InitializeComponent();
        }

        private void Window_Close(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            try {
                DragMove();
            }
            catch {

            }
        }

        private void UpdateMod_Click(object sender, RoutedEventArgs e) {
            ModManager.TipToUpdateResource((DEModResource)((FrameworkElement)sender).Tag);
        }
    }
}
