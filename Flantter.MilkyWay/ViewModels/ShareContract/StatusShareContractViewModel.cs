using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.ShareContract;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Flantter.MilkyWay.ViewModels.ShareContract
{
    public class StatusShareContractViewModel : IDisposable
    {
        protected CompositeDisposable Disposable { get; private set; } = new CompositeDisposable();

        public ShareOperation ShareOperation { get; set; }

        public StatusShareContractModel Model { get; set; }
        public Services.Notice Notice { get; set; }
        public Setting.SettingService Setting { get; set; }

        public ObservableCollection<AccountSetting> Accounts { get; private set; }

        public ReadOnlyReactiveCollection<PictureViewModel> Pictures { get; private set; }

        public ReactiveProperty<AccountSetting> SelectedAccount { get; set; }
        
        public ReactiveProperty<bool> IsEnableShareOperation { get; set; }

        public ReactiveProperty<string> Title { get; set; }
        public ReactiveProperty<string> Description { get; set; }

        public ReactiveProperty<string> Text { get; set; }
        public ReactiveProperty<string> CharacterCount { get; set; }

        public ReactiveProperty<string> Message { get; set; }

        public ReactiveProperty<Symbol> StateSymbol { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }
        
        public ReactiveCommand TweetCommand { get; set; }

        public StatusShareContractViewModel()
        {
            var uiThreadScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);

            this.Model = new StatusShareContractModel();

            this.Accounts = AdvancedSettingService.AdvancedSetting.Accounts;
            this.SelectedAccount = new ReactiveProperty<AccountSetting>(uiThreadScheduler, AdvancedSettingService.AdvancedSetting.Accounts.Count > 0 ? AdvancedSettingService.AdvancedSetting.Accounts.First() : null);

            this.IsEnableShareOperation = new ReactiveProperty<bool>(uiThreadScheduler, AdvancedSettingService.AdvancedSetting.Accounts.Count > 0);

            this.Title = new ReactiveProperty<string>(uiThreadScheduler);
            this.Description = new ReactiveProperty<string>(uiThreadScheduler);

            this.Text = this.Model.ToReactivePropertyAsSynchronized(x => x.Text, uiThreadScheduler).AddTo(this.Disposable);
            this.CharacterCount = this.Model.ObserveProperty(x => x.CharacterCount).Select(x => x.ToString()).ToReactiveProperty(uiThreadScheduler).AddTo(this.Disposable);

            this.Message = this.Model.ObserveProperty(x => x.Message).ToReactiveProperty(uiThreadScheduler).AddTo(this.Disposable);
            
            this.StateSymbol = this.Model.ObserveProperty(x => x.State).Select(x =>
            {
                switch (x)
                {
                    case "Accept":
                        return Symbol.Accept;
                    case "Cancel":
                        return Symbol.Cancel;
                    default:
                        return Symbol.Accept;
                }
            }).ToReactiveProperty(uiThreadScheduler).AddTo(this.Disposable);

            this.Updating = this.Model.ObserveProperty(x => x.Updating).ToReactiveProperty(uiThreadScheduler).AddTo(this.Disposable);
            
            this.Notice = Services.Notice.Instance;
            this.Setting = SettingService.Setting;

            this.Pictures = this.Model.ReadonlyPictures.ToReadOnlyReactiveCollection(x => new PictureViewModel(x), uiThreadScheduler).AddTo(this.Disposable);
            
            this.TweetCommand = this.Model.ObserveProperty(x => x.CharacterCount).Select(x => x >= 0).ToReactiveCommand(uiThreadScheduler).AddTo(this.Disposable);
            this.TweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var complete = await this.Model.Tweet(this.SelectedAccount.Value);
                if (!complete)
                    return;

                this.ShareOperation.ReportCompleted();
            }).AddTo(this.Disposable);
            
            Services.ShareNotice.Instance.ShareContractAccountChangeCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var accountSetting = x as AccountSetting;
                this.SelectedAccount.Value = accountSetting;
            }).AddTo(this.Disposable);
        }

        public void Dispose()
        {
            this.Disposable.Dispose();
        }
    }

    public class PictureViewModel : IDisposable
    {
        public PictureViewModel(PictureModel picture)
        {
            var uiThreadScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);

            this.Image = picture.ObserveProperty(x => x.Stream).SubscribeOn(uiThreadScheduler).Select(x =>
            {
                if (!picture.IsVideo)
                {
                    BitmapImage bitmap = new BitmapImage();

                    var stream = x as IRandomAccessStream;
                    if (stream != null)
                        bitmap.SetSource(stream);

                    return (ImageSource)bitmap;
                }
                else if (picture.StorageFile != null)
                {
                    var thumbnailTask = picture.StorageFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.VideosView).AsTask();
                    thumbnailTask.Wait();

                    BitmapImage bitmap = new BitmapImage();

                    var stream = thumbnailTask.Result as IRandomAccessStream;
                    if (stream != null)
                        bitmap.SetSource(stream);

                    return (ImageSource)bitmap;
                }

                return new BitmapImage();

            }).ToReactiveProperty(uiThreadScheduler);

            this.Notice = Services.Notice.Instance;
            this._PictureModel = picture;
        }

        public ReactiveProperty<ImageSource> Image { get; set; }

        public PictureModel _PictureModel { get; set; }
        public Services.Notice Notice { get; set; }

        public void Dispose()
        {
            this.Image.Dispose();
        }
    }
}
