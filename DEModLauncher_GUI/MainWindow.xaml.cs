using DEModLauncher_GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DEModLauncher_GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Point _heldPoint;
        private IModManager _modManager;

        public IModManager ModManager {
            get {
                return _modManager;
            }
        }

        public MainWindow() {
            _modManager = DEModManager.GetInstance();
            InitializeComponent();
        }

        #region 启动与保存
        private void LoadMod_Click(object sender, RoutedEventArgs e) {
            _modManager.LoadMod();
        }
        private void LaunchMod_Click(object sender, RoutedEventArgs e) {
            _modManager.LaunchMod();
        }
        private void LaunchGame_Click(object sender, RoutedEventArgs e) {
            _modManager.LaunchGame();
        }
        private void SaveToFile_Click(object sender, RoutedEventArgs e) {
            _modManager.SaveProfiles();
        }
        private void LoadFromFile_Click(object sender, RoutedEventArgs e) {
            _modManager.LoadProfiles();
        }
        private void OpenOptionMenu_Click(object sender, RoutedEventArgs e) {
            ContextMenu menu = ((Button)sender).ContextMenu;
            menu.IsOpen = !menu.IsOpen;
        }
        private void ShowAdvancedSetting_Click(object sender, RoutedEventArgs e) {
            new View.AdvancedSettingWindow() {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
        }
        #endregion

        #region 模组配置操作
        private void SelectModPack_Click(object sender, RoutedEventArgs e) {
            _modManager.SetCurrentMod(GetModPackFrom(sender)); ;
        }
        private void DuplicateModPack_Click(object sender, RoutedEventArgs e) {
            _modManager.DuplicateModPack(GetModPackFrom(sender));
        }
        private void AddModPack_Click(object sender, RoutedEventArgs e) {
            _modManager.AddModPack();
            ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.ScrollableWidth * 2);
        }
        private void DeleteModPack_Click(object sender, RoutedEventArgs e) {
            _modManager.RemoveModPack(GetModPackFrom(sender));
        }
        private void EditModPack_Click(object sender, RoutedEventArgs e) {
            GetModPackFrom(sender).Edit();
        }
        private void EditModPack_Click(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount >= 2) {
                GetModPackFrom(sender).Edit();
            }
        }
        private void CheckConflict_Click(object sender, RoutedEventArgs e) {
            GetModPackFrom(sender).CheckModConfliction();
        }
        public void ExportMergedMod_Click(object sender, RoutedEventArgs e) {
            _modManager.CurrentMod.ExportMergedResource(GetModPackFrom(sender));
        }
        #endregion

        #region 资源操作
        private void AddResource_Click(object sender, RoutedEventArgs e) {
            _modManager.CurrentMod.AddResource();
            ResourcesDisplayer.ScrollToVerticalOffset(ResourcesDisplayer.ScrollableHeight * 2);
        }
        private void AddModPackReference_Click(object sender, RoutedEventArgs e) {
            _modManager.CurrentMod.AddResourcesReference();
        }
        private void DeleteResource_Click(object sender, RoutedEventArgs e) {
            _modManager.CurrentMod.RemoveResource(GetResourceFrom(sender));
        }
        private void CurrentModDisplayer_FileDrop(object sender, DragEventArgs e) {
            _modManager.CurrentMod.AddResource(e.Data);
            ResourcesDisplayer.ScrollToVerticalOffset(ResourcesDisplayer.ScrollableHeight * 2);
            FileDragArea.IsHitTestVisible = false;
        }
        private void CurrentModDisplayer_DragEnter(object sender, DragEventArgs e) {
            FileDragArea.IsHitTestVisible = true;
        }
        private void CurrentModDisplayer_DragLeave(object sender, DragEventArgs e) {
            FileDragArea.IsHitTestVisible = false;
        }
        private void OpenResourceFile_Click(object sender, RoutedEventArgs e) {
            _modManager.OpenResourceFile(GetResourceFrom(sender));
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
            App.Close();
        }
        private void Window_Minimum(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                switch (e.Key) {
                    case Key.S:
                        _modManager.SaveProfiles();
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
        }
        private void Direction_MouseWHeel(object sender, MouseWheelEventArgs e) {
            ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
        #endregion

        #region 模组配置列表拖动排序实现
        private async void ModPack_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            // 拖拽触发检测
            bool isOk = await Task.Run(() => {
                for (int i = 0; i < 4; i++) {
                    Task.Delay(TimeSpan.FromMilliseconds(25)).Wait();
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
            IModPack source = e.Data.GetData(typeof(DEModPack)) as IModPack;
            if (source == null) {
                return;
            }
            IModPack target = (sender as FrameworkElement).Tag as IModPack;
            if (target == null) {
                return;
            }

            // 如果源和目标相同，跳过
            if (ReferenceEquals(source, target)) {
                return;
            }

            // 否则将源移除，重新插入到target前
            _modManager.ResortModPack(source, target);
        }
        #endregion

        #region 资源列表拖动排序实现
        private async void ModResource_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            // 拖拽触发检测
            bool isOk = await Task.Run(() => {
                for (int i = 0; i < 4; i++) {
                    Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
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
        }
        private void ModResource_Drop(object sender, DragEventArgs e) {
            // 如果拖入的是文件列表
            if (IsFileDrop(e.Data)) {
                IModResource trg = (sender as FrameworkElement).Tag as IModResource;
                AddResourceFromFileDrop(_modManager.CurrentMod.Resources.IndexOf(trg), e.Data);
                return;
            }
            // 否则视为资源排序
            IModResource source = e.Data.GetData(typeof(DEModResource)) as IModResource;
            // 如果为空，尝试作为文件列表处理
            if (source == null) {
                return;
            }
            IModResource target = (sender as FrameworkElement).Tag as IModResource;
            if (target == null) {
                return;
            }

            // 如果源和目标相同，跳过
            if (ReferenceEquals(source, target)) {
                return;
            }

            // 否则将源移除，重新插入到target前
            _modManager.CurrentMod.ResortResource(source, target);
        }
        private void AddResourceFromFileDrop(int index, IDataObject data) {
            string[] fileList = data.GetData(DataFormats.FileDrop) as string[];
            if (fileList == null) {
                return;
            }
            List<string> errorList = new List<string>();
            foreach (var item in fileList) {
                try {
                    _modManager.CurrentMod.InsertResource(index, item);
                }
                catch (Exception e) {
                    errorList.Add($"{item}\n");
                    errorList.Add($"    原因：{e.Message}\n\n");
                }
            }
            if (errorList.Count > 0) {
                View.InformationWindow.Show(string.Join("", errorList), "", Application.Current.MainWindow);
            }
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

        private bool IsFileDrop(IDataObject data) {
            var dataFormats = new List<string>(data.GetFormats());
            if (dataFormats.Contains(DataFormats.FileDrop)) {
                return true;
            }
            return false;
        }
        private static T FindVisualParent<T>(DependencyObject dp) where T : class {
            while (dp != null) {
                if (dp is T) {
                    return dp as T;
                }
                dp = VisualTreeHelper.GetParent(dp);
            }
            return null;
        }
        private static IModResource GetResourceFrom(object sender) {
            return (sender as FrameworkElement).Tag as IModResource;
        }
        private static IModPack GetModPackFrom(object sender) {
            return (sender as FrameworkElement).Tag as IModPack;
        }
    }
}
