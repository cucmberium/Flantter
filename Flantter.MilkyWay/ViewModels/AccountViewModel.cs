using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Apis;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels
{
    public class AccountViewModel : IDisposable
    {
        private readonly ResourceLoader _resourceLoader;

        #region Constructor

        public AccountViewModel(AccountModel account)
        {
            _resourceLoader = new ResourceLoader();

            Model = account;
            ScreenName = account.ObserveProperty(x => x.ScreenName).ToReactiveProperty().AddTo(Disposable);
            Name = account.ObserveProperty(x => x.Name).ToReactiveProperty().AddTo(Disposable);
            AccountName = Observable.CombineLatest(
                    account.ObserveProperty(x => x.ScreenName),
                    account.ObserveProperty(x => x.Instance),
                    (screenName, instance) => string.IsNullOrWhiteSpace(instance)
                        ? screenName
                        : screenName + "@" + instance
                )
                .ToReactiveProperty()
                .AddTo(Disposable);
            ProfileImageUrl = account.ObserveProperty(x => x.ProfileImageUrl)
                .Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/")
                .ToReactiveProperty()
                .AddTo(Disposable);
            ProfileBannerUrl = account.ObserveProperty(x => x.ProfileBannerUrl)
                .Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/")
                .ToReactiveProperty()
                .AddTo(Disposable);
            IsPlatformTwitter = account.ObserveProperty(x => x.Platform).Select(x => x == "Twitter")
                .ToReactiveProperty().AddTo(Disposable);
            IsPlatformMastodon = account.ObserveProperty(x => x.Platform).Select(x => x == "Mastodon")
                .ToReactiveProperty().AddTo(Disposable);

            IsEnabled = account.ObserveProperty(x => x.IsEnabled).ToReactiveProperty().AddTo(Disposable);

            LeftSwipeMenuIsOpen = account.ToReactivePropertyAsSynchronized(x => x.LeftSwipeMenuIsOpen)
                .AddTo(Disposable);

            Columns = Model.ReadOnlyColumns.ToReadOnlyReactiveCollection(x => new ColumnViewModel(x)).AddTo(Disposable);

            OtherColumnNames = new ObservableCollection<string>(Model.ReadOnlyColumns.ToObservable()
                .Where(x => x.Name != "Home" && x.Name != "Mentions" && x.Name != "DirectMessages")
                .Select(x => x.Name)
                .ToEnumerable());
            Model.ReadOnlyColumns.CollectionChangedAsObservable()
                .SubscribeOnUIDispatcher()
                .Subscribe(e =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var obj in e.NewItems)
                            {
                                var column = obj as ColumnModel;
                                if (column.Name == "Home" || column.Name == "Mentions" ||
                                    column.Name == "DirectMessages")
                                    continue;

                                OtherColumnNames.Add(column.Name);
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var obj in e.OldItems)
                            {
                                var column = obj as ColumnModel;
                                if (column.Name == "Home" || column.Name == "Mentions" ||
                                    column.Name == "DirectMessages")
                                    continue;

                                OtherColumnNames.Remove(column.Name);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                })
                .AddTo(Disposable);

            ReorderColumns =
                new ObservableCollection<ColumnViewModel>(Columns.OrderBy(x => x.Index.Value).AsEnumerable());
            Columns.CollectionChangedAsObservable()
                .SubscribeOnUIDispatcher()
                .Subscribe(e =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var obj in e.NewItems)
                            {
                                var column = obj as ColumnViewModel;
                                ReorderColumns.Add(column);
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var obj in e.OldItems)
                            {
                                var column = obj as ColumnViewModel;
                                ReorderColumns.Remove(column);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                })
                .AddTo(Disposable);

            ReorderColumns.CollectionChangedAsObservable()
                .SubscribeOnUIDispatcher()
                .Subscribe(async e =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var column in ReorderColumns.Select((x, i) => new {x, i}))
                                column.x.Model.Index = column.i;

                            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                            break;

                        case NotifyCollectionChangedAction.Add:
                            foreach (var column in ReorderColumns.Select((x, i) => new {x, i}))
                                column.x.Model.Index = column.i;

                            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                            break;
                    }
                })
                .AddTo(Disposable);

            PanelWidth = LayoutHelper.Instance.ColumnWidth.CombineLatest(
                    WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                    Columns.ObserveProperty(x => x.Count),
                    (width, winHeight, count) =>
                    {
                        if (winHeight >= 500)
                            if (width < 352.0)
                                return width * count + 352.0 * 2;
                            else
                                return (width + 10.0) * count + 352.0 * 2;
                        return width * count + 352.0 * 2;
                    })
                .ToReactiveProperty()
                .AddTo(Disposable);

            SnapPointsSpaceing = LayoutHelper.Instance.ColumnWidth.Select(x => x + 10.0)
                .ToReactiveProperty()
                .AddTo(Disposable);

            MaxSnapPoint = PanelWidth
                .Select(panelWidth => panelWidth + 10.0 - WindowSizeHelper.Instance.ClientWidth + 352.0)
                .ToReactiveProperty()
                .AddTo(Disposable);

            MinSnapPoint = new ReactiveProperty<double>(352.0).AddTo(Disposable);

            ColumnSelectedIndex = Model.ToReactivePropertyAsSynchronized(x => x.ColumnSelectedIndex).AddTo(Disposable);

            BottomBarSearchBoxEnabled = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth)
                .CombineLatest(SettingService.Setting.ObserveProperty(x => x.BottomBarSearchBoxEnabled),
                    (clientWidth, searchBoxEnabled) => clientWidth >= 960.0 && searchBoxEnabled &&
                                                       AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                .ToReactiveProperty()
                .AddTo(Disposable);

            IsZoomedInViewActive = new ReactiveProperty<bool>(true).AddTo(Disposable);

            IsTweetEnabled = new ReactiveProperty<bool>(false).AddTo(Disposable);

            ZoomOutOrientation = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth)
                .Select(x =>
                {
                    if (x > 500)
                        return Orientation.Horizontal;
                    return Orientation.Vertical;
                })
                .ToReactiveProperty()
                .AddTo(Disposable);

            Notice = Notice.Instance;

            #region Command

            ShowMyUserProfileCommand = new ReactiveCommand().AddTo(Disposable);
            ShowMyUserProfileCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserProfile",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = Model.AccountSetting.UserId
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            UpdateSearchCommand = new ReactiveCommand().AddTo(Disposable);
            UpdateSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(y =>
                {
                    var e = y as SearchBoxQuerySubmittedEventArgs;
                    if (string.IsNullOrWhiteSpace(e?.QueryText.TrimStart('#', '@')))
                        return;

                    Notice.ShowSearchCommand.Execute(e.QueryText);
                })
                .AddTo(Disposable);

            UpdateUserSearchCommand = new ReactiveCommand().AddTo(Disposable);
            UpdateUserSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(y =>
                {
                    var e = y as SearchBoxResultSuggestionChosenEventArgs;
                    if (string.IsNullOrWhiteSpace(e?.Tag))
                        return;

                    var userId = Connecter.Instance.TweetCollecter[Model.AccountSetting.UserId]
                        .UserObjects.First(x => x.ScreenName == e.Tag).Id;

                    Notice.ShowUserProfileCommand.Execute(userId);
                })
                .AddTo(Disposable);

            SuggestionsRequestedSearchCommand = new ReactiveCommand().AddTo(Disposable);
            SuggestionsRequestedSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(y =>
                {
                    var e = y as SearchBoxSuggestionsRequestedEventArgs;
                    if (string.IsNullOrWhiteSpace(e?.QueryText.TrimStart('#', '@')))
                        return;

                    var deferral = e.Request.GetDeferral();

                    IEnumerable<string> suggestHashtags;
                    IEnumerable<Models.Apis.Objects.User> suggestUsers;
                    lock (Connecter.Instance.TweetCollecter[Model.AccountSetting.UserId].EntitiesObjectsLock)
                    {
                        suggestHashtags = Connecter.Instance.TweetCollecter[Model.AccountSetting.UserId]
                            .HashTagObjects.Where(x => x.StartsWith(e.QueryText.TrimStart('#')))
                            .OrderBy(x => x);
                        suggestUsers = Connecter.Instance.TweetCollecter[Model.AccountSetting.UserId]
                            .UserObjects.Where(x => x.ScreenName.StartsWith(e.QueryText.TrimStart('@')))
                            .OrderBy(x => x.ScreenName);
                    }
                    if (suggestHashtags.Any())
                    {
                        e.Request.SearchSuggestionCollection.AppendSearchSeparator("HashTag");
                        foreach (var hashtag in suggestHashtags)
                            e.Request.SearchSuggestionCollection.AppendQuerySuggestion("#" + hashtag);
                    }
                    if (suggestUsers.Any())
                    {
                        e.Request.SearchSuggestionCollection.AppendSearchSeparator("User");
                        foreach (var user in suggestUsers)
                            e.Request.SearchSuggestionCollection.AppendResultSuggestion("@" + user.ScreenName,
                                user.Name,
                                user.ScreenName,
                                RandomAccessStreamReference.CreateFromUri(new Uri(string.IsNullOrWhiteSpace(user.ProfileImageUrl) ? "http://localhost/" : user.ProfileImageUrl)),
                                "Result");
                    }

                    deferral.Complete();
                })
                .AddTo(Disposable);

            Notice.Instance.SortColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(y =>
                {
                    IsZoomedInViewActive.Value = false;
                    LeftSwipeMenuIsOpen.Value = false;
                })
                .AddTo(Disposable);

            Notice.Instance.ChangeColumnSelectedIndexCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var column = x as ColumnViewModel;
                    ColumnSelectedIndex.Value = Columns.IndexOf(column);
                })
                .AddTo(Disposable);

            Notice.Instance.LoadMentionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    if (statusViewModel.Model.InReplyToStatusId == 0)
                        return;

                    statusViewModel.IsMentionStatusLoading = true;

                    await Model.GetMentionStatus(statusViewModel.Model);
                    await Task.Delay(50);

                    if (statusViewModel.Model.MentionStatus == null)
                    {
                        statusViewModel.IsMentionStatusLoading = false;
                        statusViewModel.MentionStatusVisibility = false;
                        statusViewModel.OnPropertyChanged("IsMentionStatusLoading");
                        statusViewModel.OnPropertyChanged("MentionStatusVisibility");
                        return;
                    }

                    // この設計はメモリ使用量削減に貢献しているのだろうか・・・？
                    
                    statusViewModel.MentionStatusName = statusViewModel.Model.MentionStatus.User.Name;
                    statusViewModel.MentionStatusProfileImageUrl =
                        string.IsNullOrWhiteSpace(statusViewModel.Model.MentionStatus.User.ProfileImageUrl)
                            ? "http://localhost/"
                            : statusViewModel.Model.MentionStatus.User.ProfileImageUrl;
                    statusViewModel.MentionStatusScreenName = statusViewModel.Model.MentionStatus.User.ScreenName;
                    statusViewModel.MentionStatusText = statusViewModel.Model.MentionStatus.Text;

                    statusViewModel.OnPropertyChanged("MentionStatusEntities");
                    statusViewModel.OnPropertyChanged("MentionStatusName");
                    statusViewModel.OnPropertyChanged("MentionStatusProfileImageUrl");
                    statusViewModel.OnPropertyChanged("MentionStatusScreenName");
                    statusViewModel.OnPropertyChanged("MentionStatusText");

                    statusViewModel.IsMentionStatusLoading = false;
                    statusViewModel.IsMentionStatusLoaded = true;

                    statusViewModel.OnPropertyChanged("IsMentionStatusLoading");
                    statusViewModel.OnPropertyChanged("IsMentionStatusLoaded");
                })
                .AddTo(Disposable);

            Notice.Instance.RetweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    if (!statusViewModel.Model.IsRetweeted && SettingService.Setting.RetweetConfirmation)
                    {
                        var msgNotification = new ConfirmMessageDialogNotification
                        {
                            Message = _resourceLoader.GetString("ConfirmDialog_Retweet"),
                            Title = "Confirmation"
                        };
                        await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                        if (!msgNotification.Result)
                            return;
                    }

                    if (!statusViewModel.Model.IsRetweeted)
                        await Model.Retweet(statusViewModel.Model);
                    else
                        await Model.DestroyRetweet(statusViewModel.Model);

                    statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                    statusViewModel.OnPropertyChanged("IsRetweeted");

                    statusViewModel.IsMyRetweet = statusViewModel.Model.RetweetInformation != null &&
                                                  statusViewModel.Model.RetweetInformation.User.Id ==
                                                  Model.AccountSetting.UserId || statusViewModel.Model.IsRetweeted;
                    statusViewModel.OnPropertyChanged("IsMyRetweet");

                    if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.FavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.FavoriteTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                    statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
                })
                .AddTo(Disposable);

            Notice.Instance.FavoriteCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    if (!statusViewModel.Model.IsFavorited && SettingService.Setting.FavoriteConfirmation)
                    {
                        var msgNotification = new ConfirmMessageDialogNotification
                        {
                            Message = _resourceLoader.GetString("ConfirmDialog_Favorite"),
                            Title = "Confirmation"
                        };
                        await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                        if (!msgNotification.Result)
                            return;
                    }

                    if (!statusViewModel.Model.IsFavorited)
                        await Model.Favorite(statusViewModel.Model);
                    else
                        await Model.DestroyFavorite(statusViewModel.Model);

                    statusViewModel.IsFavorited = statusViewModel.Model.IsFavorited;
                    statusViewModel.OnPropertyChanged("IsFavorited");

                    if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.FavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.FavoriteTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                    statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
                })
                .AddTo(Disposable);

            Notice.Instance.RetweetFavoriteCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    if (SettingService.Setting.RetweetConfirmation || SettingService.Setting.FavoriteConfirmation)
                    {
                        var msgNotification = new ConfirmMessageDialogNotification
                        {
                            Message = _resourceLoader.GetString("ConfirmDialog_RetweetFavorite"),
                            Title = "Confirmation"
                        };
                        await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                        if (!msgNotification.Result)
                            return;
                    }

                    if (!statusViewModel.Model.IsRetweeted)
                        await Model.Retweet(statusViewModel.Model);
                    if (!statusViewModel.Model.IsFavorited)
                        await Model.Favorite(statusViewModel.Model);

                    statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                    statusViewModel.OnPropertyChanged("IsRetweeted");
                    statusViewModel.IsFavorited = statusViewModel.Model.IsFavorited;
                    statusViewModel.OnPropertyChanged("IsFavorited");

                    if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.FavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.FavoriteTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                    statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
                })
                .AddTo(Disposable);

            Notice.Instance.DeleteTweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    if (x is Status status)
                        await Model.DestroyStatus(status.Id);
                    else if (x is DirectMessage directMessage)
                        await Model.DestroyDirectMessage(directMessage.Id);

                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ClearColumn"));
                })
                .AddTo(Disposable);

            Notice.Instance.DeleteRetweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    await Model.DestroyRetweet(statusViewModel.Model);

                    statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                    statusViewModel.OnPropertyChanged("IsRetweeted");

                    statusViewModel.IsMyRetweet = statusViewModel.Model.RetweetInformation != null &&
                                                  statusViewModel.Model.RetweetInformation.User.Id ==
                                                  Model.AccountSetting.UserId || statusViewModel.Model.IsRetweeted;
                    statusViewModel.OnPropertyChanged("IsMyRetweet");

                    if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.FavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.FavoriteTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetTriangleIconVisibility = false;
                    if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                    else
                        statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                    statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                    statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");

                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ClearColumn"));
                })
                .AddTo(Disposable);

            Notice.Instance.DeleteFromCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    await Model.DeleteTweetFromCollection(statusViewModel.Model.Id, statusViewModel.CollectionParameter);

                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ClearColumn"));
                })
                .AddTo(Disposable);

            Notice.Instance.AddToCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "AddToCollection",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.AddToListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "AddToList",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowUserProfileCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserProfile",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowConversationCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "Conversation",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "Search",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowStatusDetailCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "StatusDetail",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var setting = x as ColumnSetting;
                    if (setting == null)
                        return;

                    if (Model.AccountSetting.Column.Any(y => y.Action == setting.Action &&
                                                             y.Parameter == setting.Parameter &&
                                                             y.Name == setting.Name))
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_AddColumn"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.AddColumn(setting);
                })
                .AddTo(Disposable);

            Notice.Instance.SendDirectMessageCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "DirectMessageConversation",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowUserListsCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserLists",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowUserCollectionsCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserCollections",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowUserMediaStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserMediaStatuses",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowListStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "ListStatuses",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowListMembersCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "ListMembers",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowCollectionStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "CollectionStatuses",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowRetweetersCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "Retweeters",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowRetweetsOfMeCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "RetweetsOfMe",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowUserFollowInfoCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserFollowInfo",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowPublicTimelineCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "PublicTimeline",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = x
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowMyListsCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserLists",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = Model.Tokens.UserId
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowMyCollectionsCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserCollections",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = Model.AccountSetting.UserId
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.ShowMySimpleListsCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "SimpleUserLists",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = Model.Tokens.UserId
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.RetweetStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var items = x as IEnumerable;
                    if (items == null)
                        return;

                    var statusViewModels = items.Cast<StatusViewModel>();
                    var viewModels = statusViewModels as StatusViewModel[] ?? statusViewModels.ToArray();
                    if (!viewModels.Any())
                        return;

                    if (SettingService.Setting.RetweetConfirmation)
                    {
                        var msgNotification = new ConfirmMessageDialogNotification
                        {
                            Message = _resourceLoader.GetString("ConfirmDialog_RetweetFavorite"),
                            Title = "Confirmation"
                        };
                        await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                        if (!msgNotification.Result)
                            return;
                    }

                    foreach (var statusViewModel in viewModels)
                    {
                        if (!statusViewModel.Model.IsRetweeted)
                            await Model.Retweet(statusViewModel.Model);

                        statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                        statusViewModel.OnPropertyChanged("IsRetweeted");

                        statusViewModel.IsMyRetweet = statusViewModel.Model.RetweetInformation != null &&
                                                      statusViewModel.Model.RetweetInformation.User.Id ==
                                                      Model.AccountSetting.UserId || statusViewModel.Model.IsRetweeted;
                        statusViewModel.OnPropertyChanged("IsMyRetweet");

                        if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                            statusViewModel.FavoriteTriangleIconVisibility = true;
                        else
                            statusViewModel.FavoriteTriangleIconVisibility = false;
                        if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                            statusViewModel.RetweetTriangleIconVisibility = true;
                        else
                            statusViewModel.RetweetTriangleIconVisibility = false;
                        if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                            statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                        else
                            statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                        statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                        statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                        statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
                    }
                })
                .AddTo(Disposable);

            Notice.Instance.FavoriteStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var items = x as IEnumerable;
                    if (items == null)
                        return;

                    var statusViewModels = items.Cast<StatusViewModel>();
                    var viewModels = statusViewModels as StatusViewModel[] ?? statusViewModels.ToArray();
                    if (!viewModels.Any())
                        return;

                    if (SettingService.Setting.FavoriteConfirmation)
                    {
                        var msgNotification = new ConfirmMessageDialogNotification
                        {
                            Message = _resourceLoader.GetString("ConfirmDialog_Favorite"),
                            Title = "Confirmation"
                        };
                        await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                        if (!msgNotification.Result)
                            return;
                    }

                    foreach (var statusViewModel in viewModels)
                    {
                        if (!statusViewModel.Model.IsFavorited)
                            await Model.Favorite(statusViewModel.Model);

                        statusViewModel.IsFavorited = statusViewModel.Model.IsFavorited;
                        statusViewModel.OnPropertyChanged("IsFavorited");

                        if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                            statusViewModel.FavoriteTriangleIconVisibility = true;
                        else
                            statusViewModel.FavoriteTriangleIconVisibility = false;
                        if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                            statusViewModel.RetweetTriangleIconVisibility = true;
                        else
                            statusViewModel.RetweetTriangleIconVisibility = false;
                        if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                            statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                        else
                            statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                        statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                        statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                        statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
                    }
                })
                .AddTo(Disposable);

            Notice.Instance.ShowLeftSwipeMenuCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    bool isOpen;
                    if (!(x is bool))
                        isOpen = true;
                    else
                        isOpen = (bool) x;

                    LeftSwipeMenuIsOpen.Value = isOpen;
                })
                .AddTo(Disposable);

            Notice.Instance.ShowSupportAccountCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    if (Model.AccountSetting.Platform == SettingSupport.PlatformEnum.Mastodon)
                    {
                        await Launcher.LaunchUriAsync(new Uri("https://twitter.com/Flantter/"));
                        return;
                    }

                    var notification = new ShowSettingsFlyoutNotification
                    {
                        SettingsFlyoutType = "UserProfile",
                        Tokens = Model.Tokens,
                        UserIcon = ProfileImageUrl.Value,
                        Content = 2431920390L // FlantterアカウントのUserId
                    };
                    Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
                })
                .AddTo(Disposable);

            Notice.Instance.MuteUserCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var user = x as Models.Apis.Objects.User;
                    if (user == null)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_Mute"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (msgNotification.Result)
                        await Model.CreateMute(user.Id);

                    msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_MuteInFlantter"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.MuteUser(user.ScreenName);
                })
                .AddTo(Disposable);

            Notice.Instance.AddListColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var list = x as List;
                    if (list == null)
                        return;

                    var columnSetting = new ColumnSetting
                    {
                        Action = SettingSupport.ColumnTypeEnum.List,
                        AutoRefresh = false,
                        AutoRefreshTimerInterval = 180.0,
                        Filter = "()",
                        Name = "List : " + list.Name,
                        Parameter = list.Id.ToString(),
                        Streaming = false,
                        Index = -1,
                        DisableStartupRefresh = false,
                        FetchingNumberOfTweet = 40
                    };
                    Notice.Instance.AddColumnCommand.Execute(columnSetting);
                })
                .AddTo(Disposable);

            Notice.Instance.AddCollectionColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var collection = x as Collection;
                    if (collection == null)
                        return;

                    var columnSetting = new ColumnSetting
                    {
                        Action = SettingSupport.ColumnTypeEnum.Collection,
                        AutoRefresh = false,
                        AutoRefreshTimerInterval = 180.0,
                        Filter = "()",
                        Name = "Collection : " + collection.Name,
                        Parameter = collection.Id.ToString(),
                        Streaming = false,
                        Index = -1,
                        DisableStartupRefresh = false,
                        FetchingNumberOfTweet = 40
                    };
                    Notice.Instance.AddColumnCommand.Execute(columnSetting);
                })
                .AddTo(Disposable);

            Notice.Instance.DeleteColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var columnSetting = x as ColumnSetting;
                    if (columnSetting == null)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_DeleteColumn"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.DeleteColumn(columnSetting);
                })
                .AddTo(Disposable);

            Notice.Instance.AddFilterColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var columnSetting = new ColumnSetting
                    {
                        Action = SettingSupport.ColumnTypeEnum.Filter,
                        AutoRefresh = false,
                        AutoRefreshTimerInterval = 180.0,
                        Filter = "()",
                        Name = "Filter : " + "Fil1",
                        Streaming = false,
                        Index = -1,
                        DisableStartupRefresh = false,
                        FetchingNumberOfTweet = 40
                    };

                    if (Model.AccountSetting.Column.Any(y => y.Action == columnSetting.Action &&
                                                             y.Parameter == columnSetting.Parameter &&
                                                             y.Name == columnSetting.Name))
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = new ResourceLoader().GetString("ConfirmDialog_AddColumn"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.AddColumn(columnSetting);

                    Notice.Instance.ShowColumnSettingCommand.Execute(columnSetting);
                })
                .AddTo(Disposable);

            Notice.Instance.IncrementColumnSelectedIndexCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var columnIndex = Columns.ElementAt(ColumnSelectedIndex.Value).Index.Value;

                    if (columnIndex >= Columns.Count - 1)
                        return;

                    if (Columns.All(y => y.Index.Value != columnIndex + 1))
                        return;

                    ColumnSelectedIndex.Value = Columns.IndexOf(Columns.First(y => y.Index.Value == columnIndex + 1));
                })
                .AddTo(Disposable);

            Notice.Instance.DecrementColumnSelectedIndexCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(x =>
                {
                    var columnIndex = Columns.ElementAt(ColumnSelectedIndex.Value).Index.Value;

                    if (columnIndex <= 0)
                        return;

                    if (Columns.All(y => y.Index.Value != columnIndex - 1))
                        return;

                    ColumnSelectedIndex.Value = Columns.IndexOf(Columns.First(y => y.Index.Value == columnIndex - 1));
                })
                .AddTo(Disposable);

            Notice.Instance.UpdateAllTimelineCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x => { await Task.WhenAll(Model.ReadOnlyColumns.Select(y => y.Update())); })
                .AddTo(Disposable);

            Notice.Instance.GetGapStatusCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
                .Subscribe(async x =>
                {
                    var gapViewModel = x as GapViewModel;
                    if (gapViewModel == null)
                        return;

                    var column = Columns.First(y => y.Tweets.Contains(gapViewModel));
                    await column.Model.Update(gapViewModel.Model.MaxId);
                    column.Model.Delete(gapViewModel.Model);
                })
                .AddTo(Disposable);

            Notice.Instance.UrlClickCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Where(_ => Model.IsEnabled)
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
                    // var userMatch = TweetRegexPatterns.UserUrl.Match(linkUrl);
                    if (statusMatch.Success && Model.Tokens.Platform == Models.Apis.Wrapper.Tokens.PlatformEnum.Twitter)
                        Notice.Instance.ShowStatusDetailCommand.Execute(long.Parse(statusMatch.Groups["Id"]
                            .ToString()));
                    // else if (userMatch.Success)
                    //     Notice.Instance.ShowUserProfileCommand.Execute(userMatch.Groups["ScreenName"].ToString());
                    else
                        await Launcher.LaunchUriAsync(new Uri(linkUrl));
                });

            #endregion
        }

        #endregion

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public AccountModel Model { get; set; }
        public Notice Notice { get; set; }

        public ReadOnlyReactiveCollection<ColumnViewModel> Columns { get; }
        public ObservableCollection<string> OtherColumnNames { get; }
        public ObservableCollection<ColumnViewModel> ReorderColumns { get; }


        public ReactiveProperty<bool> LeftSwipeMenuIsOpen { get; }

        public ReactiveProperty<string> ProfileImageUrl { get; }
        public ReactiveProperty<string> ProfileBannerUrl { get; }
        public ReactiveProperty<string> ScreenName { get; }
        public ReactiveProperty<string> Name { get; }
        public ReactiveProperty<string> AccountName { get; }
        public ReactiveProperty<bool> IsPlatformTwitter { get; }
        public ReactiveProperty<bool> IsPlatformMastodon { get; }
        public ReactiveProperty<bool> IsEnabled { get; }

        public ReactiveProperty<double> PanelWidth { get; }
        public ReactiveProperty<double> SnapPointsSpaceing { get; }
        public ReactiveProperty<double> MaxSnapPoint { get; }
        public ReactiveProperty<double> MinSnapPoint { get; }

        public ReactiveProperty<bool> BottomBarSearchBoxEnabled { get; }

        public ReactiveProperty<bool> IsZoomedInViewActive { get; }

        public ReactiveProperty<bool> IsTweetEnabled { get; }

        public ReactiveProperty<Orientation> ZoomOutOrientation { get; }

        public ReactiveProperty<int> ColumnSelectedIndex { get; }

        public ReactiveCommand ShowMyUserProfileCommand { get; }

        public ReactiveCommand UpdateUserSearchCommand { get; }

        public ReactiveCommand UpdateSearchCommand { get; }

        public ReactiveCommand SuggestionsRequestedSearchCommand { get; }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}