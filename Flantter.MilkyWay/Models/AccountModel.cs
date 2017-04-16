using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Mastonet;
using Flantter.MilkyWay.Models.Twitter.Wrapper;

namespace Flantter.MilkyWay.Models
{
    public class AccountModel : BindableBase, IDisposable
    {
        protected CompositeDisposable Disposable { get; private set; } = new CompositeDisposable();

        private IDisposable _TimerDisposable = null;

        #region IsEnabled変更通知プロパティ
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return this._IsEnabled; }
            set { this.SetProperty(ref this._IsEnabled, value); }
        }
        #endregion

        #region ProfileImageUrl変更通知プロパティ
        private string _ProfileImageUrl;
        public string ProfileImageUrl
        {
            get { return this._ProfileImageUrl; }
            set { this.SetProperty(ref this._ProfileImageUrl, value); }
        }
        #endregion

        #region ProfileBannerUrl変更通知プロパティ
        private string _ProfileBannerUrl;
        public string ProfileBannerUrl
        {
            get { return this._ProfileBannerUrl; }
            set { this.SetProperty(ref this._ProfileBannerUrl, value); }
        }
        #endregion

        #region Name変更通知プロパティ
        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set { this.SetProperty(ref this._Name, value); }
        }
        #endregion

        #region ScreenName変更通知プロパティ
        private string _ScreenName;
        public string ScreenName
        {
            get { return this._ScreenName; }
            set { this.SetProperty(ref this._ScreenName, value); }
        }
        #endregion

        #region LeftSwipeMenuIsOpen変更通知プロパティ
        private bool _LeftSwipeMenuIsOpen;
        public bool LeftSwipeMenuIsOpen
        {
            get { return this._LeftSwipeMenuIsOpen; }
            set { this.SetProperty(ref this._LeftSwipeMenuIsOpen, value); }
        }
        #endregion

        #region ColumnSelectedIndex変更通知プロパティ
        private int _ColumnSelectedIndex;
        public int ColumnSelectedIndex
        {
            get { return this._ColumnSelectedIndex; }
            set { this.SetProperty(ref this._ColumnSelectedIndex, value); }
        }
        #endregion

        #region Columns
        private ObservableCollection<ColumnModel> _Columns;
        private ReadOnlyObservableCollection<ColumnModel> _ReadOnlyColumns;
        public ReadOnlyObservableCollection<ColumnModel> ReadOnlyColumns
        {
            get
            {
                return _ReadOnlyColumns;
            }
        }
        #endregion

        #region Tokens
        public Tokens Tokens { get; set; }
        #endregion

        #region AccountSetting
        public AccountSetting AccountSetting { get; set; }
        #endregion

        #region Initialize
        public async Task Initialize()
        {
            Connecter.Instance.AddAccount(this.AccountSetting);
            
            await Task.WhenAll(this._Columns.Select(x => x.Initialize()));

            this.StartTimer();

            this.RefreshMuteIds();
            this.RefreshProfile();
        }
        #endregion

        #region Constructor
        public AccountModel(AccountSetting account)
        {
            this._Columns = new ObservableCollection<ColumnModel>();
            this._ReadOnlyColumns = new ReadOnlyObservableCollection<ColumnModel>(this._Columns);

            this.Tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret, account.UserId, account.ScreenName, account.Instance);
            this.Tokens.TwitterTokens.ConnectionOptions.UserAgent = TwitterConnectionHelper.GetUserAgent(this.Tokens.TwitterTokens);

            this.AccountSetting = account;
            
            this.IsEnabled = this.AccountSetting.IsEnabled;
            this.Name = this.AccountSetting.Name;
            this.ProfileBannerUrl = this.AccountSetting.ProfileBannerUrl;
            this.ProfileImageUrl = this.AccountSetting.ProfileImageUrl;
            this.ScreenName = this.AccountSetting.ScreenName;

            foreach (var column in account.Column)
                this._Columns.Add(new ColumnModel(column, account, this));
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
                column.Index = this._Columns.Count;

            column.Identifier = DateTime.Now.Ticks;

            this.AccountSetting.Column.Add(column);
            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

            var columnModel = new ColumnModel(column, this.AccountSetting, this);
            this._Columns.Add(columnModel);

            await columnModel.Initialize();
        }

        public async Task DeleteColumn(ColumnSetting column)
        {
            var columnModel = this._Columns.First(x => x.Action == column.Action && x.Name == column.Name && x.Parameter == column.Parameter && x.ColumnSetting.Identifier == column.Identifier);

            columnModel.Dispose();
            this._Columns.Remove(columnModel);
            
            this.AccountSetting.Column.Remove(column);

            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
        }

        public void DisconnectAllFilterStreaming(object sender = null)
        {
            foreach (var column in this._Columns)
            {
                if (column == sender)
                    continue;

                if (column.Action == SettingSupport.ColumnTypeEnum.Search || column.Action == SettingSupport.ColumnTypeEnum.List)
                    column.Streaming = false;
            }
        }

        public async Task GetMentionStatus(Twitter.Objects.Status status)
        {
            var mentionStatus = SettingService.Setting.EnableDatabase ? Database.Instance.GetStatusFromId(status.InReplyToStatusId) : null;
            if (mentionStatus == null)
            {
                try
                {
                    mentionStatus = await this.Tokens.Statuses.ShowAsync(id => status.InReplyToStatusId);
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(mentionStatus, this.AccountSetting.UserId, new List<string>() { "none://" }, false));
                }
                catch (CoreTweet.TwitterException ex)
                {
                    Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                }
                catch (NotImplementedException e)
                {
                    Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
                }
                catch (Exception e)
                {
                    Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                }
            }

            status.MentionStatus = mentionStatus;
        }

        public async Task Retweet(Twitter.Objects.Status status)
        {
            try
            {
                await this.Tokens.Statuses.RetweetAsync(id => status.Id);
                status.IsRetweeted = true;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyRetweet(Twitter.Objects.Status status)
        {
            try
            {
                await this.Tokens.Statuses.UnretweetAsync(id => status.Id);
                status.IsRetweeted = false;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task Favorite(Twitter.Objects.Status status)
        {
            try
            {
                if (SettingService.Setting.NotificateRetweetedRetweet && this.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                    await this.Tokens.Favorites.CreateAsync(id => status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id);
                else
                    await this.Tokens.Favorites.CreateAsync(id => status.Id);

                status.IsFavorited = true;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyFavorite(Twitter.Objects.Status status)
        {
            try
            {
                await this.Tokens.Favorites.DestroyAsync(id => status.Id);
                status.IsFavorited = false;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task CreateMute(long userId)
        {
            try
            {
                await this.Tokens.Mutes.Users.CreateAsync(user_id => userId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyStatus(long statusId)
        {
            try
            {
                await this.Tokens.Statuses.DestroyAsync(id => statusId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyDirectMessage(long directMessageId)
        {
            try
            {
                await this.Tokens.DirectMessages.DestroyAsync(id => directMessageId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DeleteTweetFromCollection(long statusId, string collectionId)
        {
            try
            {
                await this.Tokens.Collections.EntriesRemoveAsync(id => collectionId, tweet_id => statusId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }


        public async Task RefreshProfile()
        {
            try
            {
                var user = await this.Tokens.Users.ShowAsync(user_id => this.AccountSetting.UserId);
                this.ProfileImageUrl = user.ProfileImageUrl.Replace("_normal", "");
                this.ProfileBannerUrl = user.ProfileBannerUrl;
                this.Name = user.Name;
                this.ScreenName = user.ScreenName;

                this.AccountSetting.ProfileImageUrl = user.ProfileImageUrl.Replace("_normal", "");
                this.AccountSetting.ProfileBannerUrl = user.ProfileBannerUrl;
                this.AccountSetting.Name = user.Name;
                this.AccountSetting.ScreenName = user.ScreenName;
            }
            catch
            {
            }
            
            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
        }

        public async void RefreshMuteIds()
        {
            try
            {
                var noRetweetIds = await this.Tokens.Friendships.NoRetweetsIdsAsync();
                lock (Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].MuteIdsLock)
                {
                    Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].NoRetweetIds.Clear();

                    foreach (var id in noRetweetIds)
                        Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].NoRetweetIds.Add(id);
                }
            }
            catch
            {
            }

            try
            {
                var muteIds = await this.Tokens.Mutes.Users.IdsAsync();
                lock (Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].MuteIdsLock)
                {
                    Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].MuteIds.Clear();

                    foreach (var id in muteIds)
                        Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].MuteIds.Add(id);
                }
            }
            catch
            {
            }

            try
            {
                var blockIds = await this.Tokens.Blocks.IdsAsync();
                lock (Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].BlockIds)
                {
                    Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].BlockIds.Clear();

                    foreach (var id in blockIds)
                        Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].BlockIds.Add(id);
                }
            }
            catch
            {
            }
        }

        public void StartTimer()
        {
            if (_TimerDisposable != null)
                return;

            _TimerDisposable = Observable.Timer(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
                .SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async t =>
                {
                    foreach (var columnModel in this._Columns)
                    {
                        if (t % (int)columnModel.ColumnSetting.AutoRefreshTimerInterval == 0 && columnModel.ColumnSetting.AutoRefresh && !columnModel.Streaming)
                            await columnModel.Update();
                    }
                });
        }

        public void StopTimer()
        {
            if (_TimerDisposable == null)
                return;

            try
            {
                _TimerDisposable.Dispose();
            }
            catch
            {
            }
            finally
            {
                _TimerDisposable = null;
            }
        }

        public void Dispose()
        {
            this.StopTimer();

            foreach (var column in this._Columns)
            {
                column.Dispose();
            }

            this.Disposable.Dispose();
        }
    }
}
