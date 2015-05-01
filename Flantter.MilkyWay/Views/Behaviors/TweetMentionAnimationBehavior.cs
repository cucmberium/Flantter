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
    public class TweetMentionAnimationBehavior
	{
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsLoading(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLoadingProperty);
        }
        public static void SetIsLoading(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TweetMentionAnimationBehavior), new PropertyMetadata(false, PropertyChanged));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.RegisterAttached("IsLoading", typeof(bool), typeof(TweetMentionAnimationBehavior), new PropertyMetadata(false, PropertyChanged));

        private static void PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var grid = obj as Grid;

            if (grid == null)
                return;

            bool? newValue = false;
            if (GetIsEnabled(obj) == true)
                newValue = true;
            else if (GetIsLoading(obj) == true)
                newValue = null;

            try
            {
                if (newValue == true)
					(grid.Resources["MentionTweetFieldOpenAnimation"] as Storyboard).Begin();
                else if (newValue == null)
                    (grid.Resources["MentionTweetFieldOpenLoadingAnimation"] as Storyboard).Begin();
                else if (newValue == false)
                    (grid.Resources["MentionTweetFieldCloseAnimation"] as Storyboard).Begin();
            }
            catch { }
        }
    }
}
