using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Flantter.MilkyWay.Views.Controls
{
    public sealed class TriangleButton : ContentControl
    {
        public TriangleButton()
        {
            this.DefaultStyleKey = typeof(TriangleButton);
        }

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
                                        typeof(PointCollection),
                                        typeof(TriangleButton),
                                        null);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TriangleButton), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TriangleButton), new PropertyMetadata(null));

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

        private bool _Pressing;

        private void TriangleButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _Pressing = false;

            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void TriangleButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _Pressing = true;

            VisualStateManager.GoToState(this, "Pressed", true);
        }

        private void TriangleButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _Pressing = false;

            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void TriangleButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        private void TriangleButton_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_Pressing)
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
