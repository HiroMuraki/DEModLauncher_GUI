using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DEModLauncher_GUI.ViewModel;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// DEModPackSelectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModPackSelectWindow : Window {
        private readonly ObservableCollection<DEModPackSelector> _modPackSelectors;
        public ObservableCollection<DEModPackSelector> ModPackSelectors {
            get {
                return _modPackSelectors;
            }
        }
        public IEnumerable<DEModPack> SelectedModPacks {
            get {
                foreach (var item in _modPackSelectors) {
                    if (item.IsSelected) {
                        yield return item.DEModPack;
                    }
                }
            }
        }

        public DEModPackSelectWindow(IEnumerable<DEModPack> modPacks) {
            _modPackSelectors = new ObservableCollection<DEModPackSelector>();
            foreach (var modPack in modPacks) {
                DEModPackSelector selector = new DEModPackSelector(modPack);
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
