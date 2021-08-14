namespace DEModLauncher_GUI {
    public interface IModResource {
        string Path { get; set; }
        ResourceStatus Status { get; set; }
        IModInformation Information { get; }
    }
}
