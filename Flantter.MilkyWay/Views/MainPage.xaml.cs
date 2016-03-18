using Flantter.MilkyWay.ViewModels;
using Flantter.MilkyWay.Views.Util;
using Prism.Windows.Mvvm;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Flantter.MilkyWay.Views
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : SessionStateAwarePage
    {
        public MainPageViewModel ViewModel
        {
            get { return (MainPageViewModel)this.DataContext; }
            set { this.DataContext = value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainPageViewModel), typeof(MainPage), null);

        public MainPage()
        {
            this.InitializeComponent();

            Themes.ThemeService.Theme.PropertyChanged += Theme_PropertyChanged;
            Setting.SettingService.Setting.PropertyChanged += Setting_PropertyChanged;
            WindowSizeHelper.Instance.PropertyChanged += WindowSizeHelper_PropertyChanged;

            var titleBarVisiblity = false;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                titleBarVisiblity = false;
            else if (WindowSizeHelper.Instance.UserIntaractionMode == Util.UserInteractionMode.Mouse)
                titleBarVisiblity = true;
            else
                titleBarVisiblity = Setting.SettingService.Setting.ExtendTitleBar;

            this.UpdateTitleBar(titleBarVisiblity);
        }

        private void WindowSizeHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UserIntaractionMode")
            {
                var titleBarVisiblity = false;
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    titleBarVisiblity = false;
                else if (WindowSizeHelper.Instance.UserIntaractionMode == Util.UserInteractionMode.Mouse)
                    titleBarVisiblity = true;
                else
                    titleBarVisiblity = Setting.SettingService.Setting.ExtendTitleBar;

                this.UpdateTitleBar(titleBarVisiblity);
            }
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExtendTitleBar")
            {
                var titleBarVisiblity = false;
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    titleBarVisiblity = false;
                else if (WindowSizeHelper.Instance.UserIntaractionMode == Util.UserInteractionMode.Mouse)
                    titleBarVisiblity = true;
                else
                    titleBarVisiblity = Setting.SettingService.Setting.ExtendTitleBar;

                this.UpdateTitleBar(titleBarVisiblity);
            }
        }

        private void Theme_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var titleBarVisiblity = false;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                titleBarVisiblity = false;
            else if (WindowSizeHelper.Instance.UserIntaractionMode == Util.UserInteractionMode.Mouse)
                titleBarVisiblity = true;
            else
                titleBarVisiblity = Setting.SettingService.Setting.ExtendTitleBar;

            this.UpdateTitleBar(titleBarVisiblity);
        }

        private void UpdateTitleBar(bool isVisible)
        {
            var applicationView = ApplicationView.GetForCurrentView();

            if (isVisible)
            {
                applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonInactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            }
            else
            {
                applicationView.TitleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Frame.BackStack.Clear();
            this.Frame.ForwardStack.Clear();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
    }
}
