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

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyout
{
    public sealed partial class UserProfileSettingsFlyout : ExtendedSettingsFlyout
    {
        public UserProfileSettingsFlyout()
        {
            this.InitializeComponent();
            this.SizeChanged += UserProfileSettingsFlyout_SizeChanged;
        }

        private void UserProfileSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width - 1;

            if (width <= 320)
                width = 321;
            else if (width >= 400 && width < 800)
                width = 400;
            else if (width >= 800)
                width = 800;

            this.Width = width;

            this.UserProfileStackPanel.Orientation = width >= 800 ? Orientation.Horizontal : Orientation.Vertical;

            if (width >= 800)
                this.UserProfileTweetGrid.Height = Window.Current.Bounds.Height - 54;
            else
                this.UserProfileTweetGrid.Height = double.NaN;

        }
    }
}
