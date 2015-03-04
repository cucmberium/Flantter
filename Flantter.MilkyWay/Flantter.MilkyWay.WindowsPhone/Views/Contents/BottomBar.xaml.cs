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
    public sealed partial class BottomBar : UserControl
    {
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(BottomBar), new PropertyMetadata(0, SelectedIndex_Changed));

        private static void SelectedIndex_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bottomBar = d as BottomBar;
            bottomBar.BottomBar_SelectedIndexChanged();
        }

        public BottomBar()
        {
            this.InitializeComponent();

            this.BottomBar_OthersButton_Flyout.ItemSelected += BottomBar_OthersButton_Flyout_ItemSelected;

            this.BottomBar_OthersButton_Flyout.Source = new ObservableCollection<string>() { "Events", "Favorites" };

            this.BottomBar_SelectedIndexChanged();
        }

        private void BottomBar_OthersButton_Flyout_ItemSelected(object sender, Controls.ItemSelectedEventArgs e)
        {
            this.SelectedIndex = 3 + e.Index;
        }

        public void BottomBar_SelectedIndexChanged()
        {
            this.BottomBar_HomeButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBar_HomeIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedForegroundBrush"]);
            this.BottomBar_MentionsButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBar_MentionsIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedForegroundBrush"]);
            this.BottomBar_DirectMessagesButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBar_DirectMessagesIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedForegroundBrush"]);
            this.BottomBar_OthersButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedBackgroundBrush"]);
            this.BottomBar_OthersIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedForegroundBrush"]);

            if (this.SelectedIndex == 0)
            {
                this.BottomBar_HomeButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBar_HomeIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedForegroundBrush"]);
            }
            else if (this.SelectedIndex == 1)
            {
                this.BottomBar_MentionsButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBar_MentionsIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedForegroundBrush"]);
            }
            else if (this.SelectedIndex == 2)
            {
                this.BottomBar_DirectMessagesButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBar_DirectMessagesIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedForegroundBrush"]);
            }
            else
            {
                this.BottomBar_OthersButton.Background = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedBackgroundBrush"]);
                this.BottomBar_OthersIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedForegroundBrush"]);
            }
        }

        private void BottomBar_HomeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectedIndex = 0;
        }

        private void BottomBar_MentionsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectedIndex = 1;
        }

        private void BottomBar_DirectMessagesButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectedIndex = 2;
        }
    }
}
