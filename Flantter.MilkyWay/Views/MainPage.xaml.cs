using Flantter.MilkyWay.Setting;
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
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
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

            Themes.ThemeService.Theme.ChangeTheme();
            
            this.UpdateTitleBar(ViewModels.Services.LayoutHelper.Instance.TitleBarHeight.Value > 0);
            ViewModels.Services.LayoutHelper.Instance.TitleBarHeight.SubscribeOnUIDispatcher().Subscribe(x => this.UpdateTitleBar(x > 0));
        }
        
        private void UpdateTitleBar(bool isVisible)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                return;

            var applicationView = ApplicationView.GetForCurrentView();

            if (isVisible)
            {
                applicationView.TitleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonInactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            }
            else
            {
                applicationView.TitleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonInactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Frame.BackStack.Clear();
            this.Frame.ForwardStack.Clear();

            if (SettingService.Setting.TileNotification == SettingSupport.TileNotificationEnum.None && !SettingService.Setting.BackgroundNotification)
                return;

            await BackgroundExecutionManager.RequestAccessAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
    }
}
