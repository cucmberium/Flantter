using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Flantter.MilkyWay.Views.Util
{
	public class ShowAttachedFlyoutAction : DependencyObject, IAction
	{
		public object Execute(object sender, object parameter)
		{
			FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
			return null;
		}
	}

}
