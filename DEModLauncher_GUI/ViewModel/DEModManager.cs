using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Diagnostics;

namespace DEModLauncher_GUI.ViewModel {
    using DEModPacks = ObservableCollection<DEModPack>;
    public class DEModManager : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(Model.DEModManager));
        private static readonly string _gameMainExecutor = "DOOMEternalx64vk.exe";
        private string _gameDirectory;
        private bool _isLaunching;
        private DEModPack _currentMod;
        private readonly DEModPacks _dEModPacks;

        public string GameDirectory {
            get {
                return _gameDirectory;
            }
            set {
                _gameDirectory = value;
                foreach (var modPack in _dEModPacks) {
                    modPack.GameDirectroy = _gameDirectory;
                }
                if (!Directory.Exists(ModsDirectory)) {
                    Directory.CreateDirectory(ModsDirectory);
                }
                if (!Directory.Exists(ModPacksDirectory)) {
                    Directory.CreateDirectory(ModPacksDirectory);
                }
                OnPropertyChanged(nameof(GameDirectory));
            }
        }
        public string ModsDirectory {
            get {
                return $@"{_gameDirectory}\Mods";
            }
        }
        public string ModPacksDirectory {
            get {
                return $@"{_gameDirectory}\Mods\ModPacks";
            }
        }
        public bool IsLaunching {
            get {
                return _isLaunching;
            }
            set {
                _isLaunching = value;
                OnPropertyChanged(nameof(IsLaunching));
            }
        }
        public DEModPack CurrentMod {
            get {
                return _currentMod;
            }
            set {
                _currentMod = value;
                OnPropertyChanged(nameof(CurrentMod));
            }
        }
        public DEModPacks DEModPacks {
            get {
                return _dEModPacks;
            }
        }

        public DEModManager() {
            _currentMod = null;
            _dEModPacks = new DEModPacks();
        }

        public StreamReader Launch() {
            if (_currentMod == null) {
                throw new InvalidOperationException("当前未选择有效模组");
            }
            _currentMod.GameDirectroy = _gameDirectory;
            return _currentMod.Launch();
        }
        public void LaunchDirectly() {
            Process p = new Process();
            p.StartInfo.FileName = $@"{_gameDirectory}\{_gameMainExecutor}";
            p.Start();
        }
        public void MoveUpMod(DEModPack modPack) {
            int currentIndex = _dEModPacks.IndexOf(modPack);
            if (currentIndex <= 0) {
                return;
            }
            int newIndex = currentIndex - 1;
            var t = _dEModPacks[currentIndex];
            _dEModPacks[currentIndex] = _dEModPacks[newIndex];
            _dEModPacks[newIndex] = t;

        }
        public void MoveDownMod(DEModPack modPack) {
            int currentIndex = _dEModPacks.IndexOf(modPack);
            if (currentIndex < 0) {
                return;
            }
            if (currentIndex >= _dEModPacks.Count - 1) {
                return;
            }
            int newIndex = currentIndex + 1;
            var t = _dEModPacks[currentIndex];
            _dEModPacks[currentIndex] = _dEModPacks[newIndex];
            _dEModPacks[newIndex] = t;
        }
        public void AddMod(string modName) {
            if (_gameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            if (string.IsNullOrEmpty(modName)) {
                throw new ArgumentException("模组名不可为空");
            }
            foreach (var modPack in _dEModPacks) {
                if (modPack.PackName == modName) {
                    throw new ArgumentException($"模组配置[{modName}]已存在，不可重复添加");
                }
            }
            DEModPack dmp = new DEModPack();
            dmp.PackName = modName;
            dmp.GameDirectroy = _gameDirectory;
            _dEModPacks.Add(dmp);
            if (_currentMod == null) {
                CurrentMod = dmp;
            }
        }
        public void RenameMod(DEModPack targetModPack, string newName, string description) {
            if (targetModPack.PackName != newName) {
                foreach (var modPack in _dEModPacks) {
                    if (modPack.PackName == newName) {
                        throw new ArgumentException($"模组配置名[{newName}]已存在");
                    }
                }
            }
            targetModPack.PackName = newName;
            targetModPack.Description = description;
        }
        public void DeleteMod(DEModPack modPack) {
            _dEModPacks.Remove(modPack);
        }
        public void DuplicateMod(DEModPack modPack) {
            DEModPack ddp = new DEModPack();
            ddp.PackName = modPack.PackName;
            ddp.Description = modPack.Description;
            foreach (var res in modPack.Resources) {
                ddp.Resources.Add(res);
            }
            _dEModPacks.Insert(_dEModPacks.IndexOf(modPack), ddp);
        }
        public void SaveToFile(string fileName) {
            Model.DEModManager dm = new Model.DEModManager();
            dm.GameDirectory = _gameDirectory;
            dm.CurrentMod = _currentMod?.PackName;
            dm.ModPacks = new List<Model.DEModPack>();
            foreach (var modPack in _dEModPacks) {
                Model.DEModPack dp = new Model.DEModPack();
                dp.PackName = modPack.PackName;
                dp.Description = modPack.Description;
                dp.Resources = new List<string>();
                dp.Resources.AddRange(modPack.Resources);
                dm.ModPacks.Add(dp);
            }
            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                _serializer.WriteObject(file, dm);
            }
        }
        public void LoadFromFile(string fileName) {
            Model.DEModManager dm;
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                dm = _serializer.ReadObject(file) as Model.DEModManager;
            }
            if (dm == null) {
                throw new InvalidDataException();
            }

            CurrentMod = null;
            GameDirectory = dm.GameDirectory;
            _dEModPacks.Clear();
            foreach (var modPack in dm.ModPacks) {
                DEModPack dp = new DEModPack();
                dp.PackName = modPack.PackName;
                dp.Description = modPack.Description;
                dp.GameDirectroy = dm.GameDirectory;
                foreach (var res in modPack.Resources) {
                    dp.Resources.Add(res);
                }
                _dEModPacks.Add(dp);
                if (CurrentMod == null && modPack.PackName == dm.CurrentMod) {
                    CurrentMod = dp;
                }
            }
        }

        private void ClearResources() {
            var fileList = Directory.GetFiles(ModsDirectory);
            foreach (var file in fileList) {
                string fileName = Path.GetDirectoryName(file);
                string modBackup = $@"{ModPacksDirectory}\{fileName}";
                if (!File.Exists(modBackup)) {
                    File.Move(file, modBackup);
                }
                else {
                    File.Delete(file);
                }
            }
        }
        private void SetResources() {
            foreach (var resourceName in _currentMod.Resources) {
                string sourceFile = $@"{ModPacksDirectory}\{resourceName}";
                string destFile = $@"{ModsDirectory}\{resourceName}";
                File.Copy(sourceFile, destFile);
            }
        }
    }
}
