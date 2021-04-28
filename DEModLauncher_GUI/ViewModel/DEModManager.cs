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
        private static readonly string _windowsExplorerExecutor = "explorer.exe";
        private string _gameMainExecutor = "DOOMEternalx64vk.exe";
        private string _modLoader = "EternalModInjector.bat";
        private string _gameDirectory;
        private bool _isLaunching;
        private DEModPack _currentMod;
        private readonly DEModPacks _dEModPacks;
        private string _consoleStandardOutput;

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
                if (value != null) {
                    value.IsSelected = true;
                }
                OnPropertyChanged(nameof(CurrentMod));
            }
        }
        public DEModPacks DEModPacks {
            get {
                return _dEModPacks;
            }
        }
        public string ConsoleStandardOutput {
            get {
                return _consoleStandardOutput;
            }
            set {
                _consoleStandardOutput = value;
                OnPropertyChanged(nameof(ConsoleStandardOutput));
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
            return _currentMod.Launch(_modLoader);
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
        public void AddMod(string modName, string description) {
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
            dmp.Description = description;
            dmp.GameDirectroy = _gameDirectory;
            _dEModPacks.Add(dmp);
            CurrentMod = dmp;
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
            DEModPack copiedPack = new DEModPack();
            List<string> usedPackNames = new List<string>();
            foreach (var dmp in _dEModPacks) {
                usedPackNames.Add(dmp.PackName);
            }
            int cpyID = 1;
            string testName = $"{modPack.PackName}({cpyID})";
            while (usedPackNames.Contains(testName)) {
                testName = $"{modPack.PackName}({cpyID})";
                ++cpyID;
            }
            copiedPack.PackName = testName;
            copiedPack.Description = modPack.Description;
            copiedPack.GameDirectroy = modPack.GameDirectroy;
            foreach (var res in modPack.Resources) {
                copiedPack.Resources.Add(res);
            }
            _dEModPacks.Insert(_dEModPacks.IndexOf(modPack) + 1, copiedPack);
        }
        public void SaveToFile(string fileName) {
            Model.DEModManager dm = new Model.DEModManager();
            dm.GameDirectory = _gameDirectory;
            dm.ModLoader = _modLoader;
            dm.GameMainExecutor = _gameMainExecutor;
            dm.CurrentMod = _currentMod?.PackName;
            dm.ModPacks = new List<Model.DEModPack>();
            foreach (var modPack in _dEModPacks) {
                Model.DEModPack dp = new Model.DEModPack();
                dp.PackName = modPack.PackName;
                dp.Description = modPack.Description;
                dp.Resources = new List<string>();
                foreach (var res in modPack.Resources) {
                    dp.Resources.Add(res.Path);
                }
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
            if (!string.IsNullOrEmpty(dm.ModLoader)) {
                _modLoader = dm.ModLoader;
            }
            if (!string.IsNullOrEmpty(dm.GameMainExecutor)) {
                _gameMainExecutor = dm.GameMainExecutor;
            }
            _dEModPacks.Clear();
            foreach (var modPack in dm.ModPacks) {
                DEModPack dp = new DEModPack();
                dp.PackName = modPack.PackName;
                dp.Description = modPack.Description;
                dp.GameDirectroy = dm.GameDirectory;
                foreach (var res in modPack.Resources) {
                    DEModResource resource = new DEModResource(res);
                    dp.Resources.Add(resource);
                }
                _dEModPacks.Add(dp);
                if (CurrentMod == null && modPack.PackName == dm.CurrentMod) {
                    CurrentMod = dp;
                }
            }
        }
        public void OpenGameDirectory() {
            Process p = new Process();
            p.StartInfo.FileName = _windowsExplorerExecutor;
            p.StartInfo.Arguments = $@"/e, {_gameDirectory}";
            p.Start();
        }
        public void OpenResourceFile(string resourceName) {
            Process p = new Process();
            string filePath = $@"{ModPacksDirectory}\{resourceName}";
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"无法找到文件：{filePath}");
            }
            p.StartInfo.FileName = _windowsExplorerExecutor;
            p.StartInfo.Arguments = $@"/select, {ModPacksDirectory}\{resourceName}";
            p.Start();
        }
    }
}
