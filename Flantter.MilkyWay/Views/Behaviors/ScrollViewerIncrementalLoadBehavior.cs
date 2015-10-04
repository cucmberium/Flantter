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
    public class ScrollViewerIncrementalLoadBehavior : DependencyObject, IBehavior
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

            ((ScrollViewer)this.AssociatedObject).Loaded += (s, e) => ((ScrollViewer)this.AssociatedObject).ViewChanged += ScrollViewerObject_ViewChanged;
        }

        public void Detach()
        {
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
        
        private void ScrollViewerObject_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var verticalOffset = ((ScrollViewer)this.AssociatedObject).VerticalOffset;
            var maxVerticalOffset = ((ScrollViewer)this.AssociatedObject).ExtentHeight - ((ScrollViewer)this.AssociatedObject).ViewportHeight;
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
