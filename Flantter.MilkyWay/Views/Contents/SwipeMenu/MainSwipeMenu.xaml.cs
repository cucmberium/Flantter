using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.ViewModels;

namespace Flantter.MilkyWay.Views.Contents.SwipeMenu
{
    public sealed partial class MainSwipeMenu : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountViewModel), typeof(MainSwipeMenu), null);

        public MainSwipeMenu()
        {
            InitializeComponent();
        }

        public AccountViewModel ViewModel
        {
            get => (AccountViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}