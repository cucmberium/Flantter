using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
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
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                var titleBarVisiblity = false;
                if ((UserInteractionMode)((int)UIViewSettings.GetForCurrentView().UserInteractionMode) == UserInteractionMode.Mouse)
                    titleBarVisiblity = true;
                else
                    titleBarVisiblity = Setting.SettingService.Setting.ExtendTitleBar;

                this.WindowWidth = Window.Current.Bounds.Width;
                this.WindowHeight = Window.Current.Bounds.Height;
                this.ClientWidth = Window.Current.Bounds.Width;
                this.ClientHeight = Window.Current.Bounds.Height - (titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0);
                this.StatusBarHeight = (titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0);
                this.UserInteractionMode = (UserInteractionMode)((int)UIViewSettings.GetForCurrentView().UserInteractionMode);

                Observable.CombineLatest<WindowSizeChangedEventArgs, CoreApplicationViewTitleBar, bool, bool>(
                Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                    h => (sender, e) => h(e),
                    h => Window.Current.SizeChanged += h,
                    h => Window.Current.SizeChanged -= h),
                Observable.FromEvent<TypedEventHandler<CoreApplicationViewTitleBar, object>, CoreApplicationViewTitleBar>(
                    h => (sender, e) => h(sender),
                    h => CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged += h,
                    h => CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged -= h),
                Reactive.Bindings.Extensions.INotifyPropertyChangedExtensions.ObserveProperty(Setting.SettingService.Setting, x => x.ExtendTitleBar),
                (size, titleBar, extendTitleBar) => extendTitleBar).Subscribe(extendTitleBar =>
                {
                    titleBarVisiblity = false;
                    if ((UserInteractionMode)((int)UIViewSettings.GetForCurrentView().UserInteractionMode) == UserInteractionMode.Mouse)
                        titleBarVisiblity = true;
                    else
                        titleBarVisiblity = extendTitleBar;
                    
                    this.WindowWidth = Window.Current.Bounds.Width;
                    this.WindowHeight = Window.Current.Bounds.Height;
                    this.ClientWidth = Window.Current.Bounds.Width;
                    this.ClientHeight = Window.Current.Bounds.Height - (titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0);
                    this.StatusBarHeight = (titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0);
                    this.StatusBarWidth = 0.0;
                    this.UserInteractionMode = (UserInteractionMode)((int)UIViewSettings.GetForCurrentView().UserInteractionMode);
                });
            }
            else if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                var statusBarHeight = StatusBar.GetForCurrentView().OccludedRect.Height == Window.Current.Bounds.Height ? 0 : StatusBar.GetForCurrentView().OccludedRect.Height;
                var statusBarWidth = StatusBar.GetForCurrentView().OccludedRect.Width == Window.Current.Bounds.Width ? 0 : StatusBar.GetForCurrentView().OccludedRect.Width;

                this.WindowWidth = Window.Current.Bounds.Width;
                this.WindowHeight = Window.Current.Bounds.Height;
                this.ClientWidth = Window.Current.Bounds.Width - statusBarWidth;
                this.ClientHeight = Window.Current.Bounds.Height - statusBarHeight;
                this.StatusBarHeight = statusBarHeight;
                this.UserInteractionMode = UserInteractionMode.Touch;
                
                Observable.CombineLatest<WindowSizeChangedEventArgs, object, object>(
                Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                    h => (sender, e) => h(e),
                    h => Window.Current.SizeChanged += h,
                    h => Window.Current.SizeChanged -= h),
                Observable.FromEvent<TypedEventHandler<DisplayInformation, object>, object>(
                    h => (sender, e) => h(e),
                    h => Windows.Graphics.Display.DisplayInformation.GetForCurrentView().OrientationChanged += h,
                    h => Windows.Graphics.Display.DisplayInformation.GetForCurrentView().OrientationChanged -= h),
                (size, orientation) => orientation).Subscribe(x =>
                    {
                        statusBarHeight = StatusBar.GetForCurrentView().OccludedRect.Height == Window.Current.Bounds.Height ? 0 : StatusBar.GetForCurrentView().OccludedRect.Height;
                        statusBarWidth = StatusBar.GetForCurrentView().OccludedRect.Width == Window.Current.Bounds.Width ? 0 : StatusBar.GetForCurrentView().OccludedRect.Width;

                        this.WindowWidth = Window.Current.Bounds.Width;
                        this.WindowHeight = Window.Current.Bounds.Height;
                        this.ClientWidth = Window.Current.Bounds.Width - statusBarWidth;
                        this.ClientHeight = Window.Current.Bounds.Height - statusBarHeight;
                        this.StatusBarHeight = statusBarHeight;
                        this.StatusBarWidth = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().CurrentOrientation == DisplayOrientations.Landscape ? statusBarWidth : 0.0;
                        this.UserInteractionMode = UserInteractionMode.Touch;
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

        private double _StatusBarHeight;
        public double StatusBarHeight
        {
            get { return this._StatusBarHeight; }
            set { this.SetProperty(ref this._StatusBarHeight, value); }
        }

        private double _StatusBarWidth;
        public double StatusBarWidth
        {
            get { return this._StatusBarWidth; }
            set { this.SetProperty(ref this._StatusBarWidth, value); }
        }

        private UserInteractionMode _UserInteractionMode;
        public UserInteractionMode UserInteractionMode
        {
            get { return this._UserInteractionMode; }
            set { this.SetProperty(ref this._UserInteractionMode, value); }
        }
    }

    public enum UserInteractionMode
    {
        Mouse = 0,
        Touch = 1
    }
}
