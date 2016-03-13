using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using WinRTXamlToolkit.AwaitableUI;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class LeftSwipeMenuShowBehavior : DependencyObject, IBehavior
    {
        private DependencyObject _AssociatedObject;
        public DependencyObject AssociatedObject
        {
            get { return this._AssociatedObject; }
            set { this._AssociatedObject = value; }
        }

        private Popup rootPopup;
        public Grid RootGrid { get; set; }

        private bool capturingPointer = false;

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            var element = this.AssociatedObject as FrameworkElement;
            element.ManipulationStarted += (s, e) =>
            {
                //if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                //    return;

                if (this.IsEdgeSwipe && e.Position.X >= 5)
                    return;

                capturingPointer = true;
            };
            element.ManipulationDelta += (s, e) =>
            {
                if (!capturingPointer)
                    return;

                //if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                //    return;

                if (e.Cumulative.Translation.X < 10)
                    return;
                
                this.rootPopup.IsOpen = true;
                var x = (e.Cumulative.Translation.X - 10) - (this.SwipeMenu.ActualWidth == 0 ? 280 : this.SwipeMenu.ActualWidth);
                if (x > 0)
                    x = 0;

                this.SetTranslate(x, null);
            };
            element.ManipulationCompleted += (s, e) =>
            {
                if (!capturingPointer)
                    return;

                if (e.Cumulative.Translation.X > this.SwipeMenu.ActualWidth / 2)
                    this.Show();
                else
                    this.Hide();

                capturingPointer = false;
            };

            this.RootGrid = new Grid() { Width = Window.Current.Bounds.Width, Height = Window.Current.Bounds.Height };
            this.RootGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            this.RootGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Window.Current.SizeChanged += (s, e) => { this.RootGrid.Width = e.Size.Width; this.RootGrid.Height = e.Size.Height; };

            var canvas = new Canvas() { Background = new SolidColorBrush(Colors.Transparent) };
            canvas.Tapped += (s, e) => { this.Hide(); };
            canvas.RightTapped += (s, e) => { this.Hide(); };

            Grid.SetColumn(canvas, 1);
            this.RootGrid.Children.Add(canvas);

            rootPopup = new Popup()
            {
                Child = this.RootGrid,
                IsLightDismissEnabled = false,
                Opacity = 1
            };
            
            if (this.SwipeMenu != null)
            {
                Grid.SetColumn(this.SwipeMenu, 0);
                this.RootGrid.Children.Add(this.SwipeMenu);

                if (this.SwipeMenu.RenderTransform == null)
                    this.SwipeMenu.RenderTransform = new CompositeTransform();

                var rendarTransform = this.SwipeMenu.RenderTransform as CompositeTransform;
                rendarTransform.TranslateX = -this.SwipeMenu.ActualWidth;
                this.SwipeMenu.RenderTransform = rendarTransform;
            }
        }

        public void Detach()
        {
        }

        public Control SwipeMenu
        {
            get { return (Control)GetValue(SwipeMenuProperty); }
            set { SetValue(SwipeMenuProperty, value); }
        }

        public static readonly DependencyProperty SwipeMenuProperty =
            DependencyProperty.Register(
                "SwipeMenu",
                typeof(Control),
                typeof(LeftSwipeMenuShowBehavior),
                new PropertyMetadata(null, SwipeMenuChanged));

        private static void SwipeMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((LeftSwipeMenuShowBehavior)d).RootGrid == null)
                return;

            var oldSwipeMenu = e.OldValue as Control;
            if (oldSwipeMenu != null)
                ((LeftSwipeMenuShowBehavior)d).RootGrid.Children.Remove(oldSwipeMenu);

            var swipeMenu = e.NewValue as Control;
            if (swipeMenu == null)
                return;
            
            Grid.SetColumn(swipeMenu, 0);
            ((LeftSwipeMenuShowBehavior)d).RootGrid.Children.Add(swipeMenu);

            if (swipeMenu.RenderTransform == null)
                swipeMenu.RenderTransform = new CompositeTransform();

            var rendarTransform = swipeMenu.RenderTransform as CompositeTransform;
            rendarTransform.TranslateX = -swipeMenu.ActualWidth;
            swipeMenu.RenderTransform = rendarTransform;
        }

        public async void Hide(bool disableAnimation = false)
        {
            if (this.SwipeMenu == null)
                return;

            if (this.SwipeMenu.ActualWidth == 0)
                return;

            if (this.SwipeMenu.RenderTransform == null)
                this.SwipeMenu.RenderTransform = new CompositeTransform();

            if (disableAnimation)
            {
                var rendarTransform = this.SwipeMenu.RenderTransform as CompositeTransform;
                rendarTransform.TranslateX = -this.SwipeMenu.ActualWidth;
                this.SwipeMenu.RenderTransform = rendarTransform;
            }
            else
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimation translateAnimX = new DoubleAnimation() { To = -this.SwipeMenu.ActualWidth, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };
                storyboard.Children.Add(translateAnimX);
                Storyboard.SetTarget(translateAnimX, this.SwipeMenu);
                Storyboard.SetTargetProperty(translateAnimX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                await storyboard.BeginAsync();
            }

            rootPopup.IsOpen = false;

            this.IsOpen = false;
        }

        public void Show(bool disableAnimation = false)
        {
            if (this.SwipeMenu == null)
                return;

            if (this.SwipeMenu.RenderTransform == null)
                this.SwipeMenu.RenderTransform = new CompositeTransform();

            rootPopup.IsOpen = true;

            if (disableAnimation)
            {
                var rendarTransform = this.SwipeMenu.RenderTransform as CompositeTransform;
                rendarTransform.TranslateX = 0;
                this.SwipeMenu.RenderTransform = rendarTransform;
            }
            else
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimation translateAnimX = new DoubleAnimation() { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };
                storyboard.Children.Add(translateAnimX);
                Storyboard.SetTarget(translateAnimX, this.SwipeMenu);
                Storyboard.SetTargetProperty(translateAnimX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                storyboard.Begin();
            }

            this.IsOpen = true;
        }

        public void SetTranslate(double? x, double? y)
        {
            var transform = this.SwipeMenu.RenderTransform as CompositeTransform;

            if (x.HasValue)
                transform.TranslateX = x.Value;
            if (y.HasValue)
                transform.TranslateY = y.Value;

            this.SwipeMenu.RenderTransform = transform;
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(LeftSwipeMenuShowBehavior), new PropertyMetadata(false, IsOpenChanged));

        private static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as LeftSwipeMenuShowBehavior;

            var isOpen = (bool)e.NewValue;
            if (isOpen)
                behavior.Show();
            else
                behavior.Hide();
        }

        public bool IsEdgeSwipe
        {
            get { return (bool)GetValue(IsEdgeSwipeProperty); }
            set { SetValue(IsEdgeSwipeProperty, value); }
        }
        public static readonly DependencyProperty IsEdgeSwipeProperty =
            DependencyProperty.Register("IsEdgeSwipe", typeof(bool), typeof(LeftSwipeMenuShowBehavior), new PropertyMetadata(false));
    }
}
