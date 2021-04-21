using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DEModLauncher_GUI.ViewModel;

namespace DEModLauncher_GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static readonly string _launcherProfileFile = @"Mods\ModPacks\DEModProfiles.json";
        private DEModManager _dEModMananger;
        public DEModManager DEModManager {
            get {
                return _dEModMananger;
            }
        }

        public MainWindow() {
            _dEModMananger = new DEModManager();
            if (File.Exists(_launcherProfileFile)) {
                LoadFromFile_Click(null, null);
            }
            if (_dEModMananger.GameDirectory == null) {
                _dEModMananger.GameDirectory = Environment.CurrentDirectory;
            }
            InitializeComponent();
        }

        #region 启动与保存
        private async void LaunchMod_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.IsLaunching = true;
            StandardOutPutHandler.Text = string.Empty;
            StreamReader reader;
            try {
                reader = _dEModMananger.Launch();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message);
                _dEModMananger.IsLaunching = false;
                return;
            }
            await Task.Run(async () => {
                while (!reader.EndOfStream) {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    Application.Current.Dispatcher.Invoke(() => {
                        StandardOutPutHandler.Text += $"{reader.ReadLine()}\n";
                        StandardOutPutHandlerArea.ScrollToBottom();
                    });
                }
            });
            _dEModMananger.IsLaunching = false;
            Application.Current.Shutdown();
        }
        private void SaveToFile_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.SaveToFile(_launcherProfileFile);
        }
        private void LoadFromFile_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.LoadFromFile(_launcherProfileFile);
        }
        private void SelectGameDirectory_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _dEModMananger.GameDirectory = fbd.SelectedPath;
            }
        }
        #endregion

        #region 模组操作
        private void SelectMod_Click(object sender, RoutedEventArgs e) {
            DEModPack selectedMod = (sender as FrameworkElement).Tag as DEModPack;
            _dEModMananger.CurrentMod = selectedMod;
        }
        private void MoveUpMod_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.MoveUpMod(GetDEModPackFromControl(sender));
        }
        private void MoveDownMod_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.MoveDownMod(GetDEModPackFromControl(sender));
        }
        private void DuplicateMod_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.DuplicateMod(GetDEModPackFromControl(sender));
        }
        private void AddMod_Click(object sender, RoutedEventArgs e) {
            try {
                View.TextInputWindow textInput = new View.TextInputWindow();
                textInput.Owner = this;
                if (textInput.ShowDialog() == true) {
                    _dEModMananger.AddMod(textInput.Text);
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "添加模组配置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void DeleteMod_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.DeleteMod(GetDEModPackFromControl(sender));
        }
        private void RenameMod_Click(object sender, RoutedEventArgs e) {
            DEModPack modPack = GetDEModPackFromControl(sender);
            View.TextInputWindow textInput = new View.TextInputWindow();
            textInput.Owner = this;
            textInput.Text = modPack.PackName;
            if (textInput.ShowDialog() == true) {
                modPack.PackName = textInput.Text;
            }
        }
        #endregion

        #region 资源操作
        private void MoveUpResource_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.CurrentMod.MoveUpResource(GetResourceFromControl(sender));
        }
        private void MoveDownResource_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.CurrentMod.MoveDownResource(GetResourceFromControl(sender));
        }
        private void AddResource_Click(object sender, RoutedEventArgs e) {
            try {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Filter = "zip压缩包|*.zip";
                ofd.InitialDirectory = _dEModMananger.ModPacksDirectory;
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    foreach (var fileName in ofd.FileNames) {
                        _dEModMananger.CurrentMod.AddResource(fileName);
                    }
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "添加模组文件错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void DeleteResource_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.CurrentMod.DeleteResource(GetResourceFromControl(sender));
        }
        #endregion

        private static string GetResourceFromControl(object sender) {
            return (sender as FrameworkElement).Tag as string;
        }
        private static DEModPack GetDEModPackFromControl(object sender) {
            return (sender as FrameworkElement).Tag as DEModPack;
        }

        private void Window_Move(object sender, MouseButtonEventArgs e) {
            DragMove();
        }
        private void Window_Close(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
}
