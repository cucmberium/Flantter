using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.ViewModels;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class MainArea : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountViewModel), typeof(MainArea), null);

        public MainArea()
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