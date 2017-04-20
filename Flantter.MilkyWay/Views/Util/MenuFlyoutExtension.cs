using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    public class MenuFlyoutExtension : DependencyObject, IBehavior
    {
        public delegate void ItemSelectedEventHandler(object sender, ItemSelectedEventArgs e);

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(object), typeof(MenuFlyoutExtension),
                new PropertyMetadata(null, Items_Changed));

        public object Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;
        }

        public void Detach()
        {
        }

        private static void Items_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var menuFlyoutExtension = d as MenuFlyoutExtension;

            if (e.OldValue is INotifyCollectionChanged)
                ((INotifyCollectionChanged) e.OldValue).CollectionChanged -=
                    menuFlyoutExtension.Items_CollectionChanged;

            if (e.NewValue is INotifyCollectionChanged)
                ((INotifyCollectionChanged) e.NewValue).CollectionChanged +=
                    menuFlyoutExtension.Items_CollectionChanged;

            var menuFlyout = menuFlyoutExtension.AssociatedObject as MenuFlyout;

            var collection = e.NewValue as IList;

            if (menuFlyoutExtension == null || menuFlyout == null || collection == null)
                return;

            menuFlyout.Items.Clear();
            foreach (var item in collection)
            {
                var menuFlyoutItem = new MenuFlyoutItem {Text = item?.ToString(), Tag = item};
                menuFlyoutItem.Click += menuFlyoutExtension.MenuFlyoutItem_Click;
                menuFlyout.Items.Add(menuFlyoutItem);
            }
        }

        public void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var menuFlyoutExtension = this;
            var menuFlyout = menuFlyoutExtension.AssociatedObject as MenuFlyout;

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    var menuFlyoutItems = menuFlyout.Items.Where(x => x.Tag == item);
                    if (menuFlyoutItems.Any())
                        menuFlyout.Items.Remove(menuFlyoutItems.First());
                }

            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    var menuFlyoutItem = new MenuFlyoutItem {Text = item?.ToString(), Tag = item};
                    menuFlyoutItem.Click += menuFlyoutExtension.MenuFlyoutItem_Click;
                    menuFlyout.Items.Add(menuFlyoutItem);
                }
        }

        public void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;

            var menuFlyoutExtension = this;

            menuFlyoutExtension.ItemSelectedEvent(menuFlyoutItem);
        }

        public event ItemSelectedEventHandler ItemSelected;

        public void ItemSelectedEvent(MenuFlyoutItem item)
        {
            var menuFlyoutExtension = this;
            var menuFlyout = menuFlyoutExtension.AssociatedObject as MenuFlyout;

            ItemSelected?.Invoke(this, new ItemSelectedEventArgs {Index = menuFlyout.Items.IndexOf(item)});
        }
    }

    public class ItemSelectedEventArgs : EventArgs
    {
        public int Index { get; set; }
    }
}