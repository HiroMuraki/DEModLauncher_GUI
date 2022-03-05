using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using ModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.ViewModel.DEModPack>;

namespace DEModLauncher_GUI.ViewModel {
    public class DEModManager : ViewModelBase {
        #region 事件
        public event Action? CurrentModPackChanged;
        #endregion

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
        public DEModPack CurrentModPack {
            get {
                return _currentModPack;
            }
            private set {
                _currentModPack = value;
                OnPropertyChanged(nameof(CurrentModPack));
                CurrentModPackChanged?.Invoke();
            }
        }
        public ModPacks ModPacks { get; } = new ModPacks();
        public DEModResource[] UsedModResources {
            get {
                var ress = new List<DEModResource>();
                foreach (var modPack in ModPacks) {
                    foreach (var res in modPack.Resources) {
                        // 如果ress中未出现该模组，则加入
                        bool isDistinct = true;
                        foreach (var existed in ress) {
                            if (existed.Path == res.Path) {
                                isDistinct = false;
                                break;
                            }
                        }
                        if (isDistinct) {
                            ress.Add(res);
                        }
                    }
                }
                ress.Distinct();
                ress.Sort();

                return ress.ToArray();
            }
        }
        #endregion

        #region 公共方法
        public void LoadMod() {
            DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 0);
            LoadModHelper();

        }
        public void LaunchMod() {
            DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 1);
            LoadModHelper();
        }
        public void LaunchGame() {
            DOOMEternal.LaunchGame();
        }

        public void Initialize() {
            SetDefaultModPack();
        }
        public bool IsValidModPackSelected() {
            return !ReferenceEquals(CurrentModPack, _noModPack);
        }
        public void SetCurrentModPack(DEModPack modPack) {
            foreach (var item in ModPacks) {
                item.ToggleOff();
            }
            modPack.ToggleOn();
            CurrentModPack = modPack;
        }
        public DEModPack NewModPack() {
            var t = new DEModPack();
            AddModPackHelper(t);
            return t;
        }
        public DEModPack DuplicateModPack(DEModPack modPack) {
            // 获取已经使用过的模组包名
            var usedPackNames = new List<string>();
            foreach (var dmp in ModPacks) {
                usedPackNames.Add(dmp.PackName);
            }
            // 获取模组包副本
            var copiedPack = new DEModPack().LoadFromModel(modPack.ConvertToModel());
            // 移除被临时禁用的模组
            for (int i = 0; i < copiedPack.Resources.Count; i++) {
                if (copiedPack.Resources[i].Status == Status.Disable) {
                    copiedPack.Resources.Remove(copiedPack.Resources[i]);
                    --i;
                }
            }
            // 设置新模组包名，避免重复
            int cpyID = 1;
            string testName = copiedPack.PackName;
            while (usedPackNames.Contains(copiedPack.PackName)) {
                copiedPack.PackName = $"{testName} - 副本[{cpyID}]";
                ++cpyID;
            }
            ModPacks.Insert(ModPacks.IndexOf(modPack), copiedPack);
            DOOMEternal.ModificationSaved = false;
            return copiedPack;
        }
        public DEModPack RemoveModPack(DEModPack modPack) {
            ModPacks.Remove(modPack);
            if (ReferenceEquals(CurrentModPack, modPack)) {
                if (ModPacks.Count > 0) {
                    CurrentModPack = ModPacks[0];
                }
                else {
                    CurrentModPack = _noModPack;
                }
                OnPropertyChanged(nameof(CurrentModPack));
                CurrentModPackChanged?.Invoke();
            }
            if (ModPacks.Count <= 0) {
                SetDefaultModPack();
            }
            SetCurrentModPack(ModPacks[0]);
            DOOMEternal.ModificationSaved = false;
            return modPack;
        }
        public DEModPack ResortModPack(int index, DEModPack source) {
            ModPacks.ReInsert(index, source);
            DOOMEternal.ModificationSaved = false;
            return source;
        }
        public DEModResource UpdateResource(DEModResource resource, string resourcePath) {
            string oldResourceName = resource.Path;
            string newResourceFile = resourcePath;
            string newResourceName = Path.GetFileName(newResourceFile);
            // 如果新旧模组名同名，直接替换文件即可
            if (oldResourceName == newResourceName) {
                RemoveModResourceFileBackup(oldResourceName);
                BackupModResourceFile(newResourceFile);
            }
            else {
                // 否则逐一对模组配置中的相关文件进行修改
                BackupModResourceFile(newResourceFile);
                var newResource = new DEModResource(newResourceName);
                foreach (var modPack in ModPacks) {
                    // 如果模组列表中已有该模组，则将旧模组移除即可
                    if (modPack.ContainsResource(newResource)) {
                        for (int i = 0; i < modPack.Resources.Count; i++) {
                            if (modPack.Resources[i].Path == oldResourceName) {
                                modPack.RemoveResource(modPack.Resources[i]);
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
                OnPropertyChanged(nameof(UsedModResources));
            }
            return resource;
        }

        /// <summary>
        /// 保存配置至文件
        /// </summary>
        /// <param name="fileName">保存的文件位置</param>
        public void SaveProfile(string fileName) {
            var dm = new Model.DEModManager {
                // 写入管理器属性
                GameDirectory = DOOMEternal.GameDirectory,
                ModLoader = DOOMEternal.ModLoader,
                GameMainExecutor = DOOMEternal.GameMainExecutor,
                ModDirectory = "",
                ModPacksDirectory = "",
                ModPackImageDirectory = "",
                CurrentMod = CurrentModPack.PackName ?? "",
                // 写入ModPacks信息
                ModPacks = (from modPack in ModPacks select modPack.ConvertToModel()).ToArray()
            };

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                _serializer.WriteObject(fs, dm);
            }
        }
        /// <summary>
        /// 从文件中读取配置
        /// </summary>
        /// <param name="fileName">读取的文件路径</param>
        public void LoadProfile(string fileName) {
            // 读取文件
            Model.DEModManager? dm;
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                dm = _serializer.ReadObject(file) as Model.DEModManager;
                if (dm == null) {
                    throw new InvalidDataException();
                }
            }
            // 读取配置项
            if (!string.IsNullOrEmpty(dm.GameMainExecutor)) {
                DOOMEternal.GameMainExecutor = dm.GameMainExecutor;
            }
            if (!string.IsNullOrEmpty(dm.ModLoader)) {
                DOOMEternal.ModLoader = dm.ModLoader;
            }
            // 读取模组包，同时设置CurrentMod
            CurrentModPack = _noModPack;
            ModPacks.Clear();
            foreach (var item in dm.ModPacks) {
                var modPack = NewModPack().LoadFromModel(item);
                if (!IsValidModPackSelected() && item.PackName == dm.CurrentMod) {
                    SetCurrentModPack(modPack);
                }
            }
            if (ModPacks.Count <= 0) {
                SetDefaultModPack();
            }
        }
        /// <summary>
        /// 清理未使用的MOD文件
        /// </summary>
        /// <returns>被清除的文件列表</returns>
        public void ClearUnusedModFile(out string[] removedFiles) {
            // 获取当前正在使用的模组列表
            var usedResources = new List<string>();
            foreach (var modPack in ModPacks) {
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
            string[] existedModFiles = Directory.GetFiles(DOOMEternal.ModPacksDirectory);
            removedFiles = Util.FilesCleaner(usedResources, existedModFiles).ToArray();
        }
        /// <summary>
        /// 清理未使用的图像文件
        /// </summary>
        /// <returns>被清除的图像列表</returns>
        public void ClearUnusedImageFiles(out string[] removedFiles) {
            // 获取当前正在使用的图片文件名
            var usedImageFiles = new List<string>();
            foreach (var modPack in ModPacks) {
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
            string[] existedImageFiles = Directory.GetFiles(DOOMEternal.ModPackImagesDirectory);
            removedFiles = Util.FilesCleaner(usedImageFiles, existedImageFiles).ToArray();
        }
        #endregion

        #region 构造方法
        private DEModManager() {

        }
        public static DEModManager GetInstance() {
            if (_singletonIntance == null) {
                throw new Exception("FATAL ERROR, RESTART REQUIRED");
            }
            return _singletonIntance;
        }
        #endregion

        #region 辅助方法
        private static readonly DEModPack _noModPack = new DEModPack();
        private static readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(Model.DEModManager));
        private static readonly DEModManager _singletonIntance = new DEModManager();
        private bool _isLaunching;
        private DEModPack _currentModPack = _noModPack;
        /// <summary>
        /// 备份模组文件
        /// </summary>
        /// <param name="filePath"></param>
        private static void BackupModResourceFile(string filePath) {
            if (DOOMEternal.GameDirectory == null) {
                throw new ArgumentException("请先选择游戏文件夹");
            }
            if (!Directory.Exists(DOOMEternal.ModPacksDirectory)) {
                Directory.CreateDirectory(DOOMEternal.ModPacksDirectory);
            }
            string resourceName = Path.GetFileName(filePath);
            string modPackBackup = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(modPackBackup)) {
                File.Copy(filePath, modPackBackup);
            }
        }
        /// <summary>
        ///  移除模组文件备份
        /// </summary>
        /// <param name="modName"></param>
        private static void RemoveModResourceFileBackup(string modName) {
            File.Delete($"{DOOMEternal.ModPacksDirectory}\\{modName}");
        }
        /// <summary>
        /// 调用模组加载器
        /// </summary>
        public bool LoadModHelper() {
            try {
                IsLaunching = true;
                if (!IsValidModPackSelected()) {
                    throw new InvalidOperationException("当前未选择有效模组");
                }
                CurrentModPack.Deploy();
                DOOMEternal.LaunchModLoader();
                return true;
            }
            catch {
                return false;
            }
            finally {
                IsLaunching = false;
            }
        }
        /// <summary>
        /// 添加默认模组
        /// </summary>
        private void SetDefaultModPack() {
            if (ModPacks.Count > 0) {
                return;
            }
            var modPack = NewModPack();
            modPack.PackName = "默认模组";
            modPack.Description = "模组描述";
            modPack.SetImage(DOOMEternal.DefaultModPackImage);
        }
        /// <summary>
        /// 添加模组配置
        /// </summary>
        /// <param name="modPack">要添加的模组配置</param>
        private void AddModPackHelper(DEModPack modPack) {
            foreach (var mp in ModPacks) {
                if (mp.PackName == modPack.PackName) {
                    throw new ArgumentException($"模组配置[{modPack.PackName}]已存在，不可重复添加");
                }
            }
            ModPacks.Add(modPack);
            CurrentModPack = modPack;
        }
        #endregion
    }
}