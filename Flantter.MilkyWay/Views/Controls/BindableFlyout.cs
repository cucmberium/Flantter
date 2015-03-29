using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Collections;
using Flantter.MilkyWay.Common;

namespace Flantter.MilkyWay.Views.Controls
{
    public sealed class BindableFlyout : DependencyObject
    {
        #region ItemsSource

        public static IEnumerable GetItemsSource(DependencyObject obj)
        {
            return obj.GetValue(ItemsSourceProperty) as IEnumerable;
        }

        public static void SetItemsSource(DependencyObject obj, IEnumerable value)
        {
            obj.SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.RegisterAttached("ItemsSource", typeof(IEnumerable),
            typeof(BindableFlyout), new PropertyMetadata(null, ItemsSourceChanged));

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { 
            Setup(d as Windows.UI.Xaml.Controls.Flyout); 
        }

        #endregion

        #region ItemTemplate

        public static DataTemplate GetItemTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(ItemTemplateProperty);
        }

        public static void SetItemTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.RegisterAttached("ItemTemplate", typeof(DataTemplate),
            typeof(BindableFlyout), new PropertyMetadata(null, ItemsTemplateChanged));
        private static void ItemsTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Setup(d as Windows.UI.Xaml.Controls.Flyout);
        }

        #endregion

        private static async void Setup(Windows.UI.Xaml.Controls.Flyout m)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;
            
            var s = GetItemsSource(m);
            if (s == null)
                return;
                
            var t = GetItemTemplate(m);
            if (t == null)
                return;
            
            var c = new Windows.UI.Xaml.Controls.ItemsControl
            {
                ItemsSource = s,
                ItemTemplate = t,
            };
            var n = Windows.UI.Core.CoreDispatcherPriority.Normal;
            Windows.UI.Core.DispatchedHandler h = () => m.Content = c;
            await m.Dispatcher.RunAsync(n, h);
        }
    }
}
