using System;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Converters
{
    public sealed class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is int))
                return "";

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string))
                return 0;

            var retValue = 0;
            try
            {
                int.TryParse((string) value, out retValue);
            }
            catch
            {
            }

            return retValue;
        }
    }
}