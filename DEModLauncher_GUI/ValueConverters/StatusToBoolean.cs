using System;
using System.Globalization;
using System.Windows.Data;

namespace DEModLauncher_GUI.ValueConverters;

internal class StatusToBoolean : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            return (Status)value switch
            {
                Status.Enable => true,
                Status.Disable => false,
                _ => true,
            };
        }
        catch
        {
            return true;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            return (bool)value switch
            {
                true => Status.Enable,
                false => Status.Disable
            };
        }
        catch
        {
            return Status.Enable;
        }
    }
}
