using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// ユーザー コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class MainArea : UserControl
    {
        public MainArea()
        {
            this.InitializeComponent();
        }

		private void MainArea_BottomBar_Tapped(object sender, TappedRoutedEventArgs e)
		{
			Setting.SettingService.Setting.Theme = Setting.SettingService.Setting.Theme == "Dark" ? "Light" : "Dark";
            Setting.SettingService.Setting.TitleBarVisibility = !Setting.SettingService.Setting.TitleBarVisibility;
        }
	}
}
