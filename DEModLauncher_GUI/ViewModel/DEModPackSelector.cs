namespace DEModLauncher_GUI.ViewModel {

    public class DEModPackSelector : ViewModelBase {
        private readonly DEModPack _dEModPack;
        private bool _isSelected;

        public DEModPack DEModPack {
            get {
                return _dEModPack;
            }
        }
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public DEModPackSelector(DEModPack modPack) {
            _dEModPack = modPack;
            _isSelected = false;
        }
    }
}