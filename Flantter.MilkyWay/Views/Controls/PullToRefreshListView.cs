using System;
using System.Collections;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Controls
{
    public class PullToRefreshListView : ListView
    {
        private readonly Rect _emptyRect = new Rect(0.0, 0.0, 0.0, 0.0);
        private bool _directManipulationDelta;

        public PullToRefreshListView()
        {
            DefaultStyleKey = typeof(PullToRefreshListView);
            Loaded += PullToRefreshScrollViewer_Loaded;
            SizeChanged += PullToRefreshScrollViewer_SizeChanged;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += Timer_Tick;

            _renderTimer = new DispatcherTimer();
            _renderTimer.Interval = TimeSpan.FromMilliseconds(0);
            _renderTimer.Tick += RenderTimer_Tick;

            SelectionChanged += (s, e) => SelectedItemsList = SelectedItems;
        }

        #region Override Method

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rootScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            _rootScrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            _rootScrollViewer.ViewChanged += ScrollViewer_ViewChanged;

            _rootScrollViewer.RenderTransform = new CompositeTransform {TranslateY = 0};
            _rootScrollViewer.DirectManipulationStarted += Viewer_DirectManipulationStarted;
            _rootScrollViewer.DirectManipulationCompleted += Viewer_DirectManipulationCompleted;
            _containerGrid = GetTemplateChild("ContainerGrid") as Grid;

            PullToRefreshIndicator = GetTemplateChild("PullToRefreshIndicator") as Border;

            _listViewItemsPresenter = GetTemplateChild("ItemsPresenter") as ItemsPresenter;

            SizeChanged += OnSizeChanged;
        }

        #endregion


        private static void RefreshHeaderHeight_ChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var pullToRefreshListView = d as PullToRefreshListView;
            pullToRefreshListView.PullToRefreshIndicator.Margin = new Thickness(0, (double) e.NewValue, 0, 0);
        }

        private void PullToRefreshScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            _offsetTreshhold = ActualHeight * 40 / 640;
        }

        private void PullToRefreshScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _offsetTreshhold = ActualHeight * 40 / 640;
        }

        private void Viewer_DirectManipulationStarted(object sender, object e)
        {
            _directManipulationDelta = true;
            if (_rootScrollViewer.VerticalOffset <= 2)
            {
                _timer.Start();
                _renderTimer.Start();
            }
        }

        private void Viewer_DirectManipulationCompleted(object sender, object e)
        {
            _directManipulationDelta = false;
            _timer.Stop();
            _renderTimer.Stop();
            if (_isReadyToRefresh)
                Timer_Tick(null, null);
            //_IsCompressionTimerRunning = false;
            //_IsCompressedEnough = false;
            _isReadyToRefresh = false;
            VisualStateManager.GoToState(this, "Normal", true);
            ((CompositeTransform) PullToRefreshIndicator.RenderTransform).TranslateY = 0;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _offsetTreshhold = e.NewSize.Height * 40 / 570;

            Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (_timer == null || _renderTimer == null)
                return;

            if (e.NextView.VerticalOffset <= 2)
            {
                if (!e.IsInertial && _directManipulationDelta)
                {
                    _timer.Start();
                    _renderTimer.Start();
                }
                else
                {
                    _timer.Stop();
                    _renderTimer.Stop();
                }
            }
            else
            {
                _timer.Stop();
                _renderTimer.Stop();

                _isReadyToRefresh = false;

                VisualStateManager.GoToState(this, "Normal", true);
                ((CompositeTransform) PullToRefreshIndicator.RenderTransform).TranslateY = 0;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_rootScrollViewer.VerticalOffset == _rootScrollViewer.ScrollableHeight &&
                _rootScrollViewer.ViewportHeight != 0)
                InvokeMore();
        }

        private void RenderTimer_Tick(object sender, object e)
        {
            var elementBounds = _listViewItemsPresenter.TransformToVisual(_containerGrid).TransformBounds(_emptyRect);
            ((CompositeTransform) PullToRefreshIndicator.RenderTransform).TranslateY = elementBounds.Bottom;
        }

        private void Timer_Tick(object sender, object e)
        {
            var elementBounds = _listViewItemsPresenter.TransformToVisual(_containerGrid).TransformBounds(_emptyRect);
            var compressionOffset = elementBounds.Bottom;

            if (compressionOffset > _offsetTreshhold)
            {
                VisualStateManager.GoToState(this, "ReadyToRefresh", true);
                _isReadyToRefresh = true;
            }
            else if (compressionOffset < 0.5 && _isReadyToRefresh)
            {
                InvokeRefresh();
            }
        }

        #region Event

        public event EventHandler RefreshContent;
        public event EventHandler MoreContent;

        #endregion

        #region DependencyProperty

        public static readonly DependencyProperty PullPartTemplateProperty =
            DependencyProperty.Register("PullPartTemplate", typeof(string), typeof(PullToRefreshListView),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ReleasePartTemplateProperty =
            DependencyProperty.Register("ReleasePartTemplate", typeof(string), typeof(PullToRefreshListView),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty RefreshHeaderHeightProperty =
            DependencyProperty.Register("RefreshHeaderHeight", typeof(double), typeof(PullToRefreshListView),
                new PropertyMetadata(25.0, RefreshHeaderHeight_ChangedCallback));

        private static readonly DependencyProperty IndicatorForegroundBrushProperty =
            DependencyProperty.Register("IndicatorForegroundBrush", typeof(Brush), typeof(PullToRefreshListView),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(IEnumerable), typeof(PullToRefreshListView),
                new PropertyMetadata(null));

        #endregion


        #region Property

        public string PullPartTemplate
        {
            get => (string) GetValue(PullPartTemplateProperty);
            set => SetValue(PullPartTemplateProperty, value);
        }

        public string ReleasePartTemplate
        {
            get => (string) GetValue(ReleasePartTemplateProperty);
            set => SetValue(ReleasePartTemplateProperty, value);
        }

        public double RefreshHeaderHeight
        {
            get => (double) GetValue(RefreshHeaderHeightProperty);
            set => SetValue(RefreshHeaderHeightProperty, value);
        }

        public Brush ArrowColor
        {
            get => (Brush) GetValue(ArrowColorProperty);
            set => SetValue(ArrowColorProperty, value);
        }

        public static DependencyProperty ArrowColorProperty { get; } = DependencyProperty.Register("ArrowColor",
            typeof(Brush), typeof(PullToRefreshListView), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public Brush IndicatorForegroundBrush
        {
            get => (Brush) GetValue(IndicatorForegroundBrushProperty);
            set => SetValue(IndicatorForegroundBrushProperty, value);
        }


        public IEnumerable SelectedItemsList
        {
            get => (IEnumerable) GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        #endregion

        #region Field

        private double _offsetTreshhold = 25;
        private ScrollViewer _rootScrollViewer;
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _renderTimer;
        private Grid _containerGrid;
        public Border PullToRefreshIndicator;

        private ItemsPresenter _listViewItemsPresenter;

        private bool _isReadyToRefresh;

        #endregion

        #region Misc

        private void InvokeRefresh()
        {
            _isReadyToRefresh = false;
            VisualStateManager.GoToState(this, "Normal", true);

            RefreshContent?.Invoke(this, EventArgs.Empty);

#if DEBUG
            Debug.WriteLine("InvokeRefresh");
#endif
        }

        private void InvokeMore()
        {
            MoreContent?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}