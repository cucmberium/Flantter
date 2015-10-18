using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed class ColumnAreaListBox : ListBox
    {
        public ColumnAreaListBox()
        {
            this.DefaultStyleKey = typeof(ColumnAreaListBox);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as FrameworkElement;
            var binding = new Windows.UI.Xaml.Data.Binding
            {
                Source = item,
                Path = new PropertyPath("Left.Value"),
                Mode = BindingMode.OneWay
            };

            container.SetBinding(Canvas.LeftProperty, binding);
        }
    }
}
