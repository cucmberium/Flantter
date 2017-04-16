using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Flantter.MilkyWay.Views.Controls
{
    public class NumericTextBox : TextBox
    {
        public NumericTextBox()
        {
            LostFocus += NumericTextBox_LostFocus;
        }

        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            if (string.IsNullOrEmpty(textBox.Text))
                textBox.Text = "0";
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (VirtualKey.Number0 <= e.Key && e.Key <= VirtualKey.Number9 ||
                VirtualKey.NumberPad0 <= e.Key && e.Key <= VirtualKey.NumberPad9 ||
                e.Key == VirtualKey.Delete || e.Key == VirtualKey.Back || e.Key == VirtualKey.Tab ||
                e.Key == VirtualKey.Left || e.Key == VirtualKey.Right || e.Key == VirtualKey.Up ||
                e.Key == VirtualKey.Down)
                base.OnKeyDown(e);
            else
                e.Handled = true;
        }
    }
}