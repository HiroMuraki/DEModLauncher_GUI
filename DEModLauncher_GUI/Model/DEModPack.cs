using System.Runtime.Serialization;
using Resources = System.Collections.Generic.List<string>;

namespace DEModLauncher_GUI.Model {
    [DataContract]
    public class DEModPack {
        [DataMember(Order = 0)]
        public string PackName;
        [DataMember(Order = 1)]
        public string Description;
        [DataMember(Order = 2)]
        public string ImagePath;
        [DataMember(Order = 3)]
        public Resources Resources;
    }
}

