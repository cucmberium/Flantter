using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Converters
{
    public sealed class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !string.IsNullOrWhiteSpace((string) value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}