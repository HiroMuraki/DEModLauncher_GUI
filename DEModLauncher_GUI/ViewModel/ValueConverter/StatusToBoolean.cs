using System;
using System.Globalization;
using System.Windows.Data;

namespace DEModLauncher_GUI.ViewModel.ValueConverter {
    public class StatusToBoolean : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                Status status = (Status)value;
                switch (status) {
                    case Status.Enable:
                        return true;
                    case Status.Disable:
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
                        return Status.Enable;
                    case false:
                        return Status.Disable;
                }
            }
            catch {
                return Status.Enable;
            }
        }
    }
}
