namespace DEModLauncher_GUI {
    public interface IModPackSelector {
        IModPack ModPack { get; }
        bool IsSelected { get; set; }
    }
}
