using System.Windows;
using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModResource>;

namespace DEModLauncher_GUI {
    public interface IModPack {
        public string PackName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; }
        public Resources Resources { get; }

        void Deploy();
        void Edit();
        void AddResource();
        void AddResource(IDataObject data);
        void AddResourcesReference();
        void InsertResource(int index, string resourcePath);
        void RemoveResource(IModResource resource);
        void ResortResource(IModResource source, IModResource target);
        void ExportMergedResource(IModPack modPack);
        void CheckModConfliction();
        bool ContainResource(IModResource resourceName);
    }
}
