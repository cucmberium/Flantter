using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Core;
using Windows.Devices.Input;
using Flantter.MilkyWay.Views.Contents;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class AppBarShowBehavior : DependencyObject, IBehavior
    {
        private DependencyObject _AssociatedObject;
        public DependencyObject AssociatedObject
        {
            get { return this._AssociatedObject; }
            set { this._AssociatedObject = value; }
        }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;
            ((UIElement)this.AssociatedObject).PointerPressed += AppBarShowBehavior_PointerPressed; ;
            ((UIElement)this.AssociatedObject).PointerReleased += AppBarShowBehavior_PointerReleased; ;

            var page = this.AssociatedObject as Page;
            if (page == null)
                return;

            page.Tag = this.AppBar;
        }

        private bool _rightMouseButtonPressed;
        private void AppBarShowBehavior_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(((UIElement)this.AssociatedObject)).PointerDevice.PointerDeviceType == PointerDeviceType.Mouse &&
                !e.GetCurrentPoint(((UIElement)this.AssociatedObject)).Properties.IsLeftButtonPressed &&
                !e.GetCurrentPoint(((UIElement)this.AssociatedObject)).Properties.IsMiddleButtonPressed &&
                _rightMouseButtonPressed &&
                !e.Handled)
            {
                if (!appBarIsOpenChanging)
                {
                    if (this.IsOpen)
                        this.IsOpen = false;
                    else
                        this.IsOpen = true;
                }

                e.Handled = true;
                _rightMouseButtonPressed = false;
            }
        }

        private void AppBarShowBehavior_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.GetCurrentPoint(((UIElement)this.AssociatedObject)).PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                _rightMouseButtonPressed =
                    e.GetCurrentPoint(((UIElement)this.AssociatedObject)).Properties.IsRightButtonPressed &&
                    !e.GetCurrentPoint(((UIElement)this.AssociatedObject)).Properties.IsLeftButtonPressed &&
                    !e.GetCurrentPoint(((UIElement)this.AssociatedObject)).Properties.IsMiddleButtonPressed;

                if (_rightMouseButtonPressed)
                {
                    e.Handled = true;
                }
            }
        }

        public void Detach()
        {
        }

        public void AppBarLayoutRefresh()
        {
            ((this.AppBar.Content as TweetArea).Content as Grid).BorderThickness = this.IsTopAppBar ? new Thickness(0, 0, 0, 2) : new Thickness(0, 2, 0, 0);
            this.AppBar.Margin = this.IsTopAppBar ? new Thickness(0, WindowSizeHelper.Instance.StatusBarHeight, 0, 0) : new Thickness();
        }

        public AppBar AppBar
        {
            get { return (AppBar)GetValue(AppBarProperty); }
            set { SetValue(AppBarProperty, value); }
        }

        public static readonly DependencyProperty AppBarProperty =
            DependencyProperty.Register(
                "AppBar",
                typeof(AppBar),
                typeof(AppBarShowBehavior),
                new PropertyMetadata(null, AppBarChanged));

        public bool IsTopAppBar
        {
            get { return (bool)GetValue(IsTopAppBarProperty); }
            set { SetValue(IsTopAppBarProperty, value); }
        }

        public static readonly DependencyProperty IsTopAppBarProperty =
            DependencyProperty.Register(
                "IsTopAppBar",
                typeof(bool),
                typeof(AppBarShowBehavior),
                new PropertyMetadata(false, IsTopAppBarChanged));

        private static void IsTopAppBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AppBarShowBehavior;
            behavior.AppBarLayoutRefresh();
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

        private bool appBarIsOpenChanging = false;

        private void AppBar_Closing(object sender, object e)
        {
            appBarIsOpenChanging = true;
        }

        private void AppBar_Opening(object sender, object e)
        {
            appBarIsOpenChanging = true;
        }

        private void AppBar_Opened(object sender, object e)
        {
            appBarIsOpenChanging = false;
            this.IsOpen = true;
        }

        private void AppBar_Closed(object sender, object e)
        {
            appBarIsOpenChanging = false;
            this.IsOpen = false;

            var page = this.AssociatedObject as Page;
            if (page == null)
                return;

            page.BottomAppBar = null;
            page.TopAppBar = null;
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(AppBarShowBehavior), new PropertyMetadata(false, IsOpenChanged));
        
        private static async void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AppBarShowBehavior;
            var page = behavior.AssociatedObject as Page;
            if (page == null)
                return;

            if (behavior.appBarIsOpenChanging)
                return;

            if (behavior.AppBar.IsOpen == (bool)e.NewValue)
                return;

            behavior.AppBarLayoutRefresh();

            var isOpen = (bool)e.NewValue;
            if (behavior.IsTopAppBar)
            {
                if (isOpen)
                {
                    page.TopAppBar = behavior.AppBar;
                    await Task.Delay(10);
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
                    await Task.Delay(10);
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
