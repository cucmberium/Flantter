using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Reactive.Concurrency;

namespace Flantter.MilkyWay.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageModel _MainPageModel { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; private set; }
        public ReactiveProperty<bool> TitleBarVisivility { get; private set; }
        public ReactiveProperty<bool> AppBarIsOpen { get; private set; }
        public ReactiveProperty<TweetAreaViewModel> TweetArea { get; private set; }

        public Messenger ShowImagePreviewMessenger { get; private set; }

        public Messenger ShowVideoPreviewMessenger { get; private set; }

        public Messenger ShowSettingsFlyoutMessenger { get; private set; }

        #region Constructor
        public MainPageViewModel()
        {
            this._MainPageModel = MainPageModel.Instance;
            this.Accounts = this._MainPageModel.ReadOnlyAccounts.ToReadOnlyReactiveCollection(x => new AccountViewModel(x));

            // 設定によってTitlebarの表示を変える
            this.TitleBarVisivility = SettingService.Setting.ObserveProperty(x => x.TitleBarVisibility).Select(x => x && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile").ToReactiveProperty();

            this.AppBarIsOpen = new ReactiveProperty<bool>(false);
            this.AppBarIsOpen.Subscribe<bool>(async isOpen => 
            {
                if (isOpen && this.Accounts.Any(x => x.IsEnabled.Value))
                {
                    Services.Notice.Instance.TweetAreaAccountChangeCommand.Execute(this.Accounts.First(x => x.IsEnabled.Value));

                    await Task.Delay(50);
                    await this.TweetArea.Value.TextBoxFocusMessenger.Raise(new Notification());
                }
            });

            this.TweetArea = new ReactiveProperty<TweetAreaViewModel>(new TweetAreaViewModel(this.Accounts));

            this.ShowImagePreviewMessenger = new Messenger();
            this.ShowVideoPreviewMessenger = new Messenger();
            this.ShowSettingsFlyoutMessenger = new Messenger();

            #region Command

            Services.Notice.Instance.ShowMediaCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var media = x as MediaEntity;

                if (media == null)
                    return;

                if (media.Type == "Image")
                    await this.ShowImagePreviewMessenger.Raise(new Notification() { Content = x });
                else if (media.Type == "Video")
                    await this.ShowVideoPreviewMessenger.Raise(new Notification() { Content = x });

            });

            Services.Notice.Instance.TweetAreaOpenCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var isOpen = false;
                if (x == null)
                    isOpen = true;
                else
                    isOpen = (bool)x;

                this.AppBarIsOpen.Value = isOpen;
            });

            Services.Notice.Instance.ShowSettingsFlyoutCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.ShowSettingsFlyoutMessenger.Raise(x as Notification);
            });
            #endregion
        }
        #endregion

        #region Destructor
        ~MainPageViewModel()
        {
        }
        #endregion

        #region Others
        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            Task.Run(() => this._MainPageModel.Initialize());
        }
        #endregion
    }
}
