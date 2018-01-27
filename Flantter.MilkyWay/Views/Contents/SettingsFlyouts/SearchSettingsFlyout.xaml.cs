using System.Threading.Tasks;
using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class SearchSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SearchSettingsFlyoutViewModel),
                typeof(SearchSettingsFlyout), null);

        public SearchSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += SearchSettingsFlyout_SizeChanged;
            SearchSettingsFlyout_SizeChanged(null, null);
        }

        public SearchSettingsFlyoutViewModel ViewModel
        {
            get => (SearchSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void SearchSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            SearchSettingsFlyoutPivot.Width = width;
        }

        public async void FocusToStatusSearchBox()
        {
            await Task.Delay(50);
            SearchSettingsFlyoutStatusSearchBox.Focus(FocusState.Keyboard);
        }
    }
}