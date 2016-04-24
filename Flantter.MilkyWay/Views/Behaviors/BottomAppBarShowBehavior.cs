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

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class BottomAppBarShowBehavior : DependencyObject, IBehavior
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
            ((UIElement)this.AssociatedObject).PointerPressed += BottomAppBarShowBehavior_PointerPressed; ;
            ((UIElement)this.AssociatedObject).PointerReleased += BottomAppBarShowBehavior_PointerReleased; ;

            var page = this.AssociatedObject as Page;
            if (page == null)
                return;

            page.Tag = this.BottomAppBar;
        }

        private bool _rightMouseButtonPressed;
        private void BottomAppBarShowBehavior_PointerReleased(object sender, PointerRoutedEventArgs e)
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

        private void BottomAppBarShowBehavior_PointerPressed(object sender, PointerRoutedEventArgs e)
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
        
        public AppBar BottomAppBar
        {
            get { return (AppBar)GetValue(BottomAppBarProperty); }
            set { SetValue(BottomAppBarProperty, value); }
        }

        public static readonly DependencyProperty BottomAppBarProperty =
            DependencyProperty.Register(
                "BottomAppBar",
                typeof(AppBar),
                typeof(BottomAppBarShowBehavior),
                new PropertyMetadata(null, BottomAppBarChanged));

        private static void BottomAppBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as BottomAppBarShowBehavior;

            var oldBottomAppBar = e.OldValue as AppBar;
            if (oldBottomAppBar != null)
            {
                oldBottomAppBar.Closed -= behavior.BottomAppBar_Closed;
                oldBottomAppBar.Opened -= behavior.BottomAppBar_Opened;
                oldBottomAppBar.Opening -= behavior.BottomAppBar_Opening;
                oldBottomAppBar.Closing -= behavior.BottomAppBar_Closing;
            }
            var newBottomAppBar = e.NewValue as AppBar;
            if (newBottomAppBar != null)
            {
                newBottomAppBar.Closed += behavior.BottomAppBar_Closed;
                newBottomAppBar.Opened += behavior.BottomAppBar_Opened;
                newBottomAppBar.Opening -= behavior.BottomAppBar_Opening;
                newBottomAppBar.Closing -= behavior.BottomAppBar_Closing;

                var page = behavior.AssociatedObject as Page;
                if (page == null)
                    return;

                page.Tag = newBottomAppBar;
            }
        }

        private bool appBarIsOpenChanging = false;

        private void BottomAppBar_Closing(object sender, object e)
        {
            appBarIsOpenChanging = true;
        }

        private void BottomAppBar_Opening(object sender, object e)
        {
            appBarIsOpenChanging = true;
        }

        private void BottomAppBar_Opened(object sender, object e)
        {
            appBarIsOpenChanging = false;
            this.IsOpen = true;
        }

        private void BottomAppBar_Closed(object sender, object e)
        {
            appBarIsOpenChanging = false;
            this.IsOpen = false;

            var page = this.AssociatedObject as Page;
            if (page == null)
                return;

            page.BottomAppBar = null;
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(BottomAppBarShowBehavior), new PropertyMetadata(false, IsOpenChanged));
        
        private static async void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as BottomAppBarShowBehavior;
            var page = behavior.AssociatedObject as Page;
            if (page == null)
                return;

            if (behavior.appBarIsOpenChanging)
                return;

            if (behavior.BottomAppBar.IsOpen == (bool)e.NewValue)
                return;

            var isOpen = (bool)e.NewValue;
            if (isOpen)
            {
                page.BottomAppBar = behavior.BottomAppBar;
                await Task.Delay(10);
                page.BottomAppBar.IsOpen = true;
            }
            else
            {
                behavior.BottomAppBar.IsOpen = false;
                await Task.Delay(500);
                page.BottomAppBar = null;
            }
        }
    }
}
