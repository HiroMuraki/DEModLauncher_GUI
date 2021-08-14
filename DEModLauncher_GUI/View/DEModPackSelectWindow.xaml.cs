using DEModLauncher_GUI.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// DEModPackSelectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModPackSelectWindow : Window {
        private readonly ObservableCollection<IModPackSelector> _modPackSelectors;
        public ObservableCollection<IModPackSelector> ModPackSelectors {
            get {
                return _modPackSelectors;
            }
        }
        public IEnumerable<IModPack> SelectedModPacks {
            get {
                foreach (var item in _modPackSelectors) {
                    if (item.IsSelected) {
                        yield return item.ModPack;
                    }
                }
            }
        }

        public DEModPackSelectWindow(IEnumerable<IModPack> modPacks) {
            _modPackSelectors = new ObservableCollection<IModPackSelector>();
            foreach (var modPack in modPacks) {
                DEModPackSelector selector = new DEModPackSelector((DEModPack)modPack);
                _modPackSelectors.Add(selector);
            }
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
        private void Window_Close(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            DragMove();
        }
    }
}
