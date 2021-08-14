using System.Windows;
using ModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModPack>;
using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModResource>;

namespace DEModLauncher_GUI {
    public interface IModManager {
        IModPack CurrentMod { get; }
        ModPacks DEModPacks { get; }
        Resources UsingMods { get; }
        bool IsLaunching { get; }
        void SetCurrentMod(IModPack currentMod);
        void LoadMod();
        void LaunchMod();
        void LaunchGame();
        void SaveProfiles();
        void LoadProfiles();
        void AddModPack();
        void ResortModPack(IModPack source, IModPack target);
        void DuplicateModPack(IModPack modPack);
        void RemoveModPack(IModPack modPack);
        void EditModPack(IModPack modPack);
        void CheckModConfliction(IModPack modPack);
        void ExportMergedMod(IModPack modPack);
        void AddResource();
        void AddResource(IDataObject data);
        void RemoveResource(IModResource resource);
        void AddResourcesReference();
        void OpenResourceFile(IModResource resource);
        void UpdateResource(IModResource resource);
        void ResortResource(IModResource source, IModResource target);
        void LoadProfiles(string file);
    }
}
