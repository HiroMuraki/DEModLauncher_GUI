using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DEModLauncher_GUI.View {
    /// <summary>
    /// TextInputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DEModPackSetter : Window {
        private static string _preImagesDirectory = DOOMEternal.ModPackImagesDirectory;

        public static readonly DependencyProperty PackNameProperty =
            DependencyProperty.Register(nameof(PackName), typeof(string), typeof(DEModPackSetter), new PropertyMetadata(""));
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(DEModPackSetter), new PropertyMetadata(""));
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(DEModPackSetter), new PropertyMetadata(""));

        public string PackName {
            get {
                return (string)GetValue(PackNameProperty);
            }
            set {
                SetValue(PackNameProperty, value);
            }
        }
        public string Description {
            get {
                return (string)GetValue(DescriptionProperty);
            }
            set {
                SetValue(DescriptionProperty, value);
            }
        }
        public string ImagePath {
            get {
                return (string)GetValue(ImagePathProperty);
            }
            set {
                SetValue(ImagePathProperty, value);
            }
        }

        public DEModPackSetter() {
            InitializeComponent();
        }

        private void ChangeImage_Click(object sender, MouseButtonEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "图像文件|*.jpg;*.png;*.bmp;*.gif|JPG图片|*.jpg|PNG图片|*.png|BMP图片|*.bmp|GIF图片|*.gif";
            ofd.InitialDirectory = _preImagesDirectory;
            ofd.Title = "选择模组图片";
            if (ofd.ShowDialog() == true) {
                try {
                    ImagePath = ofd.FileName;
                    _preImagesDirectory = Path.GetDirectoryName(ofd.FileName);
                }
                catch (Exception exp) {
                    MessageBox.Show($"图片修改出错，原因：{exp.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
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
