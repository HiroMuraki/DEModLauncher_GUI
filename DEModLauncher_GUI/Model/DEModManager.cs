using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DEModLauncher_GUI.Model {
    [DataContract]
    public class DEModManager {
        [DataMember]
        public string GameMainExecutor;
        [DataMember]
        public string ModLoader;
        [DataMember]
        public string GameDirectory;
        [DataMember]
        public string CurrentMod;
        [DataMember]
        public List<DEModPack> ModPacks;
    }
}

