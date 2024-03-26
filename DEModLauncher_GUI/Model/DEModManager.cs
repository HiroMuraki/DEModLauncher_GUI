using System;
using System.Runtime.Serialization;

namespace DEModLauncher_GUI.Model;

[DataContract]
internal class DEModManager
{
    [DataMember(Order = 0)]
    public string GameMainExecutor { get; init; } = "";

    [DataMember(Order = 1)]
    public string ModLoader { get; init; } = "";

    [DataMember(Order = 2)]
    public string GameDirectory { get; init; } = "";

    [DataMember(Order = 3)]
    public string ModDirectory { get; init; } = "";

    [DataMember(Order = 4)]
    public string ModPacksDirectory { get; init; } = "";

    [DataMember(Order = 5)]
    public string ModPackImageDirectory { get; init; } = "";

    [DataMember(Order = 6)]
    public string CurrentMod { get; init; } = "";

    [DataMember(Order = 7)]
    public DEModPack[] ModPacks { get; init; } = Array.Empty<DEModPack>();
}

