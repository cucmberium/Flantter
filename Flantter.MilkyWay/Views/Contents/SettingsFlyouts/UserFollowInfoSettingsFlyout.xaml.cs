using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class UserFollowInfoSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserFollowInfoSettingsFlyoutViewModel),
                typeof(UserFollowInfoSettingsFlyout), null);

        public UserFollowInfoSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += UserFollowInfoSettingsFlyout_SizeChanged;
            UserFollowInfoSettingsFlyout_SizeChanged(null, null);
        }

        public UserFollowInfoSettingsFlyoutViewModel ViewModel
        {
            get => (UserFollowInfoSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void UserFollowInfoSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            UserFollowInfoSettingsFlyoutPivot.Width = width;
        }
    }
}