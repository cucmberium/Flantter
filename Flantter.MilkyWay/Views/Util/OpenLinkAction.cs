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
            Launcher.LaunchUriAsync(new Uri((string)Uri));
            return null;
		}
	}

}
