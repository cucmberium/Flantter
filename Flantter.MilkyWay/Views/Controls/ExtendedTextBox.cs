using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Flantter.MilkyWay.Views.Controls
{
    public class ExtendedTextBox : TextBox
    {
        public static readonly DependencyProperty CursorPositionProperty =
            DependencyProperty.Register("CursorPosition",
                typeof(int),
                typeof(ExtendedTextBox),
                new PropertyMetadata(0, CursorPositionChanged));

        private bool _changeFromUi;

        public ExtendedTextBox()
        {
            DefaultStyleKey = typeof(TextBox);
            SelectionChanged += ExtendedTextBox_SelectionChanged;
        }

        public int CursorPosition
        {
            get => (int) GetValue(CursorPositionProperty);
            set => SetValue(CursorPositionProperty, value);
        }

        private void ExtendedTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (CursorPosition != SelectionStart)
            {
                _changeFromUi = true;
                CursorPosition = SelectionStart;
            }
        }

        public event KeyEventHandler PreKeyDown;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            PreKeyDown?.Invoke(this, e);

            if (!e.Handled)
                base.OnKeyDown(e);
        }

        private static void CursorPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as ExtendedTextBox;

            if (!textBox._changeFromUi)
                textBox.SelectionStart = (int) e.NewValue;
            else
                textBox._changeFromUi = false;
        }
    }
}