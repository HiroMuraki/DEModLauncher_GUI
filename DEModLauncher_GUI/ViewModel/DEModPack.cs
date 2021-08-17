using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
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

        #region T
        private static string _preOpenModDirectory = null;
        public void Deploy() {
            LaunchCheck();
            ClearResources();
            DeployResources();
        }
        public void AddResource() {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "ѡ��ģ���ļ�";
            ofd.Filter = "zipѹ����|*.zip";
            ofd.InitialDirectory = _preOpenModDirectory ?? DOOMEternal.ModPacksDirectory;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _preOpenModDirectory = Path.GetDirectoryName(ofd.FileName);
                try {
                    AddResourcesHelper(ofd.FileNames);
                    DOOMEternal.ModificationSaved = false;
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void AddResource(IDataObject data) {
            try {
                string[] fileList = data.GetData(DataFormats.FileDrop) as string[];
                AddResourcesHelper(fileList);
                DOOMEternal.ModificationSaved = false;
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void AddResourcesReference() {
            try {
                var existedModPacks = DEModManager.GetInstance().ModPacks;
                var allowedModPack = from i in existedModPacks where i != this select i;
                View.DEModPackSelectWindow selector = new View.DEModPackSelectWindow(allowedModPack) {
                    Owner = Application.Current.MainWindow
                };
                if (selector.ShowDialog() == true) {
                    foreach (var selectedMod in selector.SelectedModPacks) {
                        AddResourcesReferenceHelper((DEModPack)selectedMod);
                    }
                }
                DOOMEternal.ModificationSaved = false;
            }
            catch (Exception exp) {
                MessageBox.Show($"���ʱģ���ļ�ʱ��������ԭ��{exp.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void InsertResource(int index, string resourcePath) {
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
            _resources.Insert(index, new DEModResource(resourceName));
            DOOMEternal.ModificationSaved = false;
        }
        public void RemoveResource(IModResource resource) {
            _resources.Remove(resource);
            DOOMEternal.ModificationSaved = false;
        }
        public void ResortResource(IModResource source, IModResource target) {
            if (ReferenceEquals(source, target)) {
                return;
            }
            _resources.Remove(source);
            _resources.Insert(_resources.IndexOf(target), source);
            DOOMEternal.ModificationSaved = false;
        }
        public void ExportMergedResource(IModPack modPack) {
            DEModPack dEModPack = (DEModPack)modPack;
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.FileName = dEModPack.PackName;
            sfd.InitialDirectory = Environment.CurrentDirectory;
            sfd.Filter = "zipѹ����|*.zip";
            sfd.AddExtension = true;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    dEModPack.GenerateMergedFile(sfd.FileName);
                    MessageBox.Show($"�����ɹ����ļ��ѱ�����{sfd.FileName}", "���", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception exp) {
                    MessageBox.Show($"�޷�������ϰ���ԭ��{exp.Message}", "���ɴ���", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void CheckModConfliction() {
            StringBuilder sb = new StringBuilder();
            try {
                var checkResult = GetConflictInformation();
                string title = "";
                sb.Append($"���ļ���: {checkResult.TotalCount}, �޳�ͻ�ļ���: {checkResult.ValidCount}, ��ͻ�ļ���: {checkResult.ConflictedCount}\n");
                if (checkResult.ConflictedCount <= 0) {
                    title = "����� - �޳�ͻ";
                }
                else {
                    title = "����� - �����ļ����ڳ�ͻ";
                    int conflictID = 1;
                    foreach (var conflictedFile in checkResult.ConflictedFiles.Keys) {
                        sb.Append($"[{conflictID}]{conflictedFile}\n");
                        foreach (var relatedFile in checkResult.ConflictedFiles[conflictedFile]) {
                            sb.Append($"   > {relatedFile}\n");
                        }
                        sb.Append('\n');
                        conflictID += 1;
                    }
                }
                View.InformationWindow.Show(sb.ToString(), title, Application.Current.MainWindow);
            }
            catch (Exception exp) {
                MessageBox.Show($"��ͻ������ԭ��{exp.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Edit() {
            View.DEModPackSetter setter = new View.DEModPackSetter() { Owner = Application.Current.MainWindow };
            setter.PackName = _packName;
            setter.Description = _description;
            setter.ImagePath = string.IsNullOrEmpty(_imagePath) ? DOOMEternal.DefaultModPackImage : _imagePath;
            if (setter.ShowDialog() == true) {
                try {
                    if (_packName != setter.PackName) {
                        foreach (var modPack in DEModManager.GetInstance().ModPacks) {
                            if (modPack.PackName == setter.PackName) {
                                throw new ArgumentException($"ģ��������[{setter.PackName}]�Ѵ���");
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
                    MessageBox.Show(exp.Message, "�޸�ģ�����ô���", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region ��������
        /// <summary>
        /// ���ģ����Դ
        /// </summary>
        /// <param name="resourcePath">ģ����Դ·��</param>
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
            copy._packName = _packName;
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

        private void AddResourceHelper(string resourcePath) {
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
        }
        private void AddResourcesHelper(IEnumerable<string> fileList) {
            List<string> errorList = new List<string>();
            foreach (var fileName in fileList) {
                try {
                    AddResourceHelper(fileName);
                }
                catch (Exception exp) {
                    errorList.Add($"{fileName}\n");
                    errorList.Add($"    {exp.Message}\n\n");
                }
            }
            if (errorList.Count > 0) {
                View.InformationWindow.Show($"�޷��������ģ���ļ���\n{string.Join("", errorList)}", "����", Application.Current.MainWindow);
            }
        }
        public void AddResourcesReferenceHelper(IModPack modPack) {
            foreach (var item in modPack.Resources) {
                try {
                    AddResourceHelper(item.Path);
                }
                catch {
                    continue;
                }
            }
        }
        private void GenerateMergedFile(string outputFile) {
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


