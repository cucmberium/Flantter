using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TweetEventMessageBehavior
    {
        public static bool GetIsSelected(DependencyObject obj) { return (bool)obj.GetValue(IsSelectedProperty); }
        public static void SetIsSelected(DependencyObject obj, bool value) { obj.SetValue(IsSelectedProperty, value); }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TweetEventMessageBehavior), new PropertyMetadata(false, IsSelectedPropertyChanged));

        private static void IsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CommandGrid_PropertyChanged(obj, e);
        }

        #region TargetStatus 関連
        public static bool GetTargetStatusVisibility(DependencyObject obj) { return (bool)obj.GetValue(GetTargetStatusVisibilityProperty); }
        public static void SetTargetStatusVisibility(DependencyObject obj, bool value) { obj.SetValue(GetTargetStatusVisibilityProperty, value); }

        public static readonly DependencyProperty GetTargetStatusVisibilityProperty =
            DependencyProperty.Register("TargetStatusVisibility", typeof(bool), typeof(TweetStatusBehavior), new PropertyMetadata(false, TargetStatus_PropertyChanged));

        private static void TargetStatus_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Grid;
            var border = status.FindName("TargetStatusBorder") as Border;
            if (GetTargetStatusVisibility(obj))
                border.Visibility = Visibility.Visible;
            else
                border.Visibility = Visibility.Collapsed;
        }
        #endregion


        #region CommandGrid 関連
        private static void CommandGrid_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Grid;
            var grid = status.FindName("CommandGrid") as Grid;

            if (grid == null)
                return;

            if ((bool)e.NewValue)
                (grid.Resources["TweetCommandBarOpenAnimation"] as Storyboard).Begin();
            else
                (grid.Resources["TweetCommandBarCloseAnimation"] as Storyboard).Begin();
        }
        #endregion
    }
}
