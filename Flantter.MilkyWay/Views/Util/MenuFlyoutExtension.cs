using Microsoft.Xaml.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.Views.Util
{
    public class MenuFlyoutExtension : DependencyObject, IBehavior
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
            
        }

        public void Detach()
        {
        }
        
        public object Items
        {
            get { return (object)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(object), typeof(MenuFlyoutExtension), new PropertyMetadata(null, Items_Changed));

        private static void Items_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var menuFlyoutExtension = d as MenuFlyoutExtension;

            if (e.OldValue != null && e.OldValue is INotifyCollectionChanged)
                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= menuFlyoutExtension.Items_CollectionChanged;

            if (e.NewValue != null && e.NewValue is INotifyCollectionChanged)
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += menuFlyoutExtension.Items_CollectionChanged;

            var menuFlyout = menuFlyoutExtension.AssociatedObject as MenuFlyout;

            var collection = e.NewValue as IList;

            if (menuFlyoutExtension == null || menuFlyout == null || collection == null)
                return;

            menuFlyout.Items.Clear();
            foreach (var item in collection)
            {
                var menuFlyoutItem = new MenuFlyoutItem() { Text = item?.ToString(), Tag = item };
                menuFlyoutItem.Click += menuFlyoutExtension.MenuFlyoutItem_Click;
                menuFlyout.Items.Add(menuFlyoutItem);
            }
        }
        public void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var menuFlyoutExtension = this;
            var menuFlyout = menuFlyoutExtension.AssociatedObject as MenuFlyout;

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var menuFlyoutItems = menuFlyout.Items.Where(x => x.Tag == item);
                    if (menuFlyoutItems.Count() > 0)
                        menuFlyout.Items.Remove(menuFlyoutItems.First());
                }
            }
            
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var menuFlyoutItem = new MenuFlyoutItem() { Text = item?.ToString(), Tag = item };
                    menuFlyoutItem.Click += menuFlyoutExtension.MenuFlyoutItem_Click;
                    menuFlyout.Items.Add(menuFlyoutItem);
                }
            }
        }

        public void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;

            var menuFlyoutExtension = this;
            
            menuFlyoutExtension.ItemSelectedEvent(menuFlyoutItem);
        }

        public delegate void ItemSelectedEventHandler(object sender, ItemSelectedEventArgs e);
        public event ItemSelectedEventHandler ItemSelected;
        public void ItemSelectedEvent(MenuFlyoutItem item)
        {
            var menuFlyoutExtension = this;
            var menuFlyout = menuFlyoutExtension.AssociatedObject as MenuFlyout;

            if (ItemSelected != null)
                ItemSelected(this, new ItemSelectedEventArgs() { Index = menuFlyout.Items.IndexOf(item) });
        }
    }

    public class ItemSelectedEventArgs : EventArgs
    {
        public int Index { get; set; }
    }
}
