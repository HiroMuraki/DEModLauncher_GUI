using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModResource>;

namespace DEModLauncher_GUI.ViewModel {
    public class DEModPack : ViewModelBase, IModPack {
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
        /// <summary>
        /// 部署模组配置中的文件
        /// </summary>
        public void Deploy() {
            LaunchCheck();
            ClearResources();
            DeployResources();
        }
        /// <summary>
        /// 添加模组资源
        /// </summary>
        /// <param name="resourcePath">模组资源路径</param>
        public void AddResource(string resourcePath) {
            if (DOOMEternal.GameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            string resourceName = Path.GetFileName(resourcePath);
            string modPackBackup = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(resourcePath, modPackBackup);
            }
            if (ExistsResource(resourceName)) {
                throw new ArgumentException($"模组包[{resourceName}]已添加，不可重复添加");
            }
            _resources.Add(new DEModResource(resourceName));
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 添加特定模组配置的内容
        /// </summary>
        /// <param name="modPack">要添加的模组配置</param>
        public void AddResourcesReference(DEModPack modPack) {
            foreach (var item in modPack.Resources) {
                try {
                    AddResource(item.Path);
                }
                catch {
                    continue;
                }
            }
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 移除模组资源
        /// </summary>
        /// <param name="resource">要移除的模组资源</param>
        public void RemoveResource(DEModResource resource) {
            _resources.Remove(resource);
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 将指定模组插入到指定模组之前
        /// </summary>
        /// <param name="source">要插入的模组资源</param>
        /// <param name="target">目标模组资源</param>
        public void InsertResource(DEModResource source, DEModResource target) {
            if (ReferenceEquals(source, target)) {
                return;
            }
            _resources.Remove(source);
            _resources.Insert(_resources.IndexOf(target), source);
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 设置模组配置图像
        /// </summary>
        /// <param name="imagePath">图像路径</param>
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
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 创建合并包
        /// </summary>
        /// <param name="outputFile">输出路径</param>
        public void GenerateMergedFile(string outputFile) {
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
        /// <summary>
        /// 获取模组冲突信息
        /// </summary>
        /// <returns></returns>
        public ModPackConflictInformation GetConflictInformation() {
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
            return new ModPackConflictInformation(totalCount, validCount, conflictedCount, GetConflictedFiles(resourceDict));
        }
        /// <summary>
        /// 获取数据MODEL
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 获取深度复制
        /// </summary>
        /// <returns></returns>
        public DEModPack GetDeepCopy() {
            DEModPack copy = new DEModPack();
            // 设置新模组包名
            copy._packName = $"{_packName} - 副本";
            // 模组图片
            copy._imagePath = _imagePath;
            // 模组描述
            copy._description = _description;
            // 复制资源列表
            foreach (var res in _resources) {
                copy.Resources.Add(((DEModResource)res).GetDeepCopy());
            }
            return copy;
        }
        /// <summary>
        /// 检查是否已存在某个模组资源
        /// </summary>
        /// <param name="resourceName">要检查的模组资源</param>
        /// <returns></returns>
        public bool ExistsResource(string resourceName) {
            foreach (var item in _resources) {
                if (item.Path == resourceName) {
                    return true;
                }
            }
            return false;
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
            // 检查模组加载器是否存在
            if (!File.Exists(DOOMEternal.ModLoader)) {
                throw new FileNotFoundException($"无法找到模组加载器{DOOMEternal.ModLoader}");
            }
            // 检查模组资源是否缺失
            List<string> lackedFiles = new List<string>();
            foreach (var resource in _resources) {
                if (resource.Status == ResourceStatus.Disabled) {
                    continue;
                }
                if (!File.Exists($@"{DOOMEternal.ModPacksDirectory}\{resource.Path}")) {
                    lackedFiles.Add(resource.Path);
                }
            }
            if (lackedFiles.Count > 0) {
                throw new FileNotFoundException($"无法找到以下模组：\n{string.Join('\n', lackedFiles)}");
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
        private void DeployResources() {
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

///// <summary>
///// 上移模组资源
///// </summary>
///// <param name="resourcePath">要上移的模组资源</param>
//public void MoveUpResource(DEModResource resourcePath) {
//    int currentIndex = _resources.IndexOf(resourcePath);
//    if (currentIndex <= 0) {
//        return;
//    }
//    int newIndex = currentIndex - 1;
//    var t = _resources[currentIndex];
//    _resources[currentIndex] = _resources[newIndex];
//    _resources[newIndex] = t;
//    DOOMEternal.ModificationSaved = false;
//}
///// <summary>
///// 下移模组资源
///// </summary>
///// <param name="resourcePath">要下移的模组资源</param>
//public void MoveDownResource(DEModResource resourcePath) {
//    int currentIndex = _resources.IndexOf(resourcePath);
//    if (currentIndex < 0) {
//        return;
//    }
//    if (currentIndex >= _resources.Count - 1) {
//        return;
//    }
//    int newIndex = currentIndex + 1;
//    var t = _resources[currentIndex];
//    _resources[currentIndex] = _resources[newIndex];
//    _resources[newIndex] = t;
//    DOOMEternal.ModificationSaved = false;
//}


