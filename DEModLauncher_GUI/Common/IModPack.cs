using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModResource>;

namespace DEModLauncher_GUI {
    public interface IModPack {
        public string PackName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; }
        public Resources Resources { get; }
    }
}
