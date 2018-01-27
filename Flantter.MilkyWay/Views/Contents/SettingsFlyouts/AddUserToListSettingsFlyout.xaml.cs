using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class AddUserToListsSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AddUserToListsSettingsFlyoutViewModel),
                typeof(AddUserToListsSettingsFlyout), null);

        public AddUserToListsSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += AddStatusToCollectionSettingsFlyout_SizeChanged;
            AddStatusToCollectionSettingsFlyout_SizeChanged(null, null);
        }

        public AddUserToListsSettingsFlyoutViewModel ViewModel
        {
            get => (AddUserToListsSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void AddStatusToCollectionSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = WindowSizeHelper.Instance.ClientWidth;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            UserListsGrid.Width = width;
        }
    }
}