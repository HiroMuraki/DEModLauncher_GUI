using DEModLauncher_GUI.ViewModel;
using System;
using System.IO;
using System.Windows;

namespace DEModLauncher_GUI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            // 若已有配置文件，则读取
            if (File.Exists(DOOMEternal.LauncherProfileFile)) {
                DEModManager.GetInstance().LoadProfiles(DOOMEternal.LauncherProfileFile);
            }
            // 强制将游戏文件夹路径设置为当前文件夹
            DOOMEternal.GameDirectory = Environment.CurrentDirectory;
            DOOMEternal.InitNecessaryDirectory();
            DOOMEternal.ModificationSaved = true;
            MainWindow window = new MainWindow();
            window.ShowDialog();
        }
        public static void Close() {
            if (!DOOMEternal.ModificationSaved) {
                var result = MessageBox.Show("有更改尚未保存，是否关闭？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK) {
                    return;
                }
            }
            Current.Shutdown();
        }
    }
}
