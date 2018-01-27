using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class ListMembersSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ListMembersSettingsFlyoutViewModel),
                typeof(ListMembersSettingsFlyout), null);

        public ListMembersSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += ListMembersSettingsFlyout_SizeChanged;
            ListMembersSettingsFlyout_SizeChanged(null, null);
        }

        public ListMembersSettingsFlyoutViewModel ViewModel
        {
            get => (ListMembersSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void ListMembersSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            ListMembersGrid.Width = width;
        }
    }
}