using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEModLauncher_GUI {
    public interface IModPackSelector {
        IModPack ModPack { get; }
        bool IsSelected { get; set; }
    }
}
