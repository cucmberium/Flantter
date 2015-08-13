using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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
        private string _Filter;
        public string Filter
        {
            get { return this._Filter; }
            set { this.SetProperty(ref this._Filter, value); }
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

        #region Name変更通知プロパティ
        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set { this.SetProperty(ref this._Name, value); }
        }
        #endregion

        #region OwnerScreenName変更通知プロパティ
        private string _OwnerScreenName;
        public string OwnerScreenName
        {
            get { return this._OwnerScreenName; }
            set { this.SetProperty(ref this._OwnerScreenName, value); }
        }
        #endregion

        #region OwnerUserId変更通知プロパティ
        private long _OwnerUserId;
        public long OwnerUserId
        {
            get { return this._OwnerUserId; }
            set { this.SetProperty(ref this._OwnerUserId, value); }
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

        #region Columns
        private ObservableCollection<ITweet> _Tweets;
        private ReadOnlyObservableCollection<ITweet> _ReadOnlyTweets;
        public ReadOnlyObservableCollection<ITweet> ReadOnlyTweets
        {
            get
            {
                return _ReadOnlyTweets;
            }
        }
        #endregion

        #region Tokens
        private Tokens _Tokens;
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
        public async void Initialize()
        {
            Observable.Merge(
                Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                h => (sender, e) => h(e),
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetReceive_CommandExecute += h,
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetReceive_CommandExecute -= h).Select(e => (object)e),
                Observable.FromEvent<EventHandler<TweetDeleteEventArgs>, TweetDeleteEventArgs>(
                h => (sender, e) => h(e),
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetDelete_CommandExecute += h,
                h => Connecter.Instance.TweetCollecter[this._AccountSetting.UserId].TweetDelete_CommandExecute -= h).Select(e => (object)e)).Subscribe(e =>
                {
                    if (e is TweetEventArgs)
                    {
                        var tweetEventArgs = (TweetEventArgs)e;
                        switch (tweetEventArgs.Type)
                        {
                            case TweetEventArgs.TypeEnum.Status:
                                this._Tweets.Add(tweetEventArgs.Status);
                                break;
                            case TweetEventArgs.TypeEnum.DirectMessage:
                                this._Tweets.Add(tweetEventArgs.DirectMessage);
                                break;
                            case TweetEventArgs.TypeEnum.EventMessage:
                                this._Tweets.Add(tweetEventArgs.EventMessage);
                                break;
                        }
                    }
                    else if (e is TweetDeleteEventArgs)
                    {

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
            this.Streaming = this._ColumnSetting.Streaming;
            this.FetchingNumberOfTweet = this._ColumnSetting.FetchingNumberOfTweet;
            this.OwnerScreenName = this._AccountSetting.ScreenName;
            this.OwnerUserId = this._AccountSetting.UserId;
        }
        #endregion

        #region Constructor
        public ColumnModel(ColumnSetting column, AccountSetting account, AccountModel accountModel)
        {
            this._Tweets = new ObservableCollection<ITweet>();
            this._ReadOnlyTweets = new ReadOnlyObservableCollection<ITweet>(this._Tweets);
            this.stream = new Subject<StreamingMessage>();

            this._SelectedIndex = -1;

            this._Tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret, account.UserId, account.ScreenName);

            this._AccountSetting = account;
            this._ColumnSetting = column;

            this._AccountModel = accountModel;
            
            this.Stream.Subscribe(
                (StreamingMessage m) =>
                {
                    switch (m.Type)
                    {
                        case MessageType.Create:
                            var tweet = m as StatusMessage;
                            var paramList = new List<string>();
                            if (this._Action == SettingSupport.ColumnTypeEnum.Home)
                                paramList.Add("home://");
                            else if (this._Action == SettingSupport.ColumnTypeEnum.Search)
                                paramList.Add("search://" + this._Parameter);
                            else if (this._Action == SettingSupport.ColumnTypeEnum.List)
                                paramList.Add("list://" + this._Parameter);

                            // Todo : Mention時の処理 (UserStream限定)

                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(tweet.Status), this._OwnerUserId, paramList, true));
                            break;
                        case MessageType.DirectMesssage:
                            var directMessage = m as DirectMessageMessage;
                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.DirectMessage(directMessage.DirectMessage), this._OwnerUserId, new List<string>() { "directmessages://" }, true));
                            break;
                        case MessageType.DeleteStatus:
                            var deleteStatus = m as DeleteMessage;
                            Connecter.Instance.TweetDelete_OnCommandExecute(this, new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.Status, deleteStatus.Id, this._OwnerUserId));
                            break;
                        case MessageType.DeleteDirectMessage:
                            var deleteDirectMessage = m as DeleteMessage;
                            Connecter.Instance.TweetDelete_OnCommandExecute(this, new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.DirectMessage, deleteDirectMessage.Id, this._OwnerUserId));
                            break;
                        case MessageType.Event:
                            var eventMessage = m as CoreTweet.Streaming.EventMessage;
                            Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.EventMessage(eventMessage), this._OwnerUserId, new List<string>() { "event://" }, true));
                            
                            if (eventMessage.Event == EventCode.Favorite && eventMessage.TargetStatus != null && eventMessage.Source != null && eventMessage.Source.Id == this._OwnerUserId)
                            {
                                eventMessage.TargetStatus.IsFavorited = true;
                                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(new Twitter.Objects.Status(eventMessage.TargetStatus), this._OwnerUserId, new List<string>() { "favorite://" }, true));
                            }

                            break;
                    }
                }
            );
        }
        #endregion

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
                this._AccountModel.DisconnectAllFilterStreaming();

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
                this._AccountModel.DisconnectAllFilterStreaming();

                List<long> userIdList = new List<long>();
                try
                {
                    var userList = await this._Tokens.Lists.Members.ListAsync(list_id => long.Parse(this.Parameter), count => 4999);
                    userIdList = userList.Select(x => x.Id.HasValue ? x.Id.Value : 0).ToList();
                }
                catch (TwitterException ex)
                {
                    // Todo : Notifications の通知システムに渡す
                }
                catch (Exception ex)
                {
                    // Todo : Notifications の通知システムに渡す
                }

                var param = new Dictionary<string, object>();
                param.Add("follow", string.Join(",", userIdList));

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
    }
}
