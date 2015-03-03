using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.Views.Controls
{
    public class ItemSelectedEventArgs : EventArgs
    {
        public int Index { get; set; }
    }

    public sealed class BindableMenuFlyout : MenuFlyout
    {
        public ObservableCollection<string> Source
        {
            get { return (ObservableCollection<string>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ObservableCollection<string>), typeof(BindableMenuFlyout), new PropertyMetadata(new ObservableCollection<string>(), ItemsSource_Changed));

        private static void ItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ObservableCollection<string>)
                (e.OldValue as ObservableCollection<string>).CollectionChanged -= ItemsSource_CollectionChanged;

            if (e.NewValue is ObservableCollection<string>)
                (e.NewValue as ObservableCollection<string>).CollectionChanged += ItemsSource_CollectionChanged;

            var bindableMenuFlyout = d as BindableMenuFlyout;
            var collection = e.NewValue as ObservableCollection<string>;

            if (bindableMenuFlyout == null)
                return;

            bindableMenuFlyout.Items.Clear();
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    var menuFlyoutItem = new MenuFlyoutItem() { Text = item, Tag = bindableMenuFlyout };
                    menuFlyoutItem.Tapped += MenuFlyoutItem_Tapped;
                    bindableMenuFlyout.Items.Add(menuFlyoutItem);
                }
            }
        }

        private static void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var bindableMenuFlyout = sender as BindableMenuFlyout;
            var collection = bindableMenuFlyout.Source;
            if (bindableMenuFlyout == null)
                return;

            bindableMenuFlyout.Items.Clear();
            if (collection == null)
            {
                foreach (var item in collection)
                {
                    var menuFlyoutItem = new MenuFlyoutItem() { Text = item, Tag = bindableMenuFlyout };
                    menuFlyoutItem.Tapped += MenuFlyoutItem_Tapped;
                    bindableMenuFlyout.Items.Add(menuFlyoutItem);
                }
                    
            }
        }

        public delegate void ItemSelectedEventHandler(object sender, ItemSelectedEventArgs e);
        public event ItemSelectedEventHandler ItemSelected;
        public void ItemSelectedEvent(MenuFlyoutItem item)
        {
            if (ItemSelected != null)
                ItemSelected(this, new ItemSelectedEventArgs() { Index = Items.IndexOf(item) });

        }

        private static void MenuFlyoutItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var bindableMenuFlyout = menuFlyoutItem.Tag as BindableMenuFlyout;
            bindableMenuFlyout.ItemSelectedEvent(menuFlyoutItem);
        }
    }
}
