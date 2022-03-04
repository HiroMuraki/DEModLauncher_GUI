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
        public DEModPacks ModPackSelectors { get; } = new DEModPacks();
        public IEnumerable<DEModPack> SelectedModPacks {
            get {
                foreach (var item in ModPackSelectors) {
                    if (item.Status == Status.Enable) {
                        yield return item;
                    }
                }
            }
        }

        public DEModPackSelectWindow(IEnumerable<DEModPack> modPacks) {
            foreach (var modPack in modPacks) {
                ModPackSelectors.Add(new DEModPack().LoadFromModel(modPack.ConvertToModel()));
            }
            InitializeComponent();
        }

        private void ModPack_Toggle(object sender, MouseButtonEventArgs e) {
            ((DEModPack)((ModPack)sender).Tag).Toggle();
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
