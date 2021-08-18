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
            MainWindow window = new MainWindow();
            // 强制将游戏文件夹路径设置为当前文件夹
            DOOMEternal.GameDirectory = Environment.CurrentDirectory;
            DOOMEternal.InitNecessaryDirectory();
            // 若已有配置文件，则读取，否则进行初始化
            if (File.Exists(DOOMEternal.LauncherProfileFile)) {
                DEModManager.GetInstance().LoadProfile(DOOMEternal.LauncherProfileFile);
            }
            else {
                DEModManager.GetInstance().Initialize();
            }
            DOOMEternal.ModificationSaved = true;
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
