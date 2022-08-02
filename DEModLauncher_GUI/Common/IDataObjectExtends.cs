using System;
using System.Linq;
using System.Windows;

namespace DEModLauncher_GUI
{
    public static class IDataObjectExtends
    {
        public static bool IsTargetType(this IDataObject data, string dataFormat)
        {
            return data.GetFormats().Contains(dataFormat);
        }
    }
}
