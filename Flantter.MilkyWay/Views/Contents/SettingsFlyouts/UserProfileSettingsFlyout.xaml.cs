using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class UserProfileSettingsFlyout : ExtendedSettingsFlyout
    {
        public UserProfileSettingsFlyoutViewModel ViewModel
        {
            get { return (UserProfileSettingsFlyoutViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserProfileSettingsFlyoutViewModel), typeof(UserProfileSettingsFlyout), null);

        private ScrollViewer _UserProfileStatusesListViewScrollViewer;
        private ScrollViewer _UserProfileFavoritesListViewScrollViewer;
        private ScrollViewer _UserProfileFollowingListViewScrollViewer;
        private ScrollViewer _UserProfileFollowersListViewScrollViewer;
        public UserProfileSettingsFlyout()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                this.RootStackPanel.AddHandler(UIElement.PointerWheelChangedEvent, new PointerEventHandler(RootStackPanel_PointerWheelChanged), true);

                this.RootScrollViewer.ViewChanged += RootScrollViewer_ViewChanged;
                this.UserProfileTweetPivot.SelectionChanged += UserProfileTweetPivot_SelectionChanged;

                this.UserProfileStatusesListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
                this.UserProfileFavoritesListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
                this.UserProfileFollowingListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
                this.UserProfileFollowersListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
            };

            this.UserProfileStatusesListView.Loaded += (s, e) =>
            {
                _UserProfileStatusesListViewScrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.UserProfileStatusesListView, 0), 0) as ScrollViewer;
                _UserProfileStatusesListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(this.RootScrollViewer, null);
            };

            this.UserProfileFavoritesListView.Loaded += (s, e) =>
            {
                _UserProfileFavoritesListViewScrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.UserProfileFavoritesListView, 0), 0) as ScrollViewer;
                _UserProfileFavoritesListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(this.RootScrollViewer, null);
            };

            this.UserProfileFollowingListView.Loaded += (s, e) =>
            {
                _UserProfileFollowingListViewScrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.UserProfileFollowingListView, 0), 0) as ScrollViewer;
                _UserProfileFollowingListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(this.RootScrollViewer, null);
            };

            this.UserProfileFollowersListView.Loaded += (s, e) =>
            {
                _UserProfileFollowersListViewScrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.UserProfileFollowersListView, 0), 0) as ScrollViewer;
                _UserProfileFollowersListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(this.RootScrollViewer, null);
            };

            this.SizeChanged += UserProfileSettingsFlyout_SizeChanged;
            UserProfileSettingsFlyout_SizeChanged(null, null);
        }

        private void UserProfileTweetPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RootScrollViewer_ViewChanged(this.RootScrollViewer, null);
        }

        private void UserProfilePivotListView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private bool _RootScrollViewerLocked = false;
        private void RootStackPanel_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (!_RootScrollViewerLocked && this.RootStackPanel.Orientation == Orientation.Vertical)
                e.Handled = false;
        }
        
        private void UserProfilePivotListView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer.VerticalOffset > 2.1)
                _RootScrollViewerLocked = true;
            else
                _RootScrollViewerLocked = false;
        }

        private void RootScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (this.RootStackPanel.Orientation == Orientation.Vertical)
            {
                if (this.RootScrollViewer.VerticalOffset == this.UserProfileInformationGrid.ActualHeight)
                {
                    if (_UserProfileStatusesListViewScrollViewer != null)
                    {
                        _UserProfileStatusesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _UserProfileStatusesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    }
                    if (_UserProfileFavoritesListViewScrollViewer != null)
                    {
                        _UserProfileFavoritesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _UserProfileFavoritesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    }
                    if (_UserProfileFollowersListViewScrollViewer != null)
                    {
                        _UserProfileFollowersListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _UserProfileFollowersListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    }
                    if (_UserProfileFollowingListViewScrollViewer != null)
                    {
                        _UserProfileFollowingListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _UserProfileFollowingListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    }
                }
                else
                {
                    if (_UserProfileStatusesListViewScrollViewer != null)
                    {
                        _UserProfileStatusesListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _UserProfileStatusesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    }
                    if (_UserProfileFavoritesListViewScrollViewer != null)
                    {
                        _UserProfileFavoritesListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _UserProfileFavoritesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    }
                    if (_UserProfileFollowersListViewScrollViewer != null)
                    {
                        _UserProfileFollowersListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _UserProfileFollowersListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    }
                    if (_UserProfileFollowingListViewScrollViewer != null)
                    {
                        _UserProfileFollowingListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _UserProfileFollowingListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    }
                }
            }
            else
            {
                if (_UserProfileStatusesListViewScrollViewer != null)
                {
                    _UserProfileStatusesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _UserProfileStatusesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                if (_UserProfileFavoritesListViewScrollViewer != null)
                {
                    _UserProfileFavoritesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _UserProfileFavoritesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                if (_UserProfileFollowersListViewScrollViewer != null)
                {
                    _UserProfileFollowersListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _UserProfileFollowersListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                if (_UserProfileFollowingListViewScrollViewer != null)
                {
                    _UserProfileFollowingListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _UserProfileFollowingListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
            }
        }

        private void UserProfileSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width <= 320)
                width = 320;
            else if (width >= 400 && width < 802)
                width = 400;
            else if (width >= 802)
                width = 802;

            this.Width = width;

            this.RootStackPanel.Orientation = width >= 800 ? Orientation.Horizontal : Orientation.Vertical;
            this.UserProfileTweetPivot.Height = Window.Current.Bounds.Height - 70;

            if (width >= 802)
            {
                this.UserProfileTweetPivot.Width = 400;
                this.UserProfileInformationGrid.Width = 400;
                this.UserProfileVerticalBar.Visibility = Visibility.Visible;
            }
            else
            {
                this.UserProfileTweetPivot.Width = double.NaN;
                this.UserProfileInformationGrid.Width = double.NaN;
                this.UserProfileVerticalBar.Visibility = Visibility.Collapsed;
            }

            RootScrollViewer_ViewChanged(this.RootScrollViewer, null);
        }
    }
}
