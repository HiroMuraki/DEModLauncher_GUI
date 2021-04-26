using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEModLauncher_GUI.ViewModel {
    public enum ResourceStatus {
        Enabled,
        Disabled
    }
    public class DEModResource : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _path;
        private ResourceStatus _status;

        public string Path {
            get {
                return _path;
            }
            set {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }
        public ResourceStatus Status {
            get {
                return _status;
            }
            set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public DEModResource() {
            _status = ResourceStatus.Enabled;
        }
        public DEModResource(string path) {
            _path = path;
            _status = ResourceStatus.Enabled;
        }
        public DEModResource(string path, ResourceStatus status) {
            _path = path;
            _status = status;
        }

        public void Enable() {
            _status = ResourceStatus.Enabled;
        }
        public void Disable() {
            _status = ResourceStatus.Disabled;
        }
    }
}
