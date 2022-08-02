using DEModLauncher_GUI.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DEModLauncher_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DEModManager ModManager { get; } = DEModManager.GetInstance();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region 启动与保存
        private async void LoadMod_Click(object sender, RoutedEventArgs e)
        {
            await ModManager.TipToStartGame(StartMode.LoadOnly);
        }
        private async void LaunchMod_Click(object sender, RoutedEventArgs e)
        {
            await ModManager.TipToStartGame(StartMode.LoadAndStart);
        }
        private async void LaunchGame_Click(object sender, RoutedEventArgs e)
        {
            await ModManager.TipToStartGame(StartMode.StartOnly);
        }
        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            ModManager.TipToSaveProfile();
        }
        private void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            ModManager.TipToLoadProfile(DOOMEternal.LauncherProfileFile);
        }
        private void OpenOptionMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = ((Button)sender).ContextMenu;
            menu.IsOpen = !menu.IsOpen;
        }
        private void ShowAdvancedSetting_Click(object sender, RoutedEventArgs e)
        {
            new View.AdvancedSettingWindow()
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
        }
        #endregion

        #region 模组配置操作
        private void SelectModPack_Click(object sender, RoutedEventArgs e)
        {
            ModManager.SetCurrentModPack(GetModPackFrom(sender)); ;
        }
        private void DuplicateModPack_Click(object sender, RoutedEventArgs e)
        {
            ModManager.DuplicateModPack(GetModPackFrom(sender));
        }
        private void AddModPack_Click(object sender, RoutedEventArgs e)
        {
            ModManager.TipToNewModPack();
            ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.ScrollableWidth * 2);
        }
        private void RemoveModPack_Click(object sender, RoutedEventArgs e)
        {
            ModManager.TipToRemoveModPack(GetModPackFrom(sender));
        }
        private void EditModPack_Click(object sender, RoutedEventArgs e)
        {
            GetModPackFrom(sender).OpenWithEditor();
        }
        private void CheckConflict_Click(object sender, RoutedEventArgs e)
        {
            GetModPackFrom(sender).TipToCheckModConfliction();
        }
        private void ExportMergedResource_Click(object sender, RoutedEventArgs e)
        {
            ModManager.CurrentModPack.TipToExportMergedResource();
        }
        #endregion

        #region 资源操作
        private void AddResource_Click(object sender, RoutedEventArgs e)
        {
            ModManager.CurrentModPack.TipToNewResources();
            ResourcesDisplayer.ScrollToEnd();
        }
        private void AddModPackReference_Click(object sender, RoutedEventArgs e)
        {
            ModManager.CurrentModPack.TipToNewResourcesReference();
        }
        private void RemoveResource_Click(object sender, RoutedEventArgs e)
        {
            ModManager.CurrentModPack.RemoveResource(GetResourceFrom(sender));
        }
        private void CurrentModPackDisplayer_FileDrop(object sender, DragEventArgs e)
        {
            ModManager.CurrentModPack.TipToInsertResources(ModManager.CurrentModPack.Resources.Count, (string[])e.Data.GetData(DataFormats.FileDrop));
            ResourcesDisplayer.ScrollToEnd();
            FileDragArea.IsHitTestVisible = false;
        }
        private void CurrentModPackDisplayer_DragEnter(object sender, DragEventArgs e)
        {
            FileDragArea.IsHitTestVisible = true;
        }
        private void CurrentModPackDisplayer_DragLeave(object sender, DragEventArgs e)
        {
            FileDragArea.IsHitTestVisible = false;
        }
        private void OpenResourceFile_Click(object sender, RoutedEventArgs e)
        {
            DEModManagerExtensions.TipToOpenResourceFile(GetResourceFrom(sender));
        }
        #endregion

        #region 窗口操作
        private void Window_Move(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {

            }
        }
        private void Window_Close(object sender, RoutedEventArgs e)
        {
            App.Close();
        }
        private void Window_Minimum(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                switch (e.Key)
                {
                    case Key.S:
                        ModManager.TipToSaveProfile();
                        break;
                }
            }
        }
        private void Direction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _heldPoint = e.GetPosition(this);
        }
        private void Direction_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
            {
                return;
            }
            var newPont = e.GetPosition(this);
            double movedDistance = newPont.X - _heldPoint.X;
            double movedDistanceAbs = Math.Abs(movedDistance);
            if (movedDistance < 0)
            {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset + movedDistanceAbs);
            }
            else
            {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - movedDistanceAbs);
            }
            _heldPoint = newPont;
        }
        private void Direction_MouseWHeel(object sender, MouseWheelEventArgs e)
        {
            ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
        #endregion

        #region 模组配置列表拖动排序实现
        private async void ModPack_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ModManager.SetCurrentModPack(GetModPackFrom(sender));
            // 拖拽触发检测
            bool isOk = await Task.Run(() =>
            {
                for (int i = 0; i < 4; i++)
                {
                    Task.Delay(TimeSpan.FromMilliseconds(25)).Wait();
                    if (e.LeftButton != MouseButtonState.Pressed)
                    {
                        return false;
                    }
                }
                return true;
            });

            if (isOk)
            {
                var dataObj = new DataObject(((View.ModPack)sender).Tag);
                DragDrop.DoDragDrop(ResourceList, dataObj, DragDropEffects.Move);
            }
        }
        private void ModPack_DataDragDrop(object sender, DataDragDropEventArgs e)
        {
            var source = e.Data.GetData(typeof(DEModPack)) as DEModPack;
            if (source == null)
            {
                return;
            }
            var target = ((FrameworkElement)sender).Tag as DEModPack;
            if (target == null)
            {
                return;
            }
            int newIndex = ModManager.ModPacks.IndexOf(target);
            switch (e.Direction)
            {
                case Direction.Left:
                    break;
                case Direction.Right:
                    newIndex += 1;
                    break;
            }
            ModManager.ResortModPack(newIndex, source);
        }
        private void ModPacksList_DragOver(object sender, DragEventArgs e)
        {
            var hoverPos = e.GetPosition(ModPackDisplayer);
            if (hoverPos.X <= 50)
            {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset - hoverPos.X - 50);
            }
            else if (hoverPos.X >= ModPackDisplayer.ActualWidth - 50)
            {
                ModPackDisplayer.ScrollToHorizontalOffset(ModPackDisplayer.HorizontalOffset + (hoverPos.X - (ModPackDisplayer.ActualWidth - 50)));
            }
        }
        #endregion

        #region 资源列表拖动排序实现
        private async void ModResource_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 拖拽触发检测
            bool isOk = await Task.Run(() =>
            {
                for (int i = 0; i < 4; i++)
                {
                    Task.Delay(TimeSpan.FromMilliseconds(25)).Wait();
                    if (e.LeftButton != MouseButtonState.Pressed)
                    {
                        return false;
                    }
                }
                return true;
            });

            if (isOk)
            {
                var dataObj = new DataObject(((FrameworkElement)sender).Tag);
                DragDrop.DoDragDrop(ResourceList, dataObj, DragDropEffects.Move);
            }
            else
            {
                GetResourceFrom(sender)?.Toggle();
            }
        }
        private void ModResource_DataDragDrop(object sender, DataDragDropEventArgs e)
        {
            var target = ((FrameworkElement)sender).Tag as DEModResource;
            if (target == null)
            {
                return;
            }
            // 如果拖入的是文件列表
            if (e.Data.IsTargetType(DataFormats.FileDrop))
            {
                int insertIndex = ModManager.CurrentModPack.Resources.IndexOf(target);
                if (e.Direction == Direction.Down)
                {
                    insertIndex += 1;
                }
                ModManager.CurrentModPack?.TipToInsertResources(insertIndex, (string[])e.Data.GetData(DataFormats.FileDrop));
                return;
            }
            // 否则视为资源排序
            var source = e.Data.GetData(typeof(DEModResource)) as DEModResource;
            if (source == null)
            {
                return;
            }
            int newIndex = ModManager.CurrentModPack.Resources.IndexOf(target);
            if (e.Direction == Direction.Down)
            {
                newIndex += 1;
            }
            ModManager.CurrentModPack.ResortResource(newIndex, source);
        }
        private void ResourceList_DragOver(object sender, DragEventArgs e)
        {
            var hoverPos = e.GetPosition(ResourcesDisplayer);
            if (hoverPos.Y <= 30)
            {
                ResourcesDisplayer.ScrollToVerticalOffset(ResourcesDisplayer.VerticalOffset - hoverPos.Y - 30);
            }
            else if (hoverPos.Y >= (ResourcesDisplayer.ActualHeight - 30))
            {
                ResourcesDisplayer.ScrollToVerticalOffset(ResourcesDisplayer.VerticalOffset + (hoverPos.Y - (ResourcesDisplayer.ActualHeight - 30)));
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

        private Point _heldPoint;
        private static DEModResource GetResourceFrom(object sender)
        {
            return (DEModResource)((FrameworkElement)sender).Tag;
        }
        private static DEModPack GetModPackFrom(object sender)
        {
            return (DEModPack)((FrameworkElement)sender).Tag;
        }
    }
}
