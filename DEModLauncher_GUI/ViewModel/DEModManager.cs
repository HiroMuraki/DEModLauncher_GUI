﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Json;

namespace DEModLauncher_GUI.ViewModel {
    using DEModPacks = ObservableCollection<DEModPack>;
    public class DEModManager : ViewModelBase {
        private static readonly DEModManager _singletonIntance;
        private static readonly DataContractJsonSerializer _serializer;
        private bool _isLaunching;
        private DEModPack _currentMod;
        private readonly DEModPacks _dEModPacks;

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
                OnPropertyChanged(nameof(CurrentMod));
            }
        }
        public DEModPacks DEModPacks {
            get {
                return _dEModPacks;
            }
        }
        #endregion

        #region 构造方法
        static DEModManager() {
            _singletonIntance = new DEModManager();
            _serializer = new DataContractJsonSerializer(typeof(Model.DEModManager));
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
        public void LaunchModLoader() {
            if (_currentMod == null) {
                throw new InvalidOperationException("当前未选择有效模组");
            }
            _currentMod.Deploy();
            DOOMEternal.LaunchModLoader();
        }
        public void Launch() {
            DOOMEternal.LaunchGame();
        }
        public void MoveUpModPack(DEModPack modPack) {
            int currentIndex = _dEModPacks.IndexOf(modPack);
            if (currentIndex <= 0) {
                return;
            }
            int newIndex = currentIndex - 1;
            var t = _dEModPacks[currentIndex];
            _dEModPacks[currentIndex] = _dEModPacks[newIndex];
            _dEModPacks[newIndex] = t;

        }
        public void MoveDownModPack(DEModPack modPack) {
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
        public void AddModPack(DEModPack modPack) {
            if (string.IsNullOrEmpty(modPack.PackName)) {
                throw new ArgumentException("模组名不可为空");
            }
            foreach (var mp in _dEModPacks) {
                if (mp.PackName == modPack.PackName) {
                    throw new ArgumentException($"模组配置[{modPack.PackName}]已存在，不可重复添加");
                }
            }
            _dEModPacks.Add(modPack);
            CurrentMod = modPack;
        }
        public void RenameModPack(DEModPack targetModPack, string newName, string description) {
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
        public void RemoveModPack(DEModPack modPack) {
            _dEModPacks.Remove(modPack);
            if (_currentMod == modPack) {
                if (_dEModPacks.Count > 0) {
                    _currentMod = _dEModPacks[0];
                    OnPropertyChanged(nameof(CurrentMod));
                }
                else {
                    _currentMod = null;
                    OnPropertyChanged(nameof(CurrentMod));
                }
            }
        }
        public void DuplicateModPack(DEModPack modPack) {
            // 获取已经使用过的模组包名
            List<string> usedPackNames = new List<string>();
            foreach (var dmp in _dEModPacks) {
                usedPackNames.Add(dmp.PackName);
            }
            // 获取模组包副本
            DEModPack copiedPack = modPack.GetDeepCopy();
            // 移除被临时禁用的模组
            for (int i = 0; i < copiedPack.Resources.Count; i++) {
                if (copiedPack.Resources[i].Status == ResourceStatus.Disabled) {
                    copiedPack.Resources.Remove(copiedPack.Resources[i]);
                    --i;
                }
            }
            // 设置新模组包名，避免重复
            int cpyID = 1;
            string testName = copiedPack.PackName;
            while (usedPackNames.Contains(copiedPack.PackName)) {
                copiedPack.PackName = $"{testName}({cpyID})";
                ++cpyID;
            }
            _dEModPacks.Insert(_dEModPacks.IndexOf(modPack), copiedPack);
        }
        public void UpdateModResourceFile(string oldResourceName, string newResourceFile) {
            string newResourceName = Path.GetFileName(newResourceFile);
            // 如果新旧模组名同名，直接替换文件即可
            if (oldResourceName == newResourceName) {
                DOOMEternal.RemoveModResourceFile(oldResourceName);
                DOOMEternal.AddModResourceFile(newResourceFile);
                return;
            }
            // 否则逐一对模组配置中的相关文件进行修改
            DOOMEternal.AddModResourceFile(newResourceFile);
            foreach (var modPack in _dEModPacks) {
                // 如果模组列表中已有该模组，则将旧模组移除即可
                if (modPack.ExistsResource(newResourceName)) {
                    for (int i = 0; i < modPack.Resources.Count; i++) {
                        if (modPack.Resources[i].Path == oldResourceName) {
                            modPack.DeleteResource(modPack.Resources[i]);
                            --i;
                        }
                    }
                }
                // 否则将所有旧模组名替换为新模组名即可
                else {
                    foreach (var res in modPack.Resources) {
                        if (res.Path == oldResourceName) {
                            res.Path = newResourceName;
                        }
                    }
                }
            }
        }
        public void SaveToFile(string fileName) {
            Model.DEModManager dm = new Model.DEModManager();
            // 写入管理器属性
            dm.GameDirectory = DOOMEternal.GameDirectory;
            dm.ModLoader = DOOMEternal.ModLoader;
            dm.GameMainExecutor = DOOMEternal.GameMainExecutor;
            dm.CurrentMod = _currentMod?.PackName;
            // 写入ModPacks信息
            dm.ModPacks = new List<Model.DEModPack>();
            foreach (var modPack in _dEModPacks) {
                var dp = modPack.GetDataModel();
                dm.ModPacks.Add(dp);
            }
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                _serializer.WriteObject(fs, dm);
            }
        }
        public void LoadFromFile(string fileName) {
            // 读取文件
            Model.DEModManager dm;
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                dm = _serializer.ReadObject(file) as Model.DEModManager;
            }
            if (dm == null) {
                throw new InvalidDataException();
            }
            // 设置游戏目录，这条暂时无效
            DOOMEternal.GameDirectory = dm.GameDirectory;
            // 如果指定了ModLoader与游戏主程序，修改
            if (!string.IsNullOrEmpty(dm.ModLoader)) {
                DOOMEternal.ModLoader = dm.ModLoader;
            }
            if (!string.IsNullOrEmpty(dm.GameMainExecutor)) {
                DOOMEternal.GameMainExecutor = dm.GameMainExecutor;
            }
            // 读取模组包，同时设置CurrentMod
            CurrentMod = null;
            _dEModPacks.Clear();
            foreach (var modPack in dm.ModPacks) {
                DEModPack dp = new DEModPack(modPack);
                _dEModPacks.Add(dp);
                if (CurrentMod == null && modPack.PackName == dm.CurrentMod) {
                    CurrentMod = dp;
                }
            }
        }
        public List<string> ClearUnusedModFile() {
            // 获取当前正在使用的模组列表
            List<string> usedResources = new List<string>();
            foreach (var modPack in _dEModPacks) {
                foreach (var resource in modPack.Resources) {
                    string filePath = $@"{DOOMEternal.ModPacksDirectory}\{resource.Path}";
                    if (!usedResources.Contains(filePath)) {
                        usedResources.Add(filePath);
                    }
                }
            }
            // 配置文件不能删
            usedResources.Add(DOOMEternal.LauncherProfileFile);
            // 查找未使用的模组文件并移除
            var existedModFiles = Directory.GetFiles(DOOMEternal.ModPacksDirectory);
            var removedFiles = FileCleaner(usedResources, existedModFiles);
            return removedFiles;
        }
        public List<string> ClearUnusedImageFiles() {
            // 获取当前正在使用的图片文件名
            List<string> usedImageFiles = new List<string>();
            foreach (var modPack in _dEModPacks) {
                // 跳过默认图片
                if (modPack.ImagePath == DOOMEternal.DefaultModPackImage) {
                    continue;
                }
                string imageName = modPack.ImagePath;
                if (!usedImageFiles.Contains(imageName)) {
                    usedImageFiles.Add(imageName);
                }
            }
            // 查找未使用的图片文件并移除
            var existedImageFiles = Directory.GetFiles(DOOMEternal.ModPackImagesDirectory);
            var removedFiles = FileCleaner(usedImageFiles, existedImageFiles);
            return removedFiles;
        }
        public List<string> GetUsedMods() {
            List<string> usedMods = new List<string>();
            foreach (var modPack in _dEModPacks) {
                foreach (var res in modPack.Resources) {
                    if (!usedMods.Contains(res.Path)) {
                        usedMods.Add(res.Path);
                    }
                }
            }
            usedMods.Sort();
            return usedMods;
        }
        public static void OpenResourceFile(string resourceName) {
            Process p = new Process();
            string filePath = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"无法找到文件：{filePath}");
            }
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = $@"/select, {DOOMEternal.ModPacksDirectory}\{resourceName}";
            p.Start();
        }
        public static void ExportModPacks(string outputPath) {
            ZipFile.CreateFromDirectory(DOOMEternal.ModPacksDirectory, outputPath, CompressionLevel.Optimal, true);
        }
        #endregion

        private List<string> FileCleaner(ICollection<string> preservedFiles, IEnumerable<string> allFiles) {
            var removedFiles = new List<string>();
            foreach (var file in allFiles) {
                if (!preservedFiles.Contains(file)) {
                    File.Delete(file);
                    removedFiles.Add(Path.GetFileName(file));
                }
            }
            return removedFiles;
        }
    }
}
