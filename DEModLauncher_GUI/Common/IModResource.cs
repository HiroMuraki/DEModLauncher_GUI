using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DEModLauncher_GUI {
    public interface IModResource {
        string Path { get; set; }
        ResourceStatus Status { get; set; }
        IModInformation Attribute { get; }
    }
}
