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
            Window.Current.CoreWindow.PointerPressed += OnCoreWindowPointerPressed;
            Window.Current.CoreWindow.PointerReleased += OnCoreWindowPointerReleased;

            var page = this.AssociatedObject as Page;
            if (page == null)
                return;

            page.Tag = this.BottomAppBar;
        }
        
        private bool _rightMouseButtonPressed;
        private void OnCoreWindowPointerReleased(CoreWindow sender, PointerEventArgs args)
        {
            if (args.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse &&
                !args.CurrentPoint.Properties.IsLeftButtonPressed &&
                !args.CurrentPoint.Properties.IsMiddleButtonPressed &&
                _rightMouseButtonPressed)
            {
                if (this.IsOpen)
                    this.BottomAppBar.IsOpen = false;
                else
                    this.IsOpen = true;

                args.Handled = true;
                _rightMouseButtonPressed = false;
            }

        }

        private void OnCoreWindowPointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            if (args.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                _rightMouseButtonPressed =
                    args.CurrentPoint.Properties.IsRightButtonPressed &&
                    !args.CurrentPoint.Properties.IsLeftButtonPressed &&
                    !args.CurrentPoint.Properties.IsMiddleButtonPressed;

                if (_rightMouseButtonPressed)
                {
                    args.Handled = true;
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
                oldBottomAppBar.Opening -= behavior.BottomAppBar_Opened;
            }
            var newBottomAppBar = e.NewValue as AppBar;
            if (newBottomAppBar != null)
            {
                newBottomAppBar.Closed += behavior.BottomAppBar_Closed;
                newBottomAppBar.Opening += behavior.BottomAppBar_Opened;

                var page = behavior.AssociatedObject as Page;
                if (page == null)
                    return;

                page.Tag = newBottomAppBar;
            }
        }

        private void BottomAppBar_Opened(object sender, object e)
        {
            this.IsOpen = true;
        }

        private void BottomAppBar_Closed(object sender, object e)
        {
            this.IsOpen = false;
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

            var isOpen = (bool)e.NewValue;
            if (isOpen)
            {
                page.BottomAppBar = behavior.BottomAppBar;

                await Task.Delay(10);

                page.BottomAppBar.IsOpen = true;
            }
            else
            {
                page.BottomAppBar.IsOpen = false;

                await Task.Delay(500);

                page.BottomAppBar = null;
            }
        }
    }
}
