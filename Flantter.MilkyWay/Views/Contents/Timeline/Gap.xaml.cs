using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;

namespace Flantter.MilkyWay.Views.Contents.Timeline
{
    public sealed partial class Gap : UserControl, IRecycleItem
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(GapViewModel), typeof(Gap), null);

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(EventMessage),
                new PropertyMetadata(false, IsSelectedPropertyChanged));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(Status),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(Status),
                new PropertyMetadata(null));

        public Gap()
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

        public GapViewModel ViewModel
        {
            get => (GapViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public bool IsSelected
        {
            get => (bool) GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public void ResetItem()
        {
            SetIsSelected(this, false);
        }

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand) obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static object GetCommandParameter(DependencyObject obj)
        {
            return obj.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        private static void IsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var gap = obj as Gap;
            var textblock = gap.FindName("GapTextblock") as TextBlock;
            var progressBar = gap.FindName("GapProgressBar") as ProgressBar;

            if (progressBar.IsIndeterminate)
                return;

            var cmd = GetCommand(obj);
            var param = GetCommandParameter(obj);
            if (cmd != null && cmd.CanExecute(param))
                cmd.Execute(param);

            textblock.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;
        }
    }
}