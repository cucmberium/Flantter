using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
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
    public sealed partial class Status : UserControl, IRecycleItem
    {
        public void ResetItem()
        {
            if (CommandGridLoaded)
            {
                this.CommandGrid.Visibility = Visibility.Collapsed;
                this.CommandGrid.Height = 0;
            }
            if (MentionStatusGridLoaded)
            {
                this.MentionStatusGrid.Visibility = Visibility.Collapsed;
                this.MentionStatusGrid.Height = 0;
                this.MentionStatusProgressBar.Visibility = Visibility.Collapsed;
                this.MentionStatusProgressBar.IsIndeterminate = false;
                this.MentionStatusMainGrid.Visibility = Visibility.Collapsed;
            }

            SetIsSelected(this, false);
        }

        public StatusViewModel ViewModel
        {
            get { return (StatusViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(StatusViewModel), typeof(Status), null);

        public static bool GetIsSelected(DependencyObject obj) { return (bool)obj.GetValue(IsSelectedProperty); }
        public static void SetIsSelected(DependencyObject obj, bool value) { obj.SetValue(IsSelectedProperty, value); }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Status), new PropertyMetadata(false, IsSelectedPropertyChanged));

        public Status()
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

        private static void IsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CommandGrid_PropertyChanged(obj, e);
            MentionStatus_PropertyChanged(obj, e);
        }

        #region MentionStatus 関連
        public static bool GetMentionStatusVisibility(DependencyObject obj) { return (bool)obj.GetValue(MentionStatusVisibilityProperty); }
        public static void SetMentionStatusVisibility(DependencyObject obj, bool value) { obj.SetValue(MentionStatusVisibilityProperty, value); }

        public static readonly DependencyProperty MentionStatusVisibilityProperty =
            DependencyProperty.Register("MentionStatusVisibility", typeof(bool), typeof(Status), new PropertyMetadata(false, MentionStatus_PropertyChanged));

        public static bool GetIsMentionStatusLoaded(DependencyObject obj) { return (bool)obj.GetValue(IsMentionStatusLoadedProperty); }
        public static void SetIsMentionStatusLoaded(DependencyObject obj, bool value) { obj.SetValue(IsMentionStatusLoadedProperty, value); }

        public static readonly DependencyProperty IsMentionStatusLoadedProperty =
            DependencyProperty.Register("IsMentionStatusLoaded", typeof(bool), typeof(Status), new PropertyMetadata(false, MentionStatus_PropertyChanged));

        public static bool GetIsMentionStatusLoading(DependencyObject obj) { return (bool)obj.GetValue(IsMentionStatusLoadingProperty); }
        public static void SetIsMentionStatusLoading(DependencyObject obj, bool value) { obj.SetValue(IsMentionStatusLoadingProperty, value); }

        public static readonly DependencyProperty IsMentionStatusLoadingProperty =
            DependencyProperty.Register("IsMentionStatusLoading", typeof(bool), typeof(Status), new PropertyMetadata(false, MentionStatus_PropertyChanged));

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }
        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(Status), new PropertyMetadata(null));

        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }
        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(Status), new PropertyMetadata(null));

        public bool MentionStatusGridLoaded = false;
        private static void MentionStatus_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;

            if (e.Property != MentionStatusVisibilityProperty && !GetMentionStatusVisibility(obj))
                return;

            try
            {
                if (e.Property == MentionStatusVisibilityProperty && !GetMentionStatusVisibility(obj))
                {
                    var grid = status.FindName("MentionStatusGrid") as Grid;
                    
                    if (grid == null)
                        return;

                    status.MentionStatusGridLoaded = true;

                    (grid.Resources["MentionStatusCloseAnimation"] as Storyboard).Begin();
                }
                else if (GetIsSelected(status))
                {
                    var grid = status.FindName("MentionStatusGrid") as Grid;

                    if (grid == null)
                        return;

                    status.MentionStatusGridLoaded = true;

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

                    status.MentionStatusGridLoaded = true;

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
            DependencyProperty.Register("MediaVisibility", typeof(bool), typeof(Status), new PropertyMetadata(false, MediaVisibility_PropertyChanged));

        private static void MediaVisibility_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;
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
            DependencyProperty.Register("QuotedStatusVisibility", typeof(bool), typeof(Status), new PropertyMetadata(false, QuotedStatus_PropertyChanged));

        private static void QuotedStatus_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;
            var border = status.FindName("QuotedStatusBorder") as Border;

            if (GetQuotedStatusVisibility(obj))
                border.Visibility = Visibility.Visible;
            else
                border.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region RetweetInformation 関連
        public static bool GetRetweetInformationVisibility(DependencyObject obj) { return (bool)obj.GetValue(RetweetInformationVisibilityProperty); }
        public static void SetRetweetInformationVisibility(DependencyObject obj, bool value) { obj.SetValue(RetweetInformationVisibilityProperty, value); }

        public static readonly DependencyProperty RetweetInformationVisibilityProperty =
            DependencyProperty.Register("RetweetInformationVisibility", typeof(bool), typeof(Status), new PropertyMetadata(false, RetweetInformationVisibility_PropertyChanged));

        private static void RetweetInformationVisibility_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;
            var itemsControl = status.FindName("RetweetInformationGrid") as Grid;

            if (GetRetweetInformationVisibility(obj))
                itemsControl.Visibility = Visibility.Visible;
            else
                itemsControl.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region RetweetFavoriteIcon 関連
        public static bool GetRetweetTriangleIconVisibility(DependencyObject obj) { return (bool)obj.GetValue(RetweetTriangleIconVisibilityProperty); }
        public static void SetRetweetTriangleIconVisibility(DependencyObject obj, bool value) { obj.SetValue(RetweetTriangleIconVisibilityProperty, value); }

        public static readonly DependencyProperty RetweetTriangleIconVisibilityProperty =
            DependencyProperty.Register("RetweetTriangleIconVisibility", typeof(bool), typeof(Status), new PropertyMetadata(false, RetweetTriangleIconVisibility_PropertyChanged));

        private static void RetweetTriangleIconVisibility_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;
            var itemsControl = status.FindName("RetweetTriangleIcon") as ContentControl;

            if (GetRetweetTriangleIconVisibility(obj))
                itemsControl.Visibility = Visibility.Visible;
            else
                itemsControl.Visibility = Visibility.Collapsed;
        }

        public static bool GetFavoriteTriangleIconVisibility(DependencyObject obj) { return (bool)obj.GetValue(FavoriteTriangleIconVisibilityProperty); }
        public static void SetFavoriteTriangleIconVisibility(DependencyObject obj, bool value) { obj.SetValue(FavoriteTriangleIconVisibilityProperty, value); }

        public static readonly DependencyProperty FavoriteTriangleIconVisibilityProperty =
            DependencyProperty.Register("FavoriteTriangleIconVisibility", typeof(bool), typeof(Status), new PropertyMetadata(false, FavoriteTriangleIconVisibility_PropertyChanged));

        private static void FavoriteTriangleIconVisibility_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;
            var itemsControl = status.FindName("FavoriteTriangleIcon") as ContentControl;

            if (GetFavoriteTriangleIconVisibility(obj))
                itemsControl.Visibility = Visibility.Visible;
            else
                itemsControl.Visibility = Visibility.Collapsed;
        }

        public static bool GetRetweetFavoriteTriangleIconVisibility(DependencyObject obj) { return (bool)obj.GetValue(RetweetFavoriteTriangleIconVisibilityProperty); }
        public static void SetRetweetFavoriteTriangleIconVisibility(DependencyObject obj, bool value) { obj.SetValue(RetweetFavoriteTriangleIconVisibilityProperty, value); }

        public static readonly DependencyProperty RetweetFavoriteTriangleIconVisibilityProperty =
            DependencyProperty.Register("RetweetFavoriteTriangleIconVisibility", typeof(bool), typeof(Status), new PropertyMetadata(false, RetweetFavoriteTriangleIconVisibility_PropertyChanged));

        private static void RetweetFavoriteTriangleIconVisibility_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;
            var itemsControl = status.FindName("RetweetFavoriteTriangleIcon") as ContentControl;

            if (GetRetweetFavoriteTriangleIconVisibility(obj))
                itemsControl.Visibility = Visibility.Visible;
            else
                itemsControl.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region CommandGrid 関連
        public bool CommandGridLoaded = false;
        private static void CommandGrid_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as Status;
            var grid = status.FindName("CommandGrid") as Grid;

            if (grid == null)
                return;

            status.CommandGridLoaded = true;

            if ((bool)e.NewValue)
            {
                (grid.Resources["TweetCommandBarOpenAnimation"] as Storyboard).Begin();
            }
            else
            {
                (grid.Resources["TweetCommandBarCloseAnimation"] as Storyboard).Begin();
            }
                
        }
        #endregion
    }
}