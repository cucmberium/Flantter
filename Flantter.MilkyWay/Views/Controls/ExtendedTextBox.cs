using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Flantter.MilkyWay.Views.Controls
{
    public class ExtendedTextBox : TextBox
    {
        public event KeyEventHandler PreKeyDown;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (PreKeyDown != null)
                PreKeyDown(this, e);

            if (!e.Handled)
                base.OnKeyDown(e);
        }
    }
}
