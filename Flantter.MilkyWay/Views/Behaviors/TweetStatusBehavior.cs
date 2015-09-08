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
    public class TweetStatusBehavior
    {
        public static bool GetIsSelected(DependencyObject obj) { return (bool)obj.GetValue(IsSelectedProperty); }
        public static void SetIsSelected(DependencyObject obj, bool value) { obj.SetValue(IsSelectedProperty, value); }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TweetStatusBehavior), new PropertyMetadata(false, IsSelectedPropertyChanged));

        private static void IsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CommandGrid_PropertyChanged(obj, e);
            MentionStatus_PropertyChanged(obj, e);
        }

        #region MentionStatus 関連
        public static bool GetMentionStatusVisibility(DependencyObject obj) { return (bool)obj.GetValue(MentionStatusVisibilityProperty); }
        public static void SetMentionStatusVisibility(DependencyObject obj, bool value) { obj.SetValue(MentionStatusVisibilityProperty, value); }

        public static readonly DependencyProperty MentionStatusVisibilityProperty =
            DependencyProperty.Register("MentionStatusVisibility", typeof(bool), typeof(TweetStatusBehavior), new PropertyMetadata(false, MentionStatus_PropertyChanged));

        public static bool GetIsMentionStatusLoaded(DependencyObject obj) { return (bool)obj.GetValue(IsMentionStatusLoadedProperty); }
        public static void SetIsMentionStatusLoaded(DependencyObject obj, bool value) { obj.SetValue(IsMentionStatusLoadedProperty, value); }

        public static readonly DependencyProperty IsMentionStatusLoadedProperty =
            DependencyProperty.Register("IsMentionStatusLoaded", typeof(bool), typeof(TweetStatusBehavior), new PropertyMetadata(false, MentionStatus_PropertyChanged));

        public static bool GetIsMentionStatusLoading(DependencyObject obj) { return (bool)obj.GetValue(IsMentionStatusLoadingProperty); }
        public static void SetIsMentionStatusLoading(DependencyObject obj, bool value) { obj.SetValue(IsMentionStatusLoadingProperty, value); }

        public static readonly DependencyProperty IsMentionStatusLoadingProperty =
            DependencyProperty.Register("IsMentionStatusLoading", typeof(bool), typeof(TweetStatusBehavior), new PropertyMetadata(false, MentionStatus_PropertyChanged));

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }
        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TweetStatusBehavior), new PropertyMetadata(null));

        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }
        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TweetStatusBehavior), new PropertyMetadata(null));


        private static void MentionStatus_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Grid;
            
            if (!GetMentionStatusVisibility(obj))
                return;

            try
            {
                if (GetIsSelected(status))
                {
                    var grid = status.FindName("MentionStatusGrid") as Grid;

                    if (grid == null)
                        return;

                    if (GetIsMentionStatusLoaded(obj))
                    {
                        (grid.Resources["MentionStatusOpenAnimation"] as Storyboard).Begin();
                    }
                    else if (GetIsMentionStatusLoading(obj))
                    {
                        (grid.Resources["MentionStatusLoadingOpenAnimation"] as Storyboard).Begin();
                    }
                    else
                    {
                        var cmd = GetCommand(obj);
                        var param = GetCommandParameter(obj);
                        if (cmd != null && cmd.CanExecute(param))
                            cmd.Execute(param);

                        (grid.Resources["MentionStatusLoadingOpenAnimation"] as Storyboard).Begin();
                    }
                }
                else if (e.Property == IsSelectedProperty)
                {
                    var grid = status.FindName("MentionStatusGrid") as Grid;

                    if (grid == null)
                        return;

                    (grid.Resources["MentionStatusCloseAnimation"] as Storyboard).Begin();
                }
            }
            catch
            {
            }
        }
        #endregion

        #region Media 関連
        public static bool GetMediaVisibility(DependencyObject obj) { return (bool)obj.GetValue(MediaVisibilityProperty); }
        public static void SetMediaVisibility(DependencyObject obj, bool value) { obj.SetValue(MediaVisibilityProperty, value); }

        public static readonly DependencyProperty MediaVisibilityProperty =
            DependencyProperty.Register("MediaVisibility", typeof(bool), typeof(TweetStatusBehavior), new PropertyMetadata(false, MediaVisibility_PropertyChanged));

        private static void MediaVisibility_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Grid;
            var itemsControl = status.FindName("MediaItemsControl") as ItemsControl;

            if (GetMediaVisibility(obj))
                itemsControl.Visibility = Visibility.Visible;
            else
                itemsControl.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region QuotedStatus 関連
        public static bool GetQuotedStatusVisibility(DependencyObject obj) { return (bool)obj.GetValue(QuotedStatusVisibilityProperty); }
        public static void SetQuotedStatusVisibility(DependencyObject obj, bool value) { obj.SetValue(QuotedStatusVisibilityProperty, value); }

        public static readonly DependencyProperty QuotedStatusVisibilityProperty =
            DependencyProperty.Register("QuotedStatusVisibility", typeof(bool), typeof(TweetStatusBehavior), new PropertyMetadata(false, QuotedStatus_PropertyChanged));

        private static void QuotedStatus_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Grid;
            var border = status.FindName("QuotedStatusBorder") as Border;

            if (GetQuotedStatusVisibility(obj))
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
