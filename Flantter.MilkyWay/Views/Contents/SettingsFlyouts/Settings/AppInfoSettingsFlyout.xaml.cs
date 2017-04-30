using Windows.ApplicationModel;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings
{
    public sealed partial class AppInfoSettingsFlyout : ExtendedSettingsFlyout
    {
        public AppInfoSettingsFlyout()
        {
            InitializeComponent();

            var version = Package.Current.Id.Version;
            SettingsFlyoutSettingsAppInfoVersionTextBlock.Text =
                "Version " + string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }
    }
}