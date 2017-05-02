using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.ShareContract;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.ShareContract
{
    public class ShareAccountViewModel : BindableBase
    {
        public ReactiveProperty<AccountSetting> AccountSetting { get; set; }
        public ReactiveProperty<bool> IsTweetEnabled { get; set; }
    }

    public class StatusShareContractViewModel : IDisposable
    {
        public StatusShareContractViewModel()
        {
            var uiThreadScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);

            Model = new StatusShareContractModel();

            IsEnableShareOperation = new ReactiveProperty<bool>(uiThreadScheduler,
                AdvancedSettingService.AdvancedSetting.Accounts?.Count > 0);

            if (IsEnableShareOperation.Value)
            {
                Accounts = new ReactiveCollection<ShareAccountViewModel>(
                    AdvancedSettingService.AdvancedSetting.Accounts.Select(x => new ShareAccountViewModel
                        {
                            AccountSetting = new ReactiveProperty<AccountSetting>(x),
                            IsTweetEnabled = new ReactiveProperty<bool>(x.IsEnabled)
                        })
                        .ToObservable(), uiThreadScheduler);
                SelectedAccounts = Accounts.ObserveElementObservableProperty(x => x.IsTweetEnabled)
                    .Select(y => Accounts.Where(z => z.IsTweetEnabled.Value))
                    .ToReactiveProperty(uiThreadScheduler);
            }
            else
            {
                Accounts = new ReactiveCollection<ShareAccountViewModel>();
                SelectedAccounts = new ReactiveProperty<IEnumerable<ShareAccountViewModel>>();
            }

            Title = new ReactiveProperty<string>(uiThreadScheduler);
            Description = new ReactiveProperty<string>(uiThreadScheduler);

            Text = Model.ToReactivePropertyAsSynchronized(x => x.Text, uiThreadScheduler).AddTo(Disposable);
            CharacterCount = Model.ObserveProperty(x => x.CharacterCount)
                .Select(x => x.ToString())
                .ToReactiveProperty(uiThreadScheduler)
                .AddTo(Disposable);

            Message = Model.ObserveProperty(x => x.Message).ToReactiveProperty(uiThreadScheduler).AddTo(Disposable);
            ToolTipIsOpen = Model.ToReactivePropertyAsSynchronized(x => x.ToolTipIsOpen, uiThreadScheduler);

            StateSymbol = Model.ObserveProperty(x => x.State)
                .Select(x =>
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
                })
                .ToReactiveProperty(uiThreadScheduler)
                .AddTo(Disposable);

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty(uiThreadScheduler).AddTo(Disposable);

            Notice = Notice.Instance;
            Setting = SettingService.Setting;

            Pictures = Model.ReadonlyPictures
                .ToReadOnlyReactiveCollection(x => new PictureViewModel(x), uiThreadScheduler)
                .AddTo(Disposable);

            TweetCommand = Model.ObserveProperty(x => x.CharacterCount)
                .Select(x => x >= 0)
                .ToReactiveCommand(uiThreadScheduler)
                .AddTo(Disposable);
            TweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var complete = await Model.Tweet(SelectedAccounts.Value.Select(y => y.AccountSetting.Value));
                    if (!complete)
                        return;

                    ShareOperation.ReportCompleted();
                })
                .AddTo(Disposable);
        }

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public ShareOperation ShareOperation { get; set; }

        public StatusShareContractModel Model { get; set; }
        public Notice Notice { get; set; }
        public SettingService Setting { get; set; }

        public ReactiveCollection<ShareAccountViewModel> Accounts { get; }
        public ReactiveProperty<IEnumerable<ShareAccountViewModel>> SelectedAccounts { get; }

        public ReadOnlyReactiveCollection<PictureViewModel> Pictures { get; }

        public ReactiveProperty<bool> IsEnableShareOperation { get; set; }

        public ReactiveProperty<string> Title { get; set; }
        public ReactiveProperty<string> Description { get; set; }

        public ReactiveProperty<string> Text { get; set; }
        public ReactiveProperty<string> CharacterCount { get; set; }

        public ReactiveProperty<string> Message { get; set; }

        public ReactiveProperty<bool> ToolTipIsOpen { get; set; }

        public ReactiveProperty<Symbol> StateSymbol { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveCommand TweetCommand { get; set; }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }

    public class PictureViewModel : IDisposable
    {
        public PictureViewModel(PictureModel picture)
        {
            var uiThreadScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);

            Image = picture.ObserveProperty(x => x.Stream)
                .SubscribeOn(uiThreadScheduler)
                .Select(x =>
                {
                    if (!picture.IsVideo)
                    {
                        var bitmap = new BitmapImage();

                        var stream = x;
                        if (stream != null)
                            bitmap.SetSource(stream);

                        return (ImageSource) bitmap;
                    }
                    if (picture.StorageFile != null)
                    {
                        var thumbnailTask = picture.StorageFile.GetThumbnailAsync(ThumbnailMode.VideosView).AsTask();
                        thumbnailTask.Wait();

                        var bitmap = new BitmapImage();

                        if (thumbnailTask.Result is IRandomAccessStream stream)
                            bitmap.SetSource(stream);

                        return (ImageSource) bitmap;
                    }

                    return new BitmapImage();
                })
                .ToReactiveProperty(uiThreadScheduler);

            Notice = Notice.Instance;
            PictureModel = picture;
        }

        public ReactiveProperty<ImageSource> Image { get; set; }

        public PictureModel PictureModel { get; set; }
        public Notice Notice { get; set; }

        public void Dispose()
        {
            Image.Dispose();
        }
    }
}