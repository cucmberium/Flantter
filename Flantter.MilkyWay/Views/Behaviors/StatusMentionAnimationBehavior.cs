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
    public class StatusMentionAnimationBehavior
    {
        public static bool GetIsSelected(DependencyObject obj) { return (bool)obj.GetValue(IsSelectedProperty); }
        public static void SetIsSelected(DependencyObject obj, bool value) { obj.SetValue(IsSelectedProperty, value); }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(StatusMentionAnimationBehavior), new PropertyMetadata(false, PropertyChanged));

        public static bool GetMentionStatusVisibility(DependencyObject obj) { return (bool)obj.GetValue(MentionStatusVisibilityProperty); }
        public static void SetMentionStatusVisibility(DependencyObject obj, bool value) { obj.SetValue(MentionStatusVisibilityProperty, value); }

        public static readonly DependencyProperty MentionStatusVisibilityProperty =
            DependencyProperty.Register("MentionStatusVisibility", typeof(bool), typeof(StatusMentionAnimationBehavior), new PropertyMetadata(false, PropertyChanged));

        public static bool GetIsMentionStatusLoaded(DependencyObject obj) { return (bool)obj.GetValue(IsMentionStatusLoadedProperty); }
        public static void SetIsMentionStatusLoaded(DependencyObject obj, bool value) { obj.SetValue(IsMentionStatusLoadedProperty, value); }

        public static readonly DependencyProperty IsMentionStatusLoadedProperty =
            DependencyProperty.Register("IsMentionStatusLoaded", typeof(bool), typeof(StatusMentionAnimationBehavior), new PropertyMetadata(false, PropertyChanged));

        public static bool GetIsMentionStatusLoading(DependencyObject obj) { return (bool)obj.GetValue(IsMentionStatusLoadingProperty); }
        public static void SetIsMentionStatusLoading(DependencyObject obj, bool value) { obj.SetValue(IsMentionStatusLoadingProperty, value); }

        public static readonly DependencyProperty IsMentionStatusLoadingProperty =
            DependencyProperty.Register("IsMentionStatusLoading", typeof(bool), typeof(StatusMentionAnimationBehavior), new PropertyMetadata(false, PropertyChanged));


        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }
        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(StatusMentionAnimationBehavior), new PropertyMetadata(null));

        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }
        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }
        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(StatusMentionAnimationBehavior), new PropertyMetadata(null));


        private static void PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var grid = obj as Grid;

            if (grid == null)
                return;

            if (!GetMentionStatusVisibility(obj))
                return;

            try
            {
                if (GetIsSelected(obj))
                {
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
                else
                {
                    (grid.Resources["MentionStatusCloseAnimation"] as Storyboard).Begin();
                }
            }
            catch
            {
            }
                
        }
    }
}
