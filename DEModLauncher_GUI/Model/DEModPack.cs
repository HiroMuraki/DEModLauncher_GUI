using System.Runtime.Serialization;
using System;

namespace DEModLauncher_GUI.Model {
    [DataContract]
    public class DEModPack {
        [DataMember(Order = 0)]
        public string PackName = "";
        [DataMember(Order = 1)]
        public string Description = "";
        [DataMember(Order = 2)]
        public string ImagePath = "";
        [DataMember(Order = 3)]
        public string[] Resources = Array.Empty<string>();
    }
}

