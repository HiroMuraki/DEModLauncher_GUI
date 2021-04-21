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
        public string Text {
            get {
                return _inputText.Text;
            }
            set {
                _inputText.Text = value;
            }
        }
        public TextInputWindow() {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
