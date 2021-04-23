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
        private static readonly string _defaultGameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\DOOMEternal";

        private string _launcherProfileFile {
            get {
                if (string.IsNullOrEmpty(_dEModMananger.GameDirectory)) {
                    return $@"Mods\ModPacks\DEModProfiles.json";
                }
                return $@"{_dEModMananger.GameDirectory}\Mods\ModPacks\DEModProfiles.json";
            }
        }
        private DEModManager _dEModMananger;
        public DEModManager DEModManager {
            get {
                return _dEModMananger;
            }
        }

        public MainWindow() {
            _dEModMananger = new DEModManager();
            if (File.Exists(_launcherProfileFile)) {
                _dEModMananger.LoadFromFile(_launcherProfileFile);
            }
            if (_dEModMananger.GameDirectory == null) {
                _dEModMananger.GameDirectory = Environment.CurrentDirectory;
            }
            InitializeComponent();
        }

        #region 启动与保存
        private async void LaunchMod_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show($"加载模组将需要一定时间，在此期间请勿关闭本程序。是否继续?",
                                         $"加载模组：{_dEModMananger.CurrentMod.PackName}",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            _dEModMananger.IsLaunching = true;
            StandardOutPutHandler.Text = string.Empty;
            StreamReader reader;
            try {
                reader = _dEModMananger.Launch();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
                _dEModMananger.IsLaunching = false;
                return;
            }
            StringBuilder sb = new StringBuilder(256);
            await Task.Run(async () => {
                while (!reader.EndOfStream) {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    for (int i = 0; i < 5; i++) {
                        sb.Append($"{reader.ReadLine()}\n");
                    }
                    Application.Current.Dispatcher.Invoke(() => {
                        StandardOutPutHandler.Text = sb.ToString();
                        StandardOutPutHandlerArea.ScrollToBottom();
                    });
                }
            });
            _dEModMananger.IsLaunching = false;
            Application.Current.Shutdown();
        }
        private void LaunchGame_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.LaunchDirectly();
            Application.Current.Shutdown();
        }
        private void SaveToFile_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("是否保存当前模组配置？", "保存配置", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {
                _dEModMananger.SaveToFile(_launcherProfileFile);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "保存配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadFromFile_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("此操作将会重新读取模组配置文件，并丢弃当前设置，是否继续？", "重新读取", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {

                _dEModMananger.LoadFromFile(_launcherProfileFile);
            }
            catch (Exception exp) {

                MessageBox.Show(exp.Message, "读取配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SelectGameDirectory_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = _defaultGameDirectory;
            fbd.Description = "选择游戏文件夹";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _dEModMananger.GameDirectory = fbd.SelectedPath;
            }
        }
        private void OpenGameDirectory_Click(object sender, MouseButtonEventArgs e) {
            _dEModMananger.OpenGameDirectory();
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
                    _dEModMananger.AddMod(textInput.TextA);
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "添加模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteMod_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.DeleteMod(GetDEModPackFromControl(sender));
        }
        private void RenameMod_Click(object sender, RoutedEventArgs e) {
            DEModPack modPack = GetDEModPackFromControl(sender);
            View.TextInputWindow textInput = new View.TextInputWindow();
            textInput.Owner = this;
            textInput.TextA = modPack.PackName;
            textInput.TextB = modPack.Description;
            if (textInput.ShowDialog() == true) {
                try {
                    _dEModMananger.RenameMod(modPack, textInput.TextA, textInput.TextB);
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "修改模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void CheckConflict_Click(object sender, RoutedEventArgs e) {
            DEModPack dmp = GetDEModPackFromControl(sender);
            StringBuilder sb = new StringBuilder();
            var checkResult = dmp.GetConflictInformation();
            int totalCount = checkResult.Item1;
            int validCount = checkResult.Item2;
            int conflictedCount = checkResult.Item3;
            var conflictedFiles = checkResult.Item4;
            string title = "";
            sb.Append($"总文件数: {totalCount}, 有效文件数: {validCount}, 冲突文件数: {conflictedCount}\n");
            if (conflictedCount <= 0) {
                title = "检查结果 - 无冲突";
            }
            else {
                title = "检查结果 - 以下文件存在冲突";
                int conflictID = 1;
                foreach (var conflictedFile in conflictedFiles.Keys) {
                    sb.Append($"[{conflictID}]{conflictedFile}\n");
                    foreach (var relatedFile in conflictedFiles[conflictedFile]) {
                        sb.Append($"   > {relatedFile}\n");
                    }
                    sb.Append('\n');
                    conflictID += 1;
                }
            }
            View.InformationWindow.Show(sb.ToString(), title, this);
            return;
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
                ofd.Title = "选择模组包文件";
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
                MessageBox.Show(exp.Message, "添加模组文件错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteResource_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.CurrentMod.DeleteResource(GetResourceFromControl(sender));
        }
        #endregion

        #region 窗口操作
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            DragMove();
        }
        private void Window_Close(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
        private void Window_Minimum(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
        #endregion

        private static string GetResourceFromControl(object sender) {
            return (sender as FrameworkElement).Tag as string;
        }
        private static DEModPack GetDEModPackFromControl(object sender) {
            return (sender as FrameworkElement).Tag as DEModPack;
        }
    }
}
