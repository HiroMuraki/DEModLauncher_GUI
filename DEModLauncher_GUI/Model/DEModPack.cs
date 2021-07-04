using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DEModLauncher_GUI.Model {
    using Resources = List<string>;
    [DataContract]
    public class DEModPack {
        [DataMember]
        public string PackName;
        [DataMember]
        public string Description;
        [DataMember]
        public string ImagePath;
        [DataMember]
        public Resources Resources;
    }
}

