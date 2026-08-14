using Microsoft.UI.Xaml.Data;
using System;

namespace NewKrepysh.WinUI.Converters
{
    internal class UtcToLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value is DateTime dt)
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime().ToString();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return value;
        }
    }
}
