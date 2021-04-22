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

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// TextInputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TextInputWindow : Window {
        public string TextA {
            get {
                return _inputTextA.Text;
            }
            set {
                _inputTextA.Text = value;
            }
        }
        public string TextB {
            get {
                return _inputTextB.Text;
            }
            set {
                _inputTextB.Text = value;
            }
        }
        public TextInputWindow() {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
