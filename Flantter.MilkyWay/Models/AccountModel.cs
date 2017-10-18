using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Apis;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models
{
    public class AccountModel : BindableBase, IDisposable
    {
        private readonly ResourceLoader _resourceLoader;
        private IDisposable _timerDisposable;

        #region Constructor

        public AccountModel(AccountSetting account)
        {
            _resourceLoader = new ResourceLoader();

            _columns = new ObservableCollection<ColumnModel>();
            ReadOnlyColumns = new ReadOnlyObservableCollection<ColumnModel>(_columns);

            Tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken,
                account.AccessTokenSecret, account.UserId, account.ScreenName, account.Instance);
            Tokens.TwitterTokens.ConnectionOptions.UserAgent =
                TwitterConnectionHelper.GetUserAgent(Tokens.TwitterTokens);

            AccountSetting = account;

            IsEnabled = AccountSetting.IsEnabled;
            Name = AccountSetting.Name;
            ProfileBannerUrl = AccountSetting.ProfileBannerUrl;
            ProfileImageUrl = AccountSetting.ProfileImageUrl;
            ScreenName = AccountSetting.ScreenName;
            Platform = AccountSetting.Platform.ToString();
            Instance = AccountSetting.Instance;

            foreach (var column in account.Column)
                _columns.Add(new ColumnModel(column, account, this));
        }

        #endregion

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        #region Tokens

        public Tokens Tokens { get; set; }

        #endregion

        #region AccountSetting

        public AccountSetting AccountSetting { get; set; }

        #endregion

        public void Dispose()
        {
            StopTimer();

            foreach (var column in _columns)
                column.Dispose();

            Disposable.Dispose();
        }

        #region Initialize

        public async Task Initialize()
        {
            Connecter.Instance.AddAccount(AccountSetting);

            await Task.WhenAll(_columns.Select(x => x.Initialize()));

            StartTimer();

            RefreshMuteIds();
            RefreshProfile();
        }

        #endregion


        public async Task MuteUser(string screenName)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteUsers == null)
                AdvancedSettingService.AdvancedSetting.MuteUsers = new ObservableCollection<string>();

            if (!AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(screenName))
            {
                AdvancedSettingService.AdvancedSetting.MuteUsers.Add(screenName);
                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public async Task AddColumn(ColumnSetting column)
        {
            if (column.Index == -1)
                column.Index = _columns.Count;

            column.Identifier = DateTime.Now.Ticks;

            AccountSetting.Column.Add(column);
            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

            var columnModel = new ColumnModel(column, AccountSetting, this);
            _columns.Add(columnModel);

            await columnModel.Initialize();
        }

        public async Task DeleteColumn(ColumnSetting column)
        {
            var columnModel = _columns.First(x => x.Action == column.Action && x.Name == column.Name &&
                                                  x.Parameter == column.Parameter &&
                                                  x.ColumnSetting.Identifier == column.Identifier);

            columnModel.Dispose();
            _columns.Remove(columnModel);

            AccountSetting.Column.Remove(column);

            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
        }

        public void DisconnectAllFilterStreaming(object sender = null)
        {
            foreach (var column in _columns)
            {
                if (column == sender)
                    continue;

                if (column.Action == SettingSupport.ColumnTypeEnum.Search ||
                    column.Action == SettingSupport.ColumnTypeEnum.List)
                    column.Streaming = false;
            }
        }

        public async Task GetMentionStatus(Status status)
        {
            var mentionStatus = SettingService.Setting.EnableDatabase
                ? Database.Instance.GetStatusFromId(status.InReplyToStatusId)
                : null;
            if (mentionStatus == null)
                try
                {
                    mentionStatus = await Tokens.Statuses.ShowAsync(id => status.InReplyToStatusId);
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(mentionStatus, AccountSetting.UserId, new List<string> {"none://"}, false));
                }
                catch (CoreTweet.TwitterException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                }
                catch (NotImplementedException e)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_NotImplementedException"),
                        _resourceLoader.GetString("Notification_System_NotImplementedException"));
                }
                catch (Exception e)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                        _resourceLoader.GetString("Notification_System_CheckNetwork"));
                }

            status.MentionStatus = mentionStatus;
        }

        public async Task Retweet(Status status)
        {
            try
            {
                await Tokens.Statuses.RetweetAsync(id => status.Id);
                status.IsRetweeted = true;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyRetweet(Status status)
        {
            try
            {
                await Tokens.Statuses.UnretweetAsync(id => status.Id);
                status.IsRetweeted = false;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task Favorite(Status status)
        {
            try
            {
                if (SettingService.Setting.NotificateRetweetedRetweet &&
                    AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                    await Tokens.Favorites.CreateAsync(
                        id => status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id);
                else
                    await Tokens.Favorites.CreateAsync(id => status.Id);

                status.IsFavorited = true;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyFavorite(Status status)
        {
            try
            {
                await Tokens.Favorites.DestroyAsync(id => status.Id);
                status.IsFavorited = false;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task CreateMute(long userId)
        {
            try
            {
                await Tokens.Mutes.Users.CreateAsync(user_id => userId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyStatus(long statusId)
        {
            try
            {
                await Tokens.Statuses.DestroyAsync(id => statusId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyDirectMessage(long directMessageId)
        {
            try
            {
                await Tokens.DirectMessages.DestroyAsync(id => directMessageId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DeleteTweetFromCollection(long statusId, string collectionId)
        {
            try
            {
                await Tokens.Collections.EntriesRemoveAsync(id => collectionId, tweet_id => statusId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }


        public async Task RefreshProfile()
        {
            try
            {
                var user = await Tokens.Users.ShowAsync(user_id => AccountSetting.UserId);
                ProfileImageUrl = user.ProfileImageUrl.Replace("_normal", "");
                ProfileBannerUrl = user.ProfileBannerUrl;
                Name = user.Name;
                ScreenName = user.ScreenName;

                AccountSetting.ProfileImageUrl = user.ProfileImageUrl.Replace("_normal", "");
                AccountSetting.ProfileBannerUrl = user.ProfileBannerUrl;
                AccountSetting.Name = user.Name;
                AccountSetting.ScreenName = user.ScreenName;
            }
            catch
            {
            }

            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
        }

        public async Task RefreshMuteIds()
        {
            try
            {
                var noRetweetIds = await Tokens.Friendships.NoRetweetsIdsAsync();
                lock (Connecter.Instance.TweetCollecter[AccountSetting.UserId].MuteIdsLock)
                {
                    Connecter.Instance.TweetCollecter[AccountSetting.UserId].NoRetweetIds.Clear();

                    foreach (var id in noRetweetIds)
                        Connecter.Instance.TweetCollecter[AccountSetting.UserId].NoRetweetIds.Add(id);
                }
            }
            catch
            {
            }

            try
            {
                var muteIds = await Tokens.Mutes.Users.IdsAsync();
                lock (Connecter.Instance.TweetCollecter[AccountSetting.UserId].MuteIdsLock)
                {
                    Connecter.Instance.TweetCollecter[AccountSetting.UserId].MuteIds.Clear();

                    foreach (var id in muteIds)
                        Connecter.Instance.TweetCollecter[AccountSetting.UserId].MuteIds.Add(id);
                }
            }
            catch
            {
            }

            try
            {
                var blockIds = await Tokens.Blocks.IdsAsync();
                lock (Connecter.Instance.TweetCollecter[AccountSetting.UserId].BlockIds)
                {
                    Connecter.Instance.TweetCollecter[AccountSetting.UserId].BlockIds.Clear();

                    foreach (var id in blockIds)
                        Connecter.Instance.TweetCollecter[AccountSetting.UserId].BlockIds.Add(id);
                }
            }
            catch
            {
            }
        }

        public void StartTimer()
        {
            if (_timerDisposable != null)
                return;

            _timerDisposable = Observable.Timer(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
                .SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async t =>
                {
                    foreach (var columnModel in _columns)
                        if (t % (int) columnModel.ColumnSetting.AutoRefreshTimerInterval == 0 &&
                            columnModel.ColumnSetting.AutoRefresh && !columnModel.Streaming)
                            await columnModel.Update();
                });
        }

        public void StopTimer()
        {
            if (_timerDisposable == null)
                return;

            try
            {
                _timerDisposable.Dispose();
            }
            catch
            {
            }
            finally
            {
                _timerDisposable = null;
            }
        }

        #region IsEnabled変更通知プロパティ

        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        #endregion

        #region ProfileImageUrl変更通知プロパティ

        private string _profileImageUrl;

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => SetProperty(ref _profileImageUrl, value);
        }

        #endregion

        #region ProfileBannerUrl変更通知プロパティ

        private string _profileBannerUrl;

        public string ProfileBannerUrl
        {
            get => _profileBannerUrl;
            set => SetProperty(ref _profileBannerUrl, value);
        }

        #endregion

        #region Name変更通知プロパティ

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        #endregion

        #region ScreenName変更通知プロパティ

        private string _screenName;

        public string ScreenName
        {
            get => _screenName;
            set => SetProperty(ref _screenName, value);
        }

        #endregion

        #region Platform変更通知プロパティ

        private string _platform;

        public string Platform
        {
            get => _platform;
            set => SetProperty(ref _platform, value);
        }

        #endregion

        #region Instance変更通知プロパティ

        private string _instance;

        public string Instance
        {
            get => _instance;
            set => SetProperty(ref _instance, value);
        }

        #endregion

        #region LeftSwipeMenuIsOpen変更通知プロパティ

        private bool _leftSwipeMenuIsOpen;

        public bool LeftSwipeMenuIsOpen
        {
            get => _leftSwipeMenuIsOpen;
            set => SetProperty(ref _leftSwipeMenuIsOpen, value);
        }

        #endregion

        #region ColumnSelectedIndex変更通知プロパティ

        private int _columnSelectedIndex;

        public int ColumnSelectedIndex
        {
            get => _columnSelectedIndex;
            set => SetProperty(ref _columnSelectedIndex, value);
        }

        #endregion

        #region Columns

        private readonly ObservableCollection<ColumnModel> _columns;

        public ReadOnlyObservableCollection<ColumnModel> ReadOnlyColumns { get; }

        #endregion
    }
}