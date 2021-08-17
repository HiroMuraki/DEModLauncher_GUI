using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModPack>;
using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModResource>;

namespace DEModLauncher_GUI.ViewModel {
    public class DEModManager : ViewModelBase, IModManager {
        private static readonly DEModManager _singletonIntance;
        private static readonly DataContractJsonSerializer _serializer;
        private string _preOpenModDirectory = "";
        private bool _isLaunching;
        private DEModPack _currentMod;
        private Resources _usingMods;
        private readonly ModPacks _dEModPacks;

        #region IModManager接口实现
        #region 属性
        public bool IsLaunching {
            get {
                return _isLaunching;
            }
            set {
                _isLaunching = value;
                OnPropertyChanged(nameof(IsLaunching));
            }
        }
        public IModPack CurrentMod {
            get {
                return _currentMod;
            }
            set {
                _currentMod = (DEModPack)value;
                OnPropertyChanged(nameof(CurrentMod));
            }
        }
        public ModPacks ModPacks {
            get {
                return _dEModPacks;
            }
        }
        public Resources UsingMods {
            get {
                List<DEModResource> ress = new List<DEModResource>();
                foreach (var modPack in _dEModPacks) {
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
                            ress.Add((DEModResource)res);
                        }
                    }
                }
                ress.Distinct();
                ress.Sort();

                _usingMods.Clear();
                foreach (var item in ress) {
                    _usingMods.Add(item);
                }
                return _usingMods;
            }
        }
        #endregion
        #region 方法
        public async void LoadMod() {
            if (_currentMod == null) {
                MessageBox.Show("请先选择一个模组配置", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 弹出提示窗口，避免误操作
            var result = MessageBox.Show($"加载模组将需要一定时间，在此期间请勿关闭本程序。是否继续?",
                                         $"加载模组：{_currentMod.PackName}",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            // 进入启动程序
            IsLaunching = true;
            try {
                DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 0);
                await Task.Run(() => {
                    LaunchModLoaderHelper();
                });
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "模组启动错误", Application.Current.MainWindow);
                return;
            }
            finally {
                IsLaunching = false;
            }
        }
        public async void LaunchMod() {
            if (_currentMod == null) {
                MessageBox.Show("请先选择一个模组配置", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 弹出提示窗口，避免误操作
            var result = MessageBox.Show($"加载模组将需要一定时间，在此期间请勿关闭本程序。是否继续?",
                                         $"加载模组：{_currentMod.PackName}",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            // 进入启动程序
            IsLaunching = true;
            try {
                DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 1);
                await Task.Run(() => {
                    LaunchModLoaderHelper();
                });
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "模组启动错误", Application.Current.MainWindow);
                return;
            }
            finally {
                IsLaunching = false;
            }
            App.Close();
        }
        public void LaunchGame() {
            try {
                LaunchHelper();
            }
            catch (Exception exp) {
                View.InformationWindow.Show(exp.Message, "模组启动错误", Application.Current.MainWindow);
                return;
            }
            App.Close();
        }
        public void SaveProfiles() {
            var result = MessageBox.Show("是否保存当前模组配置？", "保存配置", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {
                SaveProfileHelper(DOOMEternal.LauncherProfileFile);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "保存配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadProfiles() {
            var result = MessageBox.Show("此操作将会重新读取模组配置文件，并丢弃当前设置，是否继续？", "重新读取", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            try {

                LoadProfilesHelper(DOOMEternal.LauncherProfileFile);
            }
            catch (Exception exp) {

                MessageBox.Show(exp.Message, "读取配置文件出错", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadProfiles(string file) {
            LoadProfilesHelper(file);
        }
        public void SetCurrentMod(IModPack currentMod) {
            CurrentMod = currentMod;
        }
        public void AddModPack() {
            try {
                View.DEModPackSetter setter = new View.DEModPackSetter() { Owner = Application.Current.MainWindow };
                setter.PackName = "模组名";
                setter.Description = "描述信息";
                setter.ImagePath = DOOMEternal.DefaultModPackImage;
                if (setter.ShowDialog() == true) {
                    DEModPack modPack = new DEModPack();
                    modPack.PackName = setter.PackName;
                    modPack.Description = setter.Description;
                    modPack.SetImage(setter.ImagePath);
                    AddModPackHelper(modPack);
                }
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "添加模组配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void RemoveModPack(IModPack modPack) {
            var result = MessageBox.Show($"是否删除模组配置：{modPack.PackName}", "警告",
                                         MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            RemoveModPackHelper((DEModPack)modPack);
        }
        public void DuplicateModPack(IModPack modPack) {
            DuplicateModPackHelper((DEModPack)modPack);
        }
        public void ResortModPack(IModPack source, IModPack target) {
            ResortModPackHelper((DEModPack)source, (DEModPack)target);
        }
        public void UpdateResource(IModResource resource) {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.InitialDirectory = _preOpenModDirectory ?? DOOMEternal.ModPacksDirectory;
            ofd.Title = $"替换{resource.Path}";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string newFileName = ofd.FileName;
                UpdateResourceFileHelper(resource.Path, newFileName);
                OnPropertyChanged(nameof(UsingMods));
                _preOpenModDirectory = Path.GetDirectoryName(newFileName);
            }
        }
        public void OpenResourceFile(IModResource resource) {
            try {
                OpenResourceFileHelper(resource.Path);
            }
            catch (Exception exp) {
                MessageBox.Show(exp.Message, "打开错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
        public void ExportModPacks() {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = DOOMEternal.GameDirectory;
            sfd.FileName = $@"ModPacks.zip";
            sfd.Filter = "ZIP压缩包|*.zip";
            sfd.Title = "选择导出的文件";
            try {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    ExportModPacksHelper(sfd.FileName);
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
        static DEModManager() {
            _singletonIntance = new DEModManager();
            _serializer = new DataContractJsonSerializer(typeof(Model.DEModManager));
        }
        private DEModManager() {
            _currentMod = null;
            _dEModPacks = new ModPacks();
            _usingMods = new Resources();
        }
        public static DEModManager GetInstance() {
            if (_singletonIntance == null) {
                throw new Exception("FATAL ERROR, RESTART REQUIRED");
            }
            return _singletonIntance;
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 启动魔族加载器
        /// </summary>
        private void LaunchModLoaderHelper() {
            if (_currentMod == null) {
                throw new InvalidOperationException("当前未选择有效模组");
            }
            _currentMod.Deploy();
            DOOMEternal.LaunchModLoader();
        }
        /// <summary>
        /// 直接启动游戏
        /// </summary>
        private void LaunchHelper() {
            DOOMEternal.LaunchGame();
        }
        /// <summary>
        /// 添加模组配置
        /// </summary>
        /// <param name="modPack">要添加的模组配置</param>
        private void AddModPackHelper(DEModPack modPack) {
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
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 移除指定模组配置
        /// </summary>
        /// <param name="modPack">要移除的模组配置</param>
        private void RemoveModPackHelper(DEModPack modPack) {
            _dEModPacks.Remove(modPack);
            if (_currentMod == modPack) {
                if (_dEModPacks.Count > 0) {
                    _currentMod = (DEModPack)_dEModPacks[0];
                    OnPropertyChanged(nameof(CurrentMod));
                }
                else {
                    _currentMod = null;
                    OnPropertyChanged(nameof(CurrentMod));
                }
            }
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 将指定模组配置插入到指定模组之前
        /// </summary>
        /// <param name="source">要插入的模组配置</param>
        /// <param name="target">目标模组配置</param>
        private void ResortModPackHelper(DEModPack source, DEModPack target) {
            if (ReferenceEquals(source, target)) {
                return;
            }
            _dEModPacks.Remove(source);
            _dEModPacks.Insert(_dEModPacks.IndexOf(target), source);
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 制作模组文件副本
        /// </summary>
        /// <param name="modPack">要拷贝的模组</param>
        private void DuplicateModPackHelper(DEModPack modPack) {
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
                copiedPack.PackName = $"{testName} - 副本[{cpyID}]";
                ++cpyID;
            }
            _dEModPacks.Insert(_dEModPacks.IndexOf(modPack), copiedPack);
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 更新指定资源文件
        /// </summary>
        /// <param name="oldResourceName">要修改的模组名</param>
        /// <param name="newResourceFile">用以替换的模组路径</param>
        private void UpdateResourceFileHelper(string oldResourceName, string newResourceFile) {
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
                DEModPack dEModPack = (DEModPack)modPack;
                // 如果模组列表中已有该模组，则将旧模组移除即可
                if (dEModPack.ExistsResource(newResourceName)) {
                    for (int i = 0; i < dEModPack.Resources.Count; i++) {
                        if (dEModPack.Resources[i].Path == oldResourceName) {
                            dEModPack.RemoveResource((DEModResource)dEModPack.Resources[i]);
                            --i;
                        }
                    }
                }
                // 否则将所有旧模组名替换为新模组名即可
                else {
                    foreach (var res in dEModPack.Resources) {
                        if (res.Path == oldResourceName) {
                            res.Path = newResourceName;
                        }
                    }
                }
            }
            DOOMEternal.ModificationSaved = false;
        }
        /// <summary>
        /// 保存配置至文件
        /// </summary>
        /// <param name="fileName">保存的文件位置</param>
        private void SaveProfileHelper(string fileName) {
            Model.DEModManager dm = new Model.DEModManager();
            // 写入管理器属性
            dm.GameDirectory = DOOMEternal.GameDirectory;
            dm.ModLoader = DOOMEternal.ModLoader;
            dm.GameMainExecutor = DOOMEternal.GameMainExecutor;
            dm.CurrentMod = _currentMod?.PackName;
            // 写入ModPacks信息
            dm.ModPacks = new List<Model.DEModPack>();
            foreach (var modPack in _dEModPacks) {
                DEModPack dEModPack = (DEModPack)modPack;
                var dp = dEModPack.GetDataModel();
                dm.ModPacks.Add(dp);
            }
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                _serializer.WriteObject(fs, dm);
            }
            // 重置保存状态
            DOOMEternal.ModificationSaved = true;
        }
        /// <summary>
        /// 从文件中读取配置
        /// </summary>
        /// <param name="fileName">读取的文件路径</param>
        private void LoadProfilesHelper(string fileName) {
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
            DOOMEternal.ModificationSaved = true;
        }
        /// <summary>
        /// 清理未使用的MOD文件
        /// </summary>
        /// <returns>被清除的文件列表</returns>
        private List<string> ClearUnusedModFileHelper() {
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
            var removedFiles = FileCleanerHelper(usedResources, existedModFiles);
            return removedFiles;
        }
        /// <summary>
        /// 清理未使用的图像文件
        /// </summary>
        /// <returns>被清除的图像列表</returns>
        private List<string> ClearUnusedImageFilesHelper() {
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
            var removedFiles = FileCleanerHelper(usedImageFiles, existedImageFiles);
            return removedFiles;
        }
        /// <summary>
        /// 打开指定模组文件
        /// </summary>
        /// <param name="resourceName"></param>
        private static void OpenResourceFileHelper(string resourceName) {
            Process p = new Process();
            string filePath = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"无法找到文件：{filePath}");
            }
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = $@"/select, {DOOMEternal.ModPacksDirectory}\{resourceName}";
            p.Start();
        }
        /// <summary>
        /// 导出模组包
        /// </summary>
        /// <param name="outputPath">导出的路径</param>
        private static void ExportModPacksHelper(string outputPath) {
            ZipFile.CreateFromDirectory(DOOMEternal.ModPacksDirectory, outputPath, CompressionLevel.Optimal, true);
        }
        #endregion

        private List<string> FileCleanerHelper(ICollection<string> preservedFiles, IEnumerable<string> allFiles) {
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
///// <summary>
///// 上移模组配置
///// </summary>
///// <param name="modPack">要上移的模组配置</param>
//public void MoveUpModPack(DEModPack modPack) {
//    int currentIndex = _dEModPacks.IndexOf(modPack);
//    if (currentIndex <= 0) {
//        return;
//    }
//    int newIndex = currentIndex - 1;
//    var t = _dEModPacks[currentIndex];
//    _dEModPacks[currentIndex] = _dEModPacks[newIndex];
//    _dEModPacks[newIndex] = t;
//    DOOMEternal.ModificationSaved = false;
//}
///// <summary>
///// 下移模组配置
///// </summary>
///// <param name="modPack">要下移的模组配置</param>
//public void MoveDownModPack(DEModPack modPack) {
//    int currentIndex = _dEModPacks.IndexOf(modPack);
//    if (currentIndex < 0) {
//        return;
//    }
//    if (currentIndex >= _dEModPacks.Count - 1) {
//        return;
//    }
//    int newIndex = currentIndex + 1;
//    var t = _dEModPacks[currentIndex];
//    _dEModPacks[currentIndex] = _dEModPacks[newIndex];
//    _dEModPacks[newIndex] = t;
//    DOOMEternal.ModificationSaved = false;
//}