namespace DEModLauncher_GUI {
    public interface IModResource {
        string Name { get; }
        string Path { get; set; }
        ResourceStatus Status { get; set; }
        IModInformation Information { get; }
    }
}
