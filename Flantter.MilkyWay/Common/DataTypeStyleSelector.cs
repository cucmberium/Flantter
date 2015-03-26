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
    [ContentProperty(Name = "Styles")]
    public class DataTypeStyleSelector : StyleSelector
    {
        private ResourceDictionary styles = new ResourceDictionary();

        public ResourceDictionary Styles
        {
            get { return styles; }
            set { styles = value; }
        }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            var dict = (IDictionary<object, object>)this.Styles;
            object result = null;
            dict.TryGetValue(item.GetType().Name, out result);
            return result as Style;
        }
    }
}
