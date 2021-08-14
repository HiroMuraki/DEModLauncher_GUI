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

        #region ��������
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
                // ���ͼƬ�ļ������ڵĻ�����ʹ��Ĭ��ͼƬ
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

        #region ���췽��
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

        #region ��������
        /// <summary>
        /// ����ģ�������е��ļ�
        /// </summary>
        public void Deploy() {
            LaunchCheck();
            ClearResources();
            DeployResources();
        }
        /// <summary>
        /// ���ģ����Դ
        /// </summary>
        /// <param name="resourcePath">ģ����Դ·��</param>
        public void AddResource(string resourcePath) {
            if (DOOMEternal.GameDirectory == null) {
                throw new ArgumentException("����ѡ����Ϸ�ļ���");
            }
            string resourceName = Path.GetFileName(resourcePath);
            string modPackBackup = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(resourcePath, modPackBackup);
            }
            if (ExistsResource(resourceName)) {
                throw new ArgumentException($"ģ���[{resourceName}]����ӣ������ظ����");
            }
            _resources.Add(new DEModResource(resourceName));
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// ����ض�ģ�����õ�����
        /// </summary>
        /// <param name="modPack">Ҫ��ӵ�ģ������</param>
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
        /// �Ƴ�ģ����Դ
        /// </summary>
        /// <param name="resource">Ҫ�Ƴ���ģ����Դ</param>
        public void RemoveResource(DEModResource resource) {
            _resources.Remove(resource);
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// ��ָ��ģ����뵽ָ��ģ��֮ǰ
        /// </summary>
        /// <param name="source">Ҫ�����ģ����Դ</param>
        /// <param name="target">Ŀ��ģ����Դ</param>
        public void InsertResource(DEModResource source, DEModResource target) {
            if (ReferenceEquals(source, target)) {
                return;
            }
            _resources.Remove(source);
            _resources.Insert(_resources.IndexOf(target), source);
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// ����ģ������ͼ��
        /// </summary>
        /// <param name="imagePath">ͼ��·��</param>
        public void SetImage(string imagePath) {
            if (imagePath == DOOMEternal.DefaultModPackImage) {
                _imagePath = null;
                return;
            }
            if (string.IsNullOrEmpty(imagePath)) {
                return;
            }
            // ��ȡΨһ�ļ���
            string destPath;
            string imageName;
            string imageExt = Path.GetExtension(imagePath);
            do {
                imageName = $"{GetImageID()}{imageExt}";
                destPath = $@"{DOOMEternal.ModPackImagesDirectory}\{imageName}";
            } while (File.Exists(destPath));
            // ͼƬ���Ƶ�ͼƬ����
            if (File.Exists(imagePath)) {
                File.Copy(imagePath, destPath);
            }
            _imagePath = imageName;
            OnPropertyChanged(nameof(ImagePath));
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// �����ϲ���
        /// </summary>
        /// <param name="outputFile">���·��</param>
        public void GenerateMergedFile(string outputFile) {
            int conflictedItems = GetConflictInformation().ConflictedCount;
            string fileName = Path.GetFileNameWithoutExtension(outputFile);
            string mergeWorkingFolder = $@"{DOOMEternal.ModPacksDirectory}\MERGE_WORKING_FOLDER_{fileName}";
            if (conflictedItems > 0) {
                throw new NotSupportedException("��ģ�����ô��ڳ�ͻ��������ͻ���ٵ���");
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
        /// ��ȡģ���ͻ��Ϣ
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
        /// ��ȡ����MODEL
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
        /// ��ȡ��ȸ���
        /// </summary>
        /// <returns></returns>
        public DEModPack GetDeepCopy() {
            DEModPack copy = new DEModPack();
            // ������ģ�����
            copy._packName = $"{_packName} - ����";
            // ģ��ͼƬ
            copy._imagePath = _imagePath;
            // ģ������
            copy._description = _description;
            // ������Դ�б�
            foreach (var res in _resources) {
                copy.Resources.Add(((DEModResource)res).GetDeepCopy());
            }
            return copy;
        }
        /// <summary>
        /// ����Ƿ��Ѵ���ĳ��ģ����Դ
        /// </summary>
        /// <param name="resourceName">Ҫ����ģ����Դ</param>
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
            return $"{_packName}({resourceCount}��ģ��)";
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
            // ���ģ��������Ƿ����
            if (!File.Exists(DOOMEternal.ModLoader)) {
                throw new FileNotFoundException($"�޷��ҵ�ģ�������{DOOMEternal.ModLoader}");
            }
            // ���ģ����Դ�Ƿ�ȱʧ
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
                throw new FileNotFoundException($"�޷��ҵ�����ģ�飺\n{string.Join('\n', lackedFiles)}");
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
///// ����ģ����Դ
///// </summary>
///// <param name="resourcePath">Ҫ���Ƶ�ģ����Դ</param>
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
///// ����ģ����Դ
///// </summary>
///// <param name="resourcePath">Ҫ���Ƶ�ģ����Դ</param>
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


