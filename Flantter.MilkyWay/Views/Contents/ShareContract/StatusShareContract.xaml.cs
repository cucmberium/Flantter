using Flantter.MilkyWay.ViewModels.ShareContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

            Window.Current.Closed += (s, e) => 
            {
                this.ViewModel.Dispose();
            };
        }

        public async void Activate(ShareTargetActivatedEventArgs e)
        {
            this.ViewModel.Title.Value = e.ShareOperation.Data.Properties.Title;
            this.ViewModel.Description.Value = e.ShareOperation.Data.Properties.Description;
            
            if (e.ShareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
            {
                this.ViewModel.Text.Value = await e.ShareOperation.Data.GetTextAsync();
            }
            if (e.ShareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.WebLink))
            {
                this.ViewModel.Text.Value = this.ViewModel.Title.Value + " " + (await e.ShareOperation.Data.GetWebLinkAsync()).AbsoluteUri;
            }
            else if (e.ShareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.ApplicationLink))
            {
                this.ViewModel.Text.Value = this.ViewModel.Title.Value + " " + (await e.ShareOperation.Data.GetApplicationLinkAsync()).AbsoluteUri;
            }

            if (e.ShareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            {
                this.ViewModel.Text.Value = "";

                var storageItems = await e.ShareOperation.Data.GetStorageItemsAsync();
                foreach (StorageFile storageItem in storageItems)
                {
                    if (storageItem.FileType.ToLower() == ".jpg" || storageItem.FileType.ToLower() == ".jpeg" || storageItem.FileType.ToLower() == ".png" || storageItem.FileType.ToLower() == ".gif")
                    {
                        await this.ViewModel.Model.AddPicture(storageItem);
                    }
                    else if (storageItem.FileType.ToLower() == ".mp3" || storageItem.FileType.ToLower() == ".wav" || storageItem.FileType.ToLower() == ".m4a")
                    {
                        this.ViewModel.Text.Value = e.ShareOperation.Data.Properties.Title + " - " + e.ShareOperation.Data.Properties + " #nowplaying";
                        break;
                    }
                }
            }

            this.ViewModel.Text.Value = this.ViewModel.Text.Value.Trim();

            this.ViewModel.ShareOperation = e.ShareOperation;

            Window.Current.Content = this;
            Window.Current.Activate();

            this.ShareContract_TextBox.Focus(FocusState.Programmatic);
        }
    }
}
