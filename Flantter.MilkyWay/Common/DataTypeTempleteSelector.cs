using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Flantter.MilkyWay.Common
{
    [ContentProperty(Name = "Templates")]
    public class DataTypeTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary Templates { get; set; } = new ResourceDictionary();

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            var dict = (IDictionary<object, object>) Templates;
            object result;
            dict.TryGetValue(item.GetType().Name, out result);
            return result as DataTemplate;
        }
    }
}
