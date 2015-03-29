using Microsoft.Practices.Prism.StoreApps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Frame.BackStack.Clear();
            this.Frame.ForwardStack.Clear();

            /*var applicationView = ApplicationView.GetForCurrentView();
            applicationView.Title = "";

            applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 176, 30, 0);
            applicationView.TitleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
            applicationView.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 176, 30, 0);
            applicationView.TitleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 255, 255, 255);
            applicationView.TitleBar.BackgroundColor = Color.FromArgb(255, 176, 30, 0);
            applicationView.TitleBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
            applicationView.TitleBar.InactiveBackgroundColor = Color.FromArgb(255, 176, 30, 0);
            applicationView.TitleBar.InactiveForegroundColor = Color.FromArgb(255, 255, 255, 255);

            applicationView.TitleBar.ExtendViewIntoTitleBar = true;
            applicationView.TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;*/
        }
        /*private void TitleBar_IsVisibleChanged(ApplicationViewTitleBar sender, object args)
        {
            var applicationView = ApplicationView.GetForCurrentView();
            if (applicationView.TitleBar.IsVisible)
            {
                this.Flantter_TitleBar.Visibility = Visibility.Visible;
                this.Flantter_TitleBar.Height = applicationView.TitleBar.Height;
            }
            else
            {
                this.Flantter_TitleBar.Visibility = Visibility.Collapsed;
                this.Flantter_TitleBar.Height = applicationView.TitleBar.Height;
            }
        }*/

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ApplicationView.GetForCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
        }
    }
}
