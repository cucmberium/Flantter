using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Controls
{
    public class PullToRefreshPanel : ContentControl
    {
        public event EventHandler PullToRefresh;

        public PullToRefreshPanel()
        {
            DefaultStyleKey = typeof(PullToRefreshPanel);
        }

        public object RefreshContent
        {
            get { return (object)GetValue(RefreshContentProperty); }
            set { SetValue(RefreshContentProperty, value); }
        }
        public static readonly DependencyProperty RefreshContentProperty =
            DependencyProperty.Register("RefreshContent",
                                        typeof(object),
                                        typeof(PullToRefreshPanel),
                                        new PropertyMetadata("Release to Refresh"));

        public object PullContent
        {
            get { return (object)GetValue(PullContentProperty); }
            set { SetValue(PullContentProperty, value); }
        }
        public static readonly DependencyProperty PullContentProperty =
            DependencyProperty.Register("PullContent",
                                        typeof(object),
                                        typeof(PullToRefreshPanel),
                                        new PropertyMetadata("Pull to Refresh"));

        public double PullRange
        {
            get { return (double)GetValue(PullRangeProperty); }
            set { SetValue(PullRangeProperty, value); }
        }
        public static readonly DependencyProperty PullRangeProperty =
            DependencyProperty.Register("PullRange",
                                        typeof(double),
                                        typeof(PullToRefreshPanel),
                                        new PropertyMetadata(250.0));

        public ICommand Command
        {
            get { return (ICommand)this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
                        DependencyProperty.RegisterAttached("Command", typeof(ICommand),
                        typeof(PullToRefreshPanel), null);

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TriangleButton), new PropertyMetadata(null));

        private double pullGridHeight = 0.0;
        private void UpdateView()
        {
            var grid = GetTemplateChild("PullGrid") as Grid;
            ScrollViewer.ChangeView(null, grid.ActualHeight, null, true);
            var contentgrid = GetTemplateChild("ContentGrid") as Grid;
            contentgrid.Height = ScrollViewer.ActualHeight;
            contentgrid.Width = ScrollViewer.ActualWidth;
            pullGridHeight = grid.ActualHeight;

            UpdateTransform();
        }

        private void UpdateStates(bool useTransitions)
        {
            var grid = GetTemplateChild("PullGrid") as Grid;

            if (this.ScrollViewer.VerticalOffset - (grid.Height - PullRange) <= 0.0)
                VisualStateManager.GoToState(this, "Refresh", useTransitions);
            else
                VisualStateManager.GoToState(this, "Pull", useTransitions);
        }

        private void UpdateTransform()
        {
            // Todo : 滑らかになるように何とか修正
            //this.ScrollViewer.UpdateLayout();
            var element = GetTemplateChild("StackPanel") as UIElement;
            var transform = element.RenderTransform as CompositeTransform ?? new CompositeTransform();
            //var grid = GetTemplateChild("PullGrid") as Grid;
            transform.TranslateY = (ScrollViewer.VerticalOffset - pullGridHeight) * 0.75;
            element.RenderTransform = transform;
        }

        protected virtual void OnPullToRefresh(EventArgs e)
        {
            if (PullToRefresh != null)
                PullToRefresh(this, e);

            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }

        protected override void OnApplyTemplate()
        {
            this.ScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            //this.ScrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            this.ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;

            this.ScrollViewer.SizeChanged += (s, e) => { UpdateView(); };

            var pullGrid = GetTemplateChild("PullGrid") as Grid;
            pullGrid.SizeChanged += (s, e) => UpdateView();

            var contentGrid = GetTemplateChild("ContentGrid") as Grid;
            contentGrid.PointerWheelChanged += (s, e) => e.Handled = true;
            contentGrid.SizeChanged += (s, e) => UpdateView();
            contentGrid.KeyDown += ContentGrid_KeyDown;

            this.Loaded += (s, e) => UpdateView();

            base.OnApplyTemplate();
        }

        private void ContentGrid_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Down:
                case VirtualKey.Up:
                case VirtualKey.Home:
                case VirtualKey.End:
                case VirtualKey.PageDown:
                case VirtualKey.PageUp:
                    e.Handled = true;
                    break;
            }
        }

        private bool isPullRefresh = false;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateTransform();
            UpdateStates(true);

            if (ScrollViewer.VerticalOffset != 0.0 && e.IsIntermediate)
                isPullRefresh = true;

            if (!e.IsIntermediate)
            {
                if (ScrollViewer.VerticalOffset == 0.0 && isPullRefresh)
                {
                    OnPullToRefresh(new EventArgs());
                }
                isPullRefresh = false;
                var grid = GetTemplateChild("PullGrid") as Grid;
                // Todo : アニメーションをゴリ押し方式にする？
                ScrollViewer.ChangeView(null, grid.ActualHeight, null, true);
            }
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
        }

        private ScrollViewer _Scrollviewer;
        private ScrollViewer ScrollViewer
        {
            get { return this._Scrollviewer; }
            set { this._Scrollviewer = value; }
        }
    }
}
