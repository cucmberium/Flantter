using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class RetweetersSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(RetweetersSettingsFlyoutViewModel),
                typeof(RetweetersSettingsFlyout), null);

        public RetweetersSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += RetweetersSettingsFlyout_SizeChanged;
            RetweetersSettingsFlyout_SizeChanged(null, null);
        }

        public RetweetersSettingsFlyoutViewModel ViewModel
        {
            get => (RetweetersSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void RetweetersSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            RetweetersGrid.Width = width;
        }
    }
}