using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Flantter.MilkyWay.License;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Models.Filter;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        #region Constructor

        public MainPageViewModel()
        {
            Model = MainPageModel.Instance;
            Setting = SettingService.Setting;
            Notice = Notice.Instance;

            // 設定によってTitlebarの表示を変える
            TitleBarVisivility = SettingService.Setting.ObserveProperty(x => x.ExtendTitleBar)
                .CombineLatest(WindowSizeHelper.Instance.ObserveProperty(x => x.UserInteractionMode),
                    (titleBarSetting, userIntaractionMode) =>
                    {
                        if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                            return false;
                        if (userIntaractionMode == UserInteractionMode.Mouse)
                            return true;
                        return titleBarSetting;
                    })
                .ToReactiveProperty();

            AppBarIsOpen = new ReactiveProperty<bool>(false);
            AppBarIsOpen.Subscribe(async isOpen =>
            {
                if (isOpen && Accounts.Any(x => x.IsEnabled.Value))
                {
                    Notice.Instance.TweetAreaAccountChangeCommand.Execute(Accounts.First(x => x.IsEnabled.Value));

                    await Task.Delay(50);
                    await TweetArea.TextBoxFocusMessenger.Raise(new Notification());
                }
                else
                {
                    if (TweetArea != null)
                        TweetArea.ToolTipIsOpen.Value = false;
                }
            });

            SelectedTweet = new ReactiveProperty<object>();

            Accounts = Model.ReadOnlyAccounts.ToReadOnlyReactiveCollection(x => new AccountViewModel(x));

            TweetArea = new TweetAreaViewModel(Accounts);

            ShowSettingsFlyoutMessenger = new Messenger();
            ShowShareUIMessenger = new Messenger();

            #region Command

            DragOverCommand = new ReactiveCommand();
            DragOverCommand.Subscribe(x =>
            {
                var e = x as DragEventArgs;

                if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                    return;

                e.AcceptedOperation = DataPackageOperation.Copy;
                e.Handled = true;
            });

            DropCommand = new ReactiveCommand();
            DropCommand.Subscribe(async x =>
            {
                var e = x as DragEventArgs;

                if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                    return;

                var d = e.GetDeferral();

                var files = (await e.DataView.GetStorageItemsAsync()).OfType<StorageFile>();
                if (!files.Any())
                {
                    d.Complete();
                    return;
                }

                var supportedFormat = new[] {".jpg", ".jpeg", ".png", ".gif", ".mp4"};

                foreach (var file in files)
                {
                    if (!supportedFormat.Contains(file.FileType))
                        continue;

                    await TweetArea.Model.AddPicture(file);
                }

                AppBarIsOpen.Value = true;

                d.Complete();
            });

            Notice.Instance.ShowMediaCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var media = x as MediaEntity;

                    if (media == null)
                        return;

                    var notification =
                        new ShowSettingsFlyoutNotification {SettingsFlyoutType = media.Type + "Preview", Content = x};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.TweetAreaOpenCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    bool isOpen;
                    if (!(x is bool))
                        isOpen = !AppBarIsOpen.Value;
                    else if (x == null)
                        isOpen = !AppBarIsOpen.Value;
                    else
                        isOpen = (bool) x;

                    AppBarIsOpen.Value = isOpen;
                });

            Notice.Instance.ShowSettingsFlyoutCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await ShowSettingsFlyoutMessenger.Raise(x as Notification); });

            Notice.Instance.CopyTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x => { Model.CopyTweetToClipBoard(x); });

            Notice.Instance.CopyTweetUrlCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x => { Model.CopyTweetUrlToClipBoard(x); });

            Notice.Instance.UrlClickCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var linkUrl = x as string;
                    if (string.IsNullOrWhiteSpace(linkUrl))
                        return;

                    if (linkUrl.StartsWith("@"))
                    {
                        var userMention = linkUrl.Replace("@", "");
                        Notice.Instance.ShowUserProfileCommand.Execute(userMention.Replace("@", ""));
                        return;
                    }
                    if (linkUrl.StartsWith("#"))
                    {
                        var hashTag = linkUrl;
                        Notice.Instance.ShowSearchCommand.Execute(hashTag);
                        return;
                    }

                    var statusMatch = TweetRegexPatterns.StatusUrl.Match(linkUrl);
                    var userMatch = TweetRegexPatterns.UserUrl.Match(linkUrl);
                    if (statusMatch.Success)
                        Notice.Instance.ShowStatusDetailCommand.Execute(long.Parse(statusMatch.Groups["Id"]
                            .ToString()));
                    else if (userMatch.Success)
                        Notice.Instance.ShowUserProfileCommand.Execute(userMatch.Groups["ScreenName"].ToString());
                    else
                        await Launcher.LaunchUriAsync(new Uri(linkUrl));
                });

            Notice.Instance.OpenStatusUrlCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var status = x as Status;
                    if (status == null)
                        return;

                    await Launcher.LaunchUriAsync(new Uri(status.Url));
                });

            Notice.Instance.OpenCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var column = x as ColumnModel;
                    if (column != null)
                        await Launcher.LaunchUriAsync(
                            new Uri("https://twitter.com/" + column.ScreenName + "/timelines/" +
                                    column.Parameter.ToString().Replace("custom-", "")));

                    var collection = x as Collection;
                    if (collection != null)
                        await Launcher.LaunchUriAsync(new Uri("https://twitter.com/" + collection.User.ScreenName +
                                                              "/timelines/" +
                                                              collection.Id.ToString().Replace("custom-", "")));
                });

            Notice.Instance.ShareStatusCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var status = x as Status;
                    if (status == null)
                        return;

                    var notification = new ShareDataNotification();
                    notification.Title = "@" + status.User.ScreenName;
                    notification.Description = status.Text;
                    notification.Url = status.Url;
                    notification.Text = status.Text;
                    ShowShareUIMessenger.Raise(notification);
                });

            Notice.Instance.ShowChangeAccountCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "AccountChange"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ChangeAccountCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    await Model.ChangeAccount(
                        AdvancedSettingService.AdvancedSetting.Accounts.First(y => y.UserId == (long) x));
                });

            Notice.Instance.ExitAppCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = new ResourceLoader().GetString("ConfirmDialog_ExitApp"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

                    Application.Current.Exit();
                });

            Notice.Instance.ShowMainSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "MainSetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowBehaviorSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "BehaviorSetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowPostingSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "PostingSetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowDisplaySettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "DisplaySetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowNotificationSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "NotificationSetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowMuteSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "MuteSetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowDatabaseSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "DatabaseSetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowAccountsSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "AccountsSetting"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowAccountSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification =
                        new ShowSettingsFlyoutNotification {SettingsFlyoutType = "AccountSetting", Content = x};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ShowAdvancedSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (LicenseService.License.AppDonationIsActive)
                    {
                        var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "AdvancedSetting"};
                        Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                    }
                    else
                    {
                        Notice.Instance.DonateCommand.Execute();
                    }
                });

            Notice.Instance.ShowColumnSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification =
                        new ShowSettingsFlyoutNotification {SettingsFlyoutType = "ColumnSetting", Content = x};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.DonateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (LicenseService.License.AppDonationIsActive)
                    {
                        await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification
                        {
                            Message = new ResourceLoader().GetString("ConfirmDialog_DonatedAlready"),
                            Title = "Donation"
                        });
                        return;
                    }

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = new ResourceLoader().GetString("ConfirmDialog_NeedAppDonation"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    var donateResult = await LicenseService.License.PurchaseAppDonation();

                    if (donateResult)
                        await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification
                        {
                            Message = new ResourceLoader().GetString("ConfirmDialog_DonateSuccessfully"),
                            Title = "Confirmation"
                        });
                    else
                        await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification
                        {
                            Message = new ResourceLoader().GetString("ConfirmDialog_FailedToDonate"),
                            Title = "Confirmation"
                        });
                });

            Notice.Instance.ShowAppInfoCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification {SettingsFlyoutType = "AppInfo"};
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                });

            Notice.Instance.ChangeBackgroundImageCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var result = await Notice.ShowFilePickerMessenger.Raise(new FileOpenPickerNotification
                    {
                        FileTypeFilter = new[] {".bmp", ".jpeg", ".jpg", ".png"},
                        SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                        ViewMode = PickerViewMode.Thumbnail
                    });

                    if (result.Result.Count() == 0)
                        return;

                    var file = result.Result.First();

                    await Model.ChangeBackgroundImage(file);
                });

            Notice.Instance.ChangeThemeCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var result = await Notice.ShowFilePickerMessenger.Raise(new FileOpenPickerNotification
                    {
                        FileTypeFilter = new[] {".xaml"},
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                        ViewMode = PickerViewMode.Thumbnail
                    });

                    if (result.Result.Count() == 0)
                        return;

                    var file = result.Result.First();

                    await Model.ChangeTheme(file);
                });

            Notice.Instance.MuteClientCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var client = x as string;
                    if (string.IsNullOrWhiteSpace(client))
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = new ResourceLoader().GetString("ConfirmDialog_MuteClient"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.MuteClient(client);
                });

            Notice.Instance.MuteWordCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var word = x as string;
                    if (string.IsNullOrWhiteSpace(word))
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = new ResourceLoader().GetString("ConfirmDialog_MuteWord"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.MuteWord(word);
                });

            Notice.Instance.DeleteMuteUserCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var screenName = x as string;
                    if (string.IsNullOrWhiteSpace(screenName))
                        return;

                    await Model.DeleteMuteUser(screenName);
                });

            Notice.Instance.DeleteMuteClientCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var client = x as string;
                    if (string.IsNullOrWhiteSpace(client))
                        return;

                    await Model.DeleteMuteClient(client);
                });

            Notice.Instance.DeleteMuteWordCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var word = x as string;
                    if (string.IsNullOrWhiteSpace(word))
                        return;

                    await Model.DeleteMuteWord(word);
                });

            Notice.Instance.UpdateMuteFilterCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var filter = x as string;
                    if (string.IsNullOrWhiteSpace(filter))
                        return;

                    // 禁忌 : Taboo
                    try
                    {
                        Compiler.Compile(filter, true);
                    }
                    catch (FilterCompileException fex)
                    {
                        var msgNotification = new MessageDialogNotification
                        {
                            Message = fex.Error.ToString() + "\n" + fex.Message,
                            Title = "Compile Error"
                        };
                        await Notice.ShowMessageDialogMessenger.Raise(msgNotification);
                        return;
                    }
                    catch (Exception ex)
                    {
                        var msgNotification =
                            new MessageDialogNotification
                            {
                                Message = ex.ToString() + "\n" + ex.Message,
                                Title = "Compile Error"
                            };
                        await Notice.ShowMessageDialogMessenger.Raise(msgNotification);
                        return;
                    }

                    SettingService.Setting.MuteFilter = filter;

                    await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification
                    {
                        Message = new ResourceLoader().GetString("ConfirmDialog_CompiledMuteFilterSuccessfully"),
                        Title = "Compile Filter"
                    });
                });

            Notice.Instance.AuthAccountCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var auth = new AuthorizeNotification();
                    await Notice.ShowAuthorizePopupMessenger.Raise(auth);
                    var account = auth.Result;

                    if (account == null)
                        return;

                    // 禁忌 : Taboo
                    if (AdvancedSettingService.AdvancedSetting.Accounts.Any(y => y.UserId == account.UserId))
                    {
                        var accountSetting =
                            AdvancedSettingService.AdvancedSetting.Accounts.First(y => y.UserId == account.UserId);
                        accountSetting.ScreenName = account.ScreenName;
                        accountSetting.ConsumerKey = account.ConsumerKey;
                        accountSetting.ConsumerSecret = account.ConsumerSecret;
                        accountSetting.AccessToken = account.AccessToken;
                        accountSetting.AccessTokenSecret = account.AccessTokenSecret;
                        accountSetting.Platform = account.Service == "Twitter"
                            ? SettingSupport.PlatformEnum.Twitter
                            : SettingSupport.PlatformEnum.Mastodon;
                        accountSetting.Instance = account.Instance;
                    }
                    else
                    {
                        var accountSetting = new AccountSetting
                        {
                            AccessToken = account.AccessToken,
                            AccessTokenSecret = account.AccessTokenSecret,
                            ConsumerKey = account.ConsumerKey,
                            ConsumerSecret = account.ConsumerSecret,
                            ScreenName = account.ScreenName,
                            UserId = account.UserId,
                            Platform = account.Service == "Twitter"
                                ? SettingSupport.PlatformEnum.Twitter
                                : SettingSupport.PlatformEnum.Mastodon,
                            Instance = account.Instance,

                            Column = new ObservableCollection<ColumnSetting>
                            {
                                new ColumnSetting
                                {
                                    Action = SettingSupport.ColumnTypeEnum.Home,
                                    AutoRefresh = false,
                                    AutoRefreshTimerInterval = 60.0,
                                    Filter = "()",
                                    Name = "Home",
                                    Parameter = string.Empty,
                                    Streaming = true,
                                    Index = 0,
                                    DisableStartupRefresh = false,
                                    FetchingNumberOfTweet = 100,
                                    Identifier = DateTime.Now.Ticks
                                },
                                new ColumnSetting
                                {
                                    Action = SettingSupport.ColumnTypeEnum.Mentions,
                                    AutoRefresh = false,
                                    AutoRefreshTimerInterval = 180.0,
                                    Filter = "()",
                                    Name = "Mentions",
                                    Parameter = string.Empty,
                                    Streaming = false,
                                    Index = 1,
                                    DisableStartupRefresh = false,
                                    FetchingNumberOfTweet = 40,
                                    Identifier = DateTime.Now.Ticks + 1
                                },
                                new ColumnSetting
                                {
                                    Action = SettingSupport.ColumnTypeEnum.DirectMessages,
                                    AutoRefresh = false,
                                    AutoRefreshTimerInterval = 180.0,
                                    Filter = "()",
                                    Name = "DirectMessages",
                                    Parameter = string.Empty,
                                    Streaming = false,
                                    Index = 2,
                                    DisableStartupRefresh = false,
                                    FetchingNumberOfTweet = 40,
                                    Identifier = DateTime.Now.Ticks + 2
                                },
                                new ColumnSetting
                                {
                                    Action = SettingSupport.ColumnTypeEnum.Events,
                                    AutoRefresh = false,
                                    AutoRefreshTimerInterval = 180.0,
                                    Filter = "()",
                                    Name = "Events",
                                    Parameter = string.Empty,
                                    Streaming = false,
                                    Index = 3,
                                    DisableStartupRefresh = false,
                                    FetchingNumberOfTweet = 100,
                                    Identifier = DateTime.Now.Ticks + 3
                                },
                                new ColumnSetting
                                {
                                    Action = SettingSupport.ColumnTypeEnum.Favorites,
                                    AutoRefresh = false,
                                    AutoRefreshTimerInterval = 180.0,
                                    Filter = "()",
                                    Name = "Favorites",
                                    Parameter = string.Empty,
                                    Streaming = false,
                                    Index = 4,
                                    DisableStartupRefresh = false,
                                    FetchingNumberOfTweet = 40,
                                    Identifier = DateTime.Now.Ticks + 4
                                }
                            },
                            IsEnabled = false
                        };
                        if (accountSetting.Platform == SettingSupport.PlatformEnum.Mastodon)
                            accountSetting.Column.Add(new ColumnSetting
                            {
                                Action = SettingSupport.ColumnTypeEnum.Sample,
                                AutoRefresh = false,
                                AutoRefreshTimerInterval = 180.0,
                                Filter = "()",
                                Name = "Local",
                                Parameter = string.Empty,
                                Streaming = true,
                                Index = 5,
                                DisableStartupRefresh = false,
                                FetchingNumberOfTweet = 40,
                                Identifier = DateTime.Now.Ticks + 5
                            });

                        Notice.Instance.AddAccountCommand.Execute(accountSetting);
                    }
                });

            Notice.Instance.AddAccountCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var accountSetting = x as AccountSetting;
                    if (accountSetting == null)
                        return;

                    await Model.AddAccount(accountSetting);
                });

            Notice.Instance.DeleteAccountCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var accountSetting = x as AccountSetting;
                    if (accountSetting == null)
                        return;

                    if (AdvancedSettingService.AdvancedSetting.Accounts.Count <= 1)
                    {
                        await Notice.ShowMessageDialogMessenger.Raise(new MessageDialogNotification
                        {
                            Message = new ResourceLoader().GetString("ConfirmDialog_CannotDeleteAccount"),
                            Title = "Error"
                        });
                        return;
                    }

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = new ResourceLoader().GetString("ConfirmDialog_DeleteAccount"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.DeleteAccount(accountSetting);
                });

            Notice.Instance.ChangeSelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var tweet = x;
                    if (!(tweet is StatusViewModel || tweet is DirectMessageViewModel ||
                          tweet is EventMessageViewModel))
                        return;

                    SelectedTweet.Value = tweet;
                });

            Notice.Instance.CopySelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (SelectedTweet.Value == null)
                        return;

                    if (SelectedTweet.Value is StatusViewModel)
                        Notice.Instance.CopyTweetCommand.Execute(((StatusViewModel) SelectedTweet.Value).Model);
                    else if (SelectedTweet.Value is DirectMessageViewModel)
                        Notice.Instance.CopyTweetCommand.Execute(((DirectMessageViewModel) SelectedTweet.Value).Model);
                });

            Notice.Instance.ReplyToSelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (SelectedTweet.Value == null)
                        return;

                    if (SelectedTweet.Value is StatusViewModel)
                    {
                        var tweet = SelectedTweet.Value;
                        Notice.Instance.ReplyCommand.Execute(tweet);
                    }
                    else
                    {
                        var tweet = SelectedTweet.Value;
                        var screenName = string.Empty;
                        if (tweet is DirectMessageViewModel)
                            screenName = ((DirectMessageViewModel) tweet).ScreenName;
                        else if (tweet is EventMessageViewModel)
                            screenName = ((EventMessageViewModel) tweet).ScreenName;

                        Notice.Instance.ReplyCommand.Execute(screenName);
                    }
                });

            Notice.Instance.SendDirectMessageToSelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (SelectedTweet.Value == null)
                        return;

                    var tweet = SelectedTweet.Value;
                    var screenName = string.Empty;
                    if (tweet is StatusViewModel)
                        screenName = ((StatusViewModel) tweet).ScreenName;
                    else if (tweet is DirectMessageViewModel)
                        screenName = ((DirectMessageViewModel) tweet).ScreenName;
                    else if (tweet is EventMessageViewModel)
                        screenName = ((EventMessageViewModel) tweet).ScreenName;

                    Notice.Instance.SendDirectMessageCommand.Execute(screenName);
                });

            Notice.Instance.FavoriteSelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var tweet = SelectedTweet.Value as StatusViewModel;
                    if (tweet == null)
                        return;

                    Notice.Instance.FavoriteCommand.Execute(tweet);
                });

            Notice.Instance.RetweetSelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var tweet = SelectedTweet.Value as StatusViewModel;
                    if (tweet == null)
                        return;

                    Notice.Instance.RetweetCommand.Execute(tweet);
                });

            Notice.Instance.ShowUserProfileOfSelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (SelectedTweet.Value == null)
                        return;

                    var tweet = SelectedTweet.Value;
                    var screenName = string.Empty;
                    if (tweet is StatusViewModel)
                        screenName = ((StatusViewModel) tweet).ScreenName;
                    else if (tweet is DirectMessageViewModel)
                        screenName = ((DirectMessageViewModel) tweet).ScreenName;
                    else if (tweet is EventMessageViewModel)
                        screenName = ((EventMessageViewModel) tweet).ScreenName;

                    Notice.Instance.ShowUserProfileCommand.Execute(screenName);
                });

            Notice.Instance.ShowConversationOfSelectedTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var tweet = SelectedTweet.Value as StatusViewModel;
                    if (tweet == null)
                        return;

                    Notice.Instance.ShowConversationCommand.Execute(tweet.Model);
                });

            Notice.Instance.DeleteDatabaseFileCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync("tweet.db");
                    if (item == null)
                        return;

                    await item.DeleteAsync();
                });

            #endregion

            Application.Current.Resuming += Application_Resuming;
            Application.Current.Suspending += Application_Suspending;
        }

        #endregion

        public MainPageModel Model { get; set; }

        public SettingService Setting { get; set; }

        public Notice Notice { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; }
        public ReactiveProperty<bool> TitleBarVisivility { get; }
        public ReactiveProperty<bool> AppBarIsOpen { get; }
        public ReactiveProperty<object> SelectedTweet { get; }

        public TweetAreaViewModel TweetArea { get; }

        public ReactiveCommand DragOverCommand { get; }
        public ReactiveCommand DropCommand { get; }

        public Messenger ShowSettingsFlyoutMessenger { get; }

        public Messenger ShowShareUIMessenger { get; }

        #region Destructor

        ~MainPageViewModel()
        {
            Application.Current.Resuming -= Application_Resuming;
            Application.Current.Suspending -= Application_Suspending;
        }

        #endregion

        #region Others

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            await Task.Run(async () => await Model.Initialize());

            Notice.Instance.TweetAreaAccountChangeCommand.Execute(Accounts.First(x => x.IsEnabled.Value));
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState,
            bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void Application_Suspending(object sender, object e)
        {
            Debug.WriteLine("Suspending...");
            foreach (var account in Accounts)
            {
                account.Model.StopTimer();
                foreach (var column in account.Columns)
                    column.IsScrollLockEnabled.Value = true;
            }
        }

        private async void Application_Resuming(object sender, object e)
        {
            Debug.WriteLine("Resuming...");
            foreach (var account in Accounts)
            {
                account.Model.StartTimer();
                foreach (var column in account.Columns)
                    if (column.IsEnabledStreaming.Value && column.Model.Streaming)
                        column.Model.ReconnectStreaming();
            }

            await Task.Delay(1250);

            foreach (var account in Accounts)
            foreach (var column in account.Columns)
                column.IsScrollLockEnabled.Value = false;
        }

        #endregion
    }
}