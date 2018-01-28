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

        private UserInteractionMode _userInteractionMode;

        private double _windowHeight;

        private double _windowWidth;

        private Rect _visibleBounds;

        private bool _titleBarVisibility;

        private WindowSizeHelper()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                TitleBarVisibility =
                    (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode ==
                    UserInteractionMode.Mouse || SettingService.Setting.ExtendTitleBar;
                WindowWidth = Window.Current.Bounds.Width;
                WindowHeight = Window.Current.Bounds.Height;
                ClientWidth = Window.Current.Bounds.Width;
                ClientHeight = Window.Current.Bounds.Height -
                               (TitleBarVisibility ? 32.0 : 0);
                UserInteractionMode =
                    (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode;
                VisibleBounds = new Rect(0, TitleBarVisibility ? 32.0 : 0, ClientWidth, ClientHeight);

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
                                h => CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged -= h),
                        Observable.FromEvent<TypedEventHandler<ApplicationView, object>, object>(
                            h => (sender, e) => h(e),
                            h => ApplicationView.GetForCurrentView().VisibleBoundsChanged += h,
                            h => ApplicationView.GetForCurrentView().VisibleBoundsChanged -= h),
                        SettingService.Setting.ObserveProperty(x => x.ExtendTitleBar)
                            .Select(e => (object)e)
                    )
                    .Subscribe(_ =>
                    {
                        TitleBarVisibility =
                            (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode ==
                            UserInteractionMode.Mouse || SettingService.Setting.ExtendTitleBar;
                        WindowWidth = Window.Current.Bounds.Width;
                        WindowHeight = Window.Current.Bounds.Height;
                        ClientWidth = Window.Current.Bounds.Width;
                        ClientHeight = Window.Current.Bounds.Height -
                                       (TitleBarVisibility ? 32.0 : 0);
                        UserInteractionMode =
                            (UserInteractionMode) (int) UIViewSettings.GetForCurrentView().UserInteractionMode;
                        VisibleBounds = new Rect(0, TitleBarVisibility ? 32.0 : 0, ClientWidth, ClientHeight);
                    });
            }
            else if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                TitleBarVisibility = false;
                VisibleBounds = ApplicationView.GetForCurrentView().VisibleBounds;
                WindowWidth = Window.Current.Bounds.Width;
                WindowHeight = Window.Current.Bounds.Height;
                ClientWidth = VisibleBounds.Width;
                ClientHeight = VisibleBounds.Height;
                UserInteractionMode = UserInteractionMode.Touch;
                Observable.FromEvent<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                        h => (sender, e) => h(e),
                        h => Window.Current.SizeChanged += h,
                        h => Window.Current.SizeChanged -= h)
                    .Select(e => (object) e)
                    .Merge(Observable.FromEvent<TypedEventHandler<DisplayInformation, object>, object>(
                            h => (sender, e) => h(e),
                            h => DisplayInformation.GetForCurrentView().OrientationChanged += h,
                            h => DisplayInformation.GetForCurrentView().OrientationChanged -= h))
                    .Merge(Observable.FromEvent<TypedEventHandler<ApplicationView, object>, object>(
                            h => (sender, e) => h(e),
                            h => ApplicationView.GetForCurrentView().VisibleBoundsChanged += h,
                            h => ApplicationView.GetForCurrentView().VisibleBoundsChanged -= h))
                    .Subscribe(_ =>
                    {
                        VisibleBounds = ApplicationView.GetForCurrentView().VisibleBounds;
                        WindowWidth = Window.Current.Bounds.Width;
                        WindowHeight = Window.Current.Bounds.Height;
                        ClientWidth = VisibleBounds.Width;
                        ClientHeight = VisibleBounds.Height;
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

        public Rect VisibleBounds
        {
            get => _visibleBounds;
            set => SetProperty(ref _visibleBounds, value);
        }

        public UserInteractionMode UserInteractionMode
        {
            get => _userInteractionMode;
            set => SetProperty(ref _userInteractionMode, value);
        }

        public bool TitleBarVisibility
        {
            get => _titleBarVisibility;
            set => SetProperty(ref _titleBarVisibility, value);
        }
    }

    public enum UserInteractionMode
    {
        Mouse = 0,
        Touch = 1
    }
}