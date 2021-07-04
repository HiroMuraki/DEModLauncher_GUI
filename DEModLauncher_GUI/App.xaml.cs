using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DEModLauncher_GUI.ViewModel;

namespace DEModLauncher_GUI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            if (DOOMEternal.GameDirectory == null) {
                DOOMEternal.GameDirectory = Environment.CurrentDirectory;
            }
            if (File.Exists(DOOMEternal.LauncherProfileFile)) {
                DEModManager.GetInstance().LoadFromFile(DOOMEternal.LauncherProfileFile);
            }
            MainWindow window = new MainWindow();
            window.ShowDialog();
        }
    }
}
