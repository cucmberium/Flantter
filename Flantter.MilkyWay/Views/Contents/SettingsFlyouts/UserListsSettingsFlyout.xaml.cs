using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class UserListsSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserListsSettingsFlyoutViewModel),
                typeof(UserListsSettingsFlyout), null);

        public UserListsSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += UserListsSettingsFlyout_SizeChanged;
            UserListsSettingsFlyout_SizeChanged(null, null);
        }

        public UserListsSettingsFlyoutViewModel ViewModel
        {
            get => (UserListsSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void UserListsSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            UserListsSettingsFlyoutPivot.Width = width;
        }
    }
}