using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace DEModLauncher_GUI.Model {
    [DataContract]
    public class DEModManager {
        [DataMember(Order = 0)]
        public string GameMainExecutor = "";
        [DataMember(Order = 1)]
        public string ModLoader = "";
        [DataMember(Order = 2)]
        public string GameDirectory = "";
        [DataMember(Order = 3)]
        public string ModDirectory = "";
        [DataMember(Order = 4)]
        public string ModPacksDirectory = "";
        [DataMember(Order = 5)]
        public string ModPackImageDirectory = "";
        [DataMember(Order = 6)]
        public string CurrentMod = "";
        [DataMember(Order = 7)]
        public DEModPack[] ModPacks = Array.Empty<DEModPack>();
    }
}

