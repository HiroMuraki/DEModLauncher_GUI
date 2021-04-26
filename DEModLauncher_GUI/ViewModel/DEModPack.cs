using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace DEModLauncher_GUI.ViewModel {
    using Resources = ObservableCollection<DEModResource>;
    public class DEModPack : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly string _modLoadder = "EternalModInjector.bat";
        private string _packName;
        private string _description;
        private readonly Resources _resources;
        private string _gameDirectory;

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
        public Resources Resources {
            get {
                return _resources;
            }
        }

        public string GameDirectroy {
            get {
                return _gameDirectory;
            }
            set {
                _gameDirectory = value;
                OnPropertyChanged(nameof(GameDirectroy));
                OnPropertyChanged(nameof(ModDirectory));
                OnPropertyChanged(nameof(ModPacksDirectory));
            }
        }
        public string ModDirectory {
            get {
                return $@"{_gameDirectory}\Mods";
            }
        }
        public string ModPacksDirectory {
            get {
                return $@"{_gameDirectory}\Mods\ModPacks";
            }
        }

        public DEModPack() {
            _packName = "UNNAMED_MODPACK";
            _resources = new Resources();
        }

        public StreamReader Launch() {
            LaunchCheck();
            ClearResources();
            SetResources();
            Process p = GenerateProcess();
            p.Start();
            return p.StandardOutput;
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
            if (_gameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            string modPackBackup = $@"{ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(resourcePath, modPackBackup);
            }
            DEModResource resource = new DEModResource(resourceName);
            _resources.Add(resource);
        }
        public void DeleteResource(DEModResource resource) {
            _resources.Remove(resource);
        }
        public Tuple<int, int, int, Dictionary<string, List<string>>> GetConflictInformation() {
            Dictionary<string, List<string>> resourceDict = new Dictionary<string, List<string>>();
            int totalCount = 0;
            int validCount = 0;
            foreach (var resource in _resources) {
                if (resource.Status == ResourceStatus.Disabled) {
                    continue;
                }
                string fullFileName = $@"{ModPacksDirectory}\{resource.Path}";
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
        public override string ToString() {
            int resourceCount = _resources.Count;
            return $"{_packName}({resourceCount}个模组)";
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
            foreach (var resource in _resources) {
                if (resource.Status == ResourceStatus.Disabled) {
                    continue;
                }
                if (!File.Exists($@"{ModPacksDirectory}\{resource.Path}")) {
                    throw new FileNotFoundException($"无法找到模组包{resource.Path}\n请检查{ModPacksDirectory}\\{resource.Path}");
                }
            }
        }
        private void ClearResources() {
            var fileList = Directory.GetFiles(ModDirectory);
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
                string sourceFile = $@"{ModPacksDirectory}\{fileName}";
                string destFile = $@"{ModDirectory}\{fileName}";
                File.Copy(sourceFile, destFile);
            }
        }
        private Process GenerateProcess() {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = $@"{_gameDirectory}\{_modLoadder}";
            return p;
        }
    }
}

