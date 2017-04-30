using Windows.UI.Xaml.Input;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class AccountChangeSettingsFlyout : ExtendedSettingsFlyout
    {
        public AccountChangeSettingsFlyout()
        {
            InitializeComponent();
        }

        private void ItemsControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
        }
    }
}