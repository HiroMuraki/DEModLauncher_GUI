using DEModLauncher_GUI.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DEModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.ViewModel.DEModPack>;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// DEModPackSelectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModPackSelectWindow : Window {
        private readonly DEModPacks _modPackSelectors;
        public DEModPacks ModPackSelectors {
            get {
                return _modPackSelectors;
            }
        }
        public IEnumerable<DEModPack> SelectedModPacks {
            get {
                foreach (var item in _modPackSelectors) {
                    if (item.Status == Status.Enable) {
                        yield return item;
                    }
                }
            }
        }

        public DEModPackSelectWindow(IEnumerable<DEModPack> modPacks) {
            _modPackSelectors = new DEModPacks();
            foreach (var modPack in modPacks) {
                _modPackSelectors.Add(modPack.GetDeepCopy());
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
            try {
                DragMove();
            }
            catch {

            }
        }
    }
}
