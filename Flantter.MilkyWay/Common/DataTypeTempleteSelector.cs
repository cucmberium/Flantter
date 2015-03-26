using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Flantter.MilkyWay.Common
{
    [ContentProperty(Name = "Templates")]
    public class DataTypeTemplateSelector : DataTemplateSelector
    {
        private ResourceDictionary templates = new ResourceDictionary();

        public ResourceDictionary Templates
        {
            get { return templates; }
            set { templates = value; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            var dict = (IDictionary<object, object>)this.Templates;
            object result = null;
            dict.TryGetValue(item.GetType().Name, out result);
            return result as DataTemplate;
        }
    }
}
