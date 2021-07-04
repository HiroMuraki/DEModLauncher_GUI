using DEModLauncher_GUI.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DEModLauncher_GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Point _heldPoint;

        private DEModManager _dEModMananger;

        public DEModManager DEModManager {
            get {
                return _dEModMananger;
            }
        }

        public MainWindow() {
            _dEModMananger = DEModManager.GetInstance();
            InitializeComponent();
        }

        #region 启动与保存
        private async void LaunchMod_Click(object sender, RoutedEventArgs e) {
            // 弹出提示窗口，避免误操作
            var result = MessageBox.Show($"加载模组将需要一定时间，在此期间请勿关闭本程序。是否继续?",
                                         $"加载模组：{_dEModMananger.CurrentMod.PackName}",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            // 进入启动程序
            _dEModMananger.IsLaunching = true;
            _dEModMananger.ConsoleStandardOutput = string.Empty;
            StreamReader reader;
            try {
                reader = _dEModMananger.LaunchWithModLoader();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
                _dEModMananger.IsLaunching = false;
                return;
            }
            // 从StreamReader中读取控制台输出信息
            StringBuilder sb = new StringBuilder(256);
            await Task.Run(async () => {
                while (!reader.EndOfStream) {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    for (int i = 0; i < 5; i++) {
                        sb.Append($"{reader.ReadLine()}\n");
                    }
                    Application.Current.Dispatcher.Invoke(() => {
                        _dEModMananger.ConsoleStandardOutput = sb.ToString();
                        // StandardOutPutHandlerArea.ScrollToBottom();
                    });
                }
            });
            _dEModMananger.IsLaunching = false;
            // 启动后关闭启动器
            Application.Current.Shutdown();
        }
        private void LaunchGame_Click(object sender, RoutedEventArgs e) {
            try {
                _dEModMananger.Launch();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Application.Current.Shutdown();
        }
        private void SaveToFile_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("是否保存当前模组配置？", "保存配置", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {
                _dEModMananger.SaveToFile(DOOMEternal.LauncherProfileFile);
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

                _dEModMananger.LoadFromFile(DOOMEternal.LauncherProfileFile);
            }
            catch (Exception exp) {

                MessageBox.Show(exp.Message, "读取配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OpenOptionMenu_Click(object sender, RoutedEventArgs e) {
            ContextMenu menu = ((Button)sender).ContextMenu;
            menu.IsOpen = !menu.IsOpen;
        }
        private void ShowAdvancedSetting_Click(object sender, RoutedEventArgs e) {
            View.AdvancedSettingWindow window = new View.AdvancedSettingWindow() { Owner = this };
            window.ShowDialog();
        }
        #endregion

        #region 模组操作
        private void SelectModPack_Click(object sender, RoutedEventArgs e) {
            DEModPack selectedMod = GetDEModPackFromControl(sender);
            _dEModMananger.CurrentMod = selectedMod;
        }
        private void MoveUpModPack_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.MoveUpMod(GetDEModPackFromControl(sender));
        }
        private void MoveDownModPack_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.MoveDownMod(GetDEModPackFromControl(sender));
        }
        private void DuplicateModPack_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.DuplicateMod(GetDEModPackFromControl(sender));
        }
        private void AddModPack_Click(object sender, RoutedEventArgs e) {
            try {
                DEModPack copy = new DEModPack();
                View.DEModPackSetter textInput = new View.DEModPackSetter(copy);
                textInput.Owner = this;
                if (textInput.ShowDialog() == true) {
                    _dEModMananger.AddMod(copy.PackName, copy.Description);
                    ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.ScrollableWidth * 2);
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "添加模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteModPack_Click(object sender, RoutedEventArgs e) {
            var dmp = GetDEModPackFromControl(sender);
            var result = MessageBox.Show($"是否删除模组配置：{dmp.PackName}", "警告",
                                         MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            _dEModMananger.DeleteMod(dmp);
        }
        private void EditModPack_Click(object sender, RoutedEventArgs e) {
            DEModPack modPack = GetDEModPackFromControl(sender);
            EditModPack(modPack);
        }
        private void EditModPack_Click(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount >= 2) {
                DEModPack modPack = GetDEModPackFromControl(sender);
                EditModPack(modPack);
            }
        }
        private void CheckConflict_Click(object sender, RoutedEventArgs e) {
            DEModPack dmp = GetDEModPackFromControl(sender);
            StringBuilder sb = new StringBuilder();
            try {
                var checkResult = dmp.GetConflictInformation();
                int totalCount = checkResult.Item1;
                int validCount = checkResult.Item2;
                int conflictedCount = checkResult.Item3;
                var conflictedFiles = checkResult.Item4;
                string title = "";
                sb.Append($"总文件数: {totalCount}, 无冲突文件数: {validCount}, 冲突文件数: {conflictedCount}\n");
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
            }
            catch (Exception exp) {
                MessageBox.Show($"冲突检查出错，原因：{exp.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void GenerateMergedFile_ClicK(object sender, RoutedEventArgs e) {
            try {
                DEModPack modPack = GetDEModPackFromControl(sender);
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.FileName = modPack.PackName;
                sfd.InitialDirectory = Environment.CurrentDirectory;
                sfd.Filter = "zip压缩包|*.zip";
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    modPack.GenerateMergedFile(sfd.FileName);
                    MessageBox.Show($"导出成功，文件已保存至{sfd.FileName}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception exp) {
                MessageBox.Show($"无法生成组合包，原因：{exp.Message}", "生成错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                ofd.Title = "选择模组包文件";
                ofd.Filter = "zip压缩包|*.zip";
                ofd.InitialDirectory = DOOMEternal.ModPacksDirectory;
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    foreach (var fileName in ofd.FileNames) {
                        try {
                            _dEModMananger.CurrentMod.AddResource(fileName);
                        }
                        catch (Exception exp) {
                            MessageBox.Show($"无法添加模组文件：{fileName}\n\n原因：{exp.Message}", "添加模组文件错误",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddFile_FileDrop(object sender, DragEventArgs e) {
            try {
                string[] fileList = e.Data.GetData(DataFormats.FileDrop) as string[];
                foreach (var fileName in fileList) {
                    try {
                        _dEModMananger.CurrentMod.AddResource(fileName);
                    }
                    catch (Exception exp) {
                        MessageBox.Show($"无法添加模组文件：{fileName}\n\n原因{exp.Message}", "错误",
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddModPackReference_Click(object sender, RoutedEventArgs e) {
            try {
                DEModPack current = GetDEModPackFromControl(sender);
                var allowedModPack = from i in _dEModMananger.DEModPacks where i != current select i;
                View.DEModPackSelectWindow selector = new View.DEModPackSelectWindow(allowedModPack) {
                    Owner = this
                };
                if (selector.ShowDialog() == true) {
                    foreach (var selectedMod in selector.SelectedModPacks) {
                        current.AddResourcesReference(selectedMod);
                    }
                }
            }
            catch (Exception exp) {
                MessageBox.Show($"添加时模组文件时发生错误，原因：{exp.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteResource_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.CurrentMod.DeleteResource(GetResourceFromControl(sender));
        }
        private void OpenResourceFile_Click(object sender, RoutedEventArgs e) {
            try {
                DEModManager.OpenResourceFile(GetResourceFromControl(sender).Path);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "打开错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 窗口操作
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount >= 2) {
                return;
            }
            DragMove();
        }
        private void Window_Close(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
        private void Window_Minimum(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
        private void Direction_MouseDown(object sender, MouseButtonEventArgs e) {
            _heldPoint = e.GetPosition(this);
        }
        private void Direction_MouseMove(object sender, MouseEventArgs e) {
            if (Mouse.LeftButton != MouseButtonState.Pressed) {
                return;
            }
            Point newPont = e.GetPosition(this);
            double movedDistance = newPont.X - _heldPoint.X;
            double movedDistanceAbs = Math.Abs(movedDistance);
            if (movedDistance < 0) {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset + movedDistanceAbs);
            }
            else {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - movedDistanceAbs);
            }
            _heldPoint = newPont;
            e.Handled = true;
        }
        private void Direction_MouseWHeel(object sender, MouseWheelEventArgs e) {
            ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
        #endregion

        private static DEModResource GetResourceFromControl(object sender) {
            return (sender as FrameworkElement).Tag as DEModResource;
        }
        private static DEModPack GetDEModPackFromControl(object sender) {
            return (sender as FrameworkElement).Tag as DEModPack;
        }
        private void EditModPack(DEModPack modPack) {
            DEModPack copy = new DEModPack();
            copy.PackName = modPack.PackName;
            copy.Description = modPack.Description;
            copy.SetImage(modPack.ImagePath);
            View.DEModPackSetter textInput = new View.DEModPackSetter(copy) { Owner = this };
            if (textInput.ShowDialog() == true) {
                try {
                    _dEModMananger.RenameMod(modPack, copy.PackName, copy.Description);
                    modPack.SetImage(copy.ImagePath);
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "修改模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
