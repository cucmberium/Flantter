using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class UserMediaStatusesSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserMediaStatusesSettingsFlyoutViewModel),
                typeof(UserMediaStatusesSettingsFlyout), null);

        public UserMediaStatusesSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += UserMediaStatusesSettingsFlyout_SizeChanged;
            UserMediaStatusesSettingsFlyout_SizeChanged(null, null);
        }

        public UserMediaStatusesSettingsFlyoutViewModel ViewModel
        {
            get => (UserMediaStatusesSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void UserMediaStatusesSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            UserMediaStatusesGrid.Width = width;
        }
    }
}