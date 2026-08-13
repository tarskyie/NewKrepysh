using Microsoft.UI.Xaml.Data;
using System;
using Microsoft.UI.Xaml;
using NewKrepysh.WinUI.Views;

namespace NewKrepysh.WinUI.Converters
{
    internal class NullToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            return value != null ? true : false;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }
}
