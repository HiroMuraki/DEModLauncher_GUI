using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DEModLauncher_GUI.Model {
    [DataContract]
    public class DEModManager {
        [DataMember]
        public string GameMainExecutor;
        [DataMember]
        public string ModLoadder;
        [DataMember]
        public string GameDirectory;
        [DataMember]
        public string CurrentMod;
        [DataMember]
        public List<DEModPack> ModPacks;
    }
}

