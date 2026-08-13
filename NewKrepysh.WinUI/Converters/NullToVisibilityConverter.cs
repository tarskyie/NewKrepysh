using Microsoft.UI.Xaml.Data;
using System;
using Microsoft.UI.Xaml;
using NewKrepysh.WinUI.Views;

namespace NewKrepysh.WinUI.Converters
{
    internal class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }
}
