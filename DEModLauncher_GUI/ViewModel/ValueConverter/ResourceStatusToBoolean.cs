using System;
using System.Globalization;
using System.Windows.Data;

namespace DEModLauncher_GUI.ViewModel.ValueConverter {
    public class ResourceStatusToBoolean : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                ResourceStatus status = (ResourceStatus)value;
                switch (status) {
                    case ResourceStatus.Enabled:
                        return true;
                    case ResourceStatus.Disabled:
                        return false;
                    default:
                        return true;
                }
            }
            catch {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                bool isOn = (bool)value;
                switch (isOn) {
                    case true:
                        return ResourceStatus.Enabled;
                    case false:
                        return ResourceStatus.Disabled;
                }
            }
            catch {
                return ResourceStatus.Enabled;
            }
        }
    }
}
