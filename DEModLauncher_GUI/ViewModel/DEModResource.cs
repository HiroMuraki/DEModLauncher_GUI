namespace DEModLauncher_GUI.ViewModel {
    public class DEModResource : ViewModelBase {
        private string _path;
        private ResourceStatus _status;
        private DEModAttribute _attribute;

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
        public DEModAttribute Attribute {
            get {
                return _attribute;
            }
        }

        public DEModResource() {
            _status = ResourceStatus.Enabled;
        }
        public DEModResource(string path) {
            _path = path;
            _status = ResourceStatus.Enabled;
            try {
                _attribute = DEModAttribute.Read($"{DOOMEternal.ModPacksDirectory}\\{path}");
            }
            catch {
                _attribute = new DEModAttribute();
            }
        }

        public void Enable() {
            _status = ResourceStatus.Enabled;
        }
        public void Disable() {
            _status = ResourceStatus.Disabled;
        }
        public override string ToString() {
            string output = $"名称：{_attribute.Name}\n";
            output += $"描述：{_attribute.Description}\n";
            output += $"作者：{_attribute.Author}\n";
            output += $"文件：{_path}";
            return output;
        }
    }
}
