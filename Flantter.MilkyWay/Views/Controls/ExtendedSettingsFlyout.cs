using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

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
            DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(ExtendedSettingsFlyout), new PropertyMetadata(null));

        public bool IsOpenFromLeftEnabled
        {
            get { return (bool)GetValue(IsOpenFromLeftEnabledProperty); }
            set { SetValue(IsOpenFromLeftEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsOpenFromLeftEnabledProperty =
            DependencyProperty.Register("IsOpenFromLeftEnabled", typeof(bool), typeof(ExtendedSettingsFlyout), new PropertyMetadata(false));

        public event EventHandler Showed;
        public event EventHandler Hided;
        public event BackClickEventHandler BackClick;
        new public event SizeChangedEventHandler SizeChanged;
        
        public ExtendedSettingsFlyout()
        {
            this.DefaultStyleKey = typeof(ExtendedSettingsFlyout);
            this.contentPopup = new Popup();
            this.contentPopup.Opened += ContentPopup_Opened;
            this.contentPopup.Closed += ContentPopup_Closed;
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

        private Grid rootGrid;
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var backButton = GetTemplateChild("BackButton") as Button;
            rootGrid = GetTemplateChild("RootGrid") as Grid;
            rootGrid.Transitions.Clear();
            if (IsOpenFromLeftEnabled)
            {
                rootGrid.BorderThickness = new Thickness(0, 0, 1, 0);
                rootGrid.Transitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Left });
            }
            else
            {
                rootGrid.BorderThickness = new Thickness(1, 0, 0, 0);
                rootGrid.Transitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Right });
            }
            backButton.Click += BackButton_Click;
            Window.Current.SizeChanged += Window_SizeChanged;
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
            var popupGrid = contentPopup.Child as Grid;

            if (popupGrid == null)
                return;

            popupGrid.Width = Window.Current.Bounds.Width;
            popupGrid.Height = Window.Current.Bounds.Height;
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
            if (rootGrid != null)
            {
                rootGrid.Transitions.Clear();
                if (IsOpenFromLeftEnabled)
                {
                    rootGrid.BorderThickness = new Thickness(0, 0, 1, 0);
                    rootGrid.Transitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Left });
                }
                else
                {
                    rootGrid.BorderThickness = new Thickness(1, 0, 0, 0);
                    rootGrid.Transitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Right });
                }
            }

            this.Visibility = Visibility.Visible;
            this.Height = Window.Current.Bounds.Height;

            var popupGrid = new Grid() { Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255)), Width = Window.Current.Bounds.Width, Height = Window.Current.Bounds.Height };
            var popupTappedGrid = new Grid() { Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255)), Width = (Window.Current.Bounds.Width - this.Width) < 0 ? 0 : (Window.Current.Bounds.Width - this.Width), Height = Window.Current.Bounds.Height };

            if (!IsOpenFromLeftEnabled)
            {
                this.HorizontalAlignment = HorizontalAlignment.Right;
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                popupTappedGrid.SetValue(Grid.ColumnProperty, 0);
                this.SetValue(Grid.ColumnProperty, 1);
            }
            else
            {
                this.HorizontalAlignment = HorizontalAlignment.Left;
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                popupGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                popupTappedGrid.SetValue(Grid.ColumnProperty, 1);
                this.SetValue(Grid.ColumnProperty, 0);
            }

            popupGrid.Children.Add(popupTappedGrid);
            popupGrid.Children.Add(this);

            this.contentPopup.Child = popupGrid;
            this.contentPopup.Opacity = 1;

            this.contentPopup.HorizontalOffset = 0;
            this.contentPopup.VerticalOffset = 0;
            popupTappedGrid.Tapped += (s, e) => { this.Hide(); e.Handled = true; };
            popupTappedGrid.RightTapped += (s, e) => { this.Hide(); e.Handled = true; };
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
        public void Hide()
        {
            if (this.contentPopup != null)
            {
                this.contentPopup.IsOpen = false;

                if (this.contentPopup.Child is Grid)
                    (this.contentPopup.Child as Grid).Children.Clear();

                this.contentPopup.Child = null;
            }
                
            this.Visibility = Visibility.Collapsed;
        }
    }
}
