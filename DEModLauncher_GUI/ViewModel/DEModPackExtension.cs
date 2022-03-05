using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;

namespace DEModLauncher_GUI.ViewModel {
    public static class DEModPackExtension {
        public static void OpenWithEditor(this DEModPack self) {
            var setter = new View.DEModPackSetter() { Owner = Application.Current.MainWindow };
            setter.PackName = self.PackName;
            setter.Description = self.Description;
            setter.ImagePath = self.ImagePath;

            if (setter.ShowDialog() == true) {
                try {
                    if (self.PackName != setter.PackName) {
                        foreach (var modPack in DEModManager.GetInstance().ModPacks) {
                            if (modPack.PackName == setter.PackName) {
                                throw new ArgumentException($"模组配置名[{setter.PackName}]已存在");
                            }
                        }
                    }
                    self.PackName = setter.PackName;
                    self.Description = setter.Description;
                    if (self.ImagePath != setter.ImagePath) {
                        self.SetImage(setter.ImagePath);
                    }
                    DOOMEternal.ModificationSaved = false;
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "修改模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public static void Toggle(this DEModPack self) {
            if (self.Status == Status.Enable) {
                self.ToggleOff();
            }
            else if (self.Status == Status.Disable) {
                self.ToggleOn();
            }
        }
        public static void TipToCheckModConfliction(this DEModPack self) {
            var sb = new StringBuilder();
            try {
                var checkResult = self.GetConflictionInfo();
                string title = "";
                sb.Append($"总文件数: {checkResult.TotalCount}, 无冲突文件数: {checkResult.ValidCount}, 冲突文件数: {checkResult.ConflictedCount}\n");
                if (checkResult.ConflictedCount <= 0) {
                    title = "检查结果 - 无冲突";
                }
                else {
                    title = "检查结果 - 以下文件存在冲突";
                    int conflictID = 1;
                    foreach (string conflictedFile in checkResult.ConflictedFiles.Keys) {
                        sb.Append($"[{conflictID}]{conflictedFile}\n");
                        foreach (string relatedFile in checkResult.ConflictedFiles[conflictedFile]) {
                            sb.Append($"   > {relatedFile}\n");
                        }
                        sb.Append('\n');
                        conflictID += 1;
                    }
                }
                View.InformationWindow.Show(sb.ToString(), title, Application.Current.MainWindow);
            }
            catch (Exception exp) {
                MessageBox.Show($"冲突检查出错，原因：{exp.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void TipToNewResources(this DEModPack self) {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "选择模组文件";
            ofd.Filter = "zip压缩包|*.zip";
            ofd.InitialDirectory = string.IsNullOrEmpty(_preOpenModDirectory) ? DOOMEternal.ModPacksDirectory : _preOpenModDirectory;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _preOpenModDirectory = Path.GetDirectoryName(ofd.FileName) ?? _preOpenModDirectory;
                try {
                    var addedRes = self.NewResources(ofd.FileNames, out string[] errors);
                    if (addedRes.Length > 0) {
                        DOOMEternal.ModificationSaved = false;
                    }
                    if (errors.Length > 0) {
                        View.InformationWindow.Show($"无法添加以下模组文件：\n{string.Join("", errors)}", "错误", Application.Current.MainWindow);
                    }
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public static void TipToNewResourcesReference(this DEModPack self) {
            try {
                var allowedModPack = from i in DEModManager.GetInstance().ModPacks
                                     where !ReferenceEquals(i, self)
                                     select i;
                var selector = new View.DEModPackSelectWindow(allowedModPack) {
                    Owner = Application.Current.MainWindow
                };
                if (selector.ShowDialog() == true) {
                    foreach (var selectedMod in selector.SelectedModPacks) {
                        foreach (var item in selectedMod.Resources) {
                            try {
                                self.NewResource(item.Path);
                            }
                            catch {
                                continue;
                            }
                        }
                    }
                }
                DOOMEternal.ModificationSaved = false;
            }
            catch (Exception exp) {
                MessageBox.Show($"添加时模组文件时发生错误，原因：{exp.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void TipToInsertResources(this DEModPack self, int index, IEnumerable<string> fileList) {
            self.InsertResources(index, fileList, out string[] errorList);

            if (errorList.Length > 0) {
                View.InformationWindow.Show(string.Join("", errorList), "", Application.Current.MainWindow);
            }
        }
        public static void TipToExportMergedResource(this DEModPack self) {
            var sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.FileName = self.PackName;
            sfd.InitialDirectory = Environment.CurrentDirectory;
            sfd.Filter = "zip压缩包|*.zip";
            sfd.AddExtension = true;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    string outputFile = sfd.FileName;
                    int conflictedItems = self.GetConflictionInfo().ConflictedCount;
                    string fileName = Path.GetFileNameWithoutExtension(outputFile);
                    string mergeWorkingFolder = $@"{DOOMEternal.ModPacksDirectory}\MERGE_WORKING_FOLDER_{fileName}";
                    if (conflictedItems > 0) {
                        throw new NotSupportedException("该模组配置存在冲突，请解决冲突后再导出");
                    }
                    try {
                        if (Directory.Exists(mergeWorkingFolder)) {
                            Directory.Delete(mergeWorkingFolder, true);
                        }
                        Directory.CreateDirectory(mergeWorkingFolder);
                        foreach (var resource in self.Resources) {
                            using (var zipFile = ZipFile.OpenRead($@"{DOOMEternal.ModPacksDirectory}\{resource.Path}")) {
                                zipFile.ExtractToDirectory(mergeWorkingFolder);
                            }
                        }
                        ZipFile.CreateFromDirectory(mergeWorkingFolder, outputFile, CompressionLevel.Fastest, false);
                    }
                    catch (Exception) {
                        throw;
                    }
                    finally {
                        Directory.Delete(mergeWorkingFolder, true);
                    }
                    MessageBox.Show($"导出成功，文件已保存至{sfd.FileName}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception exp) {
                    MessageBox.Show($"无法生成组合包，原因：{exp.Message}", "生成错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static string _preOpenModDirectory = "";
    }
}