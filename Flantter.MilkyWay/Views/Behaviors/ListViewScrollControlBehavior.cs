using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Flantter.MilkyWay.Setting;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ListViewScrollControlBehavior : DependencyObject, IBehavior
    {
        private const int ChangedVertialOffset = 1;

        public static readonly DependencyProperty IsScrollControlEnabledProperty =
            DependencyProperty.RegisterAttached("IsScrollControlEnabled", typeof(bool),
                typeof(ListViewScrollControlBehavior), new PropertyMetadata(true));

        public static readonly DependencyProperty IsScrollLockEnabledProperty =
            DependencyProperty.RegisterAttached("IsScrollLockEnabled", typeof(bool),
                typeof(ListViewScrollControlBehavior), new PropertyMetadata(false));

        public static readonly DependencyProperty IsScrollLockToTopEnabledProperty =
            DependencyProperty.RegisterAttached("IsScrollLockToTopEnabled", typeof(bool),
                typeof(ListViewScrollControlBehavior), new PropertyMetadata(false, IsScrollLockToTopEnabledChanged));

        public static readonly DependencyProperty UnreadCountProperty =
            DependencyProperty.RegisterAttached("UnreadCount", typeof(int),
                typeof(ListViewScrollControlBehavior), new PropertyMetadata(0));

        private PointerEventHandler _pointerWheelChangedEventHandler;

        public volatile int Cooltime;

        public double CurrentOffset;
        private volatile bool _isAnimationCooldown;
        private volatile bool _isAnimationRunning;

        private INotifyCollectionChanged _previousItemsSource;

        private double _previousVerticalOffset = 2.0;
        private volatile int _tickCount;

        public ScrollViewer ScrollViewerObject { get; set; }

        public bool IsScrollControlEnabled
        {
            get => (bool) GetValue(IsScrollControlEnabledProperty);
            set => SetValue(IsScrollControlEnabledProperty, value);
        }

        public bool IsScrollLockEnabled
        {
            get => (bool) GetValue(IsScrollLockEnabledProperty);
            set => SetValue(IsScrollLockEnabledProperty, value);
        }

        public bool IsScrollLockToTopEnabled
        {
            get => (bool) GetValue(IsScrollLockToTopEnabledProperty);
            set => SetValue(IsScrollLockToTopEnabledProperty, value);
        }

        public int UnreadCount
        {
            get => (int) GetValue(UnreadCountProperty);
            set => SetValue(UnreadCountProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            ((ListView) this.AssociatedObject).Loaded += ListView_Loaded;
            ((ListView) this.AssociatedObject).LayoutUpdated += ListView_LayoutUpdated;
            ((ListView) this.AssociatedObject).DataContextChanged += ListView_DataContextChanged;

            _pointerWheelChangedEventHandler = ListView_PointerWheelChanged;
            ((ListView) this.AssociatedObject).AddHandler(UIElement.PointerWheelChangedEvent,
                _pointerWheelChangedEventHandler, true);
        }

        public void Detach()
        {
            if (ScrollViewerObject != null)
            {
                ScrollViewerObject.ViewChanged -= ScrollViewerObject_ViewChanged;
                ScrollViewerObject = null;
            }

            if (AssociatedObject != null)
            {
                ((ListView) AssociatedObject).Loaded -= ListView_Loaded;
                ((ListView) AssociatedObject).LayoutUpdated -= ListView_LayoutUpdated;
                ((ListView) AssociatedObject).DataContextChanged -= ListView_DataContextChanged;

                ((ListView) AssociatedObject).RemoveHandler(UIElement.PointerWheelChangedEvent,
                    _pointerWheelChangedEventHandler);
            }
        }

        private void ListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            if (_previousItemsSource != null)
            {
                _previousItemsSource.CollectionChanged -= ListView_CollectionChanged;
                _previousItemsSource = null;
            }

            var itemsSource = ((ListView) AssociatedObject).ItemsSource as INotifyCollectionChanged;
            if (itemsSource != null)
            {
                itemsSource.CollectionChanged += ListView_CollectionChanged;
                _previousItemsSource = itemsSource;
            }
        }

        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsScrollControlEnabled)
                return;

            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            if (IsScrollLockToTopEnabled)
            {
                ScrollViewerObject.ChangeView(null, 0.02, null, true);
                return;
            }

            var collection = sender as INotifyCollectionChanged;

            if (collection == null)
                return;

            var verticalOffset = _previousVerticalOffset;
            var offset = verticalOffset + ChangedVertialOffset;

            if (e.NewStartingIndex >= verticalOffset - 0.5)
                return;

            if (ScrollViewerObject.ScrollableHeight + 1 - offset <= 0)
            {
                try
                {
                    ScrollViewerObject.UpdateLayout();
                }
                catch
                {
                }

                if (ScrollViewerObject.ScrollableHeight + 1 - offset <= 0)
                    return;
            }

            _previousVerticalOffset = offset;

            if (SettingService.Setting.DisableStreamingScroll || IsScrollLockEnabled)
            {
                ScrollViewerObject.ChangeView(null, offset, null, true);
                IncrementUnreadCount(offset);
                return;
            }

            switch (SettingService.Setting.TweetAnimation)
            {
                case SettingSupport.TweetAnimationEnum.ScrollToTop:
                    if (_isAnimationRunning && !_isAnimationCooldown)
                    {
                        RunAnimation(offset);
                    }
                    else if (verticalOffset <= 2.5 && !_isAnimationCooldown)
                    {
                        RunAnimation(offset);
                    }
                    else
                    {
                        ScrollViewerObject.ChangeView(null, offset, null, true);
                        IncrementUnreadCount(offset);
                    }

                    break;
                case SettingSupport.TweetAnimationEnum.Expand:
                case SettingSupport.TweetAnimationEnum.Slide:
                    if (verticalOffset > 2.5 || _isAnimationCooldown)
                    {
                        ScrollViewerObject.ChangeView(null, offset, null, true);
                        IncrementUnreadCount(offset);
                        return;
                    }

                    var lvItem = ((ListView) AssociatedObject).ContainerFromIndex(0) as ListViewItem;
                    if (lvItem == null || lvItem.ContentTemplateRoot == null)
                        return;

                    var storyName = SettingService.Setting.TweetAnimation == SettingSupport.TweetAnimationEnum.Slide
                        ? "TweetSlideAnimation"
                        : SettingService.Setting.TweetAnimation == SettingSupport.TweetAnimationEnum.Expand
                            ? "TweetExpandAnimation"
                            : "";

                    var story = ((FrameworkElement) lvItem.ContentTemplateRoot).Resources[storyName] as Storyboard;
                    story.Begin();

                    ScrollViewerObject.ChangeView(null, 0.02, null, true);
                    _previousVerticalOffset = 2;

                    break;
                case SettingSupport.TweetAnimationEnum.None:
                    if (verticalOffset > 2.5)
                    {
                        ScrollViewerObject.ChangeView(null, offset, null, true);
                        IncrementUnreadCount(offset);
                    }
                    else
                    {
                        ScrollViewerObject.ChangeView(null, 0.02, null, true);
                        _previousVerticalOffset = 2;
                    }

                    break;
            }
        }

        public async void RunAnimation(double offset)
        {
            if (_isAnimationCooldown)
                return;

            CurrentOffset = offset;
            ScrollViewerObject.ChangeView(null, offset, null, true);

            if (_isAnimationRunning)
            {
                _tickCount += 16;

                if (_tickCount > 72)
                    _tickCount = 72;
            }
            else
            {
                _tickCount = 27;

                _isAnimationRunning = true;
                await Task.Run(() => RunAnimationTask()).ConfigureAwait(false);
            }
        }

        public void RunAnimationTask()
        {
            for (; _tickCount > 0; _tickCount--)
            {
                var dx = CurrentOffset / _tickCount;
                CurrentOffset -= dx;
                if (_isAnimationCooldown)
                {
                    _isAnimationRunning = false;
                    return;
                }
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => { ScrollViewerObject.ChangeView(null, CurrentOffset, null, true); })
                    .AsTask()
                    .Wait();

                Task.Delay(12).Wait();
            }
            _isAnimationRunning = false;
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => ScrollViewerObject.ChangeView(null, 0.02, null, true))
                .AsTask()
                .Wait();
        }

        private void ListView_LayoutUpdated(object sender, object e)
        {
            if (ScrollViewerObject != null)
                return;

            var grid = VisualTreeHelper.GetChild((ListView) AssociatedObject, 0) as Grid;
            if (grid == null)
                return;

            var listViewScroll = VisualTreeHelper.GetChild(grid, 0) as ScrollViewer;
            if (listViewScroll == null)
                return;

            ScrollViewerObject = listViewScroll;
            ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
            _previousVerticalOffset = ScrollViewerObject.VerticalOffset <= 2 ? 2 : ScrollViewerObject.VerticalOffset;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ScrollViewerObject != null)
                return;

            var grid = VisualTreeHelper.GetChild((ListView) AssociatedObject, 0) as Grid;
            if (grid == null)
                return;

            var listViewScroll = VisualTreeHelper.GetChild(grid, 0) as ScrollViewer;
            if (listViewScroll == null)
                return;

            ScrollViewerObject = listViewScroll;
            ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
            _previousVerticalOffset = ScrollViewerObject.VerticalOffset <= 2 ? 2 : ScrollViewerObject.VerticalOffset;
        }

        private void ListView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint((ListView) AssociatedObject).Properties.MouseWheelDelta < 0 && _isAnimationRunning &&
                ScrollViewerObject?.ScrollableHeight > 0.0)
                AnimationCooldown(200);
        }

        private void ScrollViewerObject_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e != null && e.IsIntermediate && _isAnimationRunning)
                AnimationCooldown(200);

            _previousVerticalOffset = ScrollViewerObject.VerticalOffset;
            CurrentOffset = _previousVerticalOffset;

            var verticalOffset = ScrollViewerObject.VerticalOffset;
            var maxVerticalOffset = ScrollViewerObject.ExtentHeight - ScrollViewerObject.ViewportHeight;
            if (verticalOffset != maxVerticalOffset)
            {
                var unreadCount = UnreadCount > verticalOffset - 2 ? (int) verticalOffset - 2 : UnreadCount;
                UnreadCount = unreadCount >= 0 ? unreadCount : 0;
            }
        }

        private async void AnimationCooldown(int time)
        {
            Cooltime += time;
            if (Cooltime > 400)
                Cooltime = 400;

            if (_isAnimationCooldown)
                return;

            _isAnimationCooldown = true;

            await Task.Run(() => AnimationCooldownTask()).ConfigureAwait(false);
        }

        private void AnimationCooldownTask()
        {
            while (true)
            {
                if (!_isAnimationCooldown)
                    break;

                if (Cooltime < 0)
                    break;

                Cooltime -= 20;
                Task.Delay(20).Wait();
            }
            Cooltime = 0;
            _isAnimationCooldown = false;
        }

        private void IncrementUnreadCount(double offset)
        {
            if (ScrollViewerObject == null)
                return;

            int unreadCount;
            if (ScrollViewerObject.ScrollableHeight <= 2)
                unreadCount = 0;
            else if (_isAnimationRunning)
                unreadCount = 0;
            else if (!IsScrollControlEnabled)
                unreadCount = 0;
            else
                unreadCount = UnreadCount > offset - 2 ? (int) offset - 2 : UnreadCount + 1;

            UnreadCount = unreadCount >= 0 ? unreadCount : 0;
        }

        private static void IsScrollLockToTopEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = obj as ListViewScrollControlBehavior;
            if (behavior == null)
                return;

            if ((bool) e.NewValue)
            {
                behavior.UnreadCount = 0;
                behavior.ScrollViewerObject.ChangeView(null, 0.01, null, true);
            }
        }
    }
}