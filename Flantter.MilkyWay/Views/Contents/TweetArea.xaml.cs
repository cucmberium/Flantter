using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.ViewModels;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class TweetArea : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(TweetAreaViewModel), typeof(TweetArea), null);

        public TweetArea()
        {
            InitializeComponent();
        }

        public TweetAreaViewModel ViewModel
        {
            get => (TweetAreaViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}