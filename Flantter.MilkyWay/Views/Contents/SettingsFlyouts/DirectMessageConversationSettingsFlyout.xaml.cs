using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class DirectMessageConversationSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(DirectMessageConversationSettingsFlyoutViewModel),
                typeof(DirectMessageConversationSettingsFlyout), null);

        public DirectMessageConversationSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += DirectMessageConversationSettingsFlyout_SizeChanged;
            DirectMessageConversationSettingsFlyout_SizeChanged(null, null);
        }

        public DirectMessageConversationSettingsFlyoutViewModel ViewModel
        {
            get => (DirectMessageConversationSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void DirectMessageConversationSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            DirectMessageConversationGrid.Width = width;
        }
    }
}