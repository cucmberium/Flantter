using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Converters
{
    /// <summary>
    ///     true を false に、および false を true に変換する値コンバーター。
    /// </summary>
    public sealed class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as DateTime?)?.ToLocalTime().ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}