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
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels.ShareContract
{
    public class StatusShareContractViewModel : IDisposable
    {
        protected CompositeDisposable Disposable { get; private set; } = new CompositeDisposable();

        public StatusShareContractModel Model { get; set; }
        public Services.Notice Notice { get; set; }
        public Setting.SettingService Setting { get; set; }

        public ObservableCollection<AccountSetting> Accounts { get; private set; }

        public ReadOnlyReactiveCollection<PictureViewModel> Pictures { get; private set; }

        public ReactiveProperty<AccountSetting> SelectedAccount { get; set; }
        
        public ReactiveProperty<string> Text { get; set; }
        public ReactiveProperty<string> CharacterCount { get; set; }

        public ReactiveProperty<string> Message { get; set; }

        public ReactiveProperty<Symbol> StateSymbol { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }
        
        public ReactiveCommand TweetCommand { get; set; }
        public ReactiveCommand AccountChangeCommand { get; set; }

        public Messenger TextBoxFocusMessenger { get; private set; }

        public StatusShareContractViewModel()
        {
            var uiThreadScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);

            this.Model = new StatusShareContractModel();

            this.Accounts = AdvancedSettingService.AdvancedSetting.Accounts;
            this.SelectedAccount = new ReactiveProperty<AccountSetting>();
            
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
            
            this.TextBoxFocusMessenger = new Messenger();

            this.Pictures = this.Model.ReadonlyPictures.ToReadOnlyReactiveCollection(x => new PictureViewModel(x), uiThreadScheduler).AddTo(this.Disposable);
            
            this.TweetCommand = this.Model.ObserveProperty(x => x.CharacterCount).Select(x => x >= 0).ToReactiveCommand(uiThreadScheduler).AddTo(this.Disposable);
            this.TweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.Tweet(this.SelectedAccount.Value);
            }).AddTo(this.Disposable);

            this.AccountChangeCommand = new ReactiveCommand(uiThreadScheduler);
            this.AccountChangeCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
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
}
