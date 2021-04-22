using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DEModLauncher_GUI.ViewModel {
    using Resources = ObservableCollection<string>;
    public class DEModPack : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _packName;
        private string _description;
        private readonly Resources _resources;
        private readonly string _modLoadder = "EternalModInjector.bat";
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
            ClearResources();
            SetResources();
            Process p = GenerateProcess();
            p.Start();
            return p.StandardOutput;
        }
        public void MoveUpResource(string resourcePath) {
            int currentIndex = _resources.IndexOf(resourcePath);
            if (currentIndex <= 0) {
                return;
            }
            int newIndex = currentIndex - 1;
            var t = _resources[currentIndex];
            _resources[currentIndex] = _resources[newIndex];
            _resources[newIndex] = t;
        }
        public void MoveDownResource(string resourcePath) {
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
            if (_resources.Contains(resourceName)) {
                throw new ArgumentException($"模组包[{resourceName}]已添加，不可重复添加");
            }
            if (_gameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            string modPackBackup = $@"{ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(resourcePath, modPackBackup);
            }
            _resources.Add(resourceName);
        }
        public void DeleteResource(string resourcePath) {
            _resources.Remove(resourcePath);
        }
        public override string ToString() {
            int resourceCount = _resources.Count;
            return $"{_packName}({resourceCount}个模组)";
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
            foreach (var file in _resources) {
                string fileName = Path.GetFileName(file);
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

