using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using ModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.ViewModel.DEModPackViewModel>;

namespace DEModLauncher_GUI.ViewModel;

internal class DEModManagerViewModel : ViewModelBase
{
    private DEModManagerViewModel()
    {

    }

    public static DEModManagerViewModel Instance { get; } = new();

    public event Action? CurrentModPackChanged;

    public bool IsLaunching
    {
        get
        {
            return _isLaunching;
        }
        set
        {
            _isLaunching = value;
            OnPropertyChanged(nameof(IsLaunching));
        }
    }

    public DEModPackViewModel CurrentModPack
    {
        get
        {
            return _currentModPack;
        }
        private set
        {
            _currentModPack = value;
            OnPropertyChanged(nameof(CurrentModPack));
            CurrentModPackChanged?.Invoke();
        }
    }

    public ModPacks ModPacks { get; } = new ModPacks();

    public DEModResourceViewModel[] UsedModResources
    {
        get
        {
            var ress = new List<DEModResourceViewModel>();
            foreach (DEModPackViewModel modPack in ModPacks)
            {
                foreach (DEModResourceViewModel res in modPack.Resources)
                {
                    // 如果ress中未出现该模组，则加入
                    bool isDistinct = true;
                    foreach (DEModResourceViewModel existed in ress)
                    {
                        if (existed.Path == res.Path)
                        {
                            isDistinct = false;
                            break;
                        }
                    }
                    if (isDistinct)
                    {
                        ress.Add(res);
                    }
                }
            }
            ress.Distinct();
            ress.Sort();

            return ress.ToArray();
        }
    }

    public void LoadMod()
    {
        DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 0);
        LoadModHelper();

    }

    public void LaunchMod()
    {
        DOOMEternal.SetModLoaderProfile("AUTO_LAUNCH_GAME", 1);
        LoadModHelper();
    }

    public void LaunchGame()
    {
        DOOMEternal.LaunchGame();
    }

    public void Initialize()
    {
        SetDefaultModPack();
    }

    public bool IsValidModPackSelected()
    {
        return !ReferenceEquals(CurrentModPack, _noModPack);
    }

    public void SetCurrentModPack(DEModPackViewModel modPack)
    {
        foreach (DEModPackViewModel item in ModPacks)
        {
            item.ToggleOff();
        }
        modPack.ToggleOn();
        CurrentModPack = modPack;
    }

    public DEModPackViewModel NewModPack()
    {
        var t = new DEModPackViewModel();
        AddModPackHelper(t);
        return t;
    }

    public DEModPackViewModel DuplicateModPack(DEModPackViewModel modPack)
    {
        // 获取已经使用过的模组包名
        var usedPackNames = new List<string>();
        foreach (DEModPackViewModel dmp in ModPacks)
        {
            usedPackNames.Add(dmp.PackName);
        }
        // 获取模组包副本
        DEModPackViewModel copiedPack = new DEModPackViewModel().LoadFromModel(modPack.ConvertToModel());
        // 移除被临时禁用的模组
        for (int i = 0; i < copiedPack.Resources.Count; i++)
        {
            if (copiedPack.Resources[i].Status == Status.Disable)
            {
                copiedPack.Resources.Remove(copiedPack.Resources[i]);
                --i;
            }
        }
        // 设置新模组包名，避免重复
        int cpyID = 1;
        string testName = copiedPack.PackName;
        while (usedPackNames.Contains(copiedPack.PackName))
        {
            copiedPack.PackName = $"{testName} - 副本[{cpyID}]";
            ++cpyID;
        }
        ModPacks.Insert(ModPacks.IndexOf(modPack), copiedPack);
        DOOMEternal.ModificationSaved = false;
        return copiedPack;
    }

    public DEModPackViewModel RemoveModPack(DEModPackViewModel modPack)
    {
        ModPacks.Remove(modPack);
        if (ReferenceEquals(CurrentModPack, modPack))
        {
            if (ModPacks.Count > 0)
            {
                CurrentModPack = ModPacks[0];
            }
            else
            {
                CurrentModPack = _noModPack;
            }
            OnPropertyChanged(nameof(CurrentModPack));
            CurrentModPackChanged?.Invoke();
        }
        if (ModPacks.Count <= 0)
        {
            SetDefaultModPack();
        }
        SetCurrentModPack(ModPacks[0]);
        DOOMEternal.ModificationSaved = false;
        return modPack;
    }

    public DEModPackViewModel ResortModPack(int index, DEModPackViewModel source)
    {
        ModPacks.ReInsert(index, source);
        DOOMEternal.ModificationSaved = false;
        return source;
    }

    public DEModResourceViewModel UpdateResource(DEModResourceViewModel resource, string resourcePath)
    {
        string oldResourceName = resource.Path;
        string newResourceFile = resourcePath;
        string newResourceName = Path.GetFileName(newResourceFile);
        // 如果新旧模组名同名，直接替换文件即可
        if (oldResourceName == newResourceName)
        {
            RemoveModResourceFileBackup(oldResourceName);
            BackupModResourceFile(newResourceFile);
        }
        else
        {
            // 否则逐一对模组配置中的相关文件进行修改
            BackupModResourceFile(newResourceFile);
            var newResource = new DEModResourceViewModel(newResourceName);
            foreach (DEModPackViewModel modPack in ModPacks)
            {
                // 如果模组列表中已有该模组，则将旧模组移除即可
                if (modPack.ContainsResource(newResource))
                {
                    for (int i = 0; i < modPack.Resources.Count; i++)
                    {
                        if (modPack.Resources[i].Path == oldResourceName)
                        {
                            modPack.RemoveResource(modPack.Resources[i]);
                            --i;
                        }
                    }
                }
                // 否则将所有旧模组名替换为新模组名即可
                else
                {
                    foreach (DEModResourceViewModel res in modPack.Resources)
                    {
                        if (res.Path == oldResourceName)
                        {
                            res.Path = newResourceName;
                        }
                    }
                }
            }
            OnPropertyChanged(nameof(UsedModResources));
        }
        return resource;
    }

    public void SaveProfile(string fileName)
    {
        var dm = new Model.DEModManager
        {
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

        using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            _serializer.WriteObject(fs, dm);
        }
    }

    public void LoadProfile(string fileName)
    {
        // 读取文件
        Model.DEModManager? dm;
        using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            dm = _serializer.ReadObject(file) as Model.DEModManager;
            if (dm == null)
            {
                throw new InvalidDataException();
            }
        }
        // 读取配置项
        if (!string.IsNullOrEmpty(dm.GameMainExecutor))
        {
            DOOMEternal.GameMainExecutor = dm.GameMainExecutor;
        }
        if (!string.IsNullOrEmpty(dm.ModLoader))
        {
            DOOMEternal.ModLoader = dm.ModLoader;
        }
        // 读取模组包，同时设置CurrentMod
        CurrentModPack = _noModPack;
        ModPacks.Clear();
        foreach (Model.DEModPack item in dm.ModPacks)
        {
            DEModPackViewModel modPack = NewModPack().LoadFromModel(item);
            if (!IsValidModPackSelected() && item.PackName == dm.CurrentMod)
            {
                SetCurrentModPack(modPack);
            }
        }
        if (ModPacks.Count <= 0)
        {
            SetDefaultModPack();
        }
    }

    public void ClearUnusedModFile(out string[] removedFiles)
    {
        // 获取当前正在使用的模组列表
        var usedResources = new List<string>();
        foreach (DEModPackViewModel modPack in ModPacks)
        {
            foreach (DEModResourceViewModel resource in modPack.Resources)
            {
                string filePath = $@"{DOOMEternal.ModPacksDirectory}\{resource.Path}";
                if (!usedResources.Contains(filePath))
                {
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

    public void ClearUnusedImageFiles(out string[] removedFiles)
    {
        // 获取当前正在使用的图片文件名
        var usedImageFiles = new List<string>();
        foreach (DEModPackViewModel modPack in ModPacks)
        {
            // 跳过默认图片
            if (modPack.ImagePath == DOOMEternal.DefaultModPackImage)
            {
                continue;
            }
            string imageName = modPack.ImagePath;
            if (!usedImageFiles.Contains(imageName))
            {
                usedImageFiles.Add(imageName);
            }
        }
        // 查找未使用的图片文件并移除
        string[] existedImageFiles = Directory.GetFiles(DOOMEternal.ModPackImagesDirectory);
        removedFiles = Util.FilesCleaner(usedImageFiles, existedImageFiles).ToArray();
    }

    #region 辅助方法
    private static readonly DEModPackViewModel _noModPack = new();
    private static readonly DataContractJsonSerializer _serializer = new(typeof(Model.DEModManager));
    private bool _isLaunching;
    private DEModPackViewModel _currentModPack = _noModPack;
    private static void BackupModResourceFile(string filePath)
    {
        if (DOOMEternal.GameDirectory == null)
        {
            throw new ArgumentException("请先选择游戏文件夹");
        }
        if (!Directory.Exists(DOOMEternal.ModPacksDirectory))
        {
            Directory.CreateDirectory(DOOMEternal.ModPacksDirectory);
        }
        string resourceName = Path.GetFileName(filePath);
        string modPackBackup = $@"{DOOMEternal.ModPacksDirectory}\{resourceName}";
        if (!File.Exists(modPackBackup))
        {
            File.Copy(filePath, modPackBackup);
        }
    }
    private static void RemoveModResourceFileBackup(string modName)
    {
        File.Delete($"{DOOMEternal.ModPacksDirectory}\\{modName}");
    }
    public bool LoadModHelper()
    {
        try
        {
            IsLaunching = true;
            if (!IsValidModPackSelected())
            {
                throw new InvalidOperationException("当前未选择有效模组");
            }
            CurrentModPack.Deploy();
            DOOMEternal.LaunchModLoader();
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            IsLaunching = false;
        }
    }
    private void SetDefaultModPack()
    {
        if (ModPacks.Count > 0)
        {
            return;
        }
        DEModPackViewModel modPack = NewModPack();
        modPack.PackName = "默认模组";
        modPack.Description = "模组描述";
        modPack.SetImage(DOOMEternal.DefaultModPackImage);
    }
    private void AddModPackHelper(DEModPackViewModel modPack)
    {
        foreach (DEModPackViewModel mp in ModPacks)
        {
            if (mp.PackName == modPack.PackName)
            {
                throw new ArgumentException($"模组配置[{modPack.PackName}]已存在，不可重复添加");
            }
        }
        ModPacks.Add(modPack);
        CurrentModPack = modPack;
    }
    #endregion
}