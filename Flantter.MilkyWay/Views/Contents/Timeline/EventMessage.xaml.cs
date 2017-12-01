using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Apis.Objects;

namespace Flantter.MilkyWay.Views.Contents.Timeline
{
    public sealed partial class EventMessage : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(EventMessageViewModel), typeof(EventMessage), null);

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(EventMessage),
                new PropertyMetadata(false, IsSelectedPropertyChanged));

        public EventMessage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                SelectorItem selector = null;
                DependencyObject dp = this;
                while ((dp = VisualTreeHelper.GetParent(dp)) != null)
                {
                    var i = dp as SelectorItem;
                    if (i == null)
                        continue;

                    selector = i;
                    break;
                }

                SetBinding(IsSelectedProperty, new Binding
                {
                    Path = new PropertyPath("IsSelected"),
                    Source = selector,
                    Mode = BindingMode.TwoWay
                });
            };
        }

        public EventMessageViewModel ViewModel
        {
            get => (EventMessageViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
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

            var eventMessage = obj as EventMessage;
            var textblock = eventMessage.FindName("EventMessageBodyText") as RichTextBlock;
            textblock.IsTextSelectionEnabled = (bool) e.NewValue && SettingService.Setting.EnableTweetTextSelection;
        }

        #region TargetStatus 関連

        public static bool GetTargetStatusVisibility(DependencyObject obj)
        {
            return (bool) obj.GetValue(GetTargetStatusVisibilityProperty);
        }

        public static void SetTargetStatusVisibility(DependencyObject obj, bool value)
        {
            obj.SetValue(GetTargetStatusVisibilityProperty, value);
        }

        public static readonly DependencyProperty GetTargetStatusVisibilityProperty =
            DependencyProperty.Register("TargetStatusVisibility", typeof(bool), typeof(EventMessage),
                new PropertyMetadata(false, TargetStatus_PropertyChanged));

        private static void TargetStatus_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as EventMessage;
            var border = status.FindName("TargetStatusBorder") as Border;
            border.Visibility = GetTargetStatusVisibility(obj) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static bool GetTargetStatusMediaVisibility(DependencyObject obj)
        {
            return (bool) obj.GetValue(GetTargetStatusMediaVisibilityProperty);
        }

        public static void SetTargetStatusMediaVisibility(DependencyObject obj, bool value)
        {
            obj.SetValue(GetTargetStatusMediaVisibilityProperty, value);
        }

        public static readonly DependencyProperty GetTargetStatusMediaVisibilityProperty =
            DependencyProperty.Register("TargetStatusMediaVisibility", typeof(bool), typeof(EventMessage),
                new PropertyMetadata(false, TargetStatusMediaVisibility_PropertyChanged));

        private static void TargetStatusMediaVisibility_PropertyChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var status = obj as EventMessage;
            var itemsControl = status.FindName("TargetStatusMediaItemsControl") as ItemsControl;

            itemsControl.Visibility = GetTargetStatusMediaVisibility(obj) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region CommandGrid 関連

        public bool CommandGridLoaded;

        private static void CommandGrid_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as EventMessage;
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