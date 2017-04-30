using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Flantter.MilkyWay.ViewModels;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class TimelineArea : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ColumnViewModel), typeof(TimelineArea), null);

        public TimelineArea()
        {
            InitializeComponent();
            PointerReleased += TimelineArea_PointerReleased;
            PointerPressed += TimelineArea_PointerPressed;
        }

        public ColumnViewModel ViewModel
        {
            get => (ColumnViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void TimelineArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void TimelineArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}