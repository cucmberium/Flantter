using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class ConversationSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ConversationSettingsFlyoutViewModel),
                typeof(ConversationSettingsFlyout), null);

        public ConversationSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += ConversationSettingsFlyout_SizeChanged;
            ConversationSettingsFlyout_SizeChanged(null, null);
        }

        public ConversationSettingsFlyoutViewModel ViewModel
        {
            get => (ConversationSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void ConversationSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            ConversationGrid.Width = width;
        }
    }
}