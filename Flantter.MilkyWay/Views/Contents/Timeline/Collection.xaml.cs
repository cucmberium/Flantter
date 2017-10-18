using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Flantter.MilkyWay.ViewModels.Apis.Objects;

namespace Flantter.MilkyWay.Views.Contents.Timeline
{
    public sealed partial class Collection : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CollectionViewModel), typeof(Collection), null);

        public static readonly DependencyProperty IsCommandBarEnabledProperty =
            DependencyProperty.Register("IsCommandBarEnabled", typeof(bool), typeof(Collection),
                new PropertyMetadata(true));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Collection),
                new PropertyMetadata(false, IsSelectedPropertyChanged));

        public Collection()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                SelectorItem selector = null;
                DependencyObject dp = this;
                while ((dp = VisualTreeHelper.GetParent(dp)) != null)
                {
                    var i = dp as SelectorItem;
                    if (i != null)
                    {
                        selector = i;
                        break;
                    }
                }

                SetBinding(IsSelectedProperty, new Binding
                {
                    Path = new PropertyPath("IsSelected"),
                    Source = selector,
                    Mode = BindingMode.TwoWay
                });
            };
        }

        public CollectionViewModel ViewModel
        {
            get => (CollectionViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public bool IsCommandBarEnabled
        {
            get => (bool) GetValue(IsCommandBarEnabledProperty);
            set => SetValue(IsCommandBarEnabledProperty, value);
        }

        public bool IsSelected
        {
            get => (bool) GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public void ResetItem()
        {
            if (CommandGridLoaded)
            {
                CommandGrid.Visibility = Visibility.Collapsed;
                CommandGrid.Height = 0;
            }

            SetIsSelected(this, false);
        }

        public static bool GetIsCommandBarEnabled(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsCommandBarEnabledProperty);
        }

        public static void SetIsCommandBarEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCommandBarEnabledProperty, value);
        }

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        private static void IsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CommandGrid_PropertyChanged(obj, e);
        }

        #region CommandGrid 関連

        public bool CommandGridLoaded;

        private static void CommandGrid_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!GetIsCommandBarEnabled(obj))
                return;

            var status = obj as Collection;
            var grid = status.FindName("CommandGrid") as Grid;

            if (grid == null)
                return;

            status.CommandGridLoaded = true;

            if ((bool) e.NewValue)
                (grid.Resources["TweetCommandBarOpenAnimation"] as Storyboard).Begin();
            else
                (grid.Resources["TweetCommandBarCloseAnimation"] as Storyboard).Begin();
        }

        #endregion
    }
}