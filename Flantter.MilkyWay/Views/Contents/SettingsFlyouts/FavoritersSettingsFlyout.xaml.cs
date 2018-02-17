using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class FavoritersSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(FavoritersSettingsFlyoutViewModel),
                typeof(FavoritersSettingsFlyout), null);

        public FavoritersSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += FavoritersSettingsFlyout_SizeChanged;
            FavoritersSettingsFlyout_SizeChanged(null, null);
        }

        public FavoritersSettingsFlyoutViewModel ViewModel
        {
            get => (FavoritersSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void FavoritersSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            FavoritersGrid.Width = width;
        }
    }
}