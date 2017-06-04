using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class PublicTimelineSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PublicTimelineSettingsFlyoutViewModel),
                typeof(PublicTimelineSettingsFlyout), null);

        public PublicTimelineSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += PublicTimelineSettingsFlyout_SizeChanged;
            PublicTimelineSettingsFlyout_SizeChanged(null, null);
        }

        public PublicTimelineSettingsFlyoutViewModel ViewModel
        {
            get => (PublicTimelineSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void PublicTimelineSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            PublicTimelineGrid.Width = width;
        }
    }
}