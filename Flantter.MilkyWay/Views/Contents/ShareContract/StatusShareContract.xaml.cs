using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.ViewModels.ShareContract;

namespace Flantter.MilkyWay.Views.Contents.ShareContract
{
    public sealed partial class StatusShareContract : Page
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(StatusShareContractViewModel), typeof(StatusShareContract),
                null);

        public StatusShareContract()
        {
            InitializeComponent();
            ViewModel = new StatusShareContractViewModel();
            DataContext = ViewModel;

            Window.Current.Closed += (s, e) => { ViewModel.Dispose(); };
        }

        public StatusShareContractViewModel ViewModel
        {
            get => (StatusShareContractViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public async void Activate(ShareTargetActivatedEventArgs e)
        {
            ViewModel.Title.Value = e.ShareOperation.Data.Properties.Title;
            ViewModel.Description.Value = e.ShareOperation.Data.Properties.Description;

            if (e.ShareOperation.Data.Contains(StandardDataFormats.Text))
                ViewModel.Text.Value = await e.ShareOperation.Data.GetTextAsync();
            if (e.ShareOperation.Data.Contains(StandardDataFormats.WebLink))
                ViewModel.Text.Value = ViewModel.Title.Value + " " +
                                       (await e.ShareOperation.Data.GetWebLinkAsync()).AbsoluteUri;
            else if (e.ShareOperation.Data.Contains(StandardDataFormats.ApplicationLink))
                ViewModel.Text.Value = ViewModel.Title.Value + " " +
                                       (await e.ShareOperation.Data.GetApplicationLinkAsync()).AbsoluteUri;

            if (e.ShareOperation.Data.Contains(StandardDataFormats.StorageItems))
            {
                ViewModel.Text.Value = "";

                var storageItems = await e.ShareOperation.Data.GetStorageItemsAsync();
                foreach (StorageFile storageItem in storageItems)
                    if (storageItem.FileType.ToLower() == ".jpg" || storageItem.FileType.ToLower() == ".jpeg" ||
                        storageItem.FileType.ToLower() == ".png" || storageItem.FileType.ToLower() == ".gif")
                    {
                        await ViewModel.Model.AddPicture(storageItem);
                    }
                    else if (storageItem.FileType.ToLower() == ".mp3" || storageItem.FileType.ToLower() == ".wav" ||
                             storageItem.FileType.ToLower() == ".m4a")
                    {
                        ViewModel.Text.Value = e.ShareOperation.Data.Properties.Title + " - " +
                                               e.ShareOperation.Data.Properties + " #nowplaying";
                        break;
                    }
            }

            ViewModel.Text.Value = ViewModel.Text.Value.Trim();

            ViewModel.ShareOperation = e.ShareOperation;

            Window.Current.Content = this;
            Window.Current.Activate();

            ShareContractTextBox.Focus(FocusState.Programmatic);
        }
    }
}