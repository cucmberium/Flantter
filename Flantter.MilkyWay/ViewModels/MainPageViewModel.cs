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
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Views.Contents.Authorize;
using System.Collections.ObjectModel;
using Flantter.MilkyWay.License;

namespace Flantter.MilkyWay.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageModel Model { get; set; }

        public SettingService Setting { get; set; }

        public Services.Notice Notice { get; set; }

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
            this.Model = MainPageModel.Instance;
            this.Setting = SettingService.Setting;
            this.Notice = Services.Notice.Instance;

            // 設定によってTitlebarの表示を変える
            this.TitleBarVisivility = Observable.CombineLatest<bool, UserInteractionMode, bool>(SettingService.Setting.ObserveProperty(x => x.ExtendTitleBar), WindowSizeHelper.Instance.ObserveProperty(x => x.UserIntaractionMode),
                (titleBarSetting, userIntaractionMode) =>
                {
                    if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                        return false;
                    else if (userIntaractionMode == UserInteractionMode.Mouse)
                        return true;
                    else
                        return titleBarSetting;
                }).ToReactiveProperty();

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

            this.Accounts = this.Model.ReadOnlyAccounts.ToReadOnlyReactiveCollection(x => new AccountViewModel(x));

            this.TweetArea = new TweetAreaViewModel(this.Accounts);

            this.ShowImagePreviewMessenger = new Messenger();
            this.ShowVideoPreviewMessenger = new Messenger();
            this.ShowSettingsFlyoutMessenger = new Messenger();

            #region Command
            this.DragOverCommand = new ReactiveCommand();
            this.DragOverCommand.Subscribe(x =>
            {
                var e = x as DragEventArgs;
                
                if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                    return;

                e.AcceptedOperation = DataPackageOperation.Copy;
                e.Handled = true;
            });

            this.DropCommand = new ReactiveCommand();
            this.DropCommand.Subscribe(async x =>
            {
                var e = x as DragEventArgs;

                if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                    return;
                
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
                    isOpen = !this.AppBarIsOpen.Value;
                else if (x == null)
                    isOpen = !this.AppBarIsOpen.Value;
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
                this.Model.CopyTweetToClipBoard(x);
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
                    ViewModels.Services.Notice.Instance.ShowSearchCommand.Execute(hashTag);
                    return;
                }

                var statusMatch = TweetRegexPatterns.StatusUrl.Match(linkUrl);
                var userMatch = TweetRegexPatterns.UserUrl.Match(linkUrl);
                if (statusMatch.Success)
                {
                    Services.Notice.Instance.ShowStatusDetailCommand.Execute(long.Parse(statusMatch.Groups["Id"].ToString()));
                }
                else if (userMatch.Success)
                {
                    Services.Notice.Instance.ShowUserProfileCommand.Execute(userMatch.Groups["ScreenName"].ToString());
                }
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

            Services.Notice.Instance.ShowChangeAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "AccountChange" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ChangeAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Model.ChangeAccount(AdvancedSettingService.AdvancedSetting.Accounts.First(y => y.UserId == (long)x));
            });

            Services.Notice.Instance.ExitAppCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_ExitApp"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

                Application.Current.Exit();
            });

            Services.Notice.Instance.ShowMainSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "MainSetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowBehaviorSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "BehaviorSetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowPostingSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "PostingSetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowDisplaySettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "DisplaySetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowNotificationSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "NotificationSetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowMuteSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "MuteSetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowAccountsSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "AccountsSetting" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowAccountSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "AccountSetting", Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowAdvancedSettingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                if (LicenseService.License.AppDonationIsActive)
                {
                    var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "AdvancedSetting" };
                    Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                }
                else
                {
                    Services.Notice.Instance.DonateCommand.Execute();
                }
            });

            Services.Notice.Instance.DonateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_NeedAppDonation"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                var donateResult = await LicenseService.License.PurchaseAppDonation();

                if (donateResult)
                    await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_DonateSuccessfully"), Title = "Confirmation" });
                else
                    await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_FailedToDonate"), Title = "Confirmation" });
            });

            Services.Notice.Instance.ShowAppInfoCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "AppInfo" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ChangeBackgroundImageCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var result = await Notice.ShowFilePickerMessenger.Raise(new FileOpenPickerNotification
                {
                    FileTypeFilter = new[] { ".bmp", ".jpeg", ".jpg", ".png" },
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    ViewMode = PickerViewMode.Thumbnail,
                });

                if (result.Result.Count() == 0)
                    return;

                var file = result.Result.First();

                await this.Model.ChangeBackgroundImage(file);
            });

            Services.Notice.Instance.ChangeThemeCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var result = await Notice.ShowFilePickerMessenger.Raise(new FileOpenPickerNotification
                {
                    FileTypeFilter = new[] { ".xaml" },
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    ViewMode = PickerViewMode.Thumbnail,
                });

                if (result.Result.Count() == 0)
                    return;

                var file = result.Result.First();

                await this.Model.ChangeTheme(file);
            });

            Services.Notice.Instance.MuteClientCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var client = x as string;
                if (string.IsNullOrWhiteSpace(client))
                    return;

                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_MuteClient"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                this.Model.MuteClient(client);
            });

            Services.Notice.Instance.DeleteMuteUserCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var screenName = x as string;
                if (string.IsNullOrWhiteSpace(screenName))
                    return;

                this.Model.DeleteMuteUser(screenName);
            });

            Services.Notice.Instance.DeleteMuteClientCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var client = x as string;
                if (string.IsNullOrWhiteSpace(client))
                    return;

                this.Model.DeleteMuteUser(client);
            });

            Services.Notice.Instance.UpdateMuteFilterCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var filter = x as string;
                if (string.IsNullOrWhiteSpace(filter))
                    return;

                // 禁忌 : Taboo
                try
                {
                    Models.Filter.Compiler.Compile(filter);
                }
                catch (FilterCompileException fex)
                {
                    var msgNotification = new MessageDialogNotification() { Message = fex.Error.ToString() + "\n" + fex.Message, Title = "Compile Error" };
                    await Notice.ShowMessageDialogMessenger.Raise(msgNotification);
                    return;
                }
                catch (Exception ex)
                {
                    var msgNotification = new MessageDialogNotification() { Message = ex.ToString() + "\n" + ex.Message, Title = "Compile Error" };
                    await Notice.ShowMessageDialogMessenger.Raise(msgNotification);
                    return;
                }

                SettingService.Setting.MuteFilter = filter;
                
                await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_CompiledMuteFilterSuccessfully"), Title = "Compile Filter" });
                return;
            });
            
            Services.Notice.Instance.AuthAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var auth = new AuthorizeNotification();
                await Notice.ShowAuthorizePopupMessenger.Raise(auth);
                var account = auth.Result;

                if (account == null)
                    return;

                // 禁忌 : Taboo
                if (AdvancedSettingService.AdvancedSetting.Accounts.Any(y => y.UserId == account.UserId))
                {
                    var accountSetting = AdvancedSettingService.AdvancedSetting.Accounts.First(y => y.UserId == account.UserId);
                    accountSetting.ScreenName = account.ScreenName;
                    accountSetting.ConsumerKey = account.ConsumerKey;
                    accountSetting.ConsumerSecret = account.ConsumerSecret;
                    accountSetting.AccessToken = account.AccessToken;
                    accountSetting.AccessTokenSecret = account.AccessTokenSecret;
                }
                else
                {
                    var accountSetting = new AccountSetting()
                    {
                        AccessToken = account.AccessToken,
                        AccessTokenSecret = account.AccessTokenSecret,
                        ConsumerKey = account.ConsumerKey,
                        ConsumerSecret = account.ConsumerSecret,
                        ScreenName = account.ScreenName,
                        UserId = account.UserId,

                        Column = new ObservableCollection<ColumnSetting>()
                        {
                            new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Home, AutoRefresh = false, AutoRefreshTimerInterval = 60.0, Filter = "()", Name = "Home", Parameter = string.Empty, Streaming = true, Index = 0, DisableStartupRefresh = false, FetchingNumberOfTweet = 100 },
                            new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Mentions, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "Mentions", Parameter = string.Empty, Streaming = false, Index = 1, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 },
                            new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.DirectMessages, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "DirectMessages", Parameter = string.Empty, Streaming = false, Index = 2, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 },
                            new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Events, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "Events", Parameter = string.Empty, Streaming = false, Index = 3, DisableStartupRefresh = false, FetchingNumberOfTweet = 100 },
                            new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Favorites, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "Favorites", Parameter = string.Empty, Streaming = false, Index = 4, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 },
                        },
                        IsEnabled = false,
                    };

                    Services.Notice.Instance.AddAccountCommand.Execute(accountSetting);
                }
            });

            Services.Notice.Instance.AddAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var accountSetting = x as AccountSetting;
                if (accountSetting == null)
                    return;
                
                this.Model.AddAccount(accountSetting);
            });

            Services.Notice.Instance.DeleteAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var accountSetting = x as AccountSetting;
                if (accountSetting == null)
                    return;

                if (AdvancedSettingService.AdvancedSetting.Accounts.Count <= 1)
                {
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(new MessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_CannotDeleteAccount"), Title = "Error" });
                    return;
                }

                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_DeleteAccount"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                this.Model.DeleteAccount(accountSetting);
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

            await Task.Run(async () => await this.Model.Initialize());

            Services.Notice.Instance.TweetAreaAccountChangeCommand.Execute(this.Accounts.First(x => x.IsEnabled.Value));
        }
        #endregion
    }
}
