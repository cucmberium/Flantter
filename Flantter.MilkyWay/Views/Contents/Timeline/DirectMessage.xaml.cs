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
    public sealed partial class DirectMessage : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(DirectMessageViewModel), typeof(DirectMessage), null);

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(DirectMessage),
                new PropertyMetadata(false, IsSelectedPropertyChanged));

        public DirectMessage()
        {
            InitializeComponent();
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

        public DirectMessageViewModel ViewModel
        {
            get => (DirectMessageViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
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

            var directMessage = obj as DirectMessage;
            var textblock = directMessage.FindName("DirectMessageBodyText") as TextBlock;
            textblock.IsTextSelectionEnabled = (bool) e.NewValue && SettingService.Setting.EnableTweetTextSelection;
        }

        #region Media 関連

        public static bool GetMediaVisibility(DependencyObject obj)
        {
            return (bool) obj.GetValue(MediaVisibilityProperty);
        }

        public static void SetMediaVisibility(DependencyObject obj, bool value)
        {
            obj.SetValue(MediaVisibilityProperty, value);
        }

        public static readonly DependencyProperty MediaVisibilityProperty =
            DependencyProperty.Register("MediaVisibility", typeof(bool), typeof(DirectMessage),
                new PropertyMetadata(false, MediaVisibility_PropertyChanged));

        private static void MediaVisibility_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as DirectMessage;
            var itemsControl = status.FindName("MediaItemsControl") as ItemsControl;

            itemsControl.Visibility = GetMediaVisibility(obj) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region CommandGrid 関連

        public bool CommandGridLoaded;

        private static void CommandGrid_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as DirectMessage;
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