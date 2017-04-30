using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class UserCollectionsSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserCollectionsSettingsFlyoutViewModel),
                typeof(UserCollectionsSettingsFlyout), null);

        public UserCollectionsSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += UserCollectionsSettingsFlyout_SizeChanged;
            UserCollectionsSettingsFlyout_SizeChanged(null, null);
        }

        public UserCollectionsSettingsFlyoutViewModel ViewModel
        {
            get => (UserCollectionsSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void UserCollectionsSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            UserCollectionsGrid.Width = width;
        }
    }
}