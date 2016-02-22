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
using Windows.ApplicationModel.DataTransfer;
using Flantter.MilkyWay.Common;
using Windows.System;
using Windows.Storage;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Views.Behaviors;

namespace Flantter.MilkyWay.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageModel _MainPageModel { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; private set; }
        public ReactiveProperty<bool> TitleBarVisivility { get; private set; }
        public ReactiveProperty<bool> AppBarIsOpen { get; private set; }

        public TweetAreaViewModel TweetArea { get; private set; }

        public ReactiveCommand DragOverCommand { get; private set; }
        public ReactiveCommand DropCommand { get; private set; }

        public Messenger ShowImagePreviewMessenger { get; private set; }

        public Messenger ShowVideoPreviewMessenger { get; private set; }

        public Messenger ShowSettingsFlyoutMessenger { get; private set; }

        #region Constructor
        public MainPageViewModel()
        {
            this._MainPageModel = MainPageModel.Instance;

            // 設定によってTitlebarの表示を変える
            this.TitleBarVisivility = SettingService.Setting.ObserveProperty(x => x.TitleBarVisibility).Select(x => x && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile").ToReactiveProperty();

            this.AppBarIsOpen = new ReactiveProperty<bool>(false);
            this.AppBarIsOpen.Subscribe<bool>(async isOpen => 
            {
                if (isOpen && this.Accounts.Any(x => x.IsEnabled.Value))
                {
                    Services.Notice.Instance.TweetAreaAccountChangeCommand.Execute(this.Accounts.First(x => x.IsEnabled.Value));

                    await Task.Delay(50);
                    await this.TweetArea.TextBoxFocusMessenger.Raise(new Notification());
                }
            });
            
            this.Accounts = this._MainPageModel.ReadOnlyAccounts.ToReadOnlyReactiveCollection(x => new AccountViewModel(x));

            this.TweetArea = new TweetAreaViewModel(this.Accounts);

            this.ShowImagePreviewMessenger = new Messenger();
            this.ShowVideoPreviewMessenger = new Messenger();
            this.ShowSettingsFlyoutMessenger = new Messenger();

            #region Command
            this.DragOverCommand = new ReactiveCommand();
            this.DragOverCommand.Subscribe(x =>
            {
                var e = x as DragEventArgs;
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.Handled = true;
            });

            this.DropCommand = new ReactiveCommand();
            this.DropCommand.Subscribe(async x =>
            {
                var e = x as DragEventArgs;
                var d = e.GetDeferral();

                var files = (await e.DataView.GetStorageItemsAsync()).OfType<StorageFile>();
                if (files.Count() == 0)
                {
                    d.Complete();
                    return;
                }

                var supportedFormat = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", };

                foreach (var file in files)
                {
                    if (!supportedFormat.Contains(file.FileType))
                        continue;

                    await this.TweetArea._TweetAreaModel.AddPicture(file);
                }

                this.AppBarIsOpen.Value = true;

                d.Complete();
            });

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
                if (!(x is bool))
                    isOpen = true;
                else if (x == null)
                    isOpen = true;
                else
                    isOpen = (bool)x;

                this.AppBarIsOpen.Value = isOpen;
            });

            Services.Notice.Instance.ShowSettingsFlyoutCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.ShowSettingsFlyoutMessenger.Raise(x as Notification);
            });

            Services.Notice.Instance.CopyTweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var status = x as Status;
                if (status != null)
                {
                    try
                    {
                        var textPackage = new DataPackage();
                        textPackage.SetText(status.Text);
                        Clipboard.SetContent(textPackage);
                    }
                    catch
                    {
                    }

                    return;
                }

                var directMessage = x as DirectMessage;
                if (directMessage != null)
                {
                    try
                    {
                        var textPackage = new DataPackage();
                        textPackage.SetText(directMessage.Text);
                        Clipboard.SetContent(textPackage);
                    }
                    catch
                    {
                    }

                    return;
                }
            });

            Services.Notice.Instance.UrlClickCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var linkUrl = x as string;
                if (string.IsNullOrWhiteSpace(linkUrl))
                    return;


                if (linkUrl.StartsWith("@"))
                {
                    var userMention = linkUrl.Replace("@", "");
                    ViewModels.Services.Notice.Instance.ShowUserProfileCommand.Execute(userMention.Replace("@", ""));
                    return;
                }
                else if (linkUrl.StartsWith("#"))
                {
                    var hashTag = linkUrl.Replace("#", "");
                    return;
                }

                var statusMatch = TweetRegexPatterns.StatusUrl.Match(linkUrl);
                var userMatch = TweetRegexPatterns.UserUrl.Match(linkUrl);
                if (statusMatch.Success)
                { }
                else if (userMatch.Success)
                { }
                else
                {
                    await Launcher.LaunchUriAsync(new Uri(linkUrl));
                }
            });

            Services.Notice.Instance.OpenStatusUrlCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var status = x as Status;
                if (status == null)
                    return;

                await Launcher.LaunchUriAsync(new Uri("https://twitter.com/" + status.User.ScreenName + "/status/" + status.Id.ToString()));
            });

            Services.Notice.Instance.ShareStatusCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var status = x as Status;
                if (status == null)
                    return;

                // Todo : 実装
            });

            Services.Notice.Instance.ChangeAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                // Todo : 実装
            });

            Services.Notice.Instance.ExitAppCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                // Todo : 実装
            });

            Services.Notice.Instance.ShowMainSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "MainSetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowAppInfoCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "AppInfo" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
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
        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            await Task.Run(() => this._MainPageModel.Initialize());

            Services.Notice.Instance.TweetAreaAccountChangeCommand.Execute(this.Accounts.First(x => x.IsEnabled.Value));
        }
        #endregion
    }
}
