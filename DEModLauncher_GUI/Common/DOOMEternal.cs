using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEModLauncher_GUI {
    public static class DOOMEternal {
        public static readonly string DefaultGameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\DOOMEternal";
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
        public static string ModImagesDirectory {
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
            p.StartInfo.FileName = $@"{DOOMEternal.GameDirectory}\{DOOMEternal.GameMainExecutor}";
            p.Start();
        }
    }
}
