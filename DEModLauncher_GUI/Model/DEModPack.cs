using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DEModLauncher_GUI.Model {
    using Resources = List<string>;
    [DataContract]
    public class DEModPack {
        [DataMember]
        public string PackName;
        [DataMember]
        public string Description;
        [DataMember]
        public Resources Resources;
    }
}

