using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class StatusDetailSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(StatusDetailSettingsFlyoutViewModel),
                typeof(StatusDetailSettingsFlyout), null);

        public StatusDetailSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += StatusDetailSettingsFlyout_SizeChanged;
            StatusDetailSettingsFlyout_SizeChanged(null, null);
        }

        public StatusDetailSettingsFlyoutViewModel ViewModel
        {
            get => (StatusDetailSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void StatusDetailSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            StatusDetailSettingsFlyoutPivot.Width = width;
        }
    }
}