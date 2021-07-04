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
    public class DEModManager : ViewModelBase {
        private static readonly DEModManager _singletonIntance;
        private static readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(Model.DEModManager));
        private static readonly string _windowsExplorerExecutor = "explorer.exe";
        private bool _isLaunching;
        private DEModPack _currentMod;
        private readonly DEModPacks _dEModPacks;
        private string _consoleStandardOutput;

        #region 公共属性
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
        #endregion

        #region 构造方法
        static DEModManager() {
            _singletonIntance = new DEModManager();
        }
        private DEModManager() {
            _currentMod = null;
            _dEModPacks = new DEModPacks();
        }
        public static DEModManager GetInstance() {
            if (_singletonIntance == null) {
                throw new Exception("FATAL ERROR, RESTART REQUIRED");
            }
            return _singletonIntance;
        }
        #endregion

        #region 公共方法
        public StreamReader LaunchWithModLoader() {
            if (_currentMod == null) {
                throw new InvalidOperationException("当前未选择有效模组");
            }
            return _currentMod.Launch();
        }
        public void Launch() {
            DOOMEternal.LaunchGame();
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
            if (DOOMEternal.GameDirectory == null) {
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
            foreach (var res in modPack.Resources) {
                copiedPack.Resources.Add(res);
            }
            _dEModPacks.Insert(_dEModPacks.IndexOf(modPack) + 1, copiedPack);
        }
        public void SaveToFile(string fileName) {
            Model.DEModManager dm = new Model.DEModManager();
            // 写入管理器属性
            dm.GameDirectory = DOOMEternal.GameDirectory;
            dm.ModLoader = DOOMEternal.ModLoader;
            dm.GameMainExecutor = DOOMEternal.GameMainExecutor;
            dm.CurrentMod = _currentMod?.PackName;
            dm.ModPacks = new List<Model.DEModPack>();
            // 写入ModPacks信息
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
            DOOMEternal.GameDirectory = dm.GameDirectory;
            if (!string.IsNullOrEmpty(dm.ModLoader)) {
                DOOMEternal.ModLoader = dm.ModLoader;
            }
            if (!string.IsNullOrEmpty(dm.GameMainExecutor)) {
                DOOMEternal.GameMainExecutor = dm.GameMainExecutor;
            }
            _dEModPacks.Clear();
            foreach (var modPack in dm.ModPacks) {
                DEModPack dp = new DEModPack();
                dp.PackName = modPack.PackName;
                dp.Description = modPack.Description;
                dp.SetImage(modPack.ImagePath);
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
            p.StartInfo.Arguments = $@"/e, { DOOMEternal.GameDirectory}";
            p.Start();
        }
        public void OpenResourceFile(string resourceName) {
            Process p = new Process();
            string filePath = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"无法找到文件：{filePath}");
            }
            p.StartInfo.FileName = _windowsExplorerExecutor;
            p.StartInfo.Arguments = $@"/select, {DOOMEternal.ModPacksDirectory}\{resourceName}";
            p.Start();
        }
        #endregion
    }
}
