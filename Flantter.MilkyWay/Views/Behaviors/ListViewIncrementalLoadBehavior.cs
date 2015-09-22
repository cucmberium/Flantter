using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ListViewIncrementalLoadBehavior : DependencyObject, IBehavior
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
        }

        public void Detach()
        {
            if (this.ScrollViewerObject != null)
                this.ScrollViewerObject.ViewChanged -= ScrollViewerObject_ViewChanged;

            if (this.AssociatedObject != null)
            {
                ((ListView)this.AssociatedObject).Loaded -= ListView_Loaded;
                ((ListView)this.AssociatedObject).LayoutUpdated -= ListView_LayoutUpdated;
            }
        }

        private ScrollViewer _ScrollViewerObject;
        public ScrollViewer ScrollViewerObject
        {
            get { return this._ScrollViewerObject; }
            set { this._ScrollViewerObject = value; }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ListViewScrollControlBehavior), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(ListViewScrollControlBehavior), new PropertyMetadata(null));

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
        }

        private void ScrollViewerObject_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var verticalOffset = this.ScrollViewerObject.VerticalOffset;
            var maxVerticalOffset = this.ScrollViewerObject.ExtentHeight - this.ScrollViewerObject.ViewportHeight;
            if (verticalOffset == maxVerticalOffset)
            {
                if (this.Command != null && this.Command.CanExecute(this.CommandParameter))
                {
                    this.Command.Execute(this.CommandParameter);
                }
            }
        }
    }
}
