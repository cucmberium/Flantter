using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace Flantter.MilkyWay.Views.Util
{
	public class StoryboardBeginAction : DependencyObject, IAction
	{
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(KeyTrigger),
                new PropertyMetadata(null));

        public string Key
        {
            get
            {
                return base.GetValue(KeyProperty) as string;
            }
            set
            {
                base.SetValue(KeyProperty, value);
            }
        }

        public object Execute(object sender, object parameter)
		{
            var obj = sender as FrameworkElement;
            if (obj == null)
                return null;

            if (obj.Resources.ContainsKey(this.Key))
                (obj.Resources[this.Key] as Storyboard).Begin();

			return null;
		}
	}

}
