using ModPacks = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModPack>;
using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModResource>;

namespace DEModLauncher_GUI {
    public interface IModManager {
        IModPack CurrentMod { get; }
        ModPacks ModPacks { get; }
        Resources UsingMods { get; }
        bool IsLaunching { get; }
        void LoadMod();
        void LaunchMod();
        void LaunchGame();
        void Initialize();
        void SaveProfiles();
        void LoadProfiles();
        void LoadProfiles(string file);
        void SetCurrentMod(IModPack currentMod);
        void AddModPack();
        void RemoveModPack(IModPack modPack);
        void DuplicateModPack(IModPack modPack);
        void ResortModPack(IModPack source, IModPack target);
        void UpdateResource(IModResource resource);
        void OpenResourceFile(IModResource resource);
    }
}
