using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels
{
    public class AccountViewModel : IDisposable
    {
        protected CompositeDisposable Disposable { get; private set; } = new CompositeDisposable();

        public AccountModel Model { get; set; }
        public Services.Notice Notice { get; set; }

        public ReadOnlyReactiveCollection<ColumnViewModel> Columns { get; private set; }
        public ObservableCollection<string> OtherColumnNames { get; private set; }
        public ObservableCollection<ColumnViewModel> ReorderColumns { get; private set; }


        public ReactiveProperty<bool> LeftSwipeMenuIsOpen { get; private set; }

        public ReactiveProperty<string> ProfileImageUrl { get; private set; }
        public ReactiveProperty<string> ProfileBannerUrl { get; private set; }
        public ReactiveProperty<string> ScreenName { get; private set; }
        public ReactiveProperty<string> Name { get; private set; }
        public ReactiveProperty<bool> IsEnabled { get; private set; }

        public ReactiveProperty<double> PanelWidth { get; private set; }
        public ReactiveProperty<double> SnapPointsSpaceing { get; private set; }
        public ReactiveProperty<double> MaxSnapPoint { get; private set; }
		public ReactiveProperty<double> MinSnapPoint { get; private set; }

        public ReactiveProperty<bool> BottomBarSearchBoxEnabled { get; private set; }

        public ReactiveProperty<bool> IsZoomedInViewActive { get; private set; }

        public ReactiveProperty<Orientation> ZoomOutOrientation { get; private set; }

        public ReactiveProperty<int> ColumnSelectedIndex { get; private set; }

        public ReactiveCommand ShowMyUserProfileCommand { get; private set; }

        public ReactiveCommand UpdateUserSearchCommand { get; private set; }

        public ReactiveCommand UpdateSearchCommand { get; private set; }

        public ReactiveCommand SuggestionsRequestedSearchCommand { get; private set; }

        #region Constructor
        public AccountViewModel(AccountModel account)
        {
            this.Model = account;
            this.ScreenName = account.ObserveProperty(x => x.ScreenName).ToReactiveProperty().AddTo(this.Disposable);
            this.Name = account.ObserveProperty(x => x.Name).ToReactiveProperty().AddTo(this.Disposable);
            this.ProfileImageUrl = account.ObserveProperty(x => x.ProfileImageUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty().AddTo(this.Disposable);
            this.ProfileBannerUrl = account.ObserveProperty(x => x.ProfileBannerUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty().AddTo(this.Disposable);
            this.IsEnabled = account.ObserveProperty(x => x.IsEnabled).ToReactiveProperty().AddTo(this.Disposable);

            this.LeftSwipeMenuIsOpen = account.ToReactivePropertyAsSynchronized(x => x.LeftSwipeMenuIsOpen).AddTo(this.Disposable);

            this.Columns = this.Model.ReadOnlyColumns.ToReadOnlyReactiveCollection(x => new ColumnViewModel(x)).AddTo(this.Disposable);

            this.OtherColumnNames = new ObservableCollection<string>(this.Model.ReadOnlyColumns.ToObservable().Where(x => x.Name != "Home" && x.Name != "Mentions" && x.Name != "DirectMessages").Select(x => x.Name).ToEnumerable());
            this.Model.ReadOnlyColumns.CollectionChangedAsObservable().SubscribeOnUIDispatcher().Subscribe<NotifyCollectionChangedEventArgs>(e => 
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var obj in e.NewItems)
                        {
                            var column = obj as ColumnModel;
                            if (column.Name == "Home" || column.Name == "Mentions" || column.Name == "DirectMessages")
                                continue;

                            this.OtherColumnNames.Add(column.Name);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var obj in e.OldItems)
                        {
                            var column = obj as ColumnModel;
                            if (column.Name == "Home" || column.Name == "Mentions" || column.Name == "DirectMessages")
                                continue;

                            this.OtherColumnNames.Remove(column.Name);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Reset:
                        throw new NotImplementedException();
                }
            }).AddTo(this.Disposable);

            this.ReorderColumns = new ObservableCollection<ColumnViewModel>(this.Columns.OrderBy(x => x.Index.Value).AsEnumerable());
            this.Columns.CollectionChangedAsObservable().SubscribeOnUIDispatcher().Subscribe<NotifyCollectionChangedEventArgs>(e =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var obj in e.NewItems)
                        {
                            var column = obj as ColumnViewModel;
                            this.ReorderColumns.Add(column);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var obj in e.OldItems)
                        {
                            var column = obj as ColumnViewModel;
                            this.ReorderColumns.Remove(column);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Reset:
                        throw new NotImplementedException();
                }
            }).AddTo(this.Disposable);

            this.ReorderColumns.CollectionChangedAsObservable().SubscribeOnUIDispatcher().Subscribe<NotifyCollectionChangedEventArgs>(async e =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var column in this.ReorderColumns.Select((x, i) => new { x, i }))
                            column.x.Model.Index = column.i;

                        await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                        break;

                    case NotifyCollectionChangedAction.Add:
                        foreach (var column in this.ReorderColumns.Select((x, i) => new { x, i }))
                            column.x.Model.Index = column.i;

                        await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                        break;

                }
            }).AddTo(this.Disposable);

            this.PanelWidth = Observable.CombineLatest<double, double, int, double>(
                LayoutHelper.Instance.ColumnWidth,
                WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                this.Columns.ObserveProperty(x => x.Count),
                (width, winHeight, count) =>
                {
                    if (winHeight >= 500)
                    {
                        if (width < 352.0)
                            return width * count + 352.0 * 2;
                        else
                            return (width + 10.0) * count + 352.0 * 2;
                    }
                    else
                    {
                        return width * count + 352.0 * 2;
                    }

                }).ToReactiveProperty().AddTo(this.Disposable);

            this.SnapPointsSpaceing = LayoutHelper.Instance.ColumnWidth.Select(x => x + 10.0).ToReactiveProperty().AddTo(this.Disposable);

            this.MaxSnapPoint = this.PanelWidth.Select(panelWidth => (panelWidth + 10.0) - WindowSizeHelper.Instance.ClientWidth + 352.0).ToReactiveProperty().AddTo(this.Disposable);

			this.MinSnapPoint = new ReactiveProperty<double>(352.0).AddTo(this.Disposable);

            this.ColumnSelectedIndex = this.Model.ToReactivePropertyAsSynchronized(x => x.ColumnSelectedIndex).AddTo(this.Disposable);

            this.BottomBarSearchBoxEnabled = Observable.CombineLatest(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth), 
                SettingService.Setting.ObserveProperty(x => x.BottomBarSearchBoxEnabled), 
                (clientWidth, searchBoxEnabled) =>
                {
                    return (clientWidth >= 960.0 && searchBoxEnabled && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile");
                }).ToReactiveProperty().AddTo(this.Disposable);

            this.IsZoomedInViewActive = new ReactiveProperty<bool>(true).AddTo(this.Disposable);

            this.ZoomOutOrientation = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth).Select(x =>
            {
                if (x > 500)
                    return Orientation.Horizontal;
                else
                    return Orientation.Vertical;
            }).ToReactiveProperty().AddTo(this.Disposable);

            this.Notice = Services.Notice.Instance;

            #region Command

            this.ShowMyUserProfileCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.ShowMyUserProfileCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = this.ScreenName.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            this.UpdateSearchCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.UpdateSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxQuerySubmittedEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(e.QueryText.TrimStart(new char[] { '#', '@' })))
                    return;

                Notice.ShowSearchCommand.Execute(e.QueryText);
            }).AddTo(this.Disposable);

            this.UpdateUserSearchCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.UpdateUserSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxResultSuggestionChosenEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(e.Tag))
                    return;

                Notice.ShowUserProfileCommand.Execute(e.Tag);
            }).AddTo(this.Disposable);

            this.SuggestionsRequestedSearchCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.SuggestionsRequestedSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxSuggestionsRequestedEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(e.QueryText.TrimStart(new char[] { '#', '@' })))
                    return;

                var deferral = e.Request.GetDeferral();

                IEnumerable<string> suggestHashtags = null;
                IEnumerable<Models.Twitter.Objects.User> suggestUsers = null;
                lock (Models.Services.Connecter.Instance.TweetCollecter[this.Model.AccountSetting.UserId].EntitiesObjectsLock)
                {
                    suggestHashtags = Models.Services.Connecter.Instance.TweetCollecter[this.Model.AccountSetting.UserId].HashTagObjects.Where(x => x.StartsWith(e.QueryText.TrimStart(new char[] { '#' }))).OrderBy(x => x);
                    suggestUsers = Models.Services.Connecter.Instance.TweetCollecter[this.Model.AccountSetting.UserId].UserObjects.Where(x => x.ScreenName.StartsWith(e.QueryText.TrimStart(new char[] { '@' }))).OrderBy(x => x.ScreenName);
                }
                if (suggestHashtags.Count() > 0)
                {
                    e.Request.SearchSuggestionCollection.AppendSearchSeparator("HashTag");
                    foreach (var hashtag in suggestHashtags)
                        e.Request.SearchSuggestionCollection.AppendQuerySuggestion("#" + hashtag);
                }
                if (suggestUsers.Count() > 0)
                {
                    e.Request.SearchSuggestionCollection.AppendSearchSeparator("User");
                    foreach (var user in suggestUsers)
                        e.Request.SearchSuggestionCollection.AppendResultSuggestion("@" + user.ScreenName, user.Name, user.ScreenName, RandomAccessStreamReference.CreateFromUri(new Uri(user.ProfileImageUrl)), "Result");
                }

                deferral.Complete();
            }).AddTo(this.Disposable);
            
            Services.Notice.Instance.SortColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(y =>
            {
                this.IsZoomedInViewActive.Value = false;
                this.LeftSwipeMenuIsOpen.Value = false;
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ChangeColumnSelectedIndexCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var column = x as ColumnViewModel;
                this.ColumnSelectedIndex.Value = this.Columns.IndexOf(column);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.LoadMentionCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                if (statusViewModel.Model.InReplyToStatusId == 0)
                    return;

                statusViewModel.IsMentionStatusLoading = true;

                await this.Model.GetMentionStatus(statusViewModel.Model);
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

                statusViewModel.MentionStatusEntities = statusViewModel.Model.MentionStatus.Entities;
                statusViewModel.MentionStatusName = statusViewModel.Model.MentionStatus.User.Name;
                statusViewModel.MentionStatusProfileImageUrl = string.IsNullOrWhiteSpace(statusViewModel.Model.MentionStatus.User.ProfileImageUrl) ? "http://localhost/" : statusViewModel.Model.MentionStatus.User.ProfileImageUrl;
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
            }).AddTo(this.Disposable);

            Services.Notice.Instance.RetweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                if (statusViewModel.ScreenName == this.ScreenName.Value)
                    return;

                if (!statusViewModel.Model.IsRetweeted && SettingService.Setting.RetweetConfirmation)
                {
                    var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_Retweet"), Title = "Confirmation" };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;
                }

                if (!statusViewModel.Model.IsRetweeted)
                    await this.Model.Retweet(statusViewModel.Model);
                else
                    await this.Model.DestroyRetweet(statusViewModel.Model);

                statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                statusViewModel.OnPropertyChanged("IsRetweeted");

                statusViewModel.IsMyRetweet = (statusViewModel.Model.RetweetInformation != null && statusViewModel.Model.RetweetInformation.User.Id == this.Model.AccountSetting.UserId) || statusViewModel.Model.IsRetweeted;
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
            }).AddTo(this.Disposable);

            Services.Notice.Instance.FavoriteCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;
                
                if (!statusViewModel.Model.IsFavorited && SettingService.Setting.FavoriteConfirmation)
                {
                    var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_Favorite"), Title = "Confirmation" };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;
                }

                if (!statusViewModel.Model.IsFavorited)
                    await this.Model.Favorite(statusViewModel.Model);
                else
                    await this.Model.DestroyFavorite(statusViewModel.Model);

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
            }).AddTo(this.Disposable);

            Services.Notice.Instance.RetweetFavoriteCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;
                
                if (SettingService.Setting.RetweetConfirmation || SettingService.Setting.FavoriteConfirmation)
                {
                    var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_RetweetFavorite"), Title = "Confirmation" };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;
                }

                if (!statusViewModel.Model.IsRetweeted)
                    await this.Model.Retweet(statusViewModel.Model);
                if (!statusViewModel.Model.IsFavorited)
                    await this.Model.Favorite(statusViewModel.Model);

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
            }).AddTo(this.Disposable);
            
            Services.Notice.Instance.DeleteTweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                if (x is Status)
                {
                    var status = x as Status;
                    await this.Model.DestroyStatus(status.Id);
                }
                else if (x is DirectMessage)
                {
                    var directMessage = x as DirectMessage;
                    await this.Model.DestroyDirectMessage(directMessage.Id);
                }

                Models.Notifications.Core.Instance.PopupToastNotification(Models.Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ClearColumn"));

            }).AddTo(this.Disposable);
            
            Services.Notice.Instance.DeleteRetweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;
                
                await this.Model.DestroyRetweet(statusViewModel.Model);

                statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                statusViewModel.OnPropertyChanged("IsRetweeted");

                statusViewModel.IsMyRetweet = (statusViewModel.Model.RetweetInformation != null && statusViewModel.Model.RetweetInformation.User.Id == this.Model.AccountSetting.UserId) || statusViewModel.Model.IsRetweeted;
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

                Models.Notifications.Core.Instance.PopupToastNotification(Models.Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ClearColumn"));
            }).AddTo(this.Disposable);

            Services.Notice.Instance.DeleteFromCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                await this.Model.DeleteTweetFromCollection(statusViewModel.Id, statusViewModel.CollectionParameter);

                Models.Notifications.Core.Instance.PopupToastNotification(Models.Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ClearColumn"));

            }).AddTo(this.Disposable);

            Services.Notice.Instance.AddToCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "AddToCollection", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);


            Services.Notice.Instance.ShowUserProfileCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowConversationCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Conversation", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Search", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowStatusDetailCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "StatusDetail", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var setting = x as ColumnSetting;
                if (setting == null)
                    return;

                if (this.Model.AccountSetting.Column.Any(y => y.Action == setting.Action && y.Parameter == setting.Parameter && y.Name == setting.Name))
                    return;

                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_AddColumn"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                await this.Model.AddColumn(setting);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.SendDirectMessageCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "DirectMessageConversation", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowUserListsCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserLists", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowUserCollectionsCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserCollections", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowListStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "ListStatuses", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowListMembersCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "ListMembers", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowCollectionStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "CollectionStatuses", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowRetweetersCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Retweeters", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowRetweetsOfMeCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "RetweetsOfMe", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowUserFollowInfoCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserFollowInfo", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowMyListsCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserLists", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = this.ScreenName.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowMyCollectionsCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserCollections", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = this.ScreenName.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.RetweetStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var items = x as IEnumerable;
                if (items == null)
                    return;

                var statusViewModels = items.Cast<StatusViewModel>();
                if (statusViewModels.Count() == 0)
                    return;

                if (SettingService.Setting.RetweetConfirmation)
                {
                    var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_RetweetFavorite"), Title = "Confirmation" };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;
                }

                foreach (var statusViewModel in statusViewModels)
                {
                    if (!statusViewModel.Model.IsRetweeted)
                        await this.Model.Retweet(statusViewModel.Model);

                    statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                    statusViewModel.OnPropertyChanged("IsRetweeted");

                    statusViewModel.IsMyRetweet = (statusViewModel.Model.RetweetInformation != null && statusViewModel.Model.RetweetInformation.User.Id == this.Model.AccountSetting.UserId) || statusViewModel.Model.IsRetweeted;
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

            }).AddTo(this.Disposable);

            Services.Notice.Instance.FavoriteStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var items = x as IEnumerable;
                if (items == null)
                    return;

                var statusViewModels = items.Cast<StatusViewModel>();
                if (statusViewModels.Count() == 0)
                    return;

                if (SettingService.Setting.FavoriteConfirmation)
                {
                    var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_Favorite"), Title = "Confirmation" };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;
                }

                foreach (var statusViewModel in statusViewModels)
                {
                    if (!statusViewModel.Model.IsFavorited)
                        await this.Model.Favorite(statusViewModel.Model);

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

            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowLeftSwipeMenuCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var isOpen = false;
                if (!(x is bool))
                    isOpen = true;
                else if (x == null)
                    isOpen = true;
                else
                    isOpen = (bool)x;

                this.LeftSwipeMenuIsOpen.Value = isOpen;
            }).AddTo(this.Disposable);

            Services.Notice.Instance.ShowSupportAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this.Model.CoreTweetTokens, UserIcon = this.ProfileImageUrl.Value, Content = "Flantter" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.MuteUserCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var user = x as Models.Twitter.Objects.User;
                if (user == null)
                    return;

                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_Mute"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (msgNotification.Result)
                    await this.Model.CreateMute(user.Id);

                msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_MuteInFlantter"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                await this.Model.MuteUser(user.ScreenName);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.AddListColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var list = x as List;
                if (list == null)
                    return;

                var columnSetting = new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.List, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = ("List : " + list.FullName), Parameter = list.Id.ToString(), Streaming = false, Index = -1, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 };
                Services.Notice.Instance.AddColumnCommand.Execute(columnSetting);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.AddCollectionColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var collection = x as Collection;
                if (collection == null)
                    return;

                var columnSetting = new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Collection, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = ("Collection : " + collection.Name), Parameter = collection.Id.ToString(), Streaming = false, Index = -1, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 };
                Services.Notice.Instance.AddColumnCommand.Execute(columnSetting);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.DeleteColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var columnSetting = x as ColumnSetting;
                if (columnSetting == null)
                    return;
                
                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_DeleteColumn"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                await this.Model.DeleteColumn(columnSetting);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.AddFilterColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var columnSetting = new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Filter, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = ("Filter : " + "Fil1"), Streaming = false, Index = -1, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 };

                if (this.Model.AccountSetting.Column.Any(y => y.Action == columnSetting.Action && y.Parameter == columnSetting.Parameter && y.Name == columnSetting.Name))
                    return;

                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_AddColumn"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                await this.Model.AddColumn(columnSetting);

                Services.Notice.Instance.ShowColumnSettingCommand.Execute(columnSetting);
            }).AddTo(this.Disposable);

            Services.Notice.Instance.IncrementColumnSelectedIndexCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var columnIndex = this.Columns.ElementAt(this.ColumnSelectedIndex.Value).Index.Value;

                if (columnIndex >= this.Columns.Count - 1)
                    return;

                if (!this.Columns.Any(y => y.Index.Value == columnIndex + 1))
                    return;

                this.ColumnSelectedIndex.Value = this.Columns.IndexOf(this.Columns.First(y => y.Index.Value == columnIndex + 1));
            }).AddTo(this.Disposable);

            Services.Notice.Instance.DecrementColumnSelectedIndexCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(x =>
            {
                var columnIndex = this.Columns.ElementAt(this.ColumnSelectedIndex.Value).Index.Value;

                if (columnIndex <= 0)
                    return;

                if (!this.Columns.Any(y => y.Index.Value == columnIndex - 1))
                    return;

                this.ColumnSelectedIndex.Value = this.Columns.IndexOf(this.Columns.First(y => y.Index.Value == columnIndex - 1));
            }).AddTo(this.Disposable);

            Services.Notice.Instance.GetGapStatusCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this.Model.IsEnabled).Subscribe(async x =>
            {
                var gapViewModel = x as GapViewModel;
                if (gapViewModel == null)
                    return;

                var column = this.Columns.First(y => y.Tweets.Contains(gapViewModel));
                await column.Model.Update(maxid: gapViewModel.Model.MaxId);
                column.Model.Delete(gapViewModel.Model);

            }).AddTo(this.Disposable);

            #endregion
        }
        #endregion

        #region Destructor
        ~AccountViewModel()
        {
        }
        #endregion

        public void Dispose()
        {
            this.Disposable.Dispose();
        }
    }
}
