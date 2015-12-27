using CoreTweet;
using CoreTweet.Core;
using CoreTweet.Streaming;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Flantter.MilkyWay.Models
{
    public class ColumnModel : BindableBase
    {
        #region Action変更通知プロパティ
        private SettingSupport.ColumnTypeEnum _Action;
        public SettingSupport.ColumnTypeEnum Action
        {
            get { return this._Action; }
            set { this.SetProperty(ref this._Action, value); }
        }
        #endregion

        #region AutoRefresh変更通知プロパティ
        private bool _AutoRefresh;
        public bool AutoRefresh
        {
            get { return this._AutoRefresh; }
            set { this.SetProperty(ref this._AutoRefresh, value); }
        }
        #endregion

        #region AutoRefreshTimerInterval変更通知プロパティ
        private double _AutoRefreshTimerInterval;
        public double AutoRefreshTimerInterval
        {
            get { return this._AutoRefreshTimerInterval; }
            set { this.SetProperty(ref this._AutoRefreshTimerInterval, value); }
        }
        #endregion

        #region DisableStartupRefresh変更通知プロパティ
        private bool _DisableStartupRefresh;
        public bool DisableStartupRefresh
        {
            get { return this._DisableStartupRefresh; }
            set { this.SetProperty(ref this._DisableStartupRefresh, value); }
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
                        this.FilterDelegate = Models.Filter.Compiler.Compile(this._Filter);
                    }
                    catch
                    {
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
            set { this.SetProperty(ref this._Index, value); }
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

        #region FetchingNumberOfTweet変更通知プロパティ
        private int _FetchingNumberOfTweet;
        public int FetchingNumberOfTweet
        {
            get { return this._FetchingNumberOfTweet; }
            set { this.SetProperty(ref this._FetchingNumberOfTweet, value); }
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

        #region IsScrollControlEnabled変更通知プロパティ
        private bool _IsScrollControlEnabled;
        public bool IsScrollControlEnabled
        {
            get { return this._IsScrollControlEnabled; }
            set { this.SetProperty(ref this._IsScrollControlEnabled, value); }
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

        #region UnreadCount変更通知プロパティ
        private int _UnreadCount;
        public int UnreadCount
        {
            get { return this._UnreadCount; }
            set { this.SetProperty(ref this._UnreadCount, value); }
        }
        #endregion

        #region UnreadCountIncrementalTrigger変更通知プロパティ
        private bool _UnreadCountIncrementalTrigger;
        public bool UnreadCountIncrementalTrigger
        {
            get { return this._UnreadCountIncrementalTrigger; }
            set { this.SetProperty(ref this._UnreadCountIncrementalTrigger, value); }
        }
        #endregion

        #region DisableNotifyCollectionChanged変更通知プロパティ
        private bool _DisableNotifyCollectionChanged;
        public bool DisableNotifyCollectionChanged
        {
            get { return this._DisableNotifyCollectionChanged; }
            set { this.SetProperty(ref this._DisableNotifyCollectionChanged, value); }
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
        public Tokens _Tokens;
        #endregion
        #region AccountSetting
        private AccountSetting _AccountSetting;
        #endregion
        #region ColumnSetting
        private ColumnSetting _ColumnSetting;
        #endregion
        #region AccountModel
        private AccountModel _AccountModel;
        #endregion

        #region Initialize
        public async Task Initialize()
        {
            this.Stream.SubscribeOn(NewThreadScheduler.Default).Subscribe(
                (StreamingMessage m) =>
                {
                    switch (m.Type)
                    {
                        case MessageType.Create:
                            var tweet = m as StatusMessage;
                            var paramList = new List<string>();
                            if (this._Action == SettingSupport.ColumnTypeEnum.Home)
                            {
                                paramList.Add("home://");
                                if (tweet.Status.Entities.UserMentions != null && tweet.Status.Entities.UserMentions.Any(x => x.Id == this._Tokens.UserId))
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

                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(tweet.Status), this._Tokens.UserId, paramList, true));
                            break;
                        case MessageType.DirectMesssage:
                            var directMessage = m as DirectMessageMessage;
                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.DirectMessage(directMessage.DirectMessage), this._Tokens.UserId, new List<string>() { "directmessages://" }, true));
                            break;
                        case MessageType.DeleteStatus:
                            var deleteStatus = m as DeleteMessage;
                            Connecter.Instance.TweetDelete_OnCommandExecute(this, new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.Status, deleteStatus.Id, this._Tokens.UserId));
                            break;
                        case MessageType.DeleteDirectMessage:
                            var deleteDirectMessage = m as DeleteMessage;
                            Connecter.Instance.TweetDelete_OnCommandExecute(this, new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.DirectMessage, deleteDirectMessage.Id, this._Tokens.UserId));
                            break;
                        case MessageType.Event:
                            var eventMessage = m as CoreTweet.Streaming.EventMessage;
                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.EventMessage(eventMessage), this._Tokens.UserId, new List<string>() { "events://" }, true));

                            if (eventMessage.Event == EventCode.Favorite && eventMessage.TargetStatus != null && eventMessage.Source.Id == this._Tokens.UserId)
                            {
                                eventMessage.TargetStatus.IsFavorited = true;
                                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(eventMessage.TargetStatus), this._Tokens.UserId, new List<string>() { "favorites://" }, false));
                            }

                            break;
                    }
                }
            );

            Observable.Merge(
                Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                h => (sender, e) => h(e),
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetReceive_CommandExecute += h,
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetReceive_CommandExecute -= h).Where(e => e.Streaming).Select(e => (object)e),
                Observable.FromEvent<EventHandler<TweetDeleteEventArgs>, TweetDeleteEventArgs>(
                h => (sender, e) => h(e),
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetDelete_CommandExecute += h,
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetDelete_CommandExecute -= h).Select(e => (object)e)).SubscribeOn(ThreadPoolScheduler.Default).Subscribe(e =>
                {
                    if (e is TweetEventArgs)
                    {
                        var tweetEventArgs = (TweetEventArgs)e;

                        switch (tweetEventArgs.Type)
                        {
                            case TweetEventArgs.TypeEnum.Status:
                                
                                if (!this.Check(tweetEventArgs.Status, tweetEventArgs.Parameter))
                                    return;
                                
                                this.Add(tweetEventArgs.Status, tweetEventArgs.Streaming);
                                break;
                            case TweetEventArgs.TypeEnum.DirectMessage:
                                if (this.Action == SettingSupport.ColumnTypeEnum.DirectMessages)
                                {
                                    this.Add(tweetEventArgs.DirectMessage, tweetEventArgs.Streaming);
                                }
                                break;
                            case TweetEventArgs.TypeEnum.EventMessage:
                                if (this.Action == SettingSupport.ColumnTypeEnum.Events && tweetEventArgs.EventMessage.Source.Id != this._Tokens.UserId)
                                {
                                    this.Add(tweetEventArgs.EventMessage, tweetEventArgs.Streaming);
                                }
                                break;
                        }
                    }
                    else if (e is TweetDeleteEventArgs)
                    {
                    }
                });

            SettingService.Setting.ObserveProperty(x => x.MuteFilter).Subscribe(x => 
            {
                try
                {
                    this.MuteFilterDelegate = Models.Filter.Compiler.Compile(x);
                }
                catch
                {
                    this.MuteFilterDelegate = null;
                }
            });

            this.Action = this._ColumnSetting.Action;
            this.AutoRefresh = this._ColumnSetting.AutoRefresh;
            this.AutoRefreshTimerInterval = this._ColumnSetting.AutoRefreshTimerInterval;
            this.DisableStartupRefresh = this._ColumnSetting.DisableStartupRefresh;
            this.Filter = this._ColumnSetting.Filter;
            this.Index = this._ColumnSetting.Index;
            this.Name = this._ColumnSetting.Name;
            this.Parameter = this._ColumnSetting.Parameter;
            this.FetchingNumberOfTweet = this._ColumnSetting.FetchingNumberOfTweet;
            this.ScreenName = this._AccountSetting.ScreenName;
            
            if (!this._ColumnSetting.DisableStartupRefresh)
            {
                this.DisableNotifyCollectionChanged = true;
                this.IsScrollControlEnabled = false;

                await Update();

                this.IsScrollControlEnabled = true;
                this.DisableNotifyCollectionChanged = false;
            }
            
            //await Task.Delay(100);
            this.Streaming = this._ColumnSetting.Streaming;
        }
        #endregion

        #region Constructor
        public ColumnModel(ColumnSetting column, AccountSetting account, AccountModel accountModel)
        {
            this.Name = column.Name;

            this._Tweets = new ObservableCollection<ITweet>();
            this.stream = new Subject<StreamingMessage>();

            this._SelectedIndex = -1;

            this._Tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret, account.UserId, account.ScreenName);

            this._AccountSetting = account;
            this._ColumnSetting = column;

            this._AccountModel = accountModel;
        }
        #endregion

        private List<long> listStreamUserIdList = null;
        private Subject<StreamingMessage> stream = null;
        private IDisposable twitterStreamDisposableObject = null;
        private IObservable<StreamingMessage> Stream
        {
            get { return this.stream.AsObservable(); }
        }
        private async void StartStreaming()
        {
            if (this.twitterStreamDisposableObject != null)
                return;

            IObservable<StreamingMessage> iObservable;
            if (this._Action == SettingSupport.ColumnTypeEnum.Home)
            {
                var param = new Dictionary<string, object>();
                if (this._AccountSetting.IncludeFollowingsActivity)
                    param.Add("include_followings_activity", true);

                iObservable = this._Tokens.Streaming.UserAsObservable(param);
                this.twitterStreamDisposableObject = iObservable.Catch((Exception ex) =>
                {
                    // Todo : Notifications の通知システムに渡す
                    return iObservable.DelaySubscription(TimeSpan.FromSeconds(10)).Retry();
                }).Subscribe(x => this.stream.OnNext(x), ex => this.stream.OnError(ex), () => this.stream.OnCompleted());
            }
            else if (this._Action == SettingSupport.ColumnTypeEnum.Search)
            {
                this._AccountModel.DisconnectAllFilterStreaming(this);

                var param = new Dictionary<string, object>();
                param.Add("track", this._ColumnSetting.Parameter.ToLower());

                iObservable = this._Tokens.Streaming.FilterAsObservable(param);
                this.twitterStreamDisposableObject = iObservable.Catch((Exception ex) =>
                {
                    // Todo : Notifications の通知システムに渡す
                    return iObservable.DelaySubscription(TimeSpan.FromSeconds(10)).Retry();
                }).Subscribe(x => this.stream.OnNext(x), ex => this.stream.OnError(ex), () => this.stream.OnCompleted());
            }
            else if (this.Action == SettingSupport.ColumnTypeEnum.List)
            {
                this._AccountModel.DisconnectAllFilterStreaming(this);

                listStreamUserIdList = new List<long>();
                try
                {
                    // 最大5000人まで (5000人だとたまにエラー出る？)
                    var userList = await this._Tokens.Lists.Members.ListAsync(list_id => long.Parse(this.Parameter), count => 4999);
                    listStreamUserIdList = userList.Select(x => x.Id.HasValue ? x.Id.Value : 0).ToList();
                }
                catch (TwitterException ex)
                {
                    // Todo : Notifications の通知システムに渡す
                    return;
                }
                catch (Exception ex)
                {
                    // Todo : Notifications の通知システムに渡す
                    return;
                }

                var param = new Dictionary<string, object>();
                param.Add("follow", string.Join(",", listStreamUserIdList));

                iObservable = this._Tokens.Streaming.FilterAsObservable(param);
                this.twitterStreamDisposableObject = iObservable.Catch((Exception ex) =>
                {
                    // Todo : Notifications の通知システムに渡す
                    return iObservable.DelaySubscription(TimeSpan.FromSeconds(10)).Retry();
                }).Subscribe(x => this.stream.OnNext(x), ex => this.stream.OnError(ex), () => this.stream.OnCompleted());
            }
            else
            {
                this.Streaming = false;
            }
        }

        private void StopStreaming()
        {
            if (this.twitterStreamDisposableObject == null)
                return;

            try
            {
                this.twitterStreamDisposableObject.Dispose();
                this.twitterStreamDisposableObject = null;
            }
            catch
            {
            }
        }

        public async Task Update(long maxid = 0, long sinceid = 0)
        {
            if (this.Action == SettingSupport.ColumnTypeEnum.Events || this.Action == SettingSupport.ColumnTypeEnum.Filter)
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
            }

            this.Updating = false;
        }

        private async Task UpdateHome(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", 100 }, { "include_entities", true } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var home = await this._Tokens.Statuses.HomeTimelineAsync(param);
                
                foreach (var status in home)
                {
                    var statusObject = new Twitter.Objects.Status(status);
                    if (this.Check(statusObject))
                        Add(statusObject);

                    var paramList = new List<string>() { "home://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(statusObject, this._Tokens.UserId, paramList, false));
                }
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
            }
            catch
            {
                // Todo : 通知システムに渡す
            }
        }

        private async Task UpdateMentions(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", 40 }, { "include_entities", true } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var mentions = await this._Tokens.Statuses.MentionsTimelineAsync(param);

                foreach (var status in mentions)
                {
                    var statusObject = new Twitter.Objects.Status(status);
                    if (this.Check(statusObject))
                        Add(statusObject);

                    var paramList = new List<string>() { "mentions://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(statusObject, this._Tokens.UserId, paramList, false));
                }
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
            }
            catch
            {
                // Todo : 通知システムに渡す
            }
        }

        private async Task UpdateDirectMessages(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", 20 }, { "include_entities", true } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var receivedDirectMessages = await this._Tokens.DirectMessages.ReceivedAsync(param);
                var sentDirectMessages = await this._Tokens.DirectMessages.SentAsync(param);
                var directMessages = receivedDirectMessages.Concat(sentDirectMessages).OrderByDescending(x => x.Id);

                foreach (var directMessage in directMessages)
                {
                    var directMessageObject = new Twitter.Objects.DirectMessage(directMessage);
                    Add(directMessageObject);

                    var paramList = new List<string>() { "directmessages://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(directMessageObject, this._Tokens.UserId, paramList, false));
                }
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
            }
            catch
            {
                // Todo : 通知システムに渡す
            }
        }

        private async Task UpdateFavorites(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", 25 }, { "include_entities", true } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var favorites = await this._Tokens.Favorites.ListAsync(param);

                foreach (var status in favorites)
                {
                    var statusObject = new Twitter.Objects.Status(status);
                    if (this.Check(statusObject))
                        Add(statusObject);

                    var paramList = new List<string>() { "favorites://" };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(status), this._Tokens.UserId, paramList, false));
                }
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
            }
            catch
            {
                // Todo : 通知システムに渡す
            }
        }

        private async Task UpdateList(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", 100 }, { "include_entities", true }, { "list_id", long.Parse(this._Parameter) } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var lists = await this._Tokens.Lists.StatusesAsync(param);

                foreach (var status in lists)
                {
                    var statusObject = new Twitter.Objects.Status(status);
                    if (this.Check(statusObject))
                        Add(statusObject);

                    var paramList = new List<string>() { "list://" + this._Parameter };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(status), this._Tokens.UserId, paramList, false));
                }
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
            }
            catch
            {
                // Todo : 通知システムに渡す
            }
        }

        private async Task UpdateSearch(long maxid = 0, long sinceid = 0)
        {
            try
            {
                IEnumerable<CoreTweet.Status> search = null;

                if (SettingService.Setting.UseOfficialApi && TwitterConsumerKeyPatterns.OfficialConsumerKeyList.Contains(this._Tokens.ConsumerKey))
                {
                    var param = new Dictionary<string, object>() { { "q", this._Parameter }, { "count", 100 }, { "result_type", "recent" }, { "modules", "status" } };
                    if (maxid != 0)
                        param["q"] = param["q"] + " max_id:" + maxid;
                    if (sinceid != 0)
                        param["q"] = param["q"] + " since_id:" + sinceid;

                    var res = await this._Tokens.SendRequestAsync(MethodType.Get, "https://api.twitter.com/1.1/search/universal.json", param);
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                    var modules = jsonObject["modules"].Children<Newtonsoft.Json.Linq.JObject>();

                    var tweets = new List<CoreTweet.Status>();
                    foreach (var status in modules)
                    {
                        foreach (Newtonsoft.Json.Linq.JProperty prop in status.Properties())
                        {
                            if (prop.Name == "status")
                                tweets.Add(CoreBase.Convert<CoreTweet.Status>(Newtonsoft.Json.JsonConvert.SerializeObject(status["status"]["data"])));
                        }
                    }

                    search = tweets;
                }
                else
                {
                    var param = new Dictionary<string, object>() { { "count", 100 }, { "include_entities", true }, { "q", this._Parameter } };
                    if (maxid != 0)
                        param.Add("max_id", maxid);
                    if (sinceid != 0)
                        param.Add("since_id", sinceid);

                    search = await this._Tokens.Search.TweetsAsync(param);
                }
                
                foreach (var status in search)
                {
                    var statusObject = new Twitter.Objects.Status(status);
                    if (this.Check(statusObject))
                        Add(statusObject);

                    var paramList = new List<string>() { "search://" + this._Parameter };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(status), this._Tokens.UserId, paramList, false));
                }
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
            }
            catch
            {
                // Todo : 通知システムに渡す
            }
        }

        private async Task UpdateUserTimeline(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>() { { "count", 30 }, { "include_entities", true }, { "user_id", int.Parse(this._Parameter) } };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var userTimeline = await this._Tokens.Lists.StatusesAsync(param);

                foreach (var status in userTimeline)
                {
                    var statusObject = new Twitter.Objects.Status(status);
                    if (this.Check(statusObject))
                        Add(statusObject);

                    var paramList = new List<string>() { "usertimeline://" + this._Parameter };
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(status), this._Tokens.UserId, paramList, false));
                }
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
            }
            catch
            {
                // Todo : 通知システムに渡す
            }
        }

        private async Task UpdateEvents(long maxid = 0, long sinceid = 0)
        {
            // http://api.twitter.com/i/activity/about_me.json
        }

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

            if (this.Action == SettingSupport.ColumnTypeEnum.Filter || this.Action == SettingSupport.ColumnTypeEnum.List || this.Action == SettingSupport.ColumnTypeEnum.Search || this.Action == SettingSupport.ColumnTypeEnum.UserTimeline)
            {
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
            }

            return true;
        }

        private bool Check(Twitter.Objects.Status status, List<string> param)
        {
            if (!param.Contains(this.Action.ToString("F").ToLower() + "://" + this._Parameter))
            {
                if (this.Action != SettingSupport.ColumnTypeEnum.List || !param.Contains("home://") || !status.User.IsProtected)
                    return false;
            }

            if (status.User.IsMuting)
                return false;
            else
            {
                // Todo : NoRetweetIdsの読み込み

                /*using (await Connecter.Instance.TweetCollecter[this.OwnerUserId].NoRetweetIdsAsyncLock.LockAsync())
                {
                    if (Connecter.Instance.TweetCollecter[this.OwnerUserId].NoRetweetIds.Contains(status.User.Id))
                        return false;
                }*/
            }

            if (this.Action == SettingSupport.ColumnTypeEnum.Mentions && !SettingService.Setting.ShowRetweetToMentionColumn)
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

            if (this.Action == SettingSupport.ColumnTypeEnum.Filter || this.Action == SettingSupport.ColumnTypeEnum.List || this.Action == SettingSupport.ColumnTypeEnum.Search || this.Action == SettingSupport.ColumnTypeEnum.UserTimeline)
            {
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
            }

            return true;
        }

        private void Add(Twitter.Objects.Status status, bool streaming = false)
        {
            if (streaming)
            {
                this._Tweets.Insert(0, status);
                this.UnreadCountIncrementalTrigger = true;
            }
            else
            {
                var retindex = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => (x as ITweet)?.Id == status.Id));
                if (retindex != -1 && SettingService.Setting.RemoveRetweetAlreadyReceive && status.HasRetweetInformation && status.RetweetInformation.User.ScreenName != this._ScreenName)
                    return;

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this._Tweets.IndexOf(this._Tweets.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this._Tweets.Add(status);
                    else
                        this._Tweets.Insert(index, status);

                    if (index <= this.UnreadCount && index != -1)
                        this.UnreadCountIncrementalTrigger = true;
                }
            }
        }

        private void Add(Twitter.Objects.DirectMessage directMessage, bool streaming = false)
        {
            if (streaming)
            {
                this._Tweets.Insert(0, directMessage);
                this.UnreadCountIncrementalTrigger = true;
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

                    if (index <= this.UnreadCount && index != -1)
                        this.UnreadCountIncrementalTrigger = true;
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
                    break;
                default:
                    return;
            }

            this._Tweets.Insert(0, eventMessage);

            this.UnreadCountIncrementalTrigger = true;
        }
    }
}
