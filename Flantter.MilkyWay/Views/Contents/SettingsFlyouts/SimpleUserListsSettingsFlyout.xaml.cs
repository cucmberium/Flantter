using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class SimpleUserListsSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SimpleUserListsSettingsFlyoutViewModel),
                typeof(UserCollectionsSettingsFlyout), null);

        public SimpleUserListsSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += UserCollectionsSettingsFlyout_SizeChanged;
            UserCollectionsSettingsFlyout_SizeChanged(null, null);
        }

        public SimpleUserListsSettingsFlyoutViewModel ViewModel
        {
            get => (SimpleUserListsSettingsFlyoutViewModel) GetValue(ViewModelProperty);
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

            UserListsGrid.Width = width;
        }
    }
}