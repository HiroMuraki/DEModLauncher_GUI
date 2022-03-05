using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        public void ToggleOn() {
            Status = Status.Enable;
        }
        public void ToggleOff() {
            Status = Status.Disable;
        }
        public void SetImage(string imagePath) {
            if (string.IsNullOrWhiteSpace(imagePath) || imagePath == DOOMEternal.DefaultModPackImage) {
                _imagePath = "";
            }
            else {
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
        public DEModResource NewResource(string resourcePath) {
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
            var t = new DEModResource(resourceName);
            Resources.Add(t);
            return t;
        }
        public DEModResource[] NewResources(IEnumerable<string> fileList, out string[] errorList) {
            var errors = new List<string>();
            var added = new List<DEModResource>();
            foreach (string fileName in fileList) {
                try {
                    added.Add(AddResourceHelper(fileName));
                }
                catch (Exception exp) {
                    errors.Add($"{fileName}\n");
                    errors.Add($"    {exp.Message}\n\n");
                }
            }
            errorList = errors.ToArray();
            return added.ToArray();
        }
        public DEModResource InsertResource(int index, string resourcePath) {
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
            var res = new DEModResource(resourceName);
            Resources.Insert(index, res);
            DOOMEternal.ModificationSaved = false;
            return res;
        }
        public DEModResource[] InsertResources(int index, IEnumerable<string> fileList, out string[] errorList) {
            var errors = new List<string>();
            var added = new List<DEModResource>();
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
                    errors.Add($"{item}\n");
                    errors.Add($"    原因：{e.Message}\n\n");
                }
            }
            errorList = errors.ToArray();
            return added.ToArray();
        }
        public DEModResource RemoveResource(DEModResource resource) {
            Resources.Remove(resource);
            DOOMEternal.ModificationSaved = false;
            return resource;
        }
        public DEModResource ResortResource(int index, DEModResource source) {
            Resources.ReInsert(index, source);
            DOOMEternal.ModificationSaved = false;
            return source;
        }
        public bool ContainsResource(DEModResource resource) {
            return CheckIfContainsResource(resource.Path);
        }
        public ModPackConflictInfo GetConflictionInfo() {
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
            _imagePath = model.ImagePath;
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
        private DEModResource AddResourceHelper(string resourcePath) {
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
            var t = new DEModResource(resourceName);
            Resources.Add(t);
            return t;
        }
    }
}