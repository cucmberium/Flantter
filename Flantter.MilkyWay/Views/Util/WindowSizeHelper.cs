using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
            /*
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
            */


            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                this.WindowWidth = Window.Current.Bounds.Width;
                this.WindowHeight = Window.Current.Bounds.Height;
                this.ClientWidth = Window.Current.Bounds.Width;
                this.ClientHeight = Window.Current.Bounds.Height;
                this.TitleBarHeight = CoreApplication.GetCurrentView().TitleBar.Height;

                Observable.CombineLatest<WindowSizeChangedEventArgs, CoreApplicationViewTitleBar, bool>(
                Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                    h => (sender, e) => h(e),
                    h => Window.Current.SizeChanged += h,
                    h => Window.Current.SizeChanged -= h),
                Observable.FromEvent<TypedEventHandler<CoreApplicationViewTitleBar, object>, CoreApplicationViewTitleBar>(
                    h => (sender, e) => h(sender),
                    h => CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged += h,
                    h => CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged -= h),
                (size, titleBar) => titleBar.IsVisible).Subscribe(x =>
                {
                    if (x)
                    {
                        var applicationView = ApplicationView.GetForCurrentView();
                        this.WindowWidth = Window.Current.Bounds.Width;
                        this.WindowHeight = Window.Current.Bounds.Height;
                        this.ClientWidth = Window.Current.Bounds.Width;
                        this.ClientHeight = Window.Current.Bounds.Height - CoreApplication.GetCurrentView().TitleBar.Height;
                        this.TitleBarHeight = CoreApplication.GetCurrentView().TitleBar.Height;
                    }
                    else
                    {
                        this.WindowWidth = Window.Current.Bounds.Width;
                        this.WindowHeight = Window.Current.Bounds.Height;
                        this.ClientWidth = Window.Current.Bounds.Width;
                        this.ClientHeight = Window.Current.Bounds.Height;
                        this._TitleBarHeight = 0.0;
                    }
                });
            }
            else if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                // Todo : 電話での動作の修正

                var statusBarHeight = StatusBar.GetForCurrentView().OccludedRect.Height;

                this.WindowWidth = Window.Current.Bounds.Width;
                this.WindowHeight = Window.Current.Bounds.Height;
                this.ClientWidth = Window.Current.Bounds.Width;
                this.ClientHeight = Window.Current.Bounds.Height - statusBarHeight;
                this.TitleBarHeight = statusBarHeight;

                Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                    h => (sender, e) => h(e),
                    h => Window.Current.SizeChanged += h,
                    h => Window.Current.SizeChanged -= h).Subscribe(x =>
                    {
                        this.WindowHeight = x.Size.Height;
                        this.WindowWidth = x.Size.Width;
                        this.ClientWidth = Window.Current.Bounds.Width;
                        this.ClientHeight = Window.Current.Bounds.Height - statusBarHeight;
                        this.TitleBarHeight = statusBarHeight;
                    });
            }
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

        private double _TitleBarHeight;
        public double TitleBarHeight
        {
            get { return this._TitleBarHeight; }
            set { this.SetProperty(ref this._TitleBarHeight, value); }
        }
    }
}
