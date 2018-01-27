using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class RetweetsOfMeSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(RetweetsOfMeSettingsFlyoutViewModel),
                typeof(RetweetsOfMeSettingsFlyout), null);

        public RetweetsOfMeSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += RetweetsOfMeSettingsFlyout_SizeChanged;
            RetweetsOfMeSettingsFlyout_SizeChanged(null, null);
        }

        public RetweetsOfMeSettingsFlyoutViewModel ViewModel
        {
            get => (RetweetsOfMeSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void RetweetsOfMeSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            RetweetsOfMeGrid.Width = width;
        }
    }
}