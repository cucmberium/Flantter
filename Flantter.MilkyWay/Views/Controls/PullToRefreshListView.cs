using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Controls
{
    public class PullToRefreshListView : ListView
    {
        public PullToRefreshListView()
        {
            this.DefaultStyleKey = typeof(PullToRefreshListView);
            this.Loaded += PullToRefreshScrollViewer_Loaded;
            
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromMilliseconds(100);
            _Timer.Tick += Timer_Tick;

            _RenderTimer = new DispatcherTimer();
            _RenderTimer.Interval = TimeSpan.FromMilliseconds(0);
            _RenderTimer.Tick += RenderTimer_Tick;

            this.SelectionChanged += (s, e) => this.SelectedItemsList = this.SelectedItems;
        }
        
        #region Event
        public event EventHandler RefreshContent;
        public event EventHandler MoreContent;
        #endregion

        #region DependencyProperty
        public static readonly DependencyProperty PullPartTemplateProperty =
            DependencyProperty.Register("PullPartTemplate", typeof(string), typeof(PullToRefreshListView), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ReleasePartTemplateProperty =
            DependencyProperty.Register("ReleasePartTemplate", typeof(string), typeof(PullToRefreshListView), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty RefreshHeaderHeightProperty =
            DependencyProperty.Register("RefreshHeaderHeight", typeof(double), typeof(PullToRefreshListView), new PropertyMetadata(25.0, RefreshHeaderHeight_ChangedCallback));
        private static readonly DependencyProperty arrowColorProperty =
            DependencyProperty.Register("ArrowColor", typeof(Brush), typeof(PullToRefreshListView), new PropertyMetadata(new SolidColorBrush(Colors.Red)));
        private static readonly DependencyProperty IndicatorForegroundBrushProperty =
            DependencyProperty.Register("IndicatorForegroundBrush", typeof(Brush), typeof(PullToRefreshListView), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(IEnumerable), typeof(PullToRefreshListView), new PropertyMetadata(null));
        #endregion
        

        #region Property
        public string PullPartTemplate
        {
            get
            {
                return (string)base.GetValue(PullToRefreshListView.PullPartTemplateProperty);
            }
            set
            {
                base.SetValue(PullToRefreshListView.PullPartTemplateProperty, value);
            }
        }

        public string ReleasePartTemplate
        {
            get
            {
                return (string)base.GetValue(PullToRefreshListView.ReleasePartTemplateProperty);
            }
            set
            {
                base.SetValue(PullToRefreshListView.ReleasePartTemplateProperty, value);
            }
        }

        public double RefreshHeaderHeight
        {
            get
            {
                return (double)base.GetValue(PullToRefreshListView.RefreshHeaderHeightProperty);
            }
            set
            {
                base.SetValue(PullToRefreshListView.RefreshHeaderHeightProperty, value);
            }
        }

        public Brush ArrowColor
        {
            get
            {
                return (Brush)base.GetValue(PullToRefreshListView.ArrowColorProperty);
            }
            set
            {
                base.SetValue(PullToRefreshListView.ArrowColorProperty, value);
            }
        }

        public static DependencyProperty ArrowColorProperty
        {
            get
            {
                return arrowColorProperty;
            }
        }

        public Brush IndicatorForegroundBrush
        {
            get
            {
                return (Brush)base.GetValue(PullToRefreshListView.IndicatorForegroundBrushProperty);
            }
            set
            {
                base.SetValue(PullToRefreshListView.IndicatorForegroundBrushProperty, value);
            }
        }

        
        public IEnumerable SelectedItemsList
        {
            get { return (IEnumerable)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }
        #endregion

        #region Field
        private double _OffsetTreshhold = 25;
        private ScrollViewer _RootScrollViewer;
        private DispatcherTimer _Timer;
        private DispatcherTimer _RenderTimer;
        private Grid _ContainerGrid;
        public Border _PullToRefreshIndicator;
        private ItemsPresenter _ListViewItemsPresenter;
        //private bool _IsCompressionTimerRunning;
        private bool _IsReadyToRefresh;
        //private bool _IsCompressedEnough;
        #endregion

        #region Override Method
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _RootScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            _RootScrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            _RootScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            
            _RootScrollViewer.RenderTransform = new CompositeTransform() { TranslateY = 0 };
            _RootScrollViewer.DirectManipulationStarted += Viewer_DirectManipulationStarted;
            _RootScrollViewer.DirectManipulationCompleted += Viewer_DirectManipulationCompleted;
            _ContainerGrid = GetTemplateChild("ContainerGrid") as Grid;

            _PullToRefreshIndicator = GetTemplateChild("PullToRefreshIndicator") as Border;

            _ListViewItemsPresenter = GetTemplateChild("ItemsPresenter") as ItemsPresenter;

            SizeChanged += OnSizeChanged;
        }
        #endregion


        private static void RefreshHeaderHeight_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pullToRefreshListView = d as PullToRefreshListView;
            pullToRefreshListView._PullToRefreshIndicator.Margin = new Thickness(0, (double)e.NewValue, 0, 0);
        }

        private void PullToRefreshScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            _OffsetTreshhold = this.ActualHeight * 40 / 640;
        }

        private bool _DirectManipulationDelta = false;
        private void Viewer_DirectManipulationStarted(object sender, object e)
        {
            _DirectManipulationDelta = true;
            if (_RootScrollViewer.VerticalOffset <= 2)
            {
                _Timer.Start();
                _RenderTimer.Start();
            }
        }

        private void Viewer_DirectManipulationCompleted(object sender, object e)
        {
            _DirectManipulationDelta = false;
            _Timer.Stop();
            _RenderTimer.Stop();
            if (_IsReadyToRefresh)
            {
                Timer_Tick(null, null);
            }
            //_IsCompressionTimerRunning = false;
            //_IsCompressedEnough = false;
            _IsReadyToRefresh = false;
            VisualStateManager.GoToState(this, "Normal", true);
            ((CompositeTransform)(_PullToRefreshIndicator.RenderTransform)).TranslateY = 0;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _OffsetTreshhold = e.NewSize.Height * 40 / 570;

            Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset <= 2)
            {
                if (!e.IsInertial && _Timer != null && _RenderTimer != null && _DirectManipulationDelta)
                {
                    _Timer.Start();
                    _RenderTimer.Start();
                }
                else
                {
                    _Timer.Stop();
                    _RenderTimer.Stop();
                }
            }
            else
            {
                _Timer.Stop();
                _RenderTimer.Stop();

                //_IsCompressionTimerRunning = false;
                //_IsCompressedEnough = false;
                _IsReadyToRefresh = false;

                VisualStateManager.GoToState(this, "Normal", true);
                ((CompositeTransform)(_PullToRefreshIndicator.RenderTransform)).TranslateY = 0;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_RootScrollViewer.VerticalOffset == _RootScrollViewer.ScrollableHeight && _RootScrollViewer.ViewportHeight != 0)
            {
                InvokeMore();
            }
        }

        private Rect emptyRect = new Rect(0.0, 0.0, 0.0, 0.0);
        private void RenderTimer_Tick(object sender, object e)
        {
            var elementBounds = _ListViewItemsPresenter.TransformToVisual(_ContainerGrid).TransformBounds(emptyRect);
            ((CompositeTransform)(_PullToRefreshIndicator.RenderTransform)).TranslateY = elementBounds.Bottom;
        }

        private void Timer_Tick(object sender, object e)
        {
            var elementBounds = _ListViewItemsPresenter.TransformToVisual(_ContainerGrid).TransformBounds(emptyRect);
            var compressionOffset = elementBounds.Bottom;

            if (compressionOffset > _OffsetTreshhold)
            {
                VisualStateManager.GoToState(this, "ReadyToRefresh", true);
                _IsReadyToRefresh = true;
            }
            else if (compressionOffset == 0 && _IsReadyToRefresh == true)
            {
                InvokeRefresh();
            }
            else
            {
                //_IsCompressedEnough = false;
                //_IsCompressionTimerRunning = false;
            }
        }

        #region Misc
        private void InvokeRefresh()
        {
            _IsReadyToRefresh = false;
            VisualStateManager.GoToState(this, "Normal", true);

            RefreshContent?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeMore()
        {
            MoreContent?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Helper

        T FindChild<T>(DependencyObject d)
            where T : DependencyObject
        {
            var q = new Queue<DependencyObject>();
            q.Enqueue(d);
            while (q.Count > 0)
            {
                var e = q.Dequeue();
                if (e is T) return (T)e;
                var n = VisualTreeHelper.GetChildrenCount(e);
                for (var i = 0; i < n; i++)
                {
                    var c = VisualTreeHelper.GetChild(e, i);
                    if (c is T) return (T)c;
                    q.Enqueue(c);
                }
            }
            return null;
        }
        #endregion
    }
}
