using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TweetMultipulSelectBehavior
    {
        public static readonly DependencyProperty IsMultipulSelectOpenedProperty =
            DependencyProperty.Register("IsMultipulSelectOpened", typeof(bool), typeof(TweetMultipulSelectBehavior),
                new PropertyMetadata(false, IsMultipulSelectOpened_Changed));

        public static bool GetIsMultipulSelectOpened(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsMultipulSelectOpenedProperty);
        }

        public static void SetIsMultipulSelectOpened(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMultipulSelectOpenedProperty, value);
        }


        private static void IsMultipulSelectOpened_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as Grid;

            var tweetMultipulActionGrid = grid?.FindName("TweetMultipulActionGrid") as Grid;
            if (tweetMultipulActionGrid == null)
                return;

            if ((bool) e.NewValue)
                (tweetMultipulActionGrid.Resources["TweetMultipulActionGridOpenAnimation"] as Storyboard).Begin();
            else
                (tweetMultipulActionGrid.Resources["TweetMultipulActionGridCloseAnimation"] as Storyboard).Begin();
        }
    }
}