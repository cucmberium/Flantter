using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class DirectMessagesSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(DirectMessagesSettingsFlyoutViewModel),
                typeof(DirectMessagesSettingsFlyout), null);

        public DirectMessagesSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += DirectMessagesSettingsFlyout_SizeChanged;
            DirectMessagesSettingsFlyout_SizeChanged(null, null);
        }

        public DirectMessagesSettingsFlyoutViewModel ViewModel
        {
            get => (DirectMessagesSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void DirectMessagesSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            DirectMessagesGrid.Width = width;
        }
    }
}