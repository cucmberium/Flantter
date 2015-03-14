using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.Views.Util
{
    public class WindowSizeHelper : BindableBase
    {
        private static WindowSizeHelper _Instance = new WindowSizeHelper();
        public static WindowSizeHelper Instance
        {
            get { return _Instance; }
        }

        private WindowSizeHelper()
        {
#if WINDOWS_PHONE_APP
            this.WindowWidth = Window.Current.Bounds.Width;
            this.WindowHeight = Window.Current.Bounds.Height;
            Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                    h => (sender, e) => h(e),
                    h => Window.Current.SizeChanged += h,
                    h => Window.Current.SizeChanged -= h).Subscribe(x =>
                    {
                        this.WindowHeight = x.Size.Height;
                        this.WindowWidth = x.Size.Width;
                    });

            this.ClientWidth = (((Window.Current.Content as Frame).Content as Page).Content as Grid).ActualHeight;
            this.ClientHeight = (((Window.Current.Content as Frame).Content as Page).Content as Grid).ActualHeight;
            Observable.FromEvent<SizeChangedEventHandler, SizeChangedEventArgs>(
                    h => (sender, e) => h(e),
                    h => (((Window.Current.Content as Frame).Content as Page).Content as Grid).SizeChanged += h,
                    h => (((Window.Current.Content as Frame).Content as Page).Content as Grid).SizeChanged -= h).Subscribe(x =>
                    {
                        this.ClientHeight = x.NewSize.Height;
                        this.ClientWidth = x.NewSize.Width;
                    });
#elif WINDOWS_APP
            this.WindowWidth = Window.Current.Bounds.Width;
            this.WindowHeight = Window.Current.Bounds.Height;
            this.ClientWidth = Window.Current.Bounds.Width;
            this.ClientHeight = Window.Current.Bounds.Height;

            Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                    h => (sender, e) => h(e),
                    h => Window.Current.SizeChanged += h,
                    h => Window.Current.SizeChanged -= h).Subscribe(x =>
                    {
                        this.WindowHeight = x.Size.Height;
                        this.WindowWidth = x.Size.Width;
                        this.ClientHeight = x.Size.Height;
                        this.ClientWidth = x.Size.Width;
                    });
#endif
        }

        private double _ClientWidth;
        public double ClientWidth 
        {
            get { return this._ClientWidth; }
            set { this.SetProperty(ref this._ClientWidth, value); }
        }

        private double _ClientHeight;
        public double ClientHeight
        {
            get { return this._ClientHeight; }
            set { this.SetProperty(ref this._ClientHeight, value); }
        }

        private double _WindowWidth;
        public double WindowWidth
        {
            get { return this._WindowWidth; }
            set { this.SetProperty(ref this._WindowWidth, value); }
        }

        private double _WindowHeight;
        public double WindowHeight
        {
            get { return this._WindowHeight; }
            set { this.SetProperty(ref this._WindowHeight, value); }
        }
    }
}
