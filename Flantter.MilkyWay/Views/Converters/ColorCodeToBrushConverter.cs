using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Converters
{
    public sealed class ColorCodeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return (Brush)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Brush), value);
            }
            catch
            {
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
