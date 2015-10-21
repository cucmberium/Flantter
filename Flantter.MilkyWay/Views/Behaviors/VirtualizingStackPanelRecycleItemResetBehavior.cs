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
    public class VirtualizingStackPanelRecycleItemResetBehavior : DependencyObject, IBehavior
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

            ((VirtualizingStackPanel)this.AssociatedObject).CleanUpVirtualizedItemEvent += VirtualizingStackPanelRecycleItemResetBehavior_CleanUpVirtualizedItemEvent;
        }

        public void Detach()
        {
            ((VirtualizingStackPanel)this.AssociatedObject).CleanUpVirtualizedItemEvent -= VirtualizingStackPanelRecycleItemResetBehavior_CleanUpVirtualizedItemEvent;
        }
        
        private void VirtualizingStackPanelRecycleItemResetBehavior_CleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {
            var listViewItem = (ListViewItem)e.UIElement;
            var recycleItem = listViewItem.ContentTemplateRoot as Contents.Timeline.IRecycleItem;
            if (recycleItem == null)
                return;

            recycleItem.ResetItem();
        }
    }
}
