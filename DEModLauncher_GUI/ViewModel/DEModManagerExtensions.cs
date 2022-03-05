using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;

namespace DEModLauncher_GUI.ViewModel {
    public static class DEModManagerExtensions {
        public static void TipToNewModPack(this DEModManager self) {
            try {
                var setter = new View.DEModPackSetter() { Owner = Application.Current.MainWindow };
                setter.PackName = "模组名";
                setter.Description = "描述信息";
                setter.ImagePath = DOOMEternal.DefaultModPackImage;
                if (setter.ShowDialog() == true) {
                    var modPack = self.NewModPack();
                    modPack.PackName = setter.PackName;
                    modPack.Description = setter.Description;
                    modPack.SetImage(setter.ImagePath);
                    self.SetCurrentModPack(modPack);
                    DOOMEternal.ModificationSaved = false;
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "添加模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void TipToRemoveModPack(this DEModManager self, DEModPack modPack) {
            var result = MessageBox.Show($"是否删除模组配置：{modPack.PackName}", "警告",
                                         MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            self.RemoveModPack(modPack);
        }
        public static void TipToSaveProfile(this DEModManager self) {
            var result = MessageBox.Show("是否保存当前模组配置？", "保存配置", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {
                self.SaveProfile(DOOMEternal.LauncherProfileFile);
                DOOMEternal.ModificationSaved = true;
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "保存配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void TipToLoadProfile(this DEModManager self, string file) {
            var result = MessageBox.Show("此操作将会重新读取模组配置文件，并丢弃当前设置，是否继续？", "重新读取", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {
                self.LoadProfile(file);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "读取配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void TipToUpdateResource(this DEModManager self, DEModResource resource) {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.InitialDirectory = _preOpenModDirectory ?? DOOMEternal.ModPacksDirectory;
            ofd.Title = $"替换{resource.Path}";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                self.UpdateResource(resource, ofd.FileName);
            }
        }
        public static void TipToOpenResourceFile(DEModResource resource) {
            try {
                var p = new Process();
                string filePath = $@"{DOOMEternal.ModPacksDirectory}\{resource.Path}";
                if (!File.Exists(filePath)) {
                    throw new FileNotFoundException($"无法找到文件：{filePath}");
                }
                p.StartInfo.FileName = "explorer.exe";
                p.StartInfo.Arguments = $@"/select, {DOOMEternal.ModPacksDirectory}\{resource.Path}";
                p.Start();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "打开错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void TipToResetModLoader() {
            var result = MessageBox.Show("重置模组加载器将会移除由其产生的文件备份与配置文件\n" +
                                         "请在重置前或重置后验证游戏文件完整性\n" +
                                         "是否继续",
                                         "警告",
                                         MessageBoxButton.OKCancel,
                                         MessageBoxImage.Warning);
            if (result != MessageBoxResult.OK) {
                return;
            }
            var removedFiles = new List<string>();
            removedFiles.Add("重置完成，以下文件被移除");
            // 移除哈希信息与模组加载器配置文件
            string[] modLoaderProfiles = new string[2] {
                $"{DOOMEternal.GameDirectory}\\base\\idRehash.map",
                $"{DOOMEternal.GameDirectory}\\{DOOMEternal.ModLoaderProfileFile}"
            };
            foreach (string file in modLoaderProfiles) {
                if (File.Exists(file)) {
                    File.Delete(file);
                    removedFiles.Add(file);
                }
            }
            // 移除备份文件
            foreach (string file in Util.TravelFiles(DOOMEternal.GameDirectory)) {
                if (Path.GetExtension(file) == ".backup") {
                    File.Delete(file);
                    removedFiles.Add(file);
                }
            }
            View.InformationWindow.Show(string.Join('\n', removedFiles), "重置完成", Application.Current.MainWindow);
        }
        public static void TipToExportModPacks() {
            var sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = DOOMEternal.GameDirectory;
            sfd.FileName = $@"ModPacks.zip";
            sfd.Filter = "ZIP压缩包|*.zip";
            sfd.Title = "选择导出的文件";
            try {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    ZipFile.CreateFromDirectory(DOOMEternal.ModPacksDirectory, sfd.FileName, CompressionLevel.Optimal, true);
                    MessageBox.Show("模组包导出完成");
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组包导出错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void TipToClearUnusedImageFiles(this DEModManager self) {
            var result = MessageBox.Show("该操作将会移除被使用的图片文件，是否继续?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                try {
                    self.ClearUnusedImageFiles(out string[] removedFiles);
                    string outputInf = "清理完成，以下文件被移除:\n" + string.Join('\n', removedFiles);
                    View.InformationWindow.Show(outputInf, "清理完成", Application.Current.MainWindow);
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "文件清理出错", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public static void TipToClearUnusedModFiles(this DEModManager self) {
            var result = MessageBox.Show("该操作将会移除被使用的模组文件，是否继续?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                try {
                    self.ClearUnusedModFile(out string[] removedFiles);
                    string outputInf = "清理完成，以下文件被移除:\n" + string.Join('\n', removedFiles);
                    View.InformationWindow.Show(outputInf, "清理完成", Application.Current.MainWindow);
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "文件清理出错", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public static async Task<bool> TipToStartGame(this DEModManager self, StartMode startMode) {
            if (startMode != StartMode.StartOnly) {
                if (!self.IsValidModPackSelected()) {
                    MessageBox.Show("请先选择一个模组配置", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                // 弹出提示窗口，避免误操作
                var result = MessageBox.Show($"加载模组将需要一定时间，在此期间请勿关闭本程序。是否继续?",
                                             $"加载模组：{self.CurrentModPack.PackName}",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) {
                    return false;
                }
            }

            self.IsLaunching = true;
            try {
                switch (startMode) {
                    case StartMode.LoadOnly:
                        await Task.Run(() => { self.LoadMod(); });
                        break;
                    case StartMode.LoadAndStart:
                        await Task.Run(() => { self.LaunchMod(); });
                        break;
                    case StartMode.StartOnly:
                        await Task.Run(() => { self.LaunchGame(); });
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "启动错误", Application.Current.MainWindow);
                return false;
            }
            finally {
                self.IsLaunching = false;
            }
        }

        private static string _preOpenModDirectory = "";
    }
}