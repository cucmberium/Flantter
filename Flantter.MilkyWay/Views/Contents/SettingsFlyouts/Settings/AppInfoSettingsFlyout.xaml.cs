using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings
{
    public sealed partial class AppInfoSettingsFlyout : ExtendedSettingsFlyout
    {
        public AppInfoSettingsFlyout()
        {
            this.InitializeComponent();

            var version = Package.Current.Id.Version;
            this.SettingsFlyout_Settings_AppInfo_Version_TextBlock.Text = "Version " + string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }
    }
}
