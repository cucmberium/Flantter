using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.Setting;

namespace Flantter.MilkyWay.Views.Converters
{
    public sealed class PlatformEnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((SettingSupport.PlatformEnum) value == SettingSupport.PlatformEnum.Mastodon)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}