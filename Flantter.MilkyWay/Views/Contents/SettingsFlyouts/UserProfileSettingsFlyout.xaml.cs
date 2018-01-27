using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class UserProfileSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserProfileSettingsFlyoutViewModel),
                typeof(UserProfileSettingsFlyout), null);

        private bool _rootScrollViewerLocked;
        private ScrollViewer _userProfileFavoritesListViewScrollViewer;
        private ScrollViewer _userProfileFollowersListViewScrollViewer;
        private ScrollViewer _userProfileFollowingListViewScrollViewer;

        private ScrollViewer _userProfileStatusesListViewScrollViewer;

        public UserProfileSettingsFlyout()
        {
            Showed += (s, e) => { RootScrollViewer.ChangeView(null, 0.0, null, true); };

            InitializeComponent();

            Loaded += (s, e) =>
            {
                RootStackPanel.AddHandler(PointerWheelChangedEvent,
                    new PointerEventHandler(RootStackPanel_PointerWheelChanged), true);

                RootScrollViewer.ViewChanged += RootScrollViewer_ViewChanged;
                UserProfileTweetPivot.SelectionChanged += UserProfileTweetPivot_SelectionChanged;

                UserProfileStatusesListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
                UserProfileFavoritesListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
                UserProfileFollowingListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
                UserProfileFollowersListView.PointerWheelChanged += UserProfilePivotListView_PointerWheelChanged;
            };

            UserProfileStatusesListView.Loaded += (s, e) =>
            {
                _userProfileStatusesListViewScrollViewer =
                    VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(UserProfileStatusesListView, 0),
                        0) as ScrollViewer;
                _userProfileStatusesListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(RootScrollViewer, null);
            };

            UserProfileFavoritesListView.Loaded += (s, e) =>
            {
                _userProfileFavoritesListViewScrollViewer =
                    VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(UserProfileFavoritesListView, 0),
                        0) as ScrollViewer;
                _userProfileFavoritesListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(RootScrollViewer, null);
            };

            UserProfileFollowingListView.Loaded += (s, e) =>
            {
                _userProfileFollowingListViewScrollViewer =
                    VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(UserProfileFollowingListView, 0),
                        0) as ScrollViewer;
                _userProfileFollowingListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(RootScrollViewer, null);
            };

            UserProfileFollowersListView.Loaded += (s, e) =>
            {
                _userProfileFollowersListViewScrollViewer =
                    VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(UserProfileFollowersListView, 0),
                        0) as ScrollViewer;
                _userProfileFollowersListViewScrollViewer.ViewChanged += UserProfilePivotListView_ViewChanged;

                RootScrollViewer_ViewChanged(RootScrollViewer, null);
            };

            SizeChanged += UserProfileSettingsFlyout_SizeChanged;
            UserProfileSettingsFlyout_SizeChanged(null, null);
        }

        public UserProfileSettingsFlyoutViewModel ViewModel
        {
            get => (UserProfileSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void UserProfileTweetPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RootScrollViewer_ViewChanged(RootScrollViewer, null);
        }

        private void UserProfilePivotListView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void RootStackPanel_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (!_rootScrollViewerLocked && RootStackPanel.Orientation == Orientation.Vertical)
                e.Handled = false;
        }

        private void UserProfilePivotListView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null && scrollViewer.VerticalOffset > 2.1)
                _rootScrollViewerLocked = true;
            else
                _rootScrollViewerLocked = false;
        }

        private void RootScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (RootStackPanel.Orientation == Orientation.Vertical)
            {
                if (RootScrollViewer.VerticalOffset >= UserProfileInformationGrid.ActualHeight - 0.01)
                {
                    if (_userProfileStatusesListViewScrollViewer != null)
                    {
                        _userProfileStatusesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _userProfileStatusesListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Visible;
                    }
                    if (_userProfileFavoritesListViewScrollViewer != null)
                    {
                        _userProfileFavoritesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _userProfileFavoritesListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Visible;
                    }
                    if (_userProfileFollowersListViewScrollViewer != null)
                    {
                        _userProfileFollowersListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _userProfileFollowersListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Visible;
                    }
                    if (_userProfileFollowingListViewScrollViewer != null)
                    {
                        _userProfileFollowingListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                        _userProfileFollowingListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Visible;
                    }
                }
                else
                {
                    if (_userProfileStatusesListViewScrollViewer != null)
                    {
                        _userProfileStatusesListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _userProfileStatusesListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Disabled;
                    }
                    if (_userProfileFavoritesListViewScrollViewer != null)
                    {
                        _userProfileFavoritesListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _userProfileFavoritesListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Disabled;
                    }
                    if (_userProfileFollowersListViewScrollViewer != null)
                    {
                        _userProfileFollowersListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _userProfileFollowersListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Disabled;
                    }
                    if (_userProfileFollowingListViewScrollViewer != null)
                    {
                        _userProfileFollowingListViewScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        _userProfileFollowingListViewScrollViewer.VerticalScrollBarVisibility =
                            ScrollBarVisibility.Disabled;
                    }
                }
            }
            else
            {
                if (_userProfileStatusesListViewScrollViewer != null)
                {
                    _userProfileStatusesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _userProfileStatusesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                if (_userProfileFavoritesListViewScrollViewer != null)
                {
                    _userProfileFavoritesListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _userProfileFavoritesListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                if (_userProfileFollowersListViewScrollViewer != null)
                {
                    _userProfileFollowersListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _userProfileFollowersListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                if (_userProfileFollowingListViewScrollViewer != null)
                {
                    _userProfileFollowingListViewScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                    _userProfileFollowingListViewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
            }
        }

        private void UserProfileSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width <= 320)
                width = 320;
            else if (width >= 400 && width < 802)
                width = 400;
            else if (width >= 802)
                width = 802;

            Width = width;

            RootStackPanel.Orientation = width >= 800 ? Orientation.Horizontal : Orientation.Vertical;
            UserProfileTweetPivot.Height = WindowSizeHelper.Instance.ClientHeight + WindowSizeHelper.Instance.VisibleBounds.Top - 70;

            if (width >= 802)
            {
                UserProfileTweetPivot.Width = 400;
                UserProfileInformationGrid.Width = 400;
                UserProfileVerticalBar.Visibility = Visibility.Visible;
            }
            else
            {
                UserProfileTweetPivot.Width = double.NaN;
                UserProfileInformationGrid.Width = double.NaN;
                UserProfileVerticalBar.Visibility = Visibility.Collapsed;
            }

            RootScrollViewer_ViewChanged(RootScrollViewer, null);
        }
    }
}