using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.ViewModels;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class BottomBarArea : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountViewModel), typeof(BottomBarArea), null);

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(BottomBarArea),
                new PropertyMetadata(0, SelectedIndex_Changed));

        public BottomBarArea()
        {
            InitializeComponent();

            SizeChanged += BottomBarArea_SizeChanged;

            BottomBarAreaOthersButtonMenuFlyoutExtension.ItemSelected +=
                BottomBarAreaOthersButton_Flyout_ItemSelected;
            BottomBarAreaOthersTextButtonMenuFlyoutExtension.ItemSelected +=
                BottomBarAreaOthersButton_Flyout_ItemSelected;

            BottomBarArea_SelectedIndexChanged();
        }

        public AccountViewModel ViewModel
        {
            get => (AccountViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public int SelectedIndex
        {
            get => (int) GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        private static void SelectedIndex_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bottomBarArea = d as BottomBarArea;
            bottomBarArea.BottomBarArea_SelectedIndexChanged();
        }

        private void BottomBarAreaOthersButton_Flyout_ItemSelected(object sender, ItemSelectedEventArgs e)
        {
            SelectedIndex = e.Index + 3;
        }

        public void BottomBarArea_SelectedIndexChanged()
        {
            BottomBarAreaHomeButton.Background =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"];
            BottomBarAreaHomeIcon.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"];
            BottomBarAreaMentionsButton.Background =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"];
            BottomBarAreaMentionsIcon.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"];
            BottomBarAreaDirectMessagesButton.Background =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"];
            BottomBarAreaDirectMessagesIcon.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"];
            BottomBarAreaOthersButton.Background =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"];
            BottomBarAreaOthersIcon.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"];

            BottomBarAreaHomeTextButton.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"];
            BottomBarAreaMentionsTextButton.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"];
            BottomBarAreaDirectMessagesTextButton.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"];
            BottomBarAreaOthersTextButton.Foreground =
                (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"];

            if (SelectedIndex == 0)
            {
                BottomBarAreaHomeButton.Background =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"];
                BottomBarAreaHomeIcon.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"];
                BottomBarAreaHomeTextButton.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"];
            }
            else if (SelectedIndex == 1)
            {
                BottomBarAreaMentionsButton.Background =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"];
                BottomBarAreaMentionsIcon.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"];
                BottomBarAreaMentionsTextButton.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"];
            }
            else if (SelectedIndex == 2)
            {
                BottomBarAreaDirectMessagesButton.Background =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"];
                BottomBarAreaDirectMessagesIcon.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"];
                BottomBarAreaDirectMessagesTextButton.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"];
            }
            else
            {
                BottomBarAreaOthersButton.Background =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"];
                BottomBarAreaOthersIcon.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"];
                BottomBarAreaOthersTextButton.Foreground =
                    (SolidColorBrush) Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"];
            }
        }

        private void BottomBarArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BottomBarAreaProfileImageButton.Width = e.NewSize.Height;
            BottomBarAreaProfileImageButton.Height = e.NewSize.Height;
        }

        private void BottomBarAreaHomeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectedIndex = 0;
        }

        private void BottomBarAreaMentionsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectedIndex = 1;
        }

        private void BottomBarAreaDirectMessagesButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectedIndex = 2;
        }
    }
}