using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Flantter.MilkyWay.Views.Contents;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class AppBarShowBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty AppBarProperty =
            DependencyProperty.Register(
                "AppBar",
                typeof(AppBar),
                typeof(AppBarShowBehavior),
                new PropertyMetadata(null, AppBarChanged));

        public static readonly DependencyProperty IsTopAppBarProperty =
            DependencyProperty.Register(
                "IsTopAppBar",
                typeof(bool),
                typeof(AppBarShowBehavior),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool?), typeof(AppBarShowBehavior),
                new PropertyMetadata(null, IsOpenChanged));

        private bool _rightMouseButtonPressed;

        private bool _appBarIsOpenChanging;

        public AppBar AppBar
        {
            get => (AppBar) GetValue(AppBarProperty);
            set => SetValue(AppBarProperty, value);
        }

        public bool? IsTopAppBar
        {
            get => (bool?) GetValue(IsTopAppBarProperty);
            set => SetValue(IsTopAppBarProperty, value);
        }

        public bool IsOpen
        {
            get => (bool) GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;
            ((UIElement) this.AssociatedObject).PointerPressed += AppBarShowBehavior_PointerPressed;
            ((UIElement) this.AssociatedObject).PointerReleased += AppBarShowBehavior_PointerReleased;

            var page = this.AssociatedObject as Page;
            if (page == null)
                return;

            page.Tag = AppBar;
        }

        public void Detach()
        {
        }

        private void AppBarShowBehavior_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint((UIElement) AssociatedObject).PointerDevice.PointerDeviceType ==
                PointerDeviceType.Mouse &&
                !e.GetCurrentPoint((UIElement) AssociatedObject).Properties.IsLeftButtonPressed &&
                !e.GetCurrentPoint((UIElement) AssociatedObject).Properties.IsMiddleButtonPressed &&
                _rightMouseButtonPressed &&
                !e.Handled)
            {
                if (!_appBarIsOpenChanging)
                    IsOpen = !IsOpen;

                e.Handled = true;
                _rightMouseButtonPressed = false;
            }
        }

        private void AppBarShowBehavior_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.GetCurrentPoint((UIElement) AssociatedObject).PointerDevice.PointerDeviceType ==
                PointerDeviceType.Mouse)
            {
                _rightMouseButtonPressed =
                    e.GetCurrentPoint((UIElement) AssociatedObject).Properties.IsRightButtonPressed &&
                    !e.GetCurrentPoint((UIElement) AssociatedObject).Properties.IsLeftButtonPressed &&
                    !e.GetCurrentPoint((UIElement) AssociatedObject).Properties.IsMiddleButtonPressed;

                if (_rightMouseButtonPressed)
                    e.Handled = true;
            }
        }

        public void AppBarLayoutRefresh()
        {
            // TODO : 禁忌
            if (IsTopAppBar == null)
                IsTopAppBar = Setting.SettingService.Setting.ShowAppBarToTop;

            ((AppBar.Content as TweetArea).Content as Grid).BorderThickness =
                IsTopAppBar.Value ? new Thickness(0, 0, 0, 2) : new Thickness(0, 2, 0, 0);
            AppBar.Margin = IsTopAppBar.Value
                ? new Thickness(0, WindowSizeHelper.Instance.StatusBarHeight, 0, 0)
                : new Thickness();
        }

        private static void AppBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AppBarShowBehavior;

            var oldAppBar = e.OldValue as AppBar;
            if (oldAppBar != null)
            {
                oldAppBar.Closed -= behavior.AppBar_Closed;
                oldAppBar.Opened -= behavior.AppBar_Opened;
                oldAppBar.Opening -= behavior.AppBar_Opening;
                oldAppBar.Closing -= behavior.AppBar_Closing;
            }
            var newAppBar = e.NewValue as AppBar;
            if (newAppBar != null)
            {
                newAppBar.Closed += behavior.AppBar_Closed;
                newAppBar.Opened += behavior.AppBar_Opened;
                newAppBar.Opening -= behavior.AppBar_Opening;
                newAppBar.Closing -= behavior.AppBar_Closing;

                behavior.AppBarLayoutRefresh();

                var page = behavior.AssociatedObject as Page;
                if (page == null)
                    return;

                page.Tag = newAppBar;
            }
        }

        private void AppBar_Closing(object sender, object e)
        {
            _appBarIsOpenChanging = true;
        }

        private void AppBar_Opening(object sender, object e)
        {
            _appBarIsOpenChanging = true;
        }

        private void AppBar_Opened(object sender, object e)
        {
            _appBarIsOpenChanging = false;
            IsOpen = true;
        }

        private void AppBar_Closed(object sender, object e)
        {
            _appBarIsOpenChanging = false;
            IsOpen = false;

            var page = AssociatedObject as Page;
            if (page == null)
                return;

            page.BottomAppBar = null;
            page.TopAppBar = null;
        }

        private static async void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AppBarShowBehavior;
            var page = behavior.AssociatedObject as Page;
            if (page == null)
                return;

            if (behavior._appBarIsOpenChanging)
                return;

            if (behavior.AppBar.IsOpen == (bool) e.NewValue)
                return;

            behavior.AppBarLayoutRefresh();

            var isOpen = (bool) e.NewValue;
            if (behavior.IsTopAppBar.Value)
            {
                if (isOpen)
                {
                    page.TopAppBar = behavior.AppBar;
                    await Task.Delay(20);
                    page.TopAppBar.IsOpen = true;
                }
                else
                {
                    behavior.AppBar.IsOpen = false;
                    await Task.Delay(500);
                    page.TopAppBar = null;
                }
            }
            else
            {
                if (isOpen)
                {
                    page.BottomAppBar = behavior.AppBar;
                    await Task.Delay(20);
                    page.BottomAppBar.IsOpen = true;
                }
                else
                {
                    behavior.AppBar.IsOpen = false;
                    await Task.Delay(500);
                    page.BottomAppBar = null;
                }
            }
        }
    }
}