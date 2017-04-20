using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.Views.Contents.Timeline;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class VirtualizingStackPanelRecycleItemResetBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            ((VirtualizingStackPanel) this.AssociatedObject).CleanUpVirtualizedItemEvent +=
                VirtualizingStackPanelRecycleItemResetBehavior_CleanUpVirtualizedItemEvent;
        }

        public void Detach()
        {
            ((VirtualizingStackPanel) AssociatedObject).CleanUpVirtualizedItemEvent -=
                VirtualizingStackPanelRecycleItemResetBehavior_CleanUpVirtualizedItemEvent;
        }

        private void VirtualizingStackPanelRecycleItemResetBehavior_CleanUpVirtualizedItemEvent(object sender,
            CleanUpVirtualizedItemEventArgs e)
        {
            var listViewItem = (ListViewItem) e.UIElement;
            var recycleItem = listViewItem.ContentTemplateRoot as IRecycleItem;

            recycleItem?.ResetItem();
        }
    }
}