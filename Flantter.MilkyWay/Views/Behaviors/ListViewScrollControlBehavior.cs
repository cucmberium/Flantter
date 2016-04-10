using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ListViewScrollControlBehavior : DependencyObject, IBehavior
    {
        private DependencyObject _AssociatedObject;
        public DependencyObject AssociatedObject
        {
            get { return this._AssociatedObject; }
            set { this._AssociatedObject = value; }
        }

        private PointerEventHandler _PointerWheelChangedEventHandler = null;
        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            ((ListView)this.AssociatedObject).Loaded += ListView_Loaded;
            ((ListView)this.AssociatedObject).LayoutUpdated += ListView_LayoutUpdated;
            ((ListView)this.AssociatedObject).DataContextChanged += ListView_DataContextChanged;
            ((ListView)this.AssociatedObject).PointerWheelChanged += ListView_PointerWheelChanged;

            this._PointerWheelChangedEventHandler = new PointerEventHandler(ListView_PointerWheelChanged);
            ((ListView)this.AssociatedObject).AddHandler(ListView.PointerWheelChangedEvent, _PointerWheelChangedEventHandler, true);
        }

        public void Detach()
        {
            if (this.ScrollViewerObject != null)
            {
                this.ScrollViewerObject.ViewChanged -= ScrollViewerObject_ViewChanged;
                this.ScrollViewerObject = null;
            }

            if (this.AssociatedObject != null)
            {
                ((ListView)this.AssociatedObject).Loaded -= ListView_Loaded;
                ((ListView)this.AssociatedObject).LayoutUpdated -= ListView_LayoutUpdated;
                ((ListView)this.AssociatedObject).DataContextChanged -= ListView_DataContextChanged;
                ((ListView)this.AssociatedObject).PointerWheelChanged -= ListView_PointerWheelChanged;

                ((ListView)this.AssociatedObject).RemoveHandler(ListView.PointerWheelChangedEvent, _PointerWheelChangedEventHandler);
            }
        }

        private ScrollViewer _ScrollViewerObject;
        public ScrollViewer ScrollViewerObject
        {
            get { return this._ScrollViewerObject; }
            set { this._ScrollViewerObject = value; }
        }

        public bool IsScrollControlEnabled
        {
            get { return (bool)this.GetValue(IsScrollControlEnabledProperty); }
            set { this.SetValue(IsScrollControlEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsScrollControlEnabledProperty =
                        DependencyProperty.RegisterAttached("IsScrollControlEnabled", typeof(bool),
                        typeof(ListViewScrollControlBehavior), new PropertyMetadata(true));

        public bool IsScrollLockEnabled
        {
            get { return (bool)this.GetValue(IsScrollLockEnabledProperty); }
            set { this.SetValue(IsScrollLockEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsScrollLockEnabledProperty =
                        DependencyProperty.RegisterAttached("IsScrollLockEnabled", typeof(bool),
                        typeof(ListViewScrollControlBehavior), new PropertyMetadata(false));

        public bool IsScrollLockToTopEnabled
        {
            get { return (bool)this.GetValue(IsScrollLockToTopEnabledProperty); }
            set { this.SetValue(IsScrollLockToTopEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsScrollLockToTopEnabledProperty =
                        DependencyProperty.RegisterAttached("IsScrollLockToTopEnabled", typeof(bool),
                        typeof(ListViewScrollControlBehavior), new PropertyMetadata(false, IsScrollLockToTopEnabledChanged));

        public int UnreadCount
        {
            get { return (int)this.GetValue(UnreadCountProperty); }
            set { this.SetValue(UnreadCountProperty, value); }
        }
        public static readonly DependencyProperty UnreadCountProperty =
                        DependencyProperty.RegisterAttached("UnreadCount", typeof(int),
                        typeof(ListViewScrollControlBehavior), new PropertyMetadata(0));

        public bool UnreadCountIncrementalTrigger
        {
            get { return (bool)this.GetValue(UnreadCountIncrementalTriggerProperty); }
            set { this.SetValue(UnreadCountIncrementalTriggerProperty, value); }
        }
        public static readonly DependencyProperty UnreadCountIncrementalTriggerProperty =
                        DependencyProperty.RegisterAttached("UnreadCountIncrementalTrigger", typeof(bool),
                        typeof(ListViewScrollControlBehavior), new PropertyMetadata(false, UnreadCountIncrementalTriggerChanged));

        private INotifyCollectionChanged previousItemsSource = null;
        private void ListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            if (previousItemsSource != null)
            {
                previousItemsSource.CollectionChanged -= ListView_CollectionChanged;
                previousItemsSource = null;
            }

            var itemsSource = ((ListView)this.AssociatedObject).ItemsSource as INotifyCollectionChanged;
            if (itemsSource != null)
            {
                itemsSource.CollectionChanged += ListView_CollectionChanged;
                previousItemsSource = itemsSource;
            }
        }

        private const int ChangedVertialOffset = 1;
        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!this.IsScrollControlEnabled)
                return;

            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            if (this.IsScrollLockToTopEnabled)
            {
                this.ScrollViewerObject.ChangeView(null, 0.02, null, true);
                return;
            }

            var collection = sender as INotifyCollectionChanged;

            if (collection == null)
                return;

            var verticalOffset = previousVerticalOffset;
            var offset = verticalOffset + ChangedVertialOffset;

            if (this.ScrollViewerObject.ScrollableHeight + 1 - offset <= 0)
                return;

            previousVerticalOffset = offset;

            if (e.NewStartingIndex >= verticalOffset - 1)
                return;

            if (SettingService.Setting.DisableStreamingScroll || this.IsScrollLockEnabled)
            {
                this.ScrollViewerObject.ChangeView(null, offset, null, true);
                return;
            }

            switch (SettingService.Setting.TweetAnimation)
            {
                case SettingSupport.TweetAnimationEnum.ScrollToTop:
                    if (isAnimationRunning && !isAnimationCooldown)
                        this.RunAnimation(offset);
                    else if (verticalOffset <= 2.5 && !isAnimationCooldown)
                        this.RunAnimation(offset);
                    else
                        this.ScrollViewerObject.ChangeView(null, offset, null, true);

                    break;
                case SettingSupport.TweetAnimationEnum.Expand:
                case SettingSupport.TweetAnimationEnum.Slide:
                    if (verticalOffset > 2.5 || isAnimationCooldown)
                    {
                        this.ScrollViewerObject.ChangeView(null, offset, null, true);
                        return;
                    }

                    var lvItem = ((ListView)this.AssociatedObject).ContainerFromIndex(0) as ListViewItem;
                    if (lvItem == null || lvItem.ContentTemplateRoot == null)
                        return;

                    var storyName = SettingService.Setting.TweetAnimation == SettingSupport.TweetAnimationEnum.Slide ? "TweetSlideAnimation" :
                                    SettingService.Setting.TweetAnimation == SettingSupport.TweetAnimationEnum.Expand ? "TweetExpandAnimation" : "";

                    var story = ((FrameworkElement)lvItem.ContentTemplateRoot).Resources[storyName] as Storyboard;
                    story.Begin();

                    this.ScrollViewerObject.ChangeView(null, 0.02, null, true);
                    previousVerticalOffset = 0;

                    break;
                case SettingSupport.TweetAnimationEnum.None:
                    if (verticalOffset > 2.5)
                    {
                        this.ScrollViewerObject.ChangeView(null, offset, null, true);
                    }
                    else
                    {
                        this.ScrollViewerObject.ChangeView(null, 0.02, null, true);
                        previousVerticalOffset = 0;
                    }

                    break;
            }
        }

        public double currentOffset;
        volatile private bool isAnimationRunning;
        volatile private bool isAnimationCooldown;
        volatile private int tickCount;
        public async void RunAnimation(double offset)
        {
            if (isAnimationCooldown)
                return;

            currentOffset = offset;
            this.ScrollViewerObject.ChangeView(null, offset, null, true);

            if (isAnimationRunning)
            {
                tickCount += 16;

                if (tickCount > 72)
                    tickCount = 72;
            }
            else
            {
                tickCount = 26;

                isAnimationRunning = true;
                await Task.Run(() => RunAnimationTask()).ConfigureAwait(false);
            }
        }
        public void RunAnimationTask()
        {
            for (; tickCount > 0; tickCount--)
            {
                var dx = currentOffset / tickCount;
                currentOffset -= dx;
                if (isAnimationCooldown)
                {
                    isAnimationRunning = false;
                    return;
                }
                else
                {
                    this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.ScrollViewerObject.ChangeView(null, currentOffset, null, true);
                    }).AsTask().Wait();
                }

                Task.Delay(12).Wait();
            }
            isAnimationRunning = false;
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.ScrollViewerObject.ChangeView(null, 0.02, null, true)).AsTask().Wait();
        }

        private void ListView_LayoutUpdated(object sender, object e)
        {
            if (this.ScrollViewerObject != null)
                return;

            var grid = VisualTreeHelper.GetChild(((ListView)this.AssociatedObject), 0) as Grid;
            if (grid == null)
                return;

            var listViewScroll = VisualTreeHelper.GetChild(grid, 0) as ScrollViewer;
            if (listViewScroll == null)
                return;

            this.ScrollViewerObject = listViewScroll;
            this.ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
            previousVerticalOffset = this.ScrollViewerObject.VerticalOffset;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ScrollViewerObject != null)
                return;

            var grid = VisualTreeHelper.GetChild(((ListView)this.AssociatedObject), 0) as Grid;
            if (grid == null)
                return;

            var listViewScroll = VisualTreeHelper.GetChild(grid, 0) as ScrollViewer;
            if (listViewScroll == null)
                return;

            this.ScrollViewerObject = listViewScroll;
            this.ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
            previousVerticalOffset = this.ScrollViewerObject.VerticalOffset;
        }
        
        private void ListView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint((ListView)this.AssociatedObject).Properties.MouseWheelDelta < 0 && this.isAnimationRunning)
                this.AnimationCooldown(200);
        }

        private double previousVerticalOffset = 0.0;
        private void ScrollViewerObject_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e != null && e.IsIntermediate && this.isAnimationRunning)
                this.AnimationCooldown(200);
            
            previousVerticalOffset = this.ScrollViewerObject.VerticalOffset;
            currentOffset = previousVerticalOffset;

            var verticalOffset = this.ScrollViewerObject.VerticalOffset;
            var maxVerticalOffset = this.ScrollViewerObject.ExtentHeight - this.ScrollViewerObject.ViewportHeight;
            if (verticalOffset != maxVerticalOffset)
            {
                var unreadCount = this.UnreadCount > verticalOffset - 2 ? (int)verticalOffset - 2 : this.UnreadCount;
                this.UnreadCount = unreadCount >= 0 ? unreadCount : 0;
            }
        }

        public volatile int cooltime;
        private async void AnimationCooldown(int time)
        {
            cooltime += time;
            if (cooltime > 400)
                cooltime = 400;

            if (isAnimationCooldown)
                return;

            isAnimationCooldown = true;

            await Task.Run(() => AnimationCooldownTask()).ConfigureAwait(false);
        }

        private void AnimationCooldownTask()
        {
            while (true)
            {
                if (!isAnimationCooldown)
                    break;

                if (cooltime < 0)
                    break;

                cooltime -= 20;
                Task.Delay(20).Wait();
            }
            cooltime = 0;
            isAnimationCooldown = false;
        }

        private static void UnreadCountIncrementalTriggerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = obj as ListViewScrollControlBehavior;
            if (behavior == null)
                return;

            if (behavior.ScrollViewerObject == null)
                return;

            if ((bool)e.NewValue == true)
            {
                var verticalOffset = behavior.ScrollViewerObject.VerticalOffset;
                var maxVerticalOffset = behavior.ScrollViewerObject.ExtentHeight - behavior.ScrollViewerObject.ViewportHeight;

                var selectedItemIndex = ((ListView)behavior.AssociatedObject).SelectedIndex;

                var unreadCount = 0;
                if (behavior.ScrollViewerObject.ScrollableHeight <= 2)
                    unreadCount = 0;
                else if (behavior.isAnimationRunning)
                    unreadCount = 0;
                else if (!behavior.IsScrollControlEnabled)
                    unreadCount = 0;
                else if (selectedItemIndex != -1)
                    unreadCount = behavior.UnreadCount + 1;
                else
                    unreadCount = behavior.UnreadCount > verticalOffset - 2.5 ? (int)verticalOffset - 2 : behavior.UnreadCount + 1;

                behavior.UnreadCount = unreadCount >= 0 ? unreadCount : 0;

                behavior.UnreadCountIncrementalTrigger = false;
            }
        }

        private static void IsScrollLockToTopEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = obj as ListViewScrollControlBehavior;
            if (behavior == null)
                return;

            if ((bool)e.NewValue)
            {
                behavior.UnreadCount = 0;
                behavior.ScrollViewerObject.ChangeView(null, 0.01, null, true);
            }
        }
    }
}
