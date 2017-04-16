using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Controls
{
    public sealed class TriangleButton : ContentControl
    {
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
                typeof(PointCollection),
                typeof(TriangleButton),
                null);

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TriangleButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TriangleButton),
                new PropertyMetadata(null));

        private bool _pressing;

        public TriangleButton()
        {
            DefaultStyleKey = typeof(TriangleButton);
        }

        public PointCollection Points
        {
            get => (PointCollection) GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            var grid = GetTemplateChild("TriangleButtonGrid") as Grid;
            grid.Tapped += TriangleButton_Tapped;

            grid.PointerEntered += TriangleButton_PointerEntered;
            grid.PointerMoved += TriangleButton_PointerMoved;
            grid.PointerExited += TriangleButton_PointerExited;
            grid.PointerPressed += TriangleButton_PointerPressed;
            grid.PointerReleased += TriangleButton_PointerReleased;

            base.OnApplyTemplate();
        }

        private void TriangleButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _pressing = false;

            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void TriangleButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _pressing = true;

            VisualStateManager.GoToState(this, "Pressed", true);
        }

        private void TriangleButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _pressing = false;

            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void TriangleButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        private void TriangleButton_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_pressing)
                VisualStateManager.GoToState(this, "Pressed", true);
            else
                VisualStateManager.GoToState(this, "PointerOver", true);
        }

        public new event TappedEventHandler Tapped;

        private void TriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);

            if (Tapped != null)
                Tapped(this, e);
        }
    }
}