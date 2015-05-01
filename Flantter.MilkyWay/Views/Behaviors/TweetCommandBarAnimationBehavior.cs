using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TweetCommandBarAnimationBehavior
	{
        public static bool GetSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectedProperty);
        }
        public static void SetSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectedProperty, value);
        }


        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.RegisterAttached("Selected", typeof(bool), typeof(TweetCommandBarAnimationBehavior), new PropertyMetadata(false, PropertyChanged));       

        public static void PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var grid = obj as Grid;

            if (grid == null)
                return;

            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;

            try
            {
                if (oldValue != newValue)
                {
                    if (newValue == true)
                        (grid.Resources["TweetCommandBarOpenAnimation"] as Storyboard).Begin();
                    else
                        (grid.Resources["TweetCommandBarCloseAnimation"] as Storyboard).Begin();
                }
            }
            catch { }
        }
    }
}
