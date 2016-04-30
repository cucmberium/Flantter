using Flantter.MilkyWay.ViewModels.ShareContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Flantter.MilkyWay.Views.Contents.ShareContract
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class StatusShareContract : Page
    {
        public StatusShareContractViewModel ViewModel
        {
            get { return (StatusShareContractViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(StatusShareContractViewModel), typeof(StatusShareContract), null);

        public StatusShareContract()
        {
            this.InitializeComponent();
            this.ViewModel = new StatusShareContractViewModel();
            this.DataContext = this.ViewModel;
        }

        public async void Activate(ShareTargetActivatedEventArgs e)
        {
            Window.Current.Content = this;
            Window.Current.Activate();
        }
    }
}
