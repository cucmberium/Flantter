using System;
using System.Reactive.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.Views.Util
{
    public class WindowSizeHelper : BindableBase
    {
        private double _clientHeight;

        private double _clientWidth;

        private double _statusBarHeight;

        private double _statusBarWidth;

        private UserInteractionMode _userInteractionMode;

        private double _windowHeight;

        private double _windowWidth;

        private WindowSizeHelper()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                bool titleBarVisiblity;
                titleBarVisiblity =
                    (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode ==
                    UserInteractionMode.Mouse || SettingService.Setting.ExtendTitleBar;

                WindowWidth = Window.Current.Bounds.Width;
                WindowHeight = Window.Current.Bounds.Height;
                ClientWidth = Window.Current.Bounds.Width;
                ClientHeight = Window.Current.Bounds.Height -
                               (titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0);
                StatusBarHeight = titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0;
                StatusBarWidth = 0.0;
                UserInteractionMode =
                    (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode;

                Observable.Merge(
                        Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                                h => (sender, e) => h(e),
                                h => Window.Current.SizeChanged += h,
                                h => Window.Current.SizeChanged -= h)
                            .Select(e => (object) e),
                        Observable
                            .FromEvent<TypedEventHandler<CoreApplicationViewTitleBar, object>,
                                CoreApplicationViewTitleBar>(
                                h => (sender, e) => h(sender),
                                h => CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged += h,
                                h => CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged -= h)
                            .Select(e => (object) e),
                        SettingService.Setting.ObserveProperty(x => x.ExtendTitleBar)
                            .Select(e => (object) e)
                    )
                    .Subscribe(_ =>
                    {
                        titleBarVisiblity = false;
                        titleBarVisiblity =
                            (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode ==
                            UserInteractionMode.Mouse || SettingService.Setting.ExtendTitleBar;

                        WindowWidth = Window.Current.Bounds.Width;
                        WindowHeight = Window.Current.Bounds.Height;
                        ClientWidth = Window.Current.Bounds.Width;
                        ClientHeight = Window.Current.Bounds.Height -
                                       (titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0);
                        StatusBarHeight = titleBarVisiblity ? CoreApplication.GetCurrentView().TitleBar.Height : 0;
                        StatusBarWidth = 0.0;
                        UserInteractionMode =
                            (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode;
                    });
            }
            else if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                var statusBarHeight = StatusBar.GetForCurrentView().OccludedRect.Height == Window.Current.Bounds.Height
                    ? 0
                    : StatusBar.GetForCurrentView().OccludedRect.Height;
                var statusBarWidth = StatusBar.GetForCurrentView().OccludedRect.Width == Window.Current.Bounds.Width
                    ? 0
                    : StatusBar.GetForCurrentView().OccludedRect.Width;

                WindowWidth = Window.Current.Bounds.Width;
                WindowHeight = Window.Current.Bounds.Height;
                ClientWidth = Window.Current.Bounds.Width - statusBarWidth;
                ClientHeight = Window.Current.Bounds.Height - statusBarHeight;
                StatusBarHeight = statusBarHeight;
                StatusBarWidth = DisplayInformation.GetForCurrentView().CurrentOrientation ==
                                 DisplayOrientations.Landscape
                    ? statusBarWidth
                    : 0.0;
                UserInteractionMode = UserInteractionMode.Touch;

                Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                        h => (sender, e) => h(e),
                        h => Window.Current.SizeChanged += h,
                        h => Window.Current.SizeChanged -= h)
                    .Select(e => (object) e)
                    .Merge(Observable.FromEvent<TypedEventHandler<DisplayInformation, object>, object>(
                            h => (sender, e) => h(e),
                            h => DisplayInformation.GetForCurrentView().OrientationChanged += h,
                            h => DisplayInformation.GetForCurrentView().OrientationChanged -= h)
                        .Select(e => e))
                    .Subscribe(_ =>
                    {
                        statusBarHeight =
                            StatusBar.GetForCurrentView().OccludedRect.Height == Window.Current.Bounds.Height
                                ? 0
                                : StatusBar.GetForCurrentView().OccludedRect.Height;
                        statusBarWidth = StatusBar.GetForCurrentView().OccludedRect.Width == Window.Current.Bounds.Width
                            ? 0
                            : StatusBar.GetForCurrentView().OccludedRect.Width;

                        WindowWidth = Window.Current.Bounds.Width;
                        WindowHeight = Window.Current.Bounds.Height;
                        ClientWidth = Window.Current.Bounds.Width - statusBarWidth;
                        ClientHeight = Window.Current.Bounds.Height - statusBarHeight;
                        StatusBarHeight = statusBarHeight;
                        StatusBarWidth = DisplayInformation.GetForCurrentView().CurrentOrientation ==
                                         DisplayOrientations.Landscape
                            ? statusBarWidth
                            : 0.0;
                        UserInteractionMode = UserInteractionMode.Touch;
                    });
            }
        }

        public static WindowSizeHelper Instance { get; } = new WindowSizeHelper();

        public double ClientWidth
        {
            get => _clientWidth;
            set => SetProperty(ref _clientWidth, value);
        }

        public double ClientHeight
        {
            get => _clientHeight;
            set => SetProperty(ref _clientHeight, value);
        }

        public double WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        public double WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        public double StatusBarHeight
        {
            get => _statusBarHeight;
            set => SetProperty(ref _statusBarHeight, value);
        }

        public double StatusBarWidth
        {
            get => _statusBarWidth;
            set => SetProperty(ref _statusBarWidth, value);
        }

        public UserInteractionMode UserInteractionMode
        {
            get => _userInteractionMode;
            set => SetProperty(ref _userInteractionMode, value);
        }
    }

    public enum UserInteractionMode
    {
        Mouse = 0,
        Touch = 1
    }
}