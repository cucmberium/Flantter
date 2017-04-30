using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts.Settings;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings
{
    public sealed partial class MuteSettingSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MuteSettingSettingsFlyoutViewModel),
                typeof(MuteSettingSettingsFlyout), null);

        public MuteSettingSettingsFlyout()
        {
            InitializeComponent();
        }

        public MuteSettingSettingsFlyoutViewModel ViewModel
        {
            get => (MuteSettingSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}