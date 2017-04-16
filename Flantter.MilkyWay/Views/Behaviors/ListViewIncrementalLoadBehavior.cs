using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ListViewIncrementalLoadBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ListViewScrollControlBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object),
                typeof(ListViewScrollControlBehavior), new PropertyMetadata(null));

        public ScrollViewer ScrollViewerObject { get; set; }

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            ((ListView) this.AssociatedObject).Loaded += ListView_Loaded;
            ((ListView) this.AssociatedObject).LayoutUpdated += ListView_LayoutUpdated;
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
            }
        }

        private void ListView_LayoutUpdated(object sender, object e)
        {
            if (ScrollViewerObject != null)
                return;

            var border = VisualTreeHelper.GetChild((ListView) AssociatedObject, 0) as Border;

            var listViewScroll = border?.Child as ScrollViewer;
            if (listViewScroll == null)
                return;

            ScrollViewerObject = listViewScroll;
            ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ScrollViewerObject != null)
                return;

            var border = VisualTreeHelper.GetChild((ListView) AssociatedObject, 0) as Border;

            var listViewScroll = border?.Child as ScrollViewer;
            if (listViewScroll == null)
                return;

            ScrollViewerObject = listViewScroll;
            ScrollViewerObject.ViewChanged += ScrollViewerObject_ViewChanged;
        }

        private void ScrollViewerObject_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (ScrollViewerObject.VerticalOffset == ScrollViewerObject.ScrollableHeight &&
                ScrollViewerObject.ViewportHeight != 0)
                if (Command != null && Command.CanExecute(CommandParameter))
                    Command.Execute(CommandParameter);
        }
    }
}