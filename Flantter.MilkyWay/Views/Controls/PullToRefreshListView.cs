using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Controls
{
    public sealed class PullToRefreshListView : ListView
    {
        public PullToRefreshListView()
        {
            this.DefaultStyleKey = typeof(PullToRefreshListView);
            this.Loaded += PullToRefreshScrollViewer_Loaded;
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
            DependencyProperty.Register("RefreshHeaderHeight", typeof(double), typeof(PullToRefreshListView), new PropertyMetadata(30.0, RefreshHeaderHeight_ChangedCallback));
        private static readonly DependencyProperty arrowColorProperty =
            DependencyProperty.Register("ArrowColor", typeof(Brush), typeof(PullToRefreshListView), new PropertyMetadata(new SolidColorBrush(Colors.Red)));
        private static readonly DependencyProperty IndicatorForegroundBrushProperty =
            DependencyProperty.Register("IndicatorForegroundBrush", typeof(Brush), typeof(PullToRefreshListView), new PropertyMetadata(new SolidColorBrush(Colors.White)));
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
        #endregion

        #region Field
        private double _OffsetTreshhold = 40;
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
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromMilliseconds(100);
            _Timer.Tick += Timer_Tick;

            _RenderTimer = new DispatcherTimer();
            _RenderTimer.Interval = TimeSpan.FromMilliseconds(1); // fps?
            _RenderTimer.Tick += RenderTimer_Tick;
        }

        private void Viewer_DirectManipulationStarted(object sender, object e)
        {
            _Timer.Start();
            _RenderTimer.Start();
        }

        private void Viewer_DirectManipulationCompleted(object sender, object e)
        {
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
            Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset == 0)
            {
                if (!e.IsInertial)
                {
                    _Timer.Start();
                    _RenderTimer.Start();
                }
            }
            else
            {
                if (_Timer != null)
                    _Timer.Stop();

                if (_RenderTimer != null)
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

        private void RenderTimer_Tick(object sender, object e)
        {
            var elementBounds = _ListViewItemsPresenter.TransformToVisual(_ContainerGrid).TransformBounds(new Rect(0.0, 0.0, 0.0, 0.0));
            ((CompositeTransform)(_PullToRefreshIndicator.RenderTransform)).TranslateY = elementBounds.Bottom;
        }

        private void Timer_Tick(object sender, object e)
        {
            var elementBounds = _ListViewItemsPresenter.TransformToVisual(_ContainerGrid).TransformBounds(new Rect(0.0, 0.0, 0.0, 0.0));
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

            if (RefreshContent != null)
            {
                RefreshContent(this, EventArgs.Empty);
            }
        }

        private void InvokeMore()
        {
            if (MoreContent != null)
            {
                MoreContent(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}
