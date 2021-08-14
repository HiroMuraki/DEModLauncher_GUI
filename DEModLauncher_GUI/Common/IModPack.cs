using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resources = System.Collections.ObjectModel.ObservableCollection<DEModLauncher_GUI.IModResource>;

namespace DEModLauncher_GUI {
    public interface IModPack {
        public string PackName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; }
        public Resources Resources { get; }
    }
}
