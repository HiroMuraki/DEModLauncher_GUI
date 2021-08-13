﻿using DEModLauncher_GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DEModLauncher_GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Point _heldPoint;
        private string _preOpenModDirectory;
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
        private async void LoadMod_Click(object sender, RoutedEventArgs e) {
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
            try {
                DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 0);
                await Task.Run(() => {
                    _dEModMananger.LaunchModLoader();
                });
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组加载错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _dEModMananger.IsLaunching = false;
        }
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
            try {
                DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 1);
                await Task.Run(() => {
                    _dEModMananger.LaunchModLoader();
                });
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _dEModMananger.IsLaunching = false;
            Window_Close(null, null);
        }
        private void LaunchGame_Click(object sender, RoutedEventArgs e) {
            try {
                _dEModMananger.Launch();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Window_Close(null, null);
        }
        private void SaveToFile_Click(object sender, RoutedEventArgs e) {
            SaveToFileHelper();
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

        #region 模组配置操作
        private void SelectModPack_Click(object sender, RoutedEventArgs e) {
            DEModPack selectedMod = GetDEModPackFromControl(sender);
            _dEModMananger.CurrentMod = selectedMod;
        }
        private void DuplicateModPack_Click(object sender, RoutedEventArgs e) {
            _dEModMananger.DuplicateModPack(GetDEModPackFromControl(sender));
        }
        private void AddModPack_Click(object sender, RoutedEventArgs e) {
            try {
                View.DEModPackSetter setter = new View.DEModPackSetter() { Owner = this };
                setter.PackName = "模组名";
                setter.Description = "描述信息";
                setter.ImagePath = DOOMEternal.DefaultModPackImage;
                if (setter.ShowDialog() == true) {
                    DEModPack modPack = new DEModPack();
                    modPack.PackName = setter.PackName;
                    modPack.Description = setter.Description;
                    modPack.SetImage(setter.ImagePath);
                    _dEModMananger.AddModPack(modPack);
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
            _dEModMananger.RemoveModPack(dmp);
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
                string title = "";
                sb.Append($"总文件数: {checkResult.TotalCount}, 无冲突文件数: {checkResult.ValidCount}, 冲突文件数: {checkResult.ConflictedCount}\n");
                if (checkResult.ConflictedCount <= 0) {
                    title = "检查结果 - 无冲突";
                }
                else {
                    title = "检查结果 - 以下文件存在冲突";
                    int conflictID = 1;
                    foreach (var conflictedFile in checkResult.ConflictedFiles.Keys) {
                        sb.Append($"[{conflictID}]{conflictedFile}\n");
                        foreach (var relatedFile in checkResult.ConflictedFiles[conflictedFile]) {
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
        private void AddResource_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "选择模组文件";
            ofd.Filter = "zip压缩包|*.zip";
            ofd.InitialDirectory = _preOpenModDirectory ?? DOOMEternal.ModPacksDirectory;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _preOpenModDirectory = Path.GetDirectoryName(ofd.FileName);
                try {
                    AddModResourcesHelper(ofd.FileNames);
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void CurrentModDisplayer_FileDrop(object sender, DragEventArgs e) {
            try {
                string[] fileList = e.Data.GetData(DataFormats.FileDrop) as string[];
                AddModResourcesHelper(fileList);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            FileDragArea.IsHitTestVisible = false;
        }
        private void CurrentModDisplayer_DragEnter(object sender, DragEventArgs e) {
            FileDragArea.IsHitTestVisible = true;
        }
        private void CurrentModDisplayer_DragLeave(object sender, DragEventArgs e) {
            FileDragArea.IsHitTestVisible = false;
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
            _dEModMananger.CurrentMod.RemoveResource(GetResourceFromControl(sender));
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
            if (!DOOMEternal.ModificationSaved) {
                var result = MessageBox.Show("有更改尚未保存，是否关闭？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK) {
                    return;
                }
            }
            Application.Current.Shutdown();
        }
        private void Window_Minimum(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                switch (e.Key) {
                    case Key.S:
                        SaveToFileHelper();
                        break;
                }
            }
        }
        private void Direction_MouseDown(object sender, MouseButtonEventArgs e) {
            _heldPoint = e.GetPosition(this);
        }
        private void Direction_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed) {
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
            //e.Handled = true;
        }
        private void Direction_MouseWHeel(object sender, MouseWheelEventArgs e) {
            ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
        #endregion

        private static T FindVisualParent<T>(DependencyObject obj) where T : class {
            while (obj != null) {
                if (obj is T) {
                    return obj as T;
                }

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }
        private static DEModResource GetResourceFromControl(object sender) {
            return (sender as FrameworkElement).Tag as DEModResource;
        }
        private static DEModPack GetDEModPackFromControl(object sender) {
            return (sender as FrameworkElement).Tag as DEModPack;
        }
        private void EditModPack(DEModPack modPack) {
            View.DEModPackSetter setter = new View.DEModPackSetter() { Owner = this };
            setter.PackName = modPack.PackName;
            setter.Description = modPack.Description;
            setter.ImagePath = modPack.ImagePath;
            if (setter.ShowDialog() == true) {
                try {
                    _dEModMananger.RenameModPack(modPack, setter.PackName, setter.Description);
                    if (setter.ImagePath != modPack.ImagePath) {
                        modPack.SetImage(setter.ImagePath);
                    }
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "修改模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AddModResourcesHelper(IEnumerable<string> fileList) {
            if (_dEModMananger.DEModPacks.Count == 0) {
                DEModPack modPack = new DEModPack();
                modPack.PackName = "默认模组";
                modPack.Description = "描述信息";
                modPack.SetImage(DOOMEternal.DefaultModPackImage);
                _dEModMananger.DEModPacks.Add(modPack);
                _dEModMananger.CurrentMod = modPack;
            }
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
        private void SaveToFileHelper() {
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

        #region 模组配置列表拖动排序实现
        private async void ModPack_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            // 拖拽触发检测
            DateTime heldTimeForResource = DateTime.Now;
            bool isOk = await Task.Run(() => {
                while ((DateTime.Now - heldTimeForResource).TotalMilliseconds <= 200) {
                    if (e.LeftButton != MouseButtonState.Pressed) {
                        return false;
                    }
                }
                return true;
            });

            if (isOk) {
                View.ModPackButton source = sender as View.ModPackButton;
                source.IsChecked = true;
                DataObject dataObj = new DataObject(source.Tag);
                DragDrop.DoDragDrop(ResourceList, dataObj, DragDropEffects.Move);
            }
        }
        private void ModPacksList_DragOver(object sender, DragEventArgs e) {
            Point hoverPos = e.GetPosition(ModPackDisplayer);
            if (hoverPos.X <= 40) {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - hoverPos.X - 40);
            }
            else if (hoverPos.X >= 850) {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset + hoverPos.X - 850);
            }
        }
        private void ModPack_Drop(object sender, DragEventArgs e) {
            DEModPack source = e.Data.GetData(typeof(DEModPack)) as DEModPack;
            if (source == null) {
                return;
            }
            DEModPack target = (sender as FrameworkElement).Tag as DEModPack;
            if (target == null) {
                return;
            }

            // 如果源和目标相同，跳过
            if (ReferenceEquals(source, target)) {
                return;
            }

            // 否则将源移除，重新插入到target前
            _dEModMananger.InsetModPack(source, target);
        }
        // 方案2
        //private async void ModPacksList_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
        //    // 命中测试该pos下的元素
        //    Point pos = e.GetPosition(ModPacksList);
        //    HitTestResult result = VisualTreeHelper.HitTest(ModPacksList, pos);
        //    if (result == null) {
        //        return;
        //    }
        //    View.ModPackButton item = FindVisualParent<View.ModPackButton>(result.VisualHit);
        //    if (item == null) {
        //        return;
        //    }

        //    // 拖拽触发计时
        //    DateTime heldTimeForModPack = DateTime.Now;
        //    bool isOk = await Task.Run(() => {
        //        while ((DateTime.Now - heldTimeForModPack).TotalMilliseconds <= 200) {
        //            if (e.LeftButton != MouseButtonState.Pressed) {
        //                return false;
        //            }
        //        }
        //        return true;
        //    });

        //    // 如果触发成功，则启用拖拽
        //    if (isOk) {
        //        DataObject dataObj = new DataObject(item.Tag);
        //        DragDrop.DoDragDrop(ModPacksList, dataObj, DragDropEffects.Move);
        //    }
        //}
        //private void ModPacksList_PreviewMouseMove(object sender, MouseEventArgs e) {
        //    Debug.WriteLine(e.GetPosition(ModPacksList));
        //}
        //private void ModPacksList_Drop(object sender, DragEventArgs e) {
        //    DEModPack source = e.Data.GetData(typeof(DEModPack)) as DEModPack;
        //    if (source == null) {
        //        return;
        //    }

        //    // 获取落点位置的数据
        //    Point pos = e.GetPosition(ModPacksList);
        //    HitTestResult result = VisualTreeHelper.HitTest(ModPacksList, pos);
        //    if (result == null) {
        //        return;
        //    }
        //    View.ModPackButton item = FindVisualParent<View.ModPackButton>(result.VisualHit);
        //    if (item == null) {
        //        return;
        //    }
        //    DEModPack target = item.Tag as DEModPack;
        //    if (target == null) {
        //        return;
        //    }

        //    // 如果源和目标相同，跳过
        //    if (ReferenceEquals(source, target)) {
        //        return;
        //    }

        //    // 否则将源移除，重新插入到target前
        //    _dEModMananger.InsetModPack(source, target);
        //}
        #endregion

        #region 资源列表拖动排序实现
        private async void ModResource_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            // 拖拽触发检测
            DateTime heldTimeForResource = DateTime.Now;
            bool isOk = await Task.Run(() => {
                while ((DateTime.Now - heldTimeForResource).TotalMilliseconds <= 200) {
                    if (e.LeftButton != MouseButtonState.Pressed) {
                        return false;
                    }
                }
                return true;
            });

            if (isOk) {
                DataObject dataObj = new DataObject((sender as FrameworkElement).Tag);
                DragDrop.DoDragDrop(ResourceList, dataObj, DragDropEffects.Move);
            }
        }
        private void ResourceList_DragOver(object sender, DragEventArgs e) {
            Point hoverPos = e.GetPosition(ResourcesDisplayer);
            if (hoverPos.Y <= 30) {
                ResourcesDisplayer.ScrollToVerticalOffset(ResourcesDisplayer.VerticalOffset - hoverPos.Y - 30);
            }
            else if (hoverPos.Y >= 390) {
                ResourcesDisplayer.ScrollToVerticalOffset(ResourcesDisplayer.VerticalOffset + hoverPos.Y - 390);
            }
            return;
            // 命中测试该pos下的元素
            Point pos = e.GetPosition(ResourceList);
            HitTestResult result = VisualTreeHelper.HitTest(ResourceList, pos);
            if (result == null) {
                return;
            }
            ToggleButton item = FindVisualParent<ToggleButton>(result.VisualHit);
            if (item == null) {
                return;
            }
        }
        private void ModResource_Drop(object sender, DragEventArgs e) {
            DEModResource source = e.Data.GetData(typeof(DEModResource)) as DEModResource;
            if (source == null) {
                return;
            }
            DEModResource target = (sender as FrameworkElement).Tag as DEModResource;
            if (target == null) {
                return;
            }

            // 如果源和目标相同，跳过
            if (ReferenceEquals(source, target)) {
                return;
            }

            // 否则将源移除，重新插入到target前
            _dEModMananger.CurrentMod.InsertResource(source, target);
        }
        //// 方案2
        //private async void ResourceList_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
        //    // 命中测试该pos下的元素
        //    Point pos = e.GetPosition(ResourceList);
        //    HitTestResult result = VisualTreeHelper.HitTest(ResourceList, pos);
        //    if (result == null) {
        //        return;
        //    }
        //    ToggleButton item = FindVisualParent<ToggleButton>(result.VisualHit);
        //    if (item == null) {
        //        return;
        //    }

        //    // 拖拽触发检测
        //    DateTime heldTimeForResource = DateTime.Now;
        //    bool isOk = await Task.Run(() => {
        //        while ((DateTime.Now - heldTimeForResource).TotalMilliseconds <= 200) {
        //            if (e.LeftButton != MouseButtonState.Pressed) {
        //                return false;
        //            }
        //        }
        //        return true;
        //    });

        //    if (isOk) {
        //        DataObject dataObj = new DataObject(item.Tag);
        //        DragDrop.DoDragDrop(ResourceList, dataObj, DragDropEffects.Move);
        //    }
        //}
        //private void ResourcesList_PreviewMouseMove(object sender, MouseEventArgs e) {

        //}
        //private void ResourceList_Drop(object sender, DragEventArgs e) {
        //    DEModResource source = e.Data.GetData(typeof(DEModResource)) as DEModResource;
        //    if (source == null) {
        //        return;
        //    }

        //    // 获取落点位置的数据
        //    Point pos = e.GetPosition(ResourceList);
        //    HitTestResult result = VisualTreeHelper.HitTest(ResourceList, pos);
        //    if (result == null) {
        //        return;
        //    }
        //    ToggleButton item = FindVisualParent<ToggleButton>(result.VisualHit);
        //    if (item == null) {
        //        return;
        //    }
        //    DEModResource target = item.Tag as DEModResource;
        //    if (target == null) {
        //        return;
        //    }

        //    // 如果源和目标相同，跳过
        //    if (ReferenceEquals(source, target)) {
        //        return;
        //    }

        //    // 否则将源移除，重新插入到target前
        //    _dEModMananger.CurrentMod.InsertResource(source, target);
        //}
        #endregion
    }
}
