using System.Diagnostics;
using System.IO;

namespace DEModLauncher_GUI {
    public static class DOOMEternal {
        public static readonly string DefaultGameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\DOOMEternal";
        public static readonly string DefaultModPackImage = @"\DEModLauncher_GUI;component\Resources\Images\header3.jpg";
        public static string GameMainExecutor = "DOOMEternalx64vk.exe";
        public static string ModLoader = "EternalModInjector.bat";
        public static string GameDirectory = "";
        public static string ModDirectory {
            get {
                return $@"{GameDirectory}\Mods";
            }
        }
        public static string ModPacksDirectory {
            get {
                return $@"{GameDirectory}\Mods\ModPacks";
            }
        }
        public static string ModPackImagesDirectory {
            get {
                return $@"{GameDirectory}\Mods\ModPacks\Images";
            }
        }
        public static string LauncherProfileFile {
            get {
                if (string.IsNullOrEmpty(GameDirectory)) {
                    return $@"Mods\ModPacks\DEModProfiles.json";
                }
                return $@"{GameDirectory}\Mods\ModPacks\DEModProfiles.json";
            }
        }

        public static void LaunchGame() {
            Process p = new Process();
            p.StartInfo.FileName = $@"{GameDirectory}\{GameMainExecutor}";
            p.Start();
        }

        public static void OpenGameDirectory() {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = $@"/e, {GameDirectory}";
            p.Start();
        }
        public static void InitNecessaryDirectory() {
            if (!Directory.Exists(ModDirectory)) {
                Directory.CreateDirectory(ModDirectory);
            }
            if (!Directory.Exists(ModPacksDirectory)) {
                Directory.CreateDirectory(ModPacksDirectory);
            }
            if (!Directory.Exists(ModPackImagesDirectory)) {
                Directory.CreateDirectory(ModPackImagesDirectory);
            }
        }

    }
}
