using System;
using System.Numerics;
using System.Reactive.Linq;
using Windows.ApplicationModel.Background;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Themes;
using Flantter.MilkyWay.ViewModels;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Prism.Windows.Mvvm;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.Views
{
    public sealed partial class MainPage : SessionStateAwarePage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainPageViewModel), typeof(MainPage), null);

        private readonly Compositor _compositor;
        private SpriteVisual _hostSprite;

        public MainPage()
        {
            InitializeComponent();

            var version = (ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion) & 0x00000000FFFF0000L) >> 16;
            if (version >= 15063)
            {
                _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

                Loaded += (s, e) =>
                {
                    _hostSprite = _compositor.CreateSpriteVisual();
                    UpdateBackgroundBrush(SettingService.Setting.UseTransparentBackground);
                };
            }
                
            ThemeService.Theme.ChangeTheme();

            UpdateTitleBar(WindowSizeHelper.Instance.StatusBarHeight > 0);
            WindowSizeHelper.Instance.ObserveProperty(x => x.StatusBarHeight)
                .SubscribeOnUIDispatcher()
                .Subscribe(x => UpdateTitleBar(x > 0));
            SettingService.Setting.ObserveProperty(x => x.UseTransparentBackground)
                .SubscribeOnUIDispatcher()
                .Subscribe(x => UpdateBackgroundBrush(x));
            WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth)
                .Merge(WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight)
                )
                .SubscribeOnUIDispatcher()
                .Subscribe(x => UpdateBackgroundBrushSizeChange());

            /*SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                var behavior = Flantter.MilkyWay.Views.Behaviors.ShowSettingsFlyoutAction.GetForCurrentView();
                if (behavior == null)
                    return;

                if (behavior.ShowingPopupCount == 0)
                    return;

                e.Handled = true;
                behavior.HideTopPopup();
            };*/

            Window.Current.CoreWindow.PointerPressed += (s, e) =>
            {
                var behavior = ShowSettingsFlyoutAction.GetForCurrentView();

                var backPressed = e.CurrentPoint.Properties.IsXButton1Pressed;
                var nextPressed = e.CurrentPoint.Properties.IsXButton2Pressed;

                if (behavior != null && behavior.ShowingPopupCount != 0)
                {
                    if (backPressed)
                        behavior.HideTopPopup();

                    return;
                }

                // Taboo : 禁忌
                if (backPressed)
                    Notice.Instance.DecrementColumnSelectedIndexCommand.Execute();
                else if (nextPressed)
                    Notice.Instance.IncrementColumnSelectedIndexCommand.Execute();
            };
        }

        public MainPageViewModel ViewModel
        {
            get => (MainPageViewModel) DataContext;
            set => DataContext = value;
        }

        private void UpdateTitleBar(bool isVisible)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                return;

            var applicationView = ApplicationView.GetForCurrentView();

            if (isVisible)
            {
                applicationView.TitleBar.BackgroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonForegroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonInactiveForegroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            }
            else
            {
                applicationView.TitleBar.BackgroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonBackgroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonForegroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveBackgroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveForegroundColor =
                    ((SolidColorBrush) Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            }
        }

        private void UpdateBackgroundBrush(bool isTransparent)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                return;

            if (_hostSprite == null || _compositor == null)
                return;

            if (isTransparent)
            {
                _hostSprite.Size = new Vector2(
                    (float) FlantterHostBackgroundCanvas.ActualWidth,
                    (float) FlantterHostBackgroundCanvas.ActualHeight);
                _hostSprite.Brush = _compositor.CreateHostBackdropBrush();
                ElementCompositionPreview.SetElementChildVisual(FlantterHostBackgroundCanvas, _hostSprite);
            }
            else
            {
                ElementCompositionPreview.SetElementChildVisual(FlantterHostBackgroundCanvas, null);
            }
        }

        private void UpdateBackgroundBrushSizeChange()
        {
            if (_hostSprite != null && _compositor != null)
                _hostSprite.Size = new Vector2(
                    (float) FlantterHostBackgroundCanvas.ActualWidth,
                    (float) FlantterHostBackgroundCanvas.ActualHeight);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.BackStack.Clear();
            Frame.ForwardStack.Clear();

            if (SettingService.Setting.TileNotification == SettingSupport.TileNotificationEnum.None &&
                !SettingService.Setting.BackgroundNotification)
                return;

            await BackgroundExecutionManager.RequestAccessAsync();
        }
    }
}