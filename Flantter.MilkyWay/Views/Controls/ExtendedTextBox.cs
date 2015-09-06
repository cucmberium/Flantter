using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Flantter.MilkyWay.Views.Controls
{
    public class ExtendedTextBox : TextBox
    {
        public ExtendedTextBox() : base()
        {
            this.SelectionChanged += ExtendedTextBox_SelectionChanged;
        }

        private void ExtendedTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            this.CursorPosition = this.SelectionStart;
        }

        public event KeyEventHandler PreKeyDown;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (PreKeyDown != null)
                PreKeyDown(this, e);

            if (!e.Handled)
                base.OnKeyDown(e);
        }

        public int CursorPosition
        {
            get { return (int)GetValue(CursorPositionProperty); }
            set { SetValue(CursorPositionProperty, value); }
        }
        public static readonly DependencyProperty CursorPositionProperty =
            DependencyProperty.Register("CursorPosition",
                                        typeof(int),
                                        typeof(ExtendedTextBox),
                                        new PropertyMetadata(0, CursorPositionChanged));

        private static void CursorPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
