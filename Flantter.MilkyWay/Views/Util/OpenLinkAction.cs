using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

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
            get
            {
                return (bool)base.GetValue(IsEnabledProperty);
            }
            set
            {
                base.SetValue(IsEnabledProperty, value);
            }
        }

        public string Uri
        {
            get
            {
                return base.GetValue(UriProperty) as string;
            }
            set
            {
                base.SetValue(UriProperty, value);
            }
        }

        public object Execute(object sender, object parameter)
		{
            if (!this.IsEnabled)
                return null;

            Launcher.LaunchUriAsync(new Uri((string)this.Uri));
            return null;
		}
	}

}
