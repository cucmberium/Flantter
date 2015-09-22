using Prism.Windows.Mvvm;
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
    public sealed partial class MainPage : VisualStateAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();

            Themes.ThemeService.Theme.PropertyChanged += Theme_PropertyChanged;
            Setting.SettingService.Setting.PropertyChanged += Setting_PropertyChanged;

            //Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode;

            var applicationView = ApplicationView.GetForCurrentView();
            
            applicationView.TitleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color;
            applicationView.TitleBar.ButtonInactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            applicationView.TitleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
            applicationView.TitleBar.ForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color;
            applicationView.TitleBar.InactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
            applicationView.TitleBar.InactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color;

            if (Setting.SettingService.Setting.TitleBarVisibility)
            {
                applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonForegroundColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
            }
            else
            {
                applicationView.TitleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
            }
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TitleBarVisibility")
            {
                var applicationView = ApplicationView.GetForCurrentView();

                if (Setting.SettingService.Setting.TitleBarVisibility)
                {
                    applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                    applicationView.TitleBar.ButtonForegroundColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
                }
                else
                {
                    applicationView.TitleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color;
                    applicationView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
                }
            }
        }

        private void Theme_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var applicationView = ApplicationView.GetForCurrentView();
            applicationView.TitleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color;
            applicationView.TitleBar.ButtonInactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
            applicationView.TitleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
            applicationView.TitleBar.ForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color;
            applicationView.TitleBar.InactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
            applicationView.TitleBar.InactiveForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color;

            if (Setting.SettingService.Setting.TitleBarVisibility)
            {
                applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
                applicationView.TitleBar.ButtonForegroundColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
            }
            else
            {
                applicationView.TitleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color;
                applicationView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
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
