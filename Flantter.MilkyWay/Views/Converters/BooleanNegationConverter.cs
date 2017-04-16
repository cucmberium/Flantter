using System;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Converters
{
    public sealed class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool) value);
        }
    }
}