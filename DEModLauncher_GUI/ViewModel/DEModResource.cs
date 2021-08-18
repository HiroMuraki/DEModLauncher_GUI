using System;

namespace DEModLauncher_GUI.ViewModel {
    public class DEModResource : ViewModelBase, IModResource, IComparable<DEModResource> {
        private string _path;
        private ResourceStatus _status;
        private DEModInformation _information;

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
        public IModInformation Information {
            get {
                return _information;
            }
        }

        private DEModResource() {

        }
        public DEModResource(string path) {
            _path = path;
            _status = ResourceStatus.Enabled;
            try {
                _information = DEModInformation.Read($"{DOOMEternal.ModPacksDirectory}\\{path}");
            }
            catch {
                _information = new DEModInformation();
            }
        }

        public void Enable() {
            _status = ResourceStatus.Enabled;
        }
        public void Disable() {
            _status = ResourceStatus.Disabled;
        }
        public DEModResource GetDeepCopy() {
            DEModResource copy = new DEModResource();
            copy._path = _path;
            copy._status = _status;
            copy._information = _information.GetDeepCopy();
            return copy;
        }
        public override string ToString() {
            if (_path.EndsWith(".zip")) {
                return _path[0..(_path.Length - 4)];
            }
            return _path;
        }

        public int CompareTo(DEModResource other) {
            return _path.CompareTo(other?._path);
        }
    }
}
