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
            if (File.Exists(DOOMEternal.LauncherProfileFile)) {
                DEModManager.GetInstance().LoadFromFile(DOOMEternal.LauncherProfileFile);
            }
            // 强制将游戏文件夹路径设置为当前文件夹
            DOOMEternal.GameDirectory = Environment.CurrentDirectory;
            DOOMEternal.InitNecessaryDirectory();
            MainWindow window = new MainWindow();
            window.ShowDialog();
        }
    }
}
