using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Converters
{
    public sealed class DoublePlus25Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is double ? (double)value + 25 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is double ? (double)value - 25 : 0;
        }
    }
}
