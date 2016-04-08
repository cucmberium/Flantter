using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
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

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings
{
    public sealed partial class AccountSettingSettingsFlyout : ExtendedSettingsFlyout
    {
        public AccountSettingSettingsFlyout()
        {
            this.InitializeComponent();
        }

        public AccountSetting ViewModel
        {
            get { return (AccountSetting)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountSetting), typeof(AccountSettingSettingsFlyout), null);

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
