using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
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
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Flantter.MilkyWay.Models
{
    public class ColumnModel : BindableBase, IDisposable
    {
        protected CompositeDisposable Disposable { get; private set; } = new CompositeDisposable();

        #region Action変更通知プロパティ
        private SettingSupport.ColumnTypeEnum _Action;
        public SettingSupport.ColumnTypeEnum Action
        {
            get { return this._Action; }
            set { this.SetProperty(ref this._Action, value); }
        }
        #endregion

        #region Filter変更通知プロパティ
        private Delegate MuteFilterDelegate { get; set; }
        private Delegate FilterDelegate { get; set; }

        private string _Filter;
        public string Filter
        {
            get
            {
                return this._Filter;
            }
            set
            {
                if (this._Filter != value)
                {
                    this._Filter = value;
                    this.OnPropertyChanged("Filter");

                    try
                    {
                        this.FilterDelegate = Models.Filter.Compiler.Compile(this._Filter, false);
                    }
                    catch
                    {
                        this.FilterDelegate = null;
                    }
                }
            }
        }

        private string _MuteFilter;
        public string MuteFilter
        {
            get
            {
                return this._MuteFilter;
            }
            set
            {
                if (this._MuteFilter != value)
                {
                    this._MuteFilter = value;
                    this.OnPropertyChanged("MuteFilter");

                    try
                    {
                        this.MuteFilterDelegate = Models.Filter.Compiler.Compile(this._MuteFilter, true);
                    }
                    catch
                    {
                        this.MuteFilterDelegate = null;
                    }
                }
            }
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

        #region Index変更通知プロパティ
        private int _Index;
        public int Index
        {
            get { return this._Index; }
            set { this.SetProperty(ref this._Index, value); this.ColumnSetting.Index = value; }
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

        #region ProfileImageUrl変更通知プロパティ
        private string _ProfileImageUrl;
        public string ProfileImageUrl
        {
            get { return this._ProfileImageUrl; }
            set { this.SetProperty(ref this._ProfileImageUrl, value); }
        }
        #endregion

        #region Parameter変更通知プロパティ
        private string _Parameter;
        public string Parameter
        {
            get { return this._Parameter; }
            set { this.SetProperty(ref this._Parameter, value); }
        }
		#endregion

		#region Streaming変更通知プロパティ
		private bool _Streaming;
        public bool Streaming
		{
            get
            {
                return this._Streaming;
            }
            set
            {
                if (this._Streaming != value)
                {
                    this._Streaming = value;

                    if (value)
                        this.StartStreaming();
                    else
                        this.StopStreaming();

                    this.OnPropertyChanged("Streaming");
                }
            }
        }
        #endregion

        #region SelectedIndex変更通知プロパティ
        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return this._SelectedIndex; }
            set { this.SetProperty(ref this._SelectedIndex, value); }
        }
        #endregion

        #region Updating変更通知プロパティ
        private bool _Updating;
        public bool Updating
        {
            get { return this._Updating; }
            set { this.SetProperty(ref this._Updating, value); }
        }
        #endregion

        #region UnreadCount変更通知プロパティ
        private int _UnreadCount;
        public int UnreadCount
        {
            get { return this._UnreadCount; }
            set { this.SetProperty(ref this._UnreadCount, value); }
        }
        #endregion

        #region IsScrollLockEnabled変更通知プロパティ
        private bool _IsScrollLockEnabled;
        public bool IsScrollLockEnabled
        {
            get { return this._IsScrollLockEnabled; }
            set { this.SetProperty(ref this._IsScrollLockEnabled, value); }
        }
        #endregion
        

        #region Columns
        private ObservableCollection<ITweet> _Tweets;
        public ObservableCollection<ITweet> Tweets
        {
            get
            {
                return _Tweets;
            }
        }
        #endregion

        #region Tokens
        public Tokens Tokens { get; set; }
        #endregion

        #region AccountSetting
        public AccountSetting AccountSetting { get; set; }
        #endregion

        #region ColumnSetting
        public ColumnSetting ColumnSetting { get; set; }
        #endregion

        #region AccountModel
        private AccountModel _AccountModel { get; set; }
        #endregion

        #region Initialize
        public async Task Initialize()
        {
            this.Stream.SubscribeOn(NewThreadScheduler.Default).Subscribe(
                (Twitter.Objects.StreamingMessage m) =>
                {
                    switch (m.Type)
                    {
                        case StreamingMessage.MessageType.Create:
                            var status = m.Status;
                            var paramList = new List<string>();

                            // ミュートチェック
                            if (!this.MuteCheck(status))
                                break;

                            if (this._Action == SettingSupport.ColumnTypeEnum.Home)
                            {
                                paramList.Add("home://");
                                paramList.Add("filter://");
                                if (status.Entities.UserMentions != null && status.Entities.UserMentions.Any(x => x.Id == this.AccountSetting.UserId))
                                    paramList.Add("mentions://");
                            }
                            else if (this._Action == SettingSupport.ColumnTypeEnum.Search)
                            {
                                paramList.Add("search://" + this._Parameter);
                            }
                            else if (this._Action == SettingSupport.ColumnTypeEnum.List)
                            {
                                paramList.Add("list://" + this._Parameter);
                            }
                            else if (this._Action == SettingSupport.ColumnTypeEnum.Sample)
                            {
                                paramList.Add("sample://" + this._Parameter);
                            }

                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.AccountSetting.UserId, paramList, true));
                            
                            if (status.HasRetweetInformation && status.User.Id == this.AccountSetting.UserId && this.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.EventMessage(status), this.AccountSetting.UserId, new List<string>() { "events://" }, true));

                            break;
                        case StreamingMessage.MessageType.DirectMesssage:
                            var directMessage = m.DirectMessage;
                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(directMessage, this.AccountSetting.UserId, new List<string>() { "directmessages://" }, true));
                            break;
                        case StreamingMessage.MessageType.Event:
                            var eventMessage = m.EventMessage;
                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(eventMessage, this.AccountSetting.UserId, new List<string>() { "events://" }, true));

                            if (eventMessage.Type == "Favorite" && eventMessage.TargetStatus != null && eventMessage.Source.Id == this.AccountSetting.UserId && this.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                            {
                                eventMessage.TargetStatus.IsFavorited = true;
                                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(eventMessage.TargetStatus, this.AccountSetting.UserId, new List<string>() { "favorites://" }, true));
                            }
                            break;
                        case StreamingMessage.MessageType.DeleteStatus:
                            var deletedStatusId = m.DeletedStatusId;
                            Connecter.Instance.TweetDelete_OnCommandExecute(this, new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.Status, deletedStatusId, this.AccountSetting.UserId));
                            break;
                        case StreamingMessage.MessageType.DeleteDirectMessage:
                            var deletedDirectMessageId = m.DeletedDirectMessageId;
                            Connecter.Instance.TweetDelete_OnCommandExecute(this, new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.DirectMessage, deletedDirectMessageId, this.AccountSetting.UserId));
                            break;
                    }
                }
            ).AddTo(this.Disposable);

            Observable.Merge(
                Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                h => (sender, e) => h(e),
                h => Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].TweetReceive_CommandExecute += h,
                h => Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].TweetReceive_CommandExecute -= h).Select(e => (object)e),
                Observable.FromEvent<EventHandler<TweetDeleteEventArgs>, TweetDeleteEventArgs>(
                h => (sender, e) => h(e),
                h => Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].TweetDelete_CommandExecute += h,
                h => Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].TweetDelete_CommandExecute -= h).Select(e => (object)e)).SubscribeOn(ThreadPoolScheduler.Default).Subscribe(e =>
                {
                    if (e is TweetEventArgs)
                    {
                        var tweetEventArgs = (TweetEventArgs)e;

                        if (tweetEventArgs.Streaming == false && this.Action != SettingSupport.ColumnTypeEnum.Filter)
                            return;

                        switch (tweetEventArgs.Type)
                        {
                            case TweetEventArgs.TypeEnum.Status:
                                if (!this.Check(tweetEventArgs.Status, tweetEventArgs.Parameter))
                                    return;

                                if (this.Action == SettingSupport.ColumnTypeEnum.Favorites)
                                    this.Add(tweetEventArgs.Status, false);
                                else
                                    this.Add(tweetEventArgs.Status, tweetEventArgs.Streaming);

                                break;
                            case TweetEventArgs.TypeEnum.DirectMessage:
                                if (this.Action != SettingSupport.ColumnTypeEnum.DirectMessages)
                                    return;

                                this.Add(tweetEventArgs.DirectMessage, tweetEventArgs.Streaming);
                                break;
                            case TweetEventArgs.TypeEnum.EventMessage:
                                if (this.Action != SettingSupport.ColumnTypeEnum.Events || tweetEventArgs.EventMessage.Source.Id == this.AccountSetting.UserId)
                                    return;

                                this.Add(tweetEventArgs.EventMessage, tweetEventArgs.Streaming);
                                break;
                        }
                    }
                    else if (e is TweetDeleteEventArgs)
                    {
                        var tweetDeleteEventArgs = (TweetDeleteEventArgs)e;
                        this.Delete(tweetDeleteEventArgs.Id);
                    }
                }).AddTo(this.Disposable);

            SettingService.Setting.ObserveProperty(x => x.MuteFilter).Subscribe(x => this.MuteFilter = x).AddTo(this.Disposable);
            this.ColumnSetting.ObserveProperty(x => x.Filter).Subscribe(x => this.Filter = x).AddTo(this.Disposable);
            this.ColumnSetting.ObserveProperty(x => x.Name).Subscribe(x => this.Name = x).AddTo(this.Disposable);
            this.AccountSetting.ObserveProperty(x => x.ScreenName).Subscribe(x => this.ScreenName = x).AddTo(this.Disposable);
            this.AccountSetting.ObserveProperty(x => x.ProfileImageUrl).Subscribe(x => this.ProfileImageUrl = x).AddTo(this.Disposable);

            if (SettingService.Setting.RestoreTimelineOnStartup && SettingService.Setting.EnableDatabase)
            {
                switch (this.Action)
                {
                    case SettingSupport.ColumnTypeEnum.Collection:
                        foreach (var collection in Database.Instance.GetCollectionEntryFromParam(this.AccountSetting.UserId))
                        {
                            this.Add(collection);
                        }
                        break;
                    case SettingSupport.ColumnTypeEnum.DirectMessages:
                        foreach (var dm in Database.Instance.GetDirectMessagesFromParam(this.AccountSetting.UserId))
                        {
                            this.Add(dm);
                        }
                        break;
                    case SettingSupport.ColumnTypeEnum.Events:
                        foreach (var ev in Database.Instance.GetEventMessagesFromParam(this.AccountSetting.UserId))
                        {
                            this.Add(ev);
                        }
                        break;
                    default:
                        foreach (var status in Database.Instance.GetStatusesFromParam(this.Action.ToString("F").ToLower() + "://" + this._Parameter, this.AccountSetting.UserId))
                        {
                            if (!this.Check(status))
                                continue;

                            this.Add(status);
                        }
                        break;
                }
            }

            if (!this.ColumnSetting.DisableStartupRefresh && !SettingService.Setting.DisableStartupTimelineUpdate)
                await Update();
            
            this.Streaming = this.ColumnSetting.Streaming;
        }
        #endregion

        #region Constructor
        public ColumnModel(ColumnSetting column, AccountSetting account, AccountModel accountModel)
        {
            this._Tweets = new ObservableCollection<ITweet>();
            this.stream = new Subject<Twitter.Objects.StreamingMessage>();

            this._SelectedIndex = -1;

            this.Tokens = accountModel.Tokens;

            this.AccountSetting = account;
            this.ColumnSetting = column;

            this._AccountModel = accountModel;

            this.Name = this.ColumnSetting.Name;
            this.Action = this.ColumnSetting.Action;
            this.Index = this.ColumnSetting.Index;
            this.Parameter = this.ColumnSetting.Parameter;
        }
        #endregion

        private List<long> listStreamUserIdList = null;

        private IDisposable streamingDisposableObject = null;

        private Subject<Twitter.Objects.StreamingMessage> stream = null;
        private IObservable<Twitter.Objects.StreamingMessage> Stream
        {
            get { return this.stream.AsObservable(); }
        }
        
        private async void StartStreaming()
        {
            if (this.streamingDisposableObject != null)
                return;

            try
            {
                IObservable<Twitter.Objects.StreamingMessage> iObservable;

                if (this._Action == SettingSupport.ColumnTypeEnum.Home)
                {
                    var param = new Dictionary<string, object>();
                    if (this.AccountSetting.IncludeFollowingsActivity)
                        param.Add("include_followings_activity", true);
                    iObservable = this.Tokens.Streaming.UserAsObservable(param);
                }
                else if (this._Action == SettingSupport.ColumnTypeEnum.Search)
                {
                    if (this.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                        this._AccountModel.DisconnectAllFilterStreaming(this);

                    var param = new Dictionary<string, object>();
                    param.Add("track", this.ColumnSetting.Parameter.ToLower());
                    iObservable = this.Tokens.Streaming.FilterAsObservable(param);
                }
                else if (this.Action == SettingSupport.ColumnTypeEnum.List)
                {
                    if (this.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                        this._AccountModel.DisconnectAllFilterStreaming(this);

                    var userList = await this.Tokens.Lists.Members.ListAsync(list_id => long.Parse(this.Parameter), count => 4999);
                    listStreamUserIdList = userList.Select(x => x.Id).ToList();

                    var param = new Dictionary<string, object>();
                    param.Add("follow", string.Join(",", listStreamUserIdList));
                    iObservable = this.Tokens.Streaming.FilterAsObservable(param);
                }
                else if (this._Action == SettingSupport.ColumnTypeEnum.Sample)
                {
                    var param = new Dictionary<string, object>();
                    iObservable = this.Tokens.Streaming.SampleAsObservable(param);
                }
                else
                {
                    this.Streaming = false;
                    return;
                }

                this.streamingDisposableObject = iObservable.Catch((Exception ex) =>
                {
                    return iObservable.DelaySubscription(TimeSpan.FromSeconds(10)).Retry();
                }).Repeat().Subscribe(x => this.stream.OnNext(x), ex => this.stream.OnError(ex), () => this.stream.OnCompleted());
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                this.Streaming = false;
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
                this.Streaming = false;
            }
            catch (Exception ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                this.Streaming = false;
            }
        }

        private void StopStreaming()
        {
            if (this.streamingDisposableObject == null)
                return;

            try
            {
                this.streamingDisposableObject.Dispose();
            }
            catch
            {
            }
            finally
            {
                this.streamingDisposableObject = null;
            }
        }

        public void ReconnectStreaming()
        {
            this.StopStreaming();
            this.StartStreaming();

            this._Streaming = true;
            this.OnPropertyChanged("Streaming");
        }

        public async Task Update(long maxid = 0, long sinceid = 0)
        {
            if (this.Action == SettingSupport.ColumnTypeEnum.Filter)
                return;

            if (this.Updating)
                return;

            this.Updating = true;
            
            switch (this.Action)
            {
                case SettingSupport.ColumnTypeEnum.Home:
                    await UpdateHome(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.Mentions:
                    await UpdateMentions(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.DirectMessages:
                    await UpdateDirectMessages(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.Favorites:
                    await UpdateFavorites(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.List:
                    await UpdateList(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.Search:
                    await UpdateSearch(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.UserTimeline:
                    await UpdateUserTimeline(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.Events:
                    await UpdateEvents(maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.Collection:
                    await UpdateCollection(maxid, sinceid);
                    break;
            }

            this.Updating = false;
        }

        private async Task UpdateHome(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", this.ColumnSetting.FetchingNumberOfTweet }, { "include_entities", true }, { "tweet_mode", CoreTweet.TweetMode.extended } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var home = await this.Tokens.Statuses.HomeTimelineAsync(param);
                var lastId = home.Count > 0 ? home.OrderByDescending(x => x.Id).Last().Id : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in home)
                {
                    if (this.Check(status))
                        Add(status);

                    var paramList = new List<string>() { "home://", "filter://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.AccountSetting.UserId, paramList, false));
                }

                if (gapCheck)
                    Add(new Twitter.Objects.Gap(0, lastId - 1, DateTime.Now));
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

        private async Task UpdateMentions(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", this.ColumnSetting.FetchingNumberOfTweet }, { "include_entities", true }, { "tweet_mode", CoreTweet.TweetMode.extended } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var mentions = await this.Tokens.Statuses.MentionsTimelineAsync(param);
                var lastId = mentions.Count > 0 ? mentions.OrderByDescending(x => x.Id).Last().Id : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in mentions)
                {
                    if (this.Check(status))
                        Add(status);

                    var paramList = new List<string>() { "mentions://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.AccountSetting.UserId, paramList, false));
                }

                if (gapCheck)
                    Add(new Twitter.Objects.Gap(0, lastId - 1, DateTime.Now));
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

        private async Task UpdateDirectMessages(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", this.ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"full_text", true}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                IEnumerable<DirectMessage> directMessages = await this.Tokens.DirectMessages.ReceivedAsync(param);
                if (this.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                    directMessages = directMessages.Concat(await this.Tokens.DirectMessages.SentAsync(param));
                directMessages = directMessages.OrderByDescending(x => x.Id);

                foreach (var directMessage in directMessages)
                {
                    Add(directMessage);

                    var paramList = new List<string>() { "directmessages://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(directMessage, this.AccountSetting.UserId, paramList, false));
                }
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

        private async Task UpdateFavorites(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", this.ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var favorites = await this.Tokens.Favorites.ListAsync(param);
                var lastId = favorites.Count > 0 ? favorites.OrderByDescending(x => x.Id).Last().Id : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in favorites)
                {
                    if (this.Check(status))
                        Add(status);

                    var paramList = new List<string>() { "favorites://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.AccountSetting.UserId, paramList, false));
                }

                if (gapCheck)
                    Add(new Twitter.Objects.Gap(0, lastId - 1, DateTime.Now));
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

        private async Task UpdateList(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", this.ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"list_id", long.Parse(this._Parameter)},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var lists = await this.Tokens.Lists.StatusesAsync(param);
                var lastId = lists.Count > 0 ? lists.OrderByDescending(x => x.Id).Last().Id : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in lists)
                {
                    if (this.Check(status))
                        Add(status);

                    var paramList = new List<string>() { "list://" + this._Parameter };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.AccountSetting.UserId, paramList, false));
                }

                if (gapCheck)
                    Add(new Twitter.Objects.Gap(0, lastId - 1, DateTime.Now));
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

        private async Task UpdateSearch(long maxid = 0, long sinceid = 0)
        {
            try
            {
                IEnumerable<Twitter.Objects.Status> search = null;

                if (SettingService.Setting.UseOfficialApi &&
                    TwitterConnectionHelper.OfficialConsumerKeyList.Contains(this.AccountSetting.ConsumerKey))
                {
                    var param = new Dictionary<string, object>()
                    {
                        {"q", this._Parameter},
                        {"count", this.ColumnSetting.FetchingNumberOfTweet},
                        {"result_type", "recent"},
                        {"modules", "status"},
                        {"tweet_mode", CoreTweet.TweetMode.extended}
                    };
                    if (maxid != 0)
                        param["q"] = param["q"] + " max_id:" + maxid;
                    if (sinceid != 0)
                        param["q"] = param["q"] + " since_id:" + sinceid;

                    var res = await this.Tokens.TwitterTokens.SendRequestAsync(CoreTweet.MethodType.Get,
                        "https://api.twitter.com/1.1/search/universal.json", param);
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                    var modules = jsonObject["modules"].Children<Newtonsoft.Json.Linq.JObject>();

                    var tweets = new List<CoreTweet.Status>();
                    foreach (var status in modules)
                    {
                        foreach (Newtonsoft.Json.Linq.JProperty prop in status.Properties())
                        {
                            if (prop.Name == "status")
                                tweets.Add(CoreTweet.Core.CoreBase.Convert<CoreTweet.Status>(
                                    Newtonsoft.Json.JsonConvert.SerializeObject(status["status"]["data"])));
                        }
                    }

                    search = tweets.Select(x => new Twitter.Objects.Status(x));
                }
                else
                {
                    var param = new Dictionary<string, object>()
                    {
                        {"count", this.ColumnSetting.FetchingNumberOfTweet},
                        {"include_entities", true},
                        {"q", this._Parameter},
                        {"tweet_mode", CoreTweet.TweetMode.extended}
                    };
                    if (maxid != 0)
                        param.Add("max_id", maxid);
                    if (sinceid != 0)
                        param.Add("since_id", sinceid);

                    search = await this.Tokens.Search.TweetsAsync(param);
                }

                var lastId = search.Count() > 0 ? search.OrderByDescending(x => x.Id).Last().Id : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in search)
                {
                    if (this.Check(status))
                        Add(status);

                    var paramList = new List<string>() { "search://" + this._Parameter };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, this.AccountSetting.UserId, paramList, false));
                }

                if (gapCheck)
                    Add(new Twitter.Objects.Gap(0, lastId - 1, DateTime.Now));
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

        private async Task UpdateUserTimeline(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", this.ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"user_id", long.Parse(this._Parameter)},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var userTimeline = await this.Tokens.Statuses.UserTimelineAsync(param);
                var lastId = userTimeline.Count > 0 ? userTimeline.OrderByDescending(x => x.Id).Last().Id : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in userTimeline)
                {
                    if (this.Check(status))
                        Add(status);

                    var paramList = new List<string>() { "usertimeline://" + this._Parameter };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, this.AccountSetting.UserId, paramList, false));
                }

                if (gapCheck)
                    Add(new Twitter.Objects.Gap(0, lastId - 1, DateTime.Now));
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

        private async Task UpdateEvents(long maxid = 0, long sinceid = 0)
        {
            try
            {
                if (this.AccountSetting.Platform == SettingSupport.PlatformEnum.Mastodon)
                {
                    var param = new Dictionary<string, object>()
                    {
                        {"count", this.ColumnSetting.FetchingNumberOfTweet},
                        {"include_entities", true},
                        {"tweet_mode", CoreTweet.TweetMode.extended}
                    };
                    if (maxid != 0)
                        param.Add("max_id", maxid);
                    if (sinceid != 0)
                        param.Add("since_id", sinceid);

                    var events = await this.Tokens.Activity.AboutMeAsync(param);
                    var lastId = events.Count() > 0 ? events.OrderByDescending(x => x.Id).Last().Id : -1;
                    var gapCheck = GapCheck(lastId);
                    foreach (var ev in events)
                    {
                        var evObject = ev;
                        Add(evObject);

                        var paramList = new List<string>() { "events://" };
                        Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(evObject, this.AccountSetting.UserId, paramList, false));
                    }

                    if (gapCheck)
                        Add(new Twitter.Objects.Gap(0, lastId - 1, DateTime.Now));
                }
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                // Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }

        }

        private async Task UpdateCollection(long maxid = 0, long sinceid = 0)
        {
            if (this.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
            {
                var maxposition = this.Tweets.Where(x => x is CollectionEntry)
                    .Cast<CollectionEntry>()
                    .FirstOrDefault(x => x.Status?.Id == maxid)
                    ?.SortIndex;
                var minposition = this.Tweets.Where(x => x is CollectionEntry)
                    .Cast<CollectionEntry>()
                    .FirstOrDefault(x => x.Status?.Id == sinceid)
                    ?.SortIndex;

                try
                {
                    var param = new Dictionary<string, object>()
                    {
                        {"count", this.ColumnSetting.FetchingNumberOfTweet},
                        {"include_entities", true},
                        {"tweet_mode", CoreTweet.TweetMode.extended},
                        {"id", this.Parameter}
                    };
                    if (maxposition != 0)
                        param.Add("max_position", maxposition);
                    if (minposition != 0)
                        param.Add("min_position", minposition);

                    var entriesResult = await this.Tokens.Collections.EntriesAsync(param);
                    foreach (var collection in entriesResult)
                    {
                        Add(collection);

                        var paramList = new List<string>() {"collection://" + this._Parameter};
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(collection, this.AccountSetting.UserId, paramList, false));
                    }
                }
                catch (CoreTweet.TwitterException ex)
                {
                    Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System,
                        new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                }
                catch (NotImplementedException e)
                {
                    // Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
                }
                catch (Exception e)
                {
                    Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System,
                        new ResourceLoader().GetString("Notification_System_ErrorOccurred"),
                        new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                }
            }

        }

        // Streaming用ツイート受信時チェック
        private bool MuteCheck(Twitter.Objects.Status status)
        {
            lock (Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].MuteIdsLock)
            {
                if (Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].MuteIds.Contains(status.User.Id))
                    return false;

                if (status.HasRetweetInformation && Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].MuteIds.Contains(status.RetweetInformation.User.Id))
                    return false;

                if (status.HasRetweetInformation && Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].NoRetweetIds.Contains(status.RetweetInformation.User.Id))
                    return false;

                if (Connecter.Instance.TweetCollecter[this.AccountSetting.UserId].BlockIds.Contains(status.User.Id))
                    return false;
            }

            if (this._Action == SettingSupport.ColumnTypeEnum.List)
            {
                if (listStreamUserIdList == null)
                    return false;

                if (!listStreamUserIdList.Contains(status.HasRetweetInformation ? status.RetweetInformation.User.Id : status.User.Id))
                    return false;

                if (!status.HasRetweetInformation && status.InReplyToUserId != 0 && !listStreamUserIdList.Contains(status.InReplyToUserId))
                    return false;
            }

            return true;
        }

        // 通常ツイート追加時チェック
        private bool Check(Twitter.Objects.Status status)
        {
            if (this.Action != SettingSupport.ColumnTypeEnum.Mentions && this.Action != SettingSupport.ColumnTypeEnum.Favorites)
            {
                if (AdvancedSettingService.AdvancedSetting.MuteClients != null)
                {
                    if (AdvancedSettingService.AdvancedSetting.MuteClients.Contains(status.Source))
                        return false;
                }
                if (AdvancedSettingService.AdvancedSetting.MuteUsers != null)
                {
                    if (AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.User.ScreenName))
                        return false;
                    else if (status.HasRetweetInformation && AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.RetweetInformation.User.ScreenName))
                        return false;
                }
                if (AdvancedSettingService.AdvancedSetting.MuteWords != null)
                {
                    if (AdvancedSettingService.AdvancedSetting.MuteWords.Any(x => status.Text.Contains(x)))
                        return false;
                }

                if (this.MuteFilterDelegate != null)
                {
                    try
                    {
                        if ((bool)this.MuteFilterDelegate.DynamicInvoke(status))
                            return false;
                    }
                    catch
                    {
                    }
                }
            }

            if (this.FilterDelegate != null)
            {
                try
                {
                    if (!(bool)this.FilterDelegate.DynamicInvoke(status))
                        return false;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        // Streaming用ツイート追加時チェック
        private bool Check(Twitter.Objects.Status status, List<string> param)
        {
            if (!param.Contains(this.Action.ToString("F").ToLower() + "://" + this._Parameter))
            {
                // リストのストリームをホームから補完するためのチェック

                if (!Setting.SettingService.Setting.ComplementListStream)
                    return false;

                if (this.Action != SettingSupport.ColumnTypeEnum.List)
                {
                    if (!this.Streaming)
                        return false;

                    if (!param.Contains("home://") || !status.User.IsProtected)
                        return false;

                    if (listStreamUserIdList == null)
                        return false;

                    if (!listStreamUserIdList.Contains(status.HasRetweetInformation ? status.RetweetInformation.User.Id : status.User.Id))
                        return false;

                    if (!status.HasRetweetInformation && status.InReplyToUserId != 0 && !listStreamUserIdList.Contains(status.InReplyToUserId))
                        return false;
                }
                else
                {
                    return false;
                }                    
            }

            if (this.Action == SettingSupport.ColumnTypeEnum.Mentions && !SettingService.Setting.ShowRetweetInMentionColumn)
            {
                if (status.HasRetweetInformation)
                    return false;
            }

            if (this.Action != SettingSupport.ColumnTypeEnum.Mentions && this.Action != SettingSupport.ColumnTypeEnum.Favorites)
            {
                if (AdvancedSettingService.AdvancedSetting.MuteClients != null)
                {
                    if (AdvancedSettingService.AdvancedSetting.MuteClients.Contains(status.Source))
                        return false;
                }
                if (AdvancedSettingService.AdvancedSetting.MuteUsers != null)
                {
                    if (AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.User.ScreenName))
                        return false;
                    else if (status.HasRetweetInformation && AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.RetweetInformation.User.ScreenName))
                        return false;
                }
                if (AdvancedSettingService.AdvancedSetting.MuteWords != null)
                {
                    if (AdvancedSettingService.AdvancedSetting.MuteWords.Any(x => status.Text.Contains(x)))
                        return false;
                }

                if (this.MuteFilterDelegate != null)
                {
                    try
                    {
                        if ((bool)this.MuteFilterDelegate.DynamicInvoke(status))
                            return false;
                    }
                    catch
                    {
                    }
                }
            }

            if (this.FilterDelegate != null)
            {
                try
                {
                    if (!(bool)this.FilterDelegate.DynamicInvoke(status))
                        return false;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private void Add(Twitter.Objects.Status status, bool streaming = false)
        {
            if (streaming)
            {
                if (SettingService.Setting.RemoveRetweetOfMyTweet && status.HasRetweetInformation && status.User.ScreenName == this._ScreenName)
                    return;

                var retindex = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => (x as ITweet)?.Id == status.Id));
                if (retindex != -1 && SettingService.Setting.RemoveRetweetAlreadyReceive && status.HasRetweetInformation && status.RetweetInformation.User.ScreenName != this._ScreenName)
                    return;

                // 重複確認(ストリーミングでも中断時の更新によっては重複する可能性あり)
                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index != -1)
                    return;

                this._Tweets.Insert(0, status);
            }
            else
            {
                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this._Tweets.Add(status);
                    else
                        this._Tweets.Insert(index, status);
                }
            }
        }

        private void Add(Twitter.Objects.DirectMessage directMessage, bool streaming = false)
        {
            if (streaming)
            {
                var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.DirectMessage && (((Twitter.Objects.DirectMessage)x).Id == directMessage.Id)));
                if (index != -1)
                    return;

                this._Tweets.Insert(0, directMessage);
            }
            else
            {
                var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.DirectMessage && (((Twitter.Objects.DirectMessage)x).Id == directMessage.Id)));
                if (index == -1)
                {
                    index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.DirectMessage && (((Twitter.Objects.DirectMessage)x).Id < directMessage.Id)));
                    if (index == -1)
                        this._Tweets.Add(directMessage);
                    else
                        this._Tweets.Insert(index, directMessage);
                }
            }
        }

        private void Add(Twitter.Objects.EventMessage eventMessage, bool streaming = false)
        {
            switch (eventMessage.Type)
            {
                case "Favorite":
                case "Unfavorite":
                case "Follow":
                case "UserUpdate":
                case "FavoritedRetweet":
                case "RetweetedRetweet":
                case "QuotedTweet":
                case "Retweet":
                    break;
                default:
                    return;
            }

            if (streaming)
            {
                var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.EventMessage && (((Twitter.Objects.EventMessage)x).Id == eventMessage.Id)));
                if (index != -1)
                    return;

                this._Tweets.Insert(0, eventMessage);
            }
            else
            {
                var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.EventMessage && (((Twitter.Objects.EventMessage)x).Id == eventMessage.Id)));
                if (index == -1)
                {
                    index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.EventMessage && (((Twitter.Objects.EventMessage)x).Id < eventMessage.Id)));
                    if (index == -1)
                        this._Tweets.Add(eventMessage);
                    else
                        this._Tweets.Insert(index, eventMessage);
                }
            }
        }
        
        private void Add(CollectionEntry collection)
        {
            if (collection.Status.HasRetweetInformation)
                return;
            
            var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.CollectionEntry && (((Twitter.Objects.CollectionEntry)x).Id == collection.Id)));
            if (index == -1)
            {
                index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.CollectionEntry && (((Twitter.Objects.CollectionEntry)x).SortIndex < collection.SortIndex)));
                if (index == -1)
                    this._Tweets.Add(collection);
                else
                    this._Tweets.Insert(index, collection);
            }
        }

        private void Add(Twitter.Objects.Gap gap, bool streaming = false)
        {
            var index = this.Tweets.IndexOf(this.Tweets.FirstOrDefault(x =>
            {
                if (x is Twitter.Objects.Gap)
                    return false;

                var tid = ((ITweet)x).Id;
                if (x is Twitter.Objects.Status)
                    tid = ((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((ITweet)x).Id;

                return tid <= gap.MaxId;
            }));
            
            if (index == -1)
                return;

            this.Tweets.Insert(index, gap);
        }

        public void Delete(long id)
        {
            var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x =>
            {
                if (x is Twitter.Objects.Status)
                {
                    var status = x as Twitter.Objects.Status;
                    return ((status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id) == id);
                }
                else
                {
                    return x.Id == id;
                }
            }));

            if (index != -1)
                this._Tweets.RemoveAt(index);
        }

        public void Delete(Twitter.Objects.Gap gap)
        {
            if (!this.Tweets.Contains(gap))
                return;

            this.Tweets.Remove(gap);
        }

        public bool GapCheck(long id)
        {
            var index = this.Tweets.IndexOf(this.Tweets.FirstOrDefault(x => 
            {
                var tid = ((ITweet)x).Id;
                if (x is Twitter.Objects.Status)
                    tid = ((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((ITweet)x).Id;

                return tid == id;
            }));
            return (index == -1 && index != this.Tweets.Count - 1);
        }

        public void ClearColumn()
        {
            this.Tweets.Clear();

            if (SettingService.Setting.EnableDatabase)
                Database.Instance.ClearTweet(this.AccountSetting.UserId, this.Action.ToString("F").ToLower() + "://" + this._Parameter);
        }

        public void Dispose()
        {
            try
            {
                if (streamingDisposableObject != null)
                    streamingDisposableObject.Dispose();
            }
            catch
            {
            }

            try
            {
                this.Disposable.Dispose();
            }
            catch
            {
            }
        }
    }
}
