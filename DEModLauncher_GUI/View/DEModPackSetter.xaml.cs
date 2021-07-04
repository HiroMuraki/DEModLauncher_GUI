using DEModLauncher_GUI.ViewModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// TextInputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModPackSetter : Window {
        private DEModPack _modPack;

        public DEModPack ModPack {
            get {
                return _modPack;
            }
            set {
                _modPack = value;
            }
        }

        public DEModPackSetter(DEModPack modPack) {
            _modPack = modPack;
            InitializeComponent();
        }

        private void ChangeImage_Click(object sender, MouseButtonEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG图片|*.jpg|PNG图片|*.png|BMP图片|*.bmp|GIF图片|*.gif";
            ofd.InitialDirectory = @"C:\";
            ofd.Title = "选择模组图片";
            if (ofd.ShowDialog() == true) {
                ModPack.SetImage(ofd.FileName);
            }
        }
        private void Ok_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
