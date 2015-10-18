using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Converters
{
    public sealed class EnumToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
			return (double)((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
			throw new NotImplementedException();
        }
    }
}
