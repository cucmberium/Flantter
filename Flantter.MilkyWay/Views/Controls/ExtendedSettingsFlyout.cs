using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// テンプレート コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234235 を参照してください

namespace Flantter.MilkyWay.Views.Controls
{
    internal interface IContentPopup
    {
        bool IsOpen { get; }

        void Show();

        void Hide();
    }

    public class ExtendedSettingsFlyout : ContentControl, IContentPopup
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ExtendedSettingsFlyout),
                new PropertyMetadata("Title"));

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(ExtendedSettingsFlyout),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));

        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(ExtendedSettingsFlyout),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(ExtendedSettingsFlyout),
                new PropertyMetadata(new BitmapImage {UriSource = new Uri("http://localhost")}));

        public static readonly DependencyProperty IsOpenFromLeftEnabledProperty =
            DependencyProperty.Register("IsOpenFromLeftEnabled", typeof(bool), typeof(ExtendedSettingsFlyout),
                new PropertyMetadata(false, IsOpenFromLeftEnabledProperty_ChangedCallback));

        private readonly SolidColorBrush _transparentBrush;
        private readonly Popup _contentPopup;
        private readonly Grid _popupGrid;

        private Border _rootBorder;
        private Grid _rootGrid;

        public ExtendedSettingsFlyout()
        {
            DefaultStyleKey = typeof(ExtendedSettingsFlyout);
            _contentPopup = new Popup();
            _contentPopup.Opened += ContentPopup_Opened;
            _contentPopup.Closed += ContentPopup_Closed;

            _transparentBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

            _popupGrid = new Grid
            {
                Background = _transparentBrush,
                Width = Window.Current.Bounds.Width,
                Height = Window.Current.Bounds.Height
            };

            _popupGrid.Tapped += (s, e) =>
            {
                Hide();
                e.Handled = true;
            };
            _popupGrid.RightTapped += (s, e) =>
            {
                Hide();
                e.Handled = true;
            };

            IsOpenFromLeftEnabledChanged();
        }

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Brush HeaderBackground
        {
            get => (Brush) GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        public Brush HeaderForeground
        {
            get => (Brush) GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        public ImageSource IconSource
        {
            get => (ImageSource) GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public bool IsOpenFromLeftEnabled
        {
            get => (bool) GetValue(IsOpenFromLeftEnabledProperty);
            set => SetValue(IsOpenFromLeftEnabledProperty, value);
        }

        public bool IsOpen => _contentPopup != null && _contentPopup.IsOpen;

        public void Show()
        {
            Visibility = Visibility.Visible;
            Height = Window.Current.Bounds.Height;

            _contentPopup.Child = _popupGrid;

            _contentPopup.HorizontalOffset = 0;
            _contentPopup.VerticalOffset = 0;

            _popupGrid.Width = Window.Current.Bounds.Width;
            _popupGrid.Height = Window.Current.Bounds.Height;

            Height = Window.Current.Bounds.Height;

            if (_rootBorder != null && _rootGrid != null)
                if (Width >= Window.Current.Bounds.Width)
                {
                    _rootGrid.BorderThickness = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    _rootGrid.BorderThickness = !IsOpenFromLeftEnabled ? new Thickness(1, 0, 0, 0) : new Thickness(0, 0, 1, 0);
                }

            _contentPopup.IsOpen = true;

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
            if (_contentPopup == null)
                return;

            if (_rootBorder != null && _rootGrid != null)
                if (_rootBorder.Child != null)
                    _rootBorder.Child = null;

            await Task.Delay(100);

            _contentPopup.IsOpen = false;
            _contentPopup.Child = null;

            Visibility = Visibility.Collapsed;


            if (_rootBorder != null && _rootGrid != null)
                if (_rootBorder.Child == null)
                    _rootBorder.Child = _rootGrid;
        }


        private static void IsOpenFromLeftEnabledProperty_ChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var extendedSettingsFlyout = d as ExtendedSettingsFlyout;

            extendedSettingsFlyout?.IsOpenFromLeftEnabledChanged();
        }

        public event EventHandler Showed;
        public event EventHandler Hided;
        public event BackClickEventHandler BackClick;
        public new event SizeChangedEventHandler SizeChanged;

        private void ContentPopup_Opened(object sender, object e)
        {
            Showed?.Invoke(this, EventArgs.Empty);
        }

        private void ContentPopup_Closed(object sender, object e)
        {
            Hided?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var backButton = GetTemplateChild("BackButton") as Button;
            _rootGrid = GetTemplateChild("RootGrid") as Grid;
            _rootBorder = GetTemplateChild("RootBorder") as Border;

            backButton.Click += BackButton_Click;
            Window.Current.SizeChanged += Window_SizeChanged;

            _rootBorder.Tapped += (s, e) => e.Handled = true;
            _rootBorder.RightTapped += (s, e) => e.Handled = true;

            _rootBorder.ChildTransitions.Add(!IsOpenFromLeftEnabled
                ? new EdgeUIThemeTransition {Edge = EdgeTransitionLocation.Right}
                : new EdgeUIThemeTransition {Edge = EdgeTransitionLocation.Left});

            if (Width >= Window.Current.Bounds.Width)
            {
                _rootGrid.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                _rootGrid.BorderThickness = !IsOpenFromLeftEnabled ? new Thickness(1, 0, 0, 0) : new Thickness(0, 0, 1, 0);
            }
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Height = Window.Current.Bounds.Height;

            SizeChanged?.Invoke(this, null);

            if (!_contentPopup.IsOpen)
                return;

            _contentPopup.HorizontalOffset = 0;
            _contentPopup.VerticalOffset = 0;

            _popupGrid.Width = Window.Current.Bounds.Width;
            _popupGrid.Height = Window.Current.Bounds.Height;

            Height = Window.Current.Bounds.Height;

            if (Width >= Window.Current.Bounds.Width)
            {
                _rootGrid.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                _rootGrid.BorderThickness = !IsOpenFromLeftEnabled ? new Thickness(1, 0, 0, 0) : new Thickness(0, 0, 1, 0);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            var backClickEventArgs = new BackClickEventArgs {Handled = false};

            BackClick?.Invoke(sender, backClickEventArgs);
        }

        public void IsOpenFromLeftEnabledChanged()
        {
            _popupGrid.Children.Clear();

            _popupGrid.ColumnDefinitions.Clear();

            if (!IsOpenFromLeftEnabled)
            {
                _popupGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
                _popupGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(0, GridUnitType.Auto)});

                SetValue(Grid.ColumnProperty, 1);
            }
            else
            {
                _popupGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(0, GridUnitType.Auto)});
                _popupGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});

                SetValue(Grid.ColumnProperty, 0);
            }

            if (_rootBorder != null && _rootGrid != null)
                if (!IsOpenFromLeftEnabled)
                {
                    _rootGrid.BorderThickness = new Thickness(1, 0, 0, 0);
                    _rootBorder.ChildTransitions.Add(new EdgeUIThemeTransition {Edge = EdgeTransitionLocation.Right});
                }
                else
                {
                    _rootGrid.BorderThickness = new Thickness(0, 0, 1, 0);
                    _rootBorder.ChildTransitions.Add(new EdgeUIThemeTransition {Edge = EdgeTransitionLocation.Left});
                }

            _popupGrid.Children.Add(this);
        }
    }
}