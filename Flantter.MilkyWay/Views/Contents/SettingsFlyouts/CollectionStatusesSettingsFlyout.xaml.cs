using Windows.UI.Xaml;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class CollectionStatusesSettingsFlyout : ExtendedSettingsFlyout
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CollectionStatusesSettingsFlyoutViewModel),
                typeof(CollectionStatusesSettingsFlyout), null);

        public CollectionStatusesSettingsFlyout()
        {
            InitializeComponent();
            SizeChanged += CollectionStatusesSettingsFlyout_SizeChanged;
            CollectionStatusesSettingsFlyout_SizeChanged(null, null);
        }

        public CollectionStatusesSettingsFlyoutViewModel ViewModel
        {
            get => (CollectionStatusesSettingsFlyoutViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void CollectionStatusesSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;

            Width = width;

            CollectionStatusesGrid.Width = width;
        }
    }
}