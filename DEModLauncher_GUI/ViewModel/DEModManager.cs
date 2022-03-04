using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using ModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.ViewModel.DEModPack>;

namespace DEModLauncher_GUI.ViewModel {
    public class DEModManager : ViewModelBase {
        private static readonly DEModPack _noModPack = new DEModPack();
        private static readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(Model.DEModManager));
        private static readonly DEModManager _singletonIntance = new DEModManager();
        private string _preOpenModDirectory = "";
        private bool _isLaunching;

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
        public async void LoadMod() {
            try {
                DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 0);
                await LoadModHelper();
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "模组启动错误", Application.Current.MainWindow);
            }
        }
        public async void LaunchMod() {
            try {
                DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 1);
                bool launched = await LoadModHelper();
                if (launched) {
                    App.Close();
                }
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "模组启动错误", Application.Current.MainWindow);
            }
        }
        public async void LaunchGame() {
            try {
                await Task.Run(() => {
                    DOOMEternal.LaunchGame();
                });
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "游戏启动错误", Application.Current.MainWindow);
            }
        }

        public void SetCurrentModPack(DEModPack modPack) {
            foreach (var item in ModPacks) {
                item.ToggleOff();
            }
            modPack.ToggleOn();
            CurrentModPack = modPack;
        }
        public void NewModPack() {
            try {
                var setter = new View.DEModPackSetter() { Owner = Application.Current.MainWindow };
                setter.PackName = "模组名";
                setter.Description = "描述信息";
                setter.ImagePath = DOOMEternal.DefaultModPackImage;
                if (setter.ShowDialog() == true) {
                    var modPack = new DEModPack();
                    modPack.PackName = setter.PackName;
                    modPack.Description = setter.Description;
                    modPack.SetImage(setter.ImagePath);
                    AddModPackHelper(modPack);
                    SetCurrentModPack(modPack);
                    DOOMEternal.ModificationSaved = false;
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "添加模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ResortModPack(int index, DEModPack source) {
            ModPacks.ReInsert(index, source);
            DOOMEternal.ModificationSaved = false;
        }
        public void DuplicateModPack(DEModPack modPack) {
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
        }
        public void RemoveModPack(DEModPack modPack) {
            var result = MessageBox.Show($"是否删除模组配置：{modPack.PackName}", "警告",
                                         MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
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
        }

        public void Initialize() {
            SetDefaultModPack();
        }
        public void SaveProfile() {
            var result = MessageBox.Show("是否保存当前模组配置？", "保存配置", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {
                SaveProfileHelper(DOOMEternal.LauncherProfileFile);
                DOOMEternal.ModificationSaved = true;
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "保存配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadProfile() {
            var result = MessageBox.Show("此操作将会重新读取模组配置文件，并丢弃当前设置，是否继续？", "重新读取", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {
                LoadProfileHelper(DOOMEternal.LauncherProfileFile);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "读取配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadProfile(string file) {
            LoadProfileHelper(file);
        }

        public void UpdateResource(DEModResource resource) {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.InitialDirectory = _preOpenModDirectory ?? DOOMEternal.ModPacksDirectory;
            ofd.Title = $"替换{resource.Path}";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string oldResourceName = resource.Path;
                string newResourceFile = ofd.FileName;
                string newResourceName = Path.GetFileName(newResourceFile);
                // 如果新旧模组名同名，直接替换文件即可
                if (oldResourceName == newResourceName) {
                    RemoveModResourceFileBackup(oldResourceName);
                    BackupModResourceFile(newResourceFile);
                    return;
                }
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
                _preOpenModDirectory = Path.GetDirectoryName(ofd.FileName) ?? "";
            }
        }
        public void OpenResourceFile(DEModResource resource) {
            try {
                var p = new Process();
                string filePath = $@"{DOOMEternal.ModPacksDirectory}\{resource.Path}";
                if (!File.Exists(filePath)) {
                    throw new FileNotFoundException($"无法找到文件：{filePath}");
                }
                p.StartInfo.FileName = "explorer.exe";
                p.StartInfo.Arguments = $@"/select, {DOOMEternal.ModPacksDirectory}\{resource.Path}";
                p.Start();
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "打开错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ResetModLoader() {
            var result = MessageBox.Show("重置模组加载器将会移除由其产生的文件备份与配置文件\n" +
                                         "请在重置前或重置后验证游戏文件完整性\n" +
                                         "是否继续",
                                         "警告",
                                         MessageBoxButton.OKCancel,
                                         MessageBoxImage.Warning);
            if (result != MessageBoxResult.OK) {
                return;
            }
            var removedFiles = new List<string>();
            removedFiles.Add("重置完成，以下文件被移除");
            // 移除哈希信息与模组加载器配置文件
            string[] modLoaderProfiles = new string[2] {
                $"{DOOMEternal.GameDirectory}\\base\\idRehash.map",
                $"{DOOMEternal.GameDirectory}\\{DOOMEternal.ModLoaderProfileFile}"
            };
            foreach (string file in modLoaderProfiles) {
                if (File.Exists(file)) {
                    File.Delete(file);
                    removedFiles.Add(file);
                }
            }
            // 移除备份文件
            foreach (string file in Util.TravelFiles(DOOMEternal.GameDirectory)) {
                if (Path.GetExtension(file) == ".backup") {
                    File.Delete(file);
                    removedFiles.Add(file);
                }
            }
            View.InformationWindow.Show(string.Join('\n', removedFiles), "重置完成", Application.Current.MainWindow);
        }
        public void ExportModPacks() {
            var sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = DOOMEternal.GameDirectory;
            sfd.FileName = $@"ModPacks.zip";
            sfd.Filter = "ZIP压缩包|*.zip";
            sfd.Title = "选择导出的文件";
            try {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    ZipFile.CreateFromDirectory(DOOMEternal.ModPacksDirectory, sfd.FileName, CompressionLevel.Optimal, true);
                    MessageBox.Show("模组包导出完成");
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "模组包导出错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ClearUnusedImageFiles() {
            var result = MessageBox.Show("该操作将会移除被使用的图片文件，是否继续?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                try {
                    var removedFiles = ClearUnusedImageFilesHelper();
                    string outputInf = "清理完成，以下文件被移除:\n" + string.Join('\n', removedFiles);
                    View.InformationWindow.Show(outputInf, "清理完成", Application.Current.MainWindow);
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "文件清理出错", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void ClearUnusedModFiles() {
            var result = MessageBox.Show("该操作将会移除被使用的模组文件，是否继续?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                try {
                    var removedFiles = ClearUnusedModFileHelper();
                    string outputInf = "清理完成，以下文件被移除:\n" + string.Join('\n', removedFiles);
                    View.InformationWindow.Show(outputInf, "清理完成", Application.Current.MainWindow);
                }
                catch (Exception exp) {
                    MessageBox.Show(exp.Message, "文件清理出错", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
        /// 检查当前选择的模组是否有效
        /// </summary>
        /// <returns></returns>
        private bool IsValidModPackSelected() {
            return !ReferenceEquals(CurrentModPack, _noModPack);
        }
        /// <summary>
        /// 调用模组加载器
        /// </summary>
        private async Task<bool> LoadModHelper() {
            if (!IsValidModPackSelected()) {
                MessageBox.Show("请先选择一个模组配置", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            // 弹出提示窗口，避免误操作
            var result = MessageBox.Show($"加载模组将需要一定时间，在此期间请勿关闭本程序。是否继续?",
                                         $"加载模组：{CurrentModPack.PackName}",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return false;
            }
            IsLaunching = true;
            try {
                await Task.Run(() => {
                    LaunchModLoaderHelper();
                });
                return true;
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "模组启动错误", Application.Current.MainWindow);
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
            var modPack = new DEModPack();
            modPack.PackName = "默认模组";
            modPack.Description = "模组描述";
            modPack.SetImage(DOOMEternal.DefaultModPackImage);
            AddModPackHelper(modPack);
        }
        /// <summary>
        /// 启动模组加载器
        /// </summary>
        private void LaunchModLoaderHelper() {
            if (IsValidModPackSelected()) {
                throw new InvalidOperationException("当前未选择有效模组");
            }
            CurrentModPack.Deploy();
            DOOMEternal.LaunchModLoader();
        }
        /// <summary>
        /// 添加模组配置
        /// </summary>
        /// <param name="modPack">要添加的模组配置</param>
        private void AddModPackHelper(DEModPack modPack) {
            if (string.IsNullOrEmpty(modPack.PackName)) {
                throw new ArgumentException("模组名不可为空");
            }
            foreach (var mp in ModPacks) {
                if (mp.PackName == modPack.PackName) {
                    throw new ArgumentException($"模组配置[{modPack.PackName}]已存在，不可重复添加");
                }
            }
            ModPacks.Add(modPack);
            CurrentModPack = modPack;
        }
        /// <summary>
        /// 保存配置至文件
        /// </summary>
        /// <param name="fileName">保存的文件位置</param>
        private void SaveProfileHelper(string fileName) {
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
        private void LoadProfileHelper(string fileName) {
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
                var modPack = new DEModPack().LoadFromModel(item);
                ModPacks.Add(modPack);
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
        private List<string> ClearUnusedModFileHelper() {
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
            var removedFiles = Util.FilesCleaner(usedResources, existedModFiles);
            return removedFiles;
        }
        /// <summary>
        /// 清理未使用的图像文件
        /// </summary>
        /// <returns>被清除的图像列表</returns>
        private List<string> ClearUnusedImageFilesHelper() {
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
            var removedFiles = Util.FilesCleaner(usedImageFiles, existedImageFiles);
            return removedFiles;
        }
        #endregion
    }
}