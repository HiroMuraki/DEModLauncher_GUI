using System;

namespace DEModLauncher_GUI.ViewModel {
    public class DEModResource : ViewModelBase, IComparable<DEModResource>, IDeepCopyable<DEModResource> {
        public string Name {
            get {
                if (_path.EndsWith(".zip")) {
                    return _path[0..(_path.Length - 4)];
                }
                return _path;
            }
        }
        public string Path {
            get {
                return _path;
            }
            set {
                _path = value;
                OnPropertyChanged(nameof(Path));
                OnPropertyChanged(nameof(Name));
                try {
                    _information = DEModInfo.Read($"{DOOMEternal.ModPacksDirectory}\\{_path}");
                }
                catch (Exception) {
                    _information = new DEModInfo();
                }
                OnPropertyChanged(nameof(Information));
            }
        }
        public Status Status {
            get {
                return _status;
            }
        }
        public DEModInfo Information {
            get {
                return _information;
            }
        }

        private DEModResource() {

        }
        public DEModResource(string path) {
            _path = path;
            _status = Status.Enable;
            try {
                _information = DEModInfo.Read($"{DOOMEternal.ModPacksDirectory}\\{path}");
            }
            catch {
                _information = new DEModInfo();
            }
        }

        public void Toggle() {
            if (_status == Status.Disable) {
                _status = Status.Enable;
            }
            else {
                _status = Status.Disable;
            }
            OnPropertyChanged(nameof(Status));
        }
        public DEModResource GetDeepCopy() {
            return new DEModResource {
                _path = _path,
                _status = _status,
                _information = _information.GetDeepCopy()
            };
        }
        public override string ToString() {
            return Name;
        }
        public int CompareTo(DEModResource? other) {
            return _path.CompareTo(other?._path);
        }

        private string _path = "";
        private Status _status = Status.Enable;
        private DEModInfo _information = new DEModInfo();
    }
}
