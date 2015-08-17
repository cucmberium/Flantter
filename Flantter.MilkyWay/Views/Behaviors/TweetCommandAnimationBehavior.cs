using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TweetCommandAnimationBehavior
    {
        public static bool GetIsSelected(DependencyObject obj) { return (bool)obj.GetValue(IsSelectedProperty); }
        public static void SetIsSelected(DependencyObject obj, bool value) { obj.SetValue(IsSelectedProperty, value); }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TweetCommandAnimationBehavior), new PropertyMetadata(false, PropertyChanged));

        private static void PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var grid = obj as Grid;

            if (grid == null)
                return;

            if (GetIsSelected(obj))
                (grid.Resources["TweetCommandBarOpenAnimation"] as Storyboard).Begin();
            else
                (grid.Resources["TweetCommandBarCloseAnimation"] as Storyboard).Begin();
        }
    }
}
