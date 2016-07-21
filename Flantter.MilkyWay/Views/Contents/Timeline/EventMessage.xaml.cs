using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents.Timeline
{
    public sealed partial class EventMessage : UserControl, IRecycleItem
    {
        public void ResetItem()
        {
            if (CommandGridLoaded)
            {
                this.CommandGrid.Visibility = Visibility.Collapsed;
                this.CommandGrid.Height = 0;
            }

            SetIsSelected(this, false);
        }

        public EventMessageViewModel ViewModel
        {
            get { return (EventMessageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(EventMessageViewModel), typeof(EventMessage), null);

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static bool GetIsSelected(DependencyObject obj) { return (bool)obj.GetValue(IsSelectedProperty); }
        public static void SetIsSelected(DependencyObject obj, bool value) { obj.SetValue(IsSelectedProperty, value); }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(EventMessage), new PropertyMetadata(false, IsSelectedPropertyChanged));

        private static void IsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CommandGrid_PropertyChanged(obj, e);

            var eventMessage = obj as EventMessage;
            var textblock = eventMessage.FindName("EventMessageBodyText") as TextBlock;
            textblock.IsTextSelectionEnabled = (bool)e.NewValue && Setting.SettingService.Setting.EnableTweetTextSelection;
        }

        #region TargetStatus 関連
        public static bool GetTargetStatusVisibility(DependencyObject obj) { return (bool)obj.GetValue(GetTargetStatusVisibilityProperty); }
        public static void SetTargetStatusVisibility(DependencyObject obj, bool value) { obj.SetValue(GetTargetStatusVisibilityProperty, value); }

        public static readonly DependencyProperty GetTargetStatusVisibilityProperty =
            DependencyProperty.Register("TargetStatusVisibility", typeof(bool), typeof(EventMessage), new PropertyMetadata(false, TargetStatus_PropertyChanged));

        private static void TargetStatus_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as EventMessage;
            var border = status.FindName("TargetStatusBorder") as Border;
            if (GetTargetStatusVisibility(obj))
                border.Visibility = Visibility.Visible;
            else
                border.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region CommandGrid 関連
        public bool CommandGridLoaded = false;
        private static void CommandGrid_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as EventMessage;
            var grid = status.FindName("CommandGrid") as Grid;

            if (grid == null)
                return;

            status.CommandGridLoaded = true;

            if ((bool)e.NewValue)
                (grid.Resources["TweetCommandBarOpenAnimation"] as Storyboard).Begin();
            else
                (grid.Resources["TweetCommandBarCloseAnimation"] as Storyboard).Begin();
        }
        #endregion

        public EventMessage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                SelectorItem selector = null;
                DependencyObject dp = this;
                while ((dp = VisualTreeHelper.GetParent(dp)) != null)
                {
                    var i = dp as SelectorItem;
                    if (i != null) { selector = i; break; }
                }

                this.SetBinding(IsSelectedProperty, new Binding
                {
                    Path = new PropertyPath("IsSelected"),
                    Source = selector,
                    Mode = BindingMode.TwoWay
                });
            };
        }
    }
}
