using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class ListStatusesSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ListStatusesSettingsFlyoutViewModel),
                typeof(ListStatusesSettingsFlyout), null);

        public ListStatusesSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += ListStatusesSettingsFlyout_SizeChanged;
            ListStatusesSettingsFlyout_SizeChanged(null, null);
        }

        public ListStatusesSettingsFlyoutViewModel ViewModel
        {
            get => (ListStatusesSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void ListStatusesSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            ListStatusesGrid.Width = width;
        }
    }
}