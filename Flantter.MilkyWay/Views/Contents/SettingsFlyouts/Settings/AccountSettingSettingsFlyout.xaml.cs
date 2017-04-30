using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings
{
    public sealed partial class AccountSettingSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountSetting), typeof(AccountSettingSettingsFlyout),
                null);

        public AccountSettingSettingsFlyout()
        {
            InitializeComponent();
        }

        public AccountSetting ViewModel
        {
            get => (AccountSetting) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
        }
    }
}