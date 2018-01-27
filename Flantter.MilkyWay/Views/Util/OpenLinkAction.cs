using System;
using Windows.System;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    public class OpenLinkAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty UriProperty =
            DependencyProperty.Register("Uri", typeof(string), typeof(OpenLinkAction),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(OpenLinkAction),
                new PropertyMetadata(true));

        public bool IsEnabled
        {
            get => (bool) GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public string Uri
        {
            get => GetValue(UriProperty) as string;
            set => SetValue(UriProperty, value);
        }

        public object Execute(object sender, object parameter)
        {
            if (!IsEnabled)
                return null;

            if (string.IsNullOrWhiteSpace(Uri))
                return null;

            Launcher.LaunchUriAsync(new Uri(Uri));
            return null;
        }
    }
}