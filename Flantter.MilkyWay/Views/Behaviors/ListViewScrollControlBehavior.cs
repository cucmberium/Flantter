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

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            ((ListView)this.AssociatedObject).Loaded += ListView_Loaded;
            ((ListView)this.AssociatedObject).LayoutUpdated += ListView_LayoutUpdated;
            ((ListView)this.AssociatedObject).DataContextChanged += ListView_DataContextChanged;
        }

        public void Detach()
        {
            if (this.ScrollViewerObject != null)
                this.ScrollViewerObject.ViewChanged -= ScrollViewerObject_ViewChanged;

            if (this.AssociatedObject != null)
            {
                ((ListView)this.AssociatedObject).Loaded -= ListView_Loaded;
                ((ListView)this.AssociatedObject).LayoutUpdated -= ListView_LayoutUpdated;
                ((ListView)this.AssociatedObject).DataContextChanged -= ListView_DataContextChanged;
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

        private void ListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var itemsSource = ((ListView)this.AssociatedObject).ItemsSource as INotifyCollectionChanged;
            if (itemsSource != null)
                itemsSource.CollectionChanged += ListView_CollectionChanged;
        }

        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsScrollControlEnabled)
                return;

            if (IsScrollLockToTopEnabled)
            {
                this.ScrollViewerObject.ChangeView(null, 0.01, null, true);
                return;
            }

            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            var collection = sender as INotifyCollectionChanged;

            if (this.AssociatedObject == null || this.ScrollViewerObject == null || collection == null)
                return;

            var oldExtentHeight = this.ScrollViewerObject.ExtentHeight;
            if (oldExtentHeight == 0)
                return;

            var oldMaxVerticalOffset = oldExtentHeight - this.ScrollViewerObject.ViewportHeight;
            var oldVerticalOffset = this.ScrollViewerObject.VerticalOffset;
            if (oldVerticalOffset < 0)
                return;

            try
            {
                ((ListView)this.AssociatedObject).UpdateLayout();
            }
            catch
            {
                return;
            }

            var newExtentHeight = this.ScrollViewerObject.ExtentHeight;
            var changedVerticalOffset = newExtentHeight - oldExtentHeight;
            var newVerticalOffset = this.ScrollViewerObject.VerticalOffset;
            var offset = newVerticalOffset + changedVerticalOffset;

            if (this.ScrollViewerObject.ExtentHeight - this.ScrollViewerObject.ViewportHeight - offset <= 0)
                return;

            if (e.NewStartingIndex >= newVerticalOffset - 1)
                return;

            this.ScrollViewerObject.ChangeView(null, offset, null, true);

            if (SettingService.Setting.DisableStreamingScroll)
                return;

            if (IsScrollLockEnabled)
                return;

            switch (SettingService.Setting.TweetAnimation)
            {
                case SettingSupport.TweetAnimationEnum.ScrollToTop:
                    if (isAnimationRunning && !isAnimationCooldown)
                        this.RunAnimation(offset, changedVerticalOffset);
                    else if (oldVerticalOffset <= 2 && !isAnimationCooldown)
                        this.RunAnimation(offset, changedVerticalOffset + oldVerticalOffset - 2.0);

                    break;
                case SettingSupport.TweetAnimationEnum.Expand:
                case SettingSupport.TweetAnimationEnum.Slide:
                    if (oldVerticalOffset > 2 || isAnimationCooldown)
                        return;

                    var lvItem = ((ListView)this.AssociatedObject).ContainerFromIndex(0) as ListViewItem;
                    if (lvItem == null || lvItem.ContentTemplateRoot == null)
                        return;

                    var storyName = SettingService.Setting.TweetAnimation == SettingSupport.TweetAnimationEnum.Slide ? "TweetSlideAnimation" :
                                    SettingService.Setting.TweetAnimation == SettingSupport.TweetAnimationEnum.Expand ? "TweetExpandAnimation" : "";

                    var story = ((FrameworkElement)lvItem.ContentTemplateRoot).Resources[storyName] as Storyboard;
                    story.Begin();
                    break;
                case SettingSupport.TweetAnimationEnum.None:
                    break;
            }
        }

        public double currentOffset;
        public double remainHeight;
        volatile public bool isAnimationRunning;
        volatile public bool isAnimationCooldown;
        volatile public int tickCount;
        public async void RunAnimation(double offset, double changedOffset)
        {
            if (isAnimationCooldown) return;

            if (remainHeight < 0) remainHeight = 0;
            currentOffset = offset;

            if (isAnimationRunning)
            {
                remainHeight = this.ScrollViewerObject.VerticalOffset;
                tickCount += 10;

                if (tickCount > 70)
                    tickCount = 70;
            }
            else
            {
                tickCount = 20;
                remainHeight = changedOffset;

                isAnimationRunning = true;
                await Task.Run(() => RunAnimationTask()).ConfigureAwait(false);
            }
        }
        public void RunAnimationTask()
        {
            for (; tickCount > 0; tickCount--)
            {
                var dx = remainHeight / tickCount;
                remainHeight -= dx;
                currentOffset -= dx;
                if (isAnimationCooldown)
                {
                    remainHeight = 0.0;
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

                new Task(() => { }).Wait(10);
            }
            isAnimationRunning = false;
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.ScrollViewerObject.ChangeView(null, 0.01, null, true)).AsTask().Wait();
        }

        private void ListView_LayoutUpdated(object sender, object e)
        {
            if (this.ScrollViewerObject != null)
                return;

            var border = VisualTreeHelper.GetChild(((ListView)this.AssociatedObject), 0) as Border;
            if (border == null)
                return;

            var listViewScroll = border.Child as ScrollViewer;
            if (listViewScroll == null)
                return;

            this.ScrollViewerObject = listViewScroll;
            this.ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
            this.ScrollViewerObject.ViewChanging += ScrollViewerObject_ViewChanging;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ScrollViewerObject != null)
                return;

            var border = VisualTreeHelper.GetChild(((ListView)this.AssociatedObject), 0) as Border;
            if (border == null)
                return;

            var listViewScroll = border.Child as ScrollViewer;
            if (listViewScroll == null)
                return;

            this.ScrollViewerObject = listViewScroll;
            this.ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
            this.ScrollViewerObject.ViewChanging += ScrollViewerObject_ViewChanging;
        }

        private void ScrollViewerObject_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            //Debug.WriteLine(e.NextView.VerticalOffset);
            //if (e.IsInertial)
            //    AnimationCooldown(200);
        }

        private void ScrollViewerObject_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e != null && e.IsIntermediate)
                this.AnimationCooldown(200);

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
            if (cooltime > 500)
                cooltime = 500;

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

                cooltime -= 10;
                new Task(() => { }).Wait(10);
            }
            cooltime = 0;
            isAnimationCooldown = false;
        }

        private static void UnreadCountIncrementalTriggerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = obj as ListViewScrollControlBehavior;
            if (behavior == null)
                return;

            if ((bool)e.NewValue == true)
            {
                var verticalOffset = behavior.ScrollViewerObject.VerticalOffset;
                var maxVerticalOffset = behavior.ScrollViewerObject.ExtentHeight - behavior.ScrollViewerObject.ViewportHeight;

                var selectedItemIndex = ((ListView)behavior.AssociatedObject).SelectedIndex;

                var unreadCount = 0;
                if (behavior.isAnimationRunning)
                    unreadCount = 0;
                else if (!behavior.IsScrollControlEnabled)
                    unreadCount = 0;
                else if (selectedItemIndex != -1)
                    unreadCount = behavior.UnreadCount + 1;
                else
                    unreadCount = behavior.UnreadCount > verticalOffset - 2 ? (int)verticalOffset - 2 : behavior.UnreadCount + 1;

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
