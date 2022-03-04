using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.ViewModel.DEModResource>;

namespace DEModLauncher_GUI.ViewModel {
    public class DEModPack : ViewModelBase, IViewModel<Model.DEModPack, DEModPack> {
        #region 公共属性
        public string PackName {
            get {
                return _packName;
            }
            set {
                _packName = value;
                OnPropertyChanged(nameof(PackName));
            }
        }
        public string Description {
            get {
                return _description;
            }
            set {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        public string ImagePath {
            get {
                // 路径为空则使用默认图片
                if (string.IsNullOrWhiteSpace(_imagePath)) {
                    return DOOMEternal.DefaultModPackImage;
                }
                string fullPath = $@"{DOOMEternal.ModPackImagesDirectory}\{_imagePath}";
                // 如果图片文件不存在则使用默认图片
                if (!File.Exists(fullPath)) {
                    return DOOMEternal.DefaultModPackImage;
                }
                return fullPath;
            }
        }
        public Status Status {
            get {
                return _status;
            }
            set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        public Resources Resources { get; } = new Resources();
        #endregion

        #region 公共方法
        public void Toggle() {
            if (_status == Status.Enable) {
                ToggleOff();
            }
            else if (_status == Status.Disable) {
                ToggleOn();
            }
        }
        public void ToggleOn() {
            Status = Status.Enable;
        }
        public void ToggleOff() {
            Status = Status.Disable;
        }
        public void Deploy() {
            // 检查模组加载器是否存在
            if (!File.Exists(DOOMEternal.ModLoader)) {
                throw new FileNotFoundException($"无法找到模组加载器{DOOMEternal.ModLoader}");
            }
            // 检查模组资源是否缺失
            var lackedFiles = new List<string>();
            foreach (var resource in Resources) {
                if (resource.Status == Status.Disable) {
                    continue;
                }
                if (!File.Exists($@"{DOOMEternal.ModPacksDirectory}\{resource.Path}")) {
                    lackedFiles.Add(resource.Path);
                }
            }
            if (lackedFiles.Count > 0) {
                throw new FileNotFoundException($"无法找到以下模组：\n{string.Join('\n', lackedFiles)}");
            }
            // 清空模组文件夹
            string[] fileList = Directory.GetFiles(DOOMEternal.ModDirectory);
            foreach (string file in fileList) {
                if (!File.Exists(file)) {
                    continue;
                }
                File.Delete(file);
            }
            // 装入选定模组
            foreach (var resource in Resources) {
                if (resource.Status == Status.Disable) {
                    continue;
                }
                string fileName = Path.GetFileName(resource.Path);
                string sourceFile = $@"{DOOMEternal.ModPacksDirectory}\{fileName}";
                string destFile = $@"{DOOMEternal.ModDirectory}\{fileName}";
                File.Copy(sourceFile, destFile);
            }
        }
        public void Edit() {
            var setter = new View.DEModPackSetter() { Owner = Application.Current.MainWindow };
            setter.PackName = _packName;
            setter.Description = _description;
            if (string.IsNullOrEmpty(_imagePath)) {
                setter.ImagePath = DOOMEternal.DefaultModPackImage;
            }
            else {
                setter.ImagePath = $"{DOOMEternal.ModPackImagesDirectory}\\{_imagePath}";
            }
            if (setter.ShowDialog() == true) {
                try {
                    if (_packName != setter.PackName) {
                        foreach (var modPack in DEModManager.GetInstance().ModPacks) {
                            if (modPack.PackName == setter.PackName) {
                                throw new ArgumentException($"模组配置名[{setter.PackName}]已存在");
                            }
                        }
                    }
                    PackName = setter.PackName;
                    Description = setter.Description;
                    if (_imagePath != setter.ImagePath) {
                        SetImage(setter.ImagePath);
                    }
                    DOOMEternal.ModificationSaved = false;
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "修改模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void NewResource() {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "选择模组文件";
            ofd.Filter = "zip压缩包|*.zip";
            ofd.InitialDirectory = string.IsNullOrEmpty(_preOpenModDirectory) ? DOOMEternal.ModPacksDirectory : _preOpenModDirectory;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _preOpenModDirectory = Path.GetDirectoryName(ofd.FileName) ?? _preOpenModDirectory;
                try {
                    AddResourcesHelper(ofd.FileNames);
                    DOOMEternal.ModificationSaved = false;
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void AddResourcesReference() {
            try {
                var existedModPacks = DEModManager.GetInstance().ModPacks;
                var allowedModPack = from i in existedModPacks where i != this select i;
                var selector = new View.DEModPackSelectWindow(allowedModPack) {
                    Owner = Application.Current.MainWindow
                };
                if (selector.ShowDialog() == true) {
                    foreach (var selectedMod in selector.SelectedModPacks) {
                        foreach (var item in selectedMod.Resources) {
                            try {
                                AddResourceHelper(item.Path);
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
        public void InsertResource(int index, string resourcePath) {
            if (DOOMEternal.GameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            string resourceName = Path.GetFileName(resourcePath);
            string modPackBackup = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(resourcePath, modPackBackup);
            }
            if (CheckIfContainsResource(resourceName)) {
                throw new ArgumentException($"模组包[{resourceName}]已添加，不可重复添加");
            }
            Resources.Insert(index, new DEModResource(resourceName));
            DOOMEternal.ModificationSaved = false;
        }
        public void InsertResources(int index, IEnumerable<string> fileList) {
            var errorList = new List<string>();
            foreach (string item in fileList) {
                try {
                    if (index < 0) {
                        InsertResource(0, item);
                    }
                    else if (index > Resources.Count - 1) {
                        AddResourceHelper(item);
                    }
                    else {
                        InsertResource(index, item);
                    }
                }
                catch (Exception e) {
                    errorList.Add($"{item}\n");
                    errorList.Add($"    原因：{e.Message}\n\n");
                }
            }
            if (errorList.Count > 0) {
                View.InformationWindow.Show(string.Join("", errorList), "", Application.Current.MainWindow);
            }
        }
        public void RemoveResource(DEModResource resource) {
            Resources.Remove(resource);
            DOOMEternal.ModificationSaved = false;
        }
        public void ResortResource(int index, DEModResource source) {
            Resources.ReInsert(index, source);
            DOOMEternal.ModificationSaved = false;
        }
        public void ExportMergedResource(DEModPack modPack) {
            var sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.FileName = modPack.PackName;
            sfd.InitialDirectory = Environment.CurrentDirectory;
            sfd.Filter = "zip压缩包|*.zip";
            sfd.AddExtension = true;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    string outputFile = sfd.FileName;
                    int conflictedItems = GetConflictInformation().ConflictedCount;
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
                        foreach (var resource in Resources) {
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
        public bool ContainsResource(DEModResource resource) {
            return CheckIfContainsResource(resource.Path);
        }
        public void CheckModConfliction() {
            var sb = new StringBuilder();
            try {
                var checkResult = GetConflictInformation();
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
        public void SetImage(string imagePath) {
            if (imagePath == DOOMEternal.DefaultModPackImage) {
                _imagePath = "";
                return;
            }
            if (string.IsNullOrEmpty(imagePath)) {
                return;
            }
            // 获取唯一文件名
            string destPath;
            string imageName;
            string imageExt = Path.GetExtension(imagePath);
            do {
                imageName = $"{GetImageID()}{imageExt}";
                destPath = $@"{DOOMEternal.ModPackImagesDirectory}\{imageName}";
            } while (File.Exists(destPath));
            // 图片复制到图片库中
            if (File.Exists(imagePath)) {
                File.Copy(imagePath, destPath);
            }
            _imagePath = imageName;
            OnPropertyChanged(nameof(ImagePath));
            DOOMEternal.ModificationSaved = false;
        }
        public Model.DEModPack ConvertToModel() {
            return new Model.DEModPack {
                PackName = _packName,
                Description = _description,
                ImagePath = _imagePath,
                Resources = (from res in Resources select res.Path).ToArray()
            };
        }
        public DEModPack LoadFromModel(Model.DEModPack model) {
            PackName = model.PackName;
            Description = model.Description;
            SetImage(model.ImagePath);
            Resources.Clear();
            foreach (string? item in model.Resources) {
                Resources.Add(new DEModResource(item));
            }
            return this;
        }
        public override string ToString() {
            return $"{_packName}({Resources.Count}个模组)";
        }
        #endregion

        private static string _preOpenModDirectory = "";
        private string _packName = "";
        private string _description = "";
        private string _imagePath = "";
        private Status _status = Status.Disable;
        private static IEnumerable<string> GetZippedFiles(string fileName) {
            var zipFile = ZipFile.OpenRead(fileName);
            foreach (var file in zipFile.Entries) {
                if (file.FullName.EndsWith("\\") || file.FullName.EndsWith("/")) {
                    continue;
                }
                yield return file.FullName;
            }
        }
        private static string GetImageID() {
            var rnd = new Random();
            int[] array = new int[16];
            for (int i = 0; i < array.Length; i++) {
                array[i] = rnd.Next(0, 10);
            }
            return string.Join("", array);
        }
        private bool CheckIfContainsResource(string resourcePath) {
            foreach (var item in Resources) {
                if (item.Path == resourcePath) {
                    return true;
                }
            }
            return false;
        }
        private void AddResourceHelper(string resourcePath) {
            if (DOOMEternal.GameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            string resourceName = Path.GetFileName(resourcePath);
            string modPackBackup = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(resourcePath, modPackBackup);
            }
            if (CheckIfContainsResource(resourceName)) {
                throw new ArgumentException($"模组包[{resourceName}]已添加，不可重复添加");
            }
            Resources.Add(new DEModResource(resourceName));
        }
        private void AddResourcesHelper(IEnumerable<string> fileList) {
            var errorList = new List<string>();
            foreach (string fileName in fileList) {
                try {
                    AddResourceHelper(fileName);
                }
                catch (Exception exp) {
                    errorList.Add($"{fileName}\n");
                    errorList.Add($"    {exp.Message}\n\n");
                }
            }
            if (errorList.Count > 0) {
                View.InformationWindow.Show($"无法添加以下模组文件：\n{string.Join("", errorList)}", "错误", Application.Current.MainWindow);
            }
        }
        private ModPackConflictInfo GetConflictInformation() {
            var resourceDict = new Dictionary<string, List<string>>();
            int totalCount = 0;
            int validCount = 0;
            foreach (var resource in Resources) {
                if (resource.Status == Status.Disable) {
                    continue;
                }
                string fullFileName = $@"{DOOMEternal.ModPacksDirectory}\{resource.Path}";
                foreach (string subFile in GetZippedFiles(fullFileName)) {
                    if (!resourceDict.ContainsKey(subFile)) {
                        resourceDict[subFile] = new List<string>();
                        validCount += 1;
                    }
                    totalCount += 1;
                    resourceDict[subFile].Add(resource.Path);
                }
            }
            int conflictedCount = totalCount - validCount;

            var conflictedFiles = new Dictionary<string, List<string>>();
            foreach (string file in resourceDict.Keys) {
                if (resourceDict[file].Count <= 1) {
                    continue;
                }
                conflictedFiles[file] = new List<string>(resourceDict[file]);
            }

            return new ModPackConflictInfo() {
                TotalCount = totalCount,
                ValidCount = validCount,
                ConflictedCount = conflictedCount,
                ConflictedFiles = conflictedFiles
            };
        }
    }
}