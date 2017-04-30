using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts.Settings;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings
{
    public sealed partial class ColumnSettingSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ColumnSettingSettingsFlyoutViewModel),
                typeof(ColumnSettingSettingsFlyout), null);

        public ColumnSettingSettingsFlyout()
        {
            InitializeComponent();
        }

        public ColumnSettingSettingsFlyoutViewModel ViewModel
        {
            get => (ColumnSettingSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}