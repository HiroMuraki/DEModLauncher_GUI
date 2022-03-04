using System;
using System.Runtime.Serialization;

namespace DEModLauncher_GUI.Model {
    [DataContract]
    public class DEModPack {
        [DataMember(Order = 0)]
        public string PackName { get; init; } = "";
        [DataMember(Order = 1)]
        public string Description { get; init; } = "";
        [DataMember(Order = 2)]
        public string ImagePath { get; init; } = "";
        [DataMember(Order = 3)]
        public string[] Resources { get; init; } = Array.Empty<string>();
    }
}

