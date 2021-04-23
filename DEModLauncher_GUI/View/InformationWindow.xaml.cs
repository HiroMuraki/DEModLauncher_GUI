using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// InformationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InformationWindow : Window, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _caption;


        public string Message {
            get {
                return _message.Text;
            }
            set {
                _message.Text = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        public string Caption {
            get {
                return _caption;
            }
            set {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public InformationWindow() {
            InitializeComponent();
        }
        public InformationWindow(string message, string caption) {
            InitializeComponent();
            Caption = caption;
            Message = message;
        }

        public static void Show(string message) {
            ShowCore(message, null, null);
        }
        public static void Show(string message, string caption) {
            ShowCore(message, caption, null);
        }
        public static void Show(string message, string caption, Window parentWindow) {
            ShowCore(message, caption, parentWindow);
        }

        private static void ShowCore(string message, string caption, Window parentWindow) {
            InformationWindow window = new InformationWindow(message, caption);
            if (parentWindow != null) {
                window.Owner = parentWindow;
            }
            window.Show();
        }
    }
}
