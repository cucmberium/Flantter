using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Controls
{
    public class PullToRefreshPanel : Control
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

        private void UpdateView()
        {
            var grid = GetTemplateChild("PullGrid") as Grid;
            ScrollViewer.ChangeView(null, grid.ActualHeight, null, true);
            var contentgrid = GetTemplateChild("ContentGrid") as Grid;
            contentgrid.SetValue(HeightProperty, ScrollViewer.ActualHeight);
            contentgrid.SetValue(WidthProperty, ScrollViewer.ActualWidth);
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
            var element = GetTemplateChild("StackPanel") as UIElement;
            var transform = element.RenderTransform as CompositeTransform ?? new CompositeTransform();
            var grid = GetTemplateChild("PullGrid") as Grid;
            transform.TranslateY = (ScrollViewer.VerticalOffset - grid.ActualHeight) * 0.75;
            element.RenderTransform = transform;
        }

        protected virtual void OnPullToRefresh(EventArgs e)
        {
            if (PullToRefresh != null)
                PullToRefresh(this, e);

            if (Command != null && Command.CanExecute(null))
                Command.Execute(null);
        }

        protected override void OnApplyTemplate()
        {
            this.ScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            //this.ScrollViewer.SizeChanged += (s, e) => UpdateView();
            Window.Current.SizeChanged += async (s, e) => { await Task.Delay(10); UpdateView(); };
            this.ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            
            this.Loaded += (s, e) => UpdateView();

            var grid = GetTemplateChild("PullGrid") as Grid;
            grid.SizeChanged += (s, e) => UpdateView();

            base.OnApplyTemplate();
        }

        private bool isPullRefresh = false;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateStates(true);
            UpdateTransform();

            if (ScrollViewer.VerticalOffset != 0.0)
                isPullRefresh = true;

            if (!e.IsIntermediate)
            {
                if (ScrollViewer.VerticalOffset == 0.0 && isPullRefresh)
                {
                    OnPullToRefresh(new EventArgs());

                    //await Task.Delay(50);
                }
                isPullRefresh = false;
                var grid = GetTemplateChild("PullGrid") as Grid;
                ScrollViewer.ChangeView(null, grid.ActualHeight, null);
            }
        }

        private ScrollViewer _Scrollviewer;
        private ScrollViewer ScrollViewer
        {
            get { return this._Scrollviewer; }
            set { this._Scrollviewer = value; }
        }

    }
}
