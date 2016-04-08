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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
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
        public AccountModel _AccountModel { get; set; }
        public Services.Notice Notice { get; set; }

        public ReadOnlyReactiveCollection<ColumnViewModel> Columns { get; private set; }
        public ObservableCollection<string> OtherColumnNames { get; private set; }
        private IDisposable OtherColumnNamesDisposable { get; set; }
        

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

        public ReactiveProperty<int> ColumnSelectedIndex { get; private set; }
        
        public ReactiveCommand ShowMyUserProfileCommand { get; private set; }

        public ReactiveCommand ShowMyUserListsCommand { get; private set; }

        public ReactiveCommand UpdateUserSearchCommand { get; private set; }

        public ReactiveCommand UpdateSearchCommand { get; private set; }

        public ReactiveCommand SuggestionsRequestedSearchCommand { get; private set; }

        #region Constructor
        /*public AccountViewModel()
        {
        }*/

        public AccountViewModel(AccountModel account)
        {
            this._AccountModel = account;
            this.ScreenName = account.ObserveProperty(x => x.ScreenName).ToReactiveProperty();
            this.Name = account.ObserveProperty(x => x.Name).ToReactiveProperty();
            this.ProfileImageUrl = account.ObserveProperty(x => x.ProfileImageUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty();
            this.ProfileBannerUrl = account.ObserveProperty(x => x.ProfileBannerUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty();
            this.IsEnabled = account.ObserveProperty(x => x.IsEnabled).ToReactiveProperty();

            this.LeftSwipeMenuIsOpen = account.ToReactivePropertyAsSynchronized(x => x.LeftSwipeMenuIsOpen);

            this.Columns = this._AccountModel.ReadOnlyColumns.ToReadOnlyReactiveCollection(x => new ColumnViewModel(x));

            this.OtherColumnNames = new ObservableCollection<string>(this._AccountModel.ReadOnlyColumns.ToObservable().Where(x => x.Name != "Home" && x.Name != "Mentions" && x.Name != "DirectMessages").Select(x => x.Name).ToEnumerable());
            this.OtherColumnNamesDisposable = this._AccountModel.ReadOnlyColumns.CollectionChangedAsObservable().SubscribeOnUIDispatcher().Subscribe<NotifyCollectionChangedEventArgs>(e => 
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
            });

            this.PanelWidth = Observable.CombineLatest<double, int, double>(
                LayoutHelper.Instance.ColumnWidth,
                this.Columns.ObserveProperty(x => x.Count),
                (width, count) =>
                {
                    if (width < 352.0)
                        return width * count + 352.0 * 2;
                    else
                        return (width + 10.0) * count + 352.0 * 2;
                }).ToReactiveProperty();

            this.SnapPointsSpaceing = LayoutHelper.Instance.ColumnWidth.Select(x => x + 10.0).ToReactiveProperty();

            this.MaxSnapPoint = Observable.CombineLatest<double, double, double>(this.PanelWidth, WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth), (panelWidth, windowWidth) => (panelWidth + 10.0) - windowWidth + 352.0).ToReactiveProperty();

			this.MinSnapPoint = new ReactiveProperty<double>(352.0);

            this.ColumnSelectedIndex = new ReactiveProperty<int>(0);

            this.BottomBarSearchBoxEnabled = Observable.CombineLatest(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth), 
                SettingService.Setting.ObserveProperty(x => x.BottomBarSearchBoxEnabled), 
                (clientWidth, searchBoxEnabled) =>
                {
                    return (clientWidth >= 960.0 && searchBoxEnabled && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile");
                }).ToReactiveProperty();


            this.Notice = Services.Notice.Instance;

            #region Command

            this.ShowMyUserProfileCommand = new ReactiveCommand();
            this.ShowMyUserProfileCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = this.ScreenName.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            this.ShowMyUserListsCommand = new ReactiveCommand();
            this.ShowMyUserListsCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserLists", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = this.ScreenName.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            this.UpdateSearchCommand = new ReactiveCommand();
            this.UpdateSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxQuerySubmittedEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(e.QueryText.TrimStart(new char[] { '#', '@' })))
                    return;

                Notice.ShowSearchCommand.Execute(e.QueryText);
            });

            this.UpdateUserSearchCommand = new ReactiveCommand();
            this.UpdateUserSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxResultSuggestionChosenEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(e.Tag))
                    return;

                Notice.ShowUserProfileCommand.Execute(e.Tag);
            });

            this.SuggestionsRequestedSearchCommand = new ReactiveCommand();
            this.SuggestionsRequestedSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxSuggestionsRequestedEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(e.QueryText.TrimStart(new char[] { '#', '@' })))
                    return;

                var deferral = e.Request.GetDeferral();

                IEnumerable<string> suggestHashtags = null;
                IEnumerable<Models.Twitter.Objects.User> suggestUsers = null;
                lock (Models.Services.Connecter.Instance.TweetCollecter[this._AccountModel.Tokens.UserId].EntitiesObjectsLock)
                {
                    suggestHashtags = Models.Services.Connecter.Instance.TweetCollecter[this._AccountModel.Tokens.UserId].HashTagObjects.Where(x => x.StartsWith(e.QueryText.TrimStart(new char[] { '#' }))).OrderBy(x => x);
                    suggestUsers = Models.Services.Connecter.Instance.TweetCollecter[this._AccountModel.Tokens.UserId].UserObjects.Where(x => x.ScreenName.StartsWith(e.QueryText.TrimStart(new char[] { '@' }))).OrderBy(x => x.ScreenName);
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
            });

            Services.Notice.Instance.LoadMentionCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                if (statusViewModel.Model.InReplyToStatusId == 0)
                    return;

                statusViewModel.IsMentionStatusLoading = true;

                await this._AccountModel.GetMentionStatus(statusViewModel.Model);

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
                statusViewModel.MentionStatusId = statusViewModel.Model.MentionStatus.Id;
                statusViewModel.MentionStatusName = statusViewModel.Model.MentionStatus.User.Name;
                statusViewModel.MentionStatusProfileImageUrl = string.IsNullOrWhiteSpace(statusViewModel.Model.MentionStatus.User.ProfileImageUrl) ? "http://localhost/" : statusViewModel.Model.MentionStatus.User.ProfileImageUrl;
                statusViewModel.MentionStatusScreenName = statusViewModel.Model.MentionStatus.User.ScreenName;
                statusViewModel.MentionStatusText = statusViewModel.Model.MentionStatus.Text;

                statusViewModel.OnPropertyChanged("MentionStatusEntities");
                statusViewModel.OnPropertyChanged("MentionStatusId");
                statusViewModel.OnPropertyChanged("MentionStatusName");
                statusViewModel.OnPropertyChanged("MentionStatusProfileImageUrl");
                statusViewModel.OnPropertyChanged("MentionStatusScreenName");
                statusViewModel.OnPropertyChanged("MentionStatusText");

                statusViewModel.IsMentionStatusLoading = false;
                statusViewModel.IsMentionStatusLoaded = true;

                statusViewModel.OnPropertyChanged("IsMentionStatusLoading");
                statusViewModel.OnPropertyChanged("IsMentionStatusLoaded");
            });

            Services.Notice.Instance.RetweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                // Taboo : 禁忌
                if (!statusViewModel.Model.IsRetweeted && SettingService.Setting.RetweetConfirmation)
                {
                    bool result = false;
                    Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(new ResourceLoader().GetString("ConfirmDialog_Retweet"), "Confirmation");
                    msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
                    msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
                    await msg.ShowAsync();

                    if (!result)
                        return;
                }

                if (!statusViewModel.Model.IsRetweeted)
                    await this._AccountModel.Retweet(statusViewModel.Model);
                else
                    await this._AccountModel.DestroyRetweet(statusViewModel.Model);

                statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                statusViewModel.OnPropertyChanged("IsRetweeted");

                statusViewModel.IsMyRetweet = (statusViewModel.Model.RetweetInformation != null && statusViewModel.Model.RetweetInformation.User.Id == this._AccountModel.UserId) || statusViewModel.Model.IsRetweeted;
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
            });

            Services.Notice.Instance.FavoriteCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                // Taboo : 禁忌
                if (!statusViewModel.Model.IsFavorited && SettingService.Setting.FavoriteConfirmation)
                {
                    bool result = false;
                    Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(new ResourceLoader().GetString("ConfirmDialog_Favorite"), "Confirmation");
                    msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
                    msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
                    await msg.ShowAsync();

                    if (!result)
                        return;
                }

                if (!statusViewModel.Model.IsFavorited)
                    await this._AccountModel.Favorite(statusViewModel.Model);
                else
                    await this._AccountModel.DestroyFavorite(statusViewModel.Model);

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
            });

            Services.Notice.Instance.RetweetFavoriteCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                // Taboo : 禁忌
                if (SettingService.Setting.RetweetConfirmation || SettingService.Setting.FavoriteConfirmation)
                {
                    bool result = false;
                    Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(new ResourceLoader().GetString("ConfirmDialog_RetweetFavorite"), "Confirmation");
                    msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
                    msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
                    await msg.ShowAsync();

                    if (!result)
                        return;
                }

                if (!statusViewModel.Model.IsRetweeted)
                    await this._AccountModel.Retweet(statusViewModel.Model);
                if (!statusViewModel.Model.IsFavorited)
                    await this._AccountModel.Favorite(statusViewModel.Model);

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
            });


            Services.Notice.Instance.DeleteTweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                if (x is Status)
                {
                    var status = x as Status;
                    await this._AccountModel.DestroyStatus(status.Id);
                }
                else if (x is DirectMessage)
                {
                    var directMessage = x as DirectMessage;
                    await this._AccountModel.DestroyDirectMessage(directMessage.Id);
                }
            });
            
            Services.Notice.Instance.DeleteRetweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;
                
                await this._AccountModel.DestroyRetweet(statusViewModel.Model);

                statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                statusViewModel.OnPropertyChanged("IsRetweeted");

                statusViewModel.IsMyRetweet = (statusViewModel.Model.RetweetInformation != null && statusViewModel.Model.RetweetInformation.User.Id == this._AccountModel.UserId) || statusViewModel.Model.IsRetweeted;
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
            });
            
            Services.Notice.Instance.ShowUserProfileCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowConversationCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Conversation", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Search", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowStatusDetailCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "StatusDetail", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var setting = x as ColumnSetting;
                if (setting == null)
                    return;
                
                // Taboo : 禁忌
                bool result = false;
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(new ResourceLoader().GetString("ConfirmDialog_AddColumn"), "Confirmation");
                msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
                msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
                await msg.ShowAsync();

                if (!result)
                    return;

                this._AccountModel.AddColumn(setting);
            });

            Services.Notice.Instance.SendDirectMessageCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "DirectMessageConversation", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowUserListsCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserLists", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowListStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "ListStatuses", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowListMembersCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "ListMembers", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowRetweetersCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Retweeters", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowRetweetsOfMeCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "RetweetsOfMe", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowUserFollowInfoCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserFollowInfo", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowLeftSwipeMenuCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var isOpen = false;
                if (!(x is bool))
                    isOpen = true;
                else if (x == null)
                    isOpen = true;
                else
                    isOpen = (bool)x;

                this.LeftSwipeMenuIsOpen.Value = isOpen;
            });

            Services.Notice.Instance.ShowSupportAccountCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this._AccountModel.Tokens, UserIcon = this.ProfileImageUrl.Value, Content = "Flantter" };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.MuteUserCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var screenName = x as string;
                if (string.IsNullOrWhiteSpace(screenName))
                    return;

                // Taboo : 禁忌
                bool result = false;
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(new ResourceLoader().GetString("ConfirmDialog_Mute"), "Confirmation");
                msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
                msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
                await msg.ShowAsync();

                if (result)
                    await this._AccountModel.CreateMute(screenName);
                
                // Taboo : 禁忌
                result = false;
                msg = new Windows.UI.Popups.MessageDialog(new ResourceLoader().GetString("ConfirmDialog_MuteInFlantter"), "Confirmation");
                msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
                msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
                await msg.ShowAsync();

                if (!result)
                    return;

                if (!AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(screenName))
                {
                    AdvancedSettingService.AdvancedSetting.MuteUsers.Add(screenName);
                    AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                }
            });

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
            this.ScreenName.Dispose();
            this.Name.Dispose();
            this.ProfileImageUrl.Dispose();
            this.ProfileBannerUrl.Dispose();
            this.IsEnabled.Dispose();
            this.Columns.Dispose();
            this.OtherColumnNamesDisposable.Dispose();
            this.PanelWidth.Dispose();
            this.SnapPointsSpaceing.Dispose();
            this.MaxSnapPoint.Dispose();
            this.MinSnapPoint.Dispose();
            this.ColumnSelectedIndex.Dispose();
        }
    }
}
