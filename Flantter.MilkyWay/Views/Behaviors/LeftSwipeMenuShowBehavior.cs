using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Xaml.Interactivity;
using WinRTXamlToolkit.AwaitableUI;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class LeftSwipeMenuShowBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty SwipeMenuProperty =
            DependencyProperty.Register(
                "SwipeMenu",
                typeof(Control),
                typeof(LeftSwipeMenuShowBehavior),
                new PropertyMetadata(null, SwipeMenuChanged));

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(LeftSwipeMenuShowBehavior),
                new PropertyMetadata(false, IsOpenChanged));

        public static readonly DependencyProperty IsEdgeSwipeProperty =
            DependencyProperty.Register("IsEdgeSwipe", typeof(bool), typeof(LeftSwipeMenuShowBehavior),
                new PropertyMetadata(false));

        private bool _capturingPointer;

        private Popup _rootPopup;
        public Grid RootGrid { get; set; }

        public Control SwipeMenu
        {
            get => (Control) GetValue(SwipeMenuProperty);
            set => SetValue(SwipeMenuProperty, value);
        }

        public bool IsOpen
        {
            get => (bool) GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public bool IsEdgeSwipe
        {
            get => (bool) GetValue(IsEdgeSwipeProperty);
            set => SetValue(IsEdgeSwipeProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            var element = this.AssociatedObject as FrameworkElement;
            element.ManipulationStarted += (s, e) =>
            {
                //if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                //    return;

                if (IsEdgeSwipe && e.Position.X >= 5)
                    return;

                _capturingPointer = true;
            };
            element.ManipulationDelta += (s, e) =>
            {
                if (!_capturingPointer)
                    return;

                //if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                //    return;

                if (e.Cumulative.Translation.X < 10)
                    return;

                _rootPopup.IsOpen = true;
                var x = e.Cumulative.Translation.X - 10 - (SwipeMenu.ActualWidth == 0 ? 280 : SwipeMenu.ActualWidth);
                if (x > 0)
                    x = 0;

                SetTranslate(x, null);
            };
            element.ManipulationCompleted += (s, e) =>
            {
                if (!_capturingPointer)
                    return;

                if (e.Cumulative.Translation.X > SwipeMenu.ActualWidth / 2)
                {
                    if (IsOpen)
                        Show();
                    else
                        IsOpen = true;
                }
                else
                {
                    if (IsOpen)
                        IsOpen = false;
                    else
                        Hide();
                }


                _capturingPointer = false;
            };

            RootGrid = new Grid {Width = Window.Current.Bounds.Width, Height = Window.Current.Bounds.Height};
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)});
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});

            Window.Current.SizeChanged += (s, e) =>
            {
                RootGrid.Width = e.Size.Width;
                RootGrid.Height = e.Size.Height;
            };

            var canvas = new Canvas {Background = new SolidColorBrush(Colors.Transparent)};
            canvas.Tapped += (s, e) => { Hide(); };
            canvas.RightTapped += (s, e) => { Hide(); };

            Grid.SetColumn(canvas, 1);
            RootGrid.Children.Add(canvas);

            _rootPopup = new Popup
            {
                Child = RootGrid,
                IsLightDismissEnabled = false,
                Opacity = 1
            };

            if (SwipeMenu != null)
            {
                Grid.SetColumn(SwipeMenu, 0);
                RootGrid.Children.Add(SwipeMenu);

                if (SwipeMenu.RenderTransform == null)
                    SwipeMenu.RenderTransform = new CompositeTransform();

                var rendarTransform = SwipeMenu.RenderTransform as CompositeTransform;
                rendarTransform.TranslateX = -280;
                SwipeMenu.RenderTransform = rendarTransform;
            }
        }

        public void Detach()
        {
        }

        private static void SwipeMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((LeftSwipeMenuShowBehavior) d).RootGrid == null)
                return;

            var oldSwipeMenu = e.OldValue as Control;
            if (oldSwipeMenu != null)
                ((LeftSwipeMenuShowBehavior) d).RootGrid.Children.Remove(oldSwipeMenu);

            var swipeMenu = e.NewValue as Control;
            if (swipeMenu == null)
                return;

            Grid.SetColumn(swipeMenu, 0);
            ((LeftSwipeMenuShowBehavior) d).RootGrid.Children.Add(swipeMenu);

            if (swipeMenu.RenderTransform == null)
                swipeMenu.RenderTransform = new CompositeTransform();

            var rendarTransform = swipeMenu.RenderTransform as CompositeTransform;
            rendarTransform.TranslateX = -swipeMenu.ActualWidth;
            swipeMenu.RenderTransform = rendarTransform;
        }

        public async void Hide(bool disableAnimation = false)
        {
            if (SwipeMenu == null)
                return;

            if (SwipeMenu.ActualWidth == 0)
                return;

            if (SwipeMenu.RenderTransform == null)
                SwipeMenu.RenderTransform = new CompositeTransform();

            if (disableAnimation)
            {
                var rendarTransform = SwipeMenu.RenderTransform as CompositeTransform;
                rendarTransform.TranslateX = -SwipeMenu.ActualWidth;
                SwipeMenu.RenderTransform = rendarTransform;
            }
            else
            {
                var storyboard = new Storyboard();
                var translateAnimX = new DoubleAnimation
                {
                    To = -SwipeMenu.ActualWidth,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new PowerEase {EasingMode = EasingMode.EaseInOut}
                };
                storyboard.Children.Add(translateAnimX);
                Storyboard.SetTarget(translateAnimX, SwipeMenu);
                Storyboard.SetTargetProperty(translateAnimX,
                    "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                await storyboard.BeginAsync();
            }

            _rootPopup.IsOpen = false;

            IsOpen = false;
        }

        public void Show(bool disableAnimation = false)
        {
            if (SwipeMenu == null)
                return;

            if (SwipeMenu.RenderTransform == null)
                SwipeMenu.RenderTransform = new CompositeTransform();

            _rootPopup.IsOpen = true;

            if (disableAnimation)
            {
                var rendarTransform = SwipeMenu.RenderTransform as CompositeTransform;
                rendarTransform.TranslateX = 0;
                SwipeMenu.RenderTransform = rendarTransform;
            }
            else
            {
                var storyboard = new Storyboard();
                var translateAnimX = new DoubleAnimation
                {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new PowerEase {EasingMode = EasingMode.EaseInOut}
                };
                storyboard.Children.Add(translateAnimX);
                Storyboard.SetTarget(translateAnimX, SwipeMenu);
                Storyboard.SetTargetProperty(translateAnimX,
                    "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                storyboard.Begin();
            }
        }

        public void SetTranslate(double? x, double? y)
        {
            var transform = SwipeMenu.RenderTransform as CompositeTransform;

            if (x.HasValue)
                transform.TranslateX = x.Value;
            if (y.HasValue)
                transform.TranslateY = y.Value;

            SwipeMenu.RenderTransform = transform;
        }

        private static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as LeftSwipeMenuShowBehavior;

            var isOpen = (bool) e.NewValue;
            if (isOpen)
                behavior.Show();
            else
                behavior.Hide();
        }
    }
}