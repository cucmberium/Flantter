using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Flantter.Clover.ViewModels;
using Prism.Windows.Mvvm;

namespace Flantter.Clover.Views
{
    public sealed partial class MainPage : SessionStateAwarePage
    {
        public MainPage()
        {
            InitializeComponent();

            // TitleBar
            Window.Current.SetTitleBar(TitleBarMain);
            var coreApplicationView = CoreApplication.GetCurrentView();
            coreApplicationView.TitleBar.ExtendViewIntoTitleBar = true;
            var applicationView = ApplicationView.GetForCurrentView();
            applicationView.TitleBar.BackgroundColor =
                ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
            applicationView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
            applicationView.TitleBar.ButtonForegroundColor =
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
            applicationView.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);
            applicationView.TitleBar.ButtonInactiveForegroundColor =
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.BackStack.Clear();
            Frame.ForwardStack.Clear();
        }
        
        public MainPageViewModel ViewModel
        {
            get => (MainPageViewModel)DataContext;
            set => DataContext = value;
        }
    }
}
