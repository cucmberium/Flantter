using Flantter.MilkyWay.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// ユーザー コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class BottomBarArea : UserControl
    {
        public AccountViewModel ViewModel
        {
            get { return (AccountViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountViewModel), typeof(BottomBarArea), null);

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(BottomBarArea), new PropertyMetadata(0, SelectedIndex_Changed));

        private static void SelectedIndex_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bottomBarArea = d as BottomBarArea;
            bottomBarArea.BottomBarArea_SelectedIndexChanged();
        }

        public BottomBarArea()
        {
            this.InitializeComponent();

            this.SizeChanged += BottomBarArea_SizeChanged;

            this.BottomBarArea_OthersButton_Flyout.ItemSelected += this.BottomBarArea_OthersButton_Flyout_ItemSelected;
            this.BottomBarArea_OthersTextButton_Flyout.ItemSelected += this.BottomBarArea_OthersButton_Flyout_ItemSelected;

            this.BottomBarArea_OthersButton_Flyout.Source = new ObservableCollection<string>() { "Events", "Favorites" };
            this.BottomBarArea_OthersTextButton_Flyout.Source = new ObservableCollection<string>() { "Events", "Favorites" };

            this.BottomBarArea_SelectedIndexChanged();
        }

        private void BottomBarArea_OthersButton_Flyout_ItemSelected(object sender, Controls.ItemSelectedEventArgs e)
        {
            this.SelectedIndex = 3 + e.Index;
        }

        public void BottomBarArea_SelectedIndexChanged()
        {
            this.BottomBarArea_HomeButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBarArea_HomeIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]);
            this.BottomBarArea_MentionsButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBarArea_MentionsIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]);
            this.BottomBarArea_DirectMessagesButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBarArea_DirectMessagesIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]);
            this.BottomBarArea_OthersButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBarArea_OthersIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]);

            this.BottomBarArea_HomeTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]);
            this.BottomBarArea_MentionsTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]);
            this.BottomBarArea_DirectMessagesTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]);
            this.BottomBarArea_OthersTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]);

            if (this.SelectedIndex == 0)
            {
                this.BottomBarArea_HomeButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBarArea_HomeIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]);
                this.BottomBarArea_HomeTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]);
            }
            else if (this.SelectedIndex == 1)
            {
                this.BottomBarArea_MentionsButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBarArea_MentionsIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]);
                this.BottomBarArea_MentionsTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]);
            }
            else if (this.SelectedIndex == 2)
            {
                this.BottomBarArea_DirectMessagesButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBarArea_DirectMessagesIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]);
                this.BottomBarArea_DirectMessagesTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]);
            }
            else
            {
                this.BottomBarArea_OthersButton.Background = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBarArea_OthersIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]);
                this.BottomBarArea_OthersTextButton.Foreground = ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]);
            }
        }

        void BottomBarArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.BottomBarArea_ProfileImageButton.Width = e.NewSize.Height;
            this.BottomBarArea_ProfileImageButton.Height = e.NewSize.Height;

            if (e.NewSize.Width < 384)
                VisualStateManager.GoToState(this, "Under384px", true);
            else if (e.NewSize.Width < 500)
                VisualStateManager.GoToState(this, "Under500px", true);
            else if (e.NewSize.Width < 700)
                VisualStateManager.GoToState(this, "Under700px", true);
            else
                VisualStateManager.GoToState(this, "Default", true);
        }

        private void BottomBarArea_HomeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectedIndex = 0;
        }

        private void BottomBarArea_MentionsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectedIndex = 1;
        }

        private void BottomBarArea_DirectMessagesButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectedIndex = 2;
        }

        private void BottomBarArea_ProfileImageButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.SettingService.Setting.Theme = Setting.SettingService.Setting.Theme == "Dark" ? "Light" : "Dark";
        }
    }
}
