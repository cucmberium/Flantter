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

        public UserProfileSettingsFlyout()
        {
            this.InitializeComponent();
            this.SizeChanged += UserProfileSettingsFlyout_SizeChanged;
            UserProfileSettingsFlyout_SizeChanged(null, null);
        }

        private void UserProfileSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400 && width < 802)
                width = 400;
            else if (width >= 802)
                width = 802;

            this.Width = width;

            this.UserProfileStackPanel.Orientation = width >= 800 ? Orientation.Horizontal : Orientation.Vertical;

            if (width >= 802)
            {
                this.UserProfileTweetGrid.Height = Window.Current.Bounds.Height - 70;
                this.UserProfileTweetGrid.Width = 400;
                this.UserProfileInformationGrid.Width = 400;
                this.UserProfileVerticalBar.Visibility = Visibility.Visible;
            }
            else
            {
                this.UserProfileTweetGrid.Height = double.NaN;
                this.UserProfileTweetGrid.Width = double.NaN;
                this.UserProfileInformationGrid.Width = double.NaN;
                this.UserProfileVerticalBar.Visibility = Visibility.Collapsed;
            }

        }
    }
}
