using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DEModLauncher_GUI {
    public static class DOOMEternal {
        public static readonly string DefaultGameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\DOOMEternal";
        public static readonly string DefaultModPackImage = @"\DEModLauncher_GUI;component\Resources\Images\header3.jpg";
        public static string GameMainExecutor = "DOOMEternalx64vk.exe";
        public static string ModLoader = "EternalModInjector.bat";
        public static string ModLoaderProfileFile = "EternalModInjector Settings.txt";
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
            if (!File.Exists(p.StartInfo.FileName)) {
                throw new FileNotFoundException($"无法找到游戏主程序{GameMainExecutor}");
            }
            p.Start();
        }
        public static void LaunchModLoader() {
            Process p = new Process();
            p.StartInfo.FileName = $@"{GameDirectory}\{ModLoader}";
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
        public static void AddModFile(string filePath) {
            if (GameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            if (!Directory.Exists(ModPacksDirectory)) {
                Directory.CreateDirectory(ModPacksDirectory);
            }
            string resourceName = Path.GetFileName(filePath);
            string modPackBackup = $@"{ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(filePath, modPackBackup);
            }
        }
        public static void RemoveModFile(string modName) {
            File.Delete($"{ModPacksDirectory}\\{modName}");
        }
        public static void SetModLoaderProfile(string option, object value) {
            if (!File.Exists(ModLoaderProfileFile)) {
                return;
            }
            Regex reg = new Regex(@$"(?<=:{option}=)[0-9\.]+");
            StringBuilder text = new StringBuilder();
            using (StreamReader reader = new StreamReader(ModLoaderProfileFile)) {
                while (!reader.EndOfStream) {
                    string currentLine = reader.ReadLine();
                    if (reg.IsMatch(currentLine)) {
                        currentLine = reg.Replace(currentLine, value.ToString());
                    }
                    text.AppendLine(currentLine);
                }
            }
            using (StreamWriter writer = new StreamWriter(ModLoaderProfileFile)) {
                writer.Write(text);
            }
        }
    }
}
