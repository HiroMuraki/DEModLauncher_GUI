using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace DEModLauncher_GUI.ViewModel {
    using Resources = ObservableCollection<DEModResource>;
    public class DEModPack : ViewModelBase {
        private string _packName;
        private string _description;
        private string _imagePath;
        private readonly Resources _resources;

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
                if (string.IsNullOrEmpty(_imagePath)) {
                    return DOOMEternal.DefaultModPackImage;
                }
                // 如果图片文件不存在的话，则使用默认图片
                string fullPath = $@"{DOOMEternal.ModPackImagesDirectory}\{_imagePath}";
                if (!File.Exists(fullPath)) {
                    return DOOMEternal.DefaultModPackImage;
                }
                return fullPath;
            }
        }
        public Resources Resources {
            get {
                return _resources;
            }
        }
        #endregion

        #region 构造方法
        public DEModPack() {
            _resources = new Resources();
        }
        public DEModPack(Model.DEModPack model) {
            _packName = model.PackName;
            _description = model.Description;
            _imagePath = model.ImagePath;
            _resources = new Resources();
            foreach (var res in model.Resources) {
                DEModResource resource = new DEModResource(res);
                _resources.Add(resource);
            }
        }
        #endregion

        #region 公共方法
        public StreamReader Launch() {
            LaunchCheck();
            ClearResources();
            SetResources();
            Process p = GenerateProcess(DOOMEternal.ModLoader);
            p.Start();
            return p.StandardOutput;
        }
        public void AddResourcesReference(DEModPack modPack) {
            foreach (var item in modPack.Resources) {
                try {
                    AddResource(item.Path);
                }
                catch {
                    continue;
                }
            }
        }
        public void MoveUpResource(DEModResource resourcePath) {
            int currentIndex = _resources.IndexOf(resourcePath);
            if (currentIndex <= 0) {
                return;
            }
            int newIndex = currentIndex - 1;
            var t = _resources[currentIndex];
            _resources[currentIndex] = _resources[newIndex];
            _resources[newIndex] = t;
        }
        public void MoveDownResource(DEModResource resourcePath) {
            int currentIndex = _resources.IndexOf(resourcePath);
            if (currentIndex < 0) {
                return;
            }
            if (currentIndex >= _resources.Count - 1) {
                return;
            }
            int newIndex = currentIndex + 1;
            var t = _resources[currentIndex];
            _resources[currentIndex] = _resources[newIndex];
            _resources[newIndex] = t;
        }
        public void AddResource(string resourcePath) {
            string resourceName = Path.GetFileName(resourcePath);
            foreach (var existedResource in _resources) {
                if (existedResource.Path == resourceName) {
                    throw new ArgumentException($"模组包[{resourceName}]已添加，不可重复添加");
                }
            }
            if (DOOMEternal.GameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            string modPackBackup = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(resourcePath, modPackBackup);
            }
            DEModResource resource = new DEModResource(resourceName);
            _resources.Add(resource);
        }
        public void DeleteResource(DEModResource resource) {
            _resources.Remove(resource);
        }
        public void SetImage(string imagePath) {
            if (imagePath == DOOMEternal.DefaultModPackImage) {
                _imagePath = null;
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
        }
        public void GenerateMergedFile(string outputFile) {
            int conflictedItems = GetConflictInformation().Item3;
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
                foreach (var resource in _resources) {
                    using (ZipArchive zipFile = ZipFile.OpenRead($@"{DOOMEternal.ModPacksDirectory}\{resource.Path}")) {
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
        }
        public Tuple<int, int, int, Dictionary<string, List<string>>> GetConflictInformation() {
            Dictionary<string, List<string>> resourceDict = new Dictionary<string, List<string>>();
            int totalCount = 0;
            int validCount = 0;
            foreach (var resource in _resources) {
                if (resource.Status == ResourceStatus.Disabled) {
                    continue;
                }
                string fullFileName = $@"{DOOMEternal.ModPacksDirectory}\{resource.Path}";
                foreach (var subFile in GetZippedFiles(fullFileName)) {
                    if (!resourceDict.ContainsKey(subFile)) {
                        resourceDict[subFile] = new List<string>();
                        validCount += 1;
                    }
                    totalCount += 1;
                    resourceDict[subFile].Add(resource.Path);
                }
            }
            int conflictedCount = totalCount - validCount;
            return Tuple.Create(totalCount, validCount, conflictedCount, GetConflictedFiles(resourceDict));
        }
        public Model.DEModPack GetDataModel() {
            Model.DEModPack mp = new Model.DEModPack();
            mp.PackName = _packName;
            mp.Description = _description;
            mp.ImagePath = _imagePath;
            mp.Resources = new List<string>();
            foreach (var item in _resources) {
                mp.Resources.Add(item.Path);
            }
            return mp;
        }
        public DEModPack GetDeepCopy() {
            DEModPack copy = new DEModPack();
            // 设置新模组包名
            copy._packName = _packName;
            // 模组图片
            copy._imagePath = _imagePath;
            // 模组描述
            copy._description = _description;
            // 复制资源列表
            foreach (var res in _resources) {
                copy.Resources.Add(res);
            }
            return copy;
        }
        public override string ToString() {
            int resourceCount = _resources.Count;
            return $"{_packName}({resourceCount}个模组)";
        }
        #endregion

        private static Dictionary<string, List<string>> GetConflictedFiles(Dictionary<string, List<string>> fileDict) {
            Dictionary<string, List<string>> conflictedFiles = new Dictionary<string, List<string>>();
            foreach (var file in fileDict.Keys) {
                if (fileDict[file].Count <= 1) {
                    continue;
                }
                conflictedFiles[file] = new List<string>(fileDict[file]);
            }
            return conflictedFiles;
        }
        private static IEnumerable<string> GetZippedFiles(string fileName) {
            ZipArchive zipFile = ZipFile.OpenRead(fileName);
            foreach (var file in zipFile.Entries) {
                if (file.FullName.EndsWith("\\") || file.FullName.EndsWith("/")) {
                    continue;
                }
                yield return file.FullName;
            }
        }
        private void LaunchCheck() {
            foreach (var resource in _resources) {
                if (resource.Status == ResourceStatus.Disabled) {
                    continue;
                }
                if (!File.Exists($@"{DOOMEternal.ModPacksDirectory}\{resource.Path}")) {
                    throw new FileNotFoundException($"无法找到模组包{resource.Path}\n请检查{DOOMEternal.ModPacksDirectory}\\{resource.Path}");
                }
            }
        }
        private void ClearResources() {
            var fileList = Directory.GetFiles(DOOMEternal.ModDirectory);
            foreach (var file in fileList) {
                if (!File.Exists(file)) {
                    continue;
                }
                File.Delete(file);
            }
        }
        private void SetResources() {
            foreach (var resource in _resources) {
                if (resource.Status == ResourceStatus.Disabled) {
                    continue;
                }
                string fileName = Path.GetFileName(resource.Path);
                string sourceFile = $@"{DOOMEternal.ModPacksDirectory}\{fileName}";
                string destFile = $@"{DOOMEternal.ModDirectory}\{fileName}";
                File.Copy(sourceFile, destFile);
            }
        }
        private Process GenerateProcess(string modLoadder) {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = $@"{DOOMEternal.GameDirectory}\{modLoadder}";
            return p;
        }
        private string GetImageID() {
            Random rnd = new Random();
            int[] array = new int[16];
            for (int i = 0; i < array.Length; i++) {
                array[i] = rnd.Next(0, 10);
            }
            return string.Join("", array);
        }
    }
}

