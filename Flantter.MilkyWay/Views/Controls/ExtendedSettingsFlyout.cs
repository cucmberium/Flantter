using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// テンプレート コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234235 を参照してください

namespace Flantter.MilkyWay.Views.Controls
{
    public class ExtendedSettingsFlyout : ContentControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ExtendedSettingsFlyout), new PropertyMetadata("Title"));

        public Brush HeaderBackground
        {
            get { return (Brush)GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }
        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(ExtendedSettingsFlyout), new PropertyMetadata(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))));

        public Brush HeaderForeground
        {
            get { return (Brush)GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }
        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(ExtendedSettingsFlyout), new PropertyMetadata(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))));

        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(ExtendedSettingsFlyout), new PropertyMetadata("http://localhost"));

        public bool IsOpenFromLeftEnabled
        {
            get { return (bool)GetValue(IsOpenFromLeftEnabledProperty); }
            set { SetValue(IsOpenFromLeftEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsOpenFromLeftEnabledProperty =
            DependencyProperty.Register("IsOpenFromLeftEnabled", typeof(bool), typeof(ExtendedSettingsFlyout), new PropertyMetadata(false, IsOpenFromLeftEnabledProperty_ChangedCallback));


        private static void IsOpenFromLeftEnabledProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedSettingsFlyout = d as ExtendedSettingsFlyout;
            if (extendedSettingsFlyout == null)
                return;

            extendedSettingsFlyout.IsOpenFromLeftEnabledChanged();
        }

        public event EventHandler Showed;
        public event EventHandler Hided;
        public event BackClickEventHandler BackClick;
        new public event SizeChangedEventHandler SizeChanged;

        private SolidColorBrush _TransparentBrush;
        public ExtendedSettingsFlyout()
        {
            this.DefaultStyleKey = typeof(ExtendedSettingsFlyout);
            this.contentPopup = new Popup();
            this.contentPopup.Opened += ContentPopup_Opened;
            this.contentPopup.Closed += ContentPopup_Closed;

            _TransparentBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255));

            popupGrid = new Grid() { Background = _TransparentBrush, Width = Window.Current.Bounds.Width, Height = Window.Current.Bounds.Height };

            popupGrid.Tapped += (s, e) => { this.Hide(); e.Handled = true; };
            popupGrid.RightTapped += (s, e) => { this.Hide(); e.Handled = true; };

            this.IsOpenFromLeftEnabledChanged();
        }

        private void ContentPopup_Opened(object sender, object e)
        {
            if (this.Showed != null)
                this.Showed(this, EventArgs.Empty);
        }

        private void ContentPopup_Closed(object sender, object e)
        {
            if (this.Hided != null)
                this.Hided(this, EventArgs.Empty);
        }

        private Border rootBorder = null;
        private Grid rootGrid = null;
        private Grid popupGrid = null;
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var backButton = GetTemplateChild("BackButton") as Button;
            rootGrid = GetTemplateChild("RootGrid") as Grid;
            rootBorder = GetTemplateChild("RootBorder") as Border;

            backButton.Click += BackButton_Click;
            Window.Current.SizeChanged += Window_SizeChanged;

            rootBorder.Tapped += (s, e) => e.Handled = true;
            rootBorder.RightTapped += (s, e) => e.Handled = true;
            
            if (!IsOpenFromLeftEnabled)
                rootBorder.ChildTransitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Right });
            else
                rootBorder.ChildTransitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Left });

            if (this.Width >= Window.Current.Bounds.Width)
            {
                rootGrid.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                if (!IsOpenFromLeftEnabled)
                    rootGrid.BorderThickness = new Thickness(1, 0, 0, 0);
                else
                    rootGrid.BorderThickness = new Thickness(0, 0, 1, 0);
            }
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.Height = Window.Current.Bounds.Height;

            if (SizeChanged != null)
                SizeChanged(this, null);

            if (!contentPopup.IsOpen)
                return;

            contentPopup.HorizontalOffset = 0;
            contentPopup.VerticalOffset = 0;

            popupGrid.Width = Window.Current.Bounds.Width;
            popupGrid.Height = Window.Current.Bounds.Height;

            if (this.Width >= Window.Current.Bounds.Width)
            {
                rootGrid.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                if (!IsOpenFromLeftEnabled)
                    rootGrid.BorderThickness = new Thickness(1, 0, 0, 0);
                else
                    rootGrid.BorderThickness = new Thickness(0, 0, 1, 0);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var backClickEventArgs = new BackClickEventArgs() { Handled = false };

            if (BackClick != null)
                BackClick(sender, backClickEventArgs);
        }

        public bool IsOpen { get { return (contentPopup != null && contentPopup.IsOpen); } }
        private Popup contentPopup = null;
        public void Show()
        {
            this.Visibility = Visibility.Visible;
            this.Height = Window.Current.Bounds.Height;

            this.contentPopup.Child = popupGrid;

            contentPopup.HorizontalOffset = 0;
            contentPopup.VerticalOffset = 0;

            popupGrid.Width = Window.Current.Bounds.Width;
            popupGrid.Height = Window.Current.Bounds.Height;

            if (rootBorder != null && rootGrid != null)
            {
                if (this.Width >= Window.Current.Bounds.Width)
                {
                    rootGrid.BorderThickness = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    if (!IsOpenFromLeftEnabled)
                        rootGrid.BorderThickness = new Thickness(1, 0, 0, 0);
                    else
                        rootGrid.BorderThickness = new Thickness(0, 0, 1, 0);
                }
            }

            this.contentPopup.IsOpen = true;

            // 影付けたかったらこれ
            //var gradient = new LinearGradientBrush();
            //gradient.StartPoint = new Point(1, 0.5);
            //gradient.EndPoint = new Point(0, 0.5);
            //gradient.GradientStops.Add(new GradientStop() { Color = Windows.UI.Color.FromArgb(72, 0, 0, 0), Offset = 0 });
            //gradient.GradientStops.Add(new GradientStop() { Color = Windows.UI.Color.FromArgb(36, 0, 0, 0), Offset = 0.01 });
            //gradient.GradientStops.Add(new GradientStop() { Color = Windows.UI.Color.FromArgb(0, 0, 0, 0), Offset = 0.02 });
            //gradient.GradientStops.Add(new GradientStop() { Color = Windows.UI.Color.FromArgb(0, 0, 0, 0), Offset = 1 });
            //popupTappedGrid.Background = gradient;
            //popupTappedGrid.Transitions = new TransitionCollection();
            //popupTappedGrid.Transitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Right });
        }
        public async void Hide()
        {
            if (this.contentPopup == null)
                return;

            if (rootBorder != null && rootGrid != null)
            {
                if (rootBorder.Child != null)
                    rootBorder.Child = null;
            }


            await Task.Delay(300);

            this.contentPopup.IsOpen = false;
            this.contentPopup.Child = null;
            
            this.Visibility = Visibility.Collapsed;


            if (rootBorder != null && rootGrid != null)
            {
                if (rootBorder.Child == null)
                    rootBorder.Child = rootGrid;
            }
        }

        public void IsOpenFromLeftEnabledChanged()
        {
            popupGrid.Children.Clear();

            popupGrid.ColumnDefinitions.Clear();

            if (!IsOpenFromLeftEnabled)
            {
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                
                this.SetValue(Grid.ColumnProperty, 1);
            }
            else
            {
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                this.SetValue(Grid.ColumnProperty, 0);
            }

            if (rootBorder != null && rootGrid != null)
            {
                if (!IsOpenFromLeftEnabled)
                {
                    rootGrid.BorderThickness = new Thickness(1, 0, 0, 0);
                    rootBorder.ChildTransitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Right });
                }
                else
                {
                    rootGrid.BorderThickness = new Thickness(0, 0, 1, 0);
                    rootBorder.ChildTransitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Left });
                }
            }

            popupGrid.Children.Add(this);
        }
    }
}
