using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Apis;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Filter;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Setting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.Models
{
    public class ColumnModel : BindableBase, IDisposable
    {
        private readonly ResourceLoader _resourceLoader;

        private List<long> _listStreamUserIdList;

        private readonly Subject<StreamingMessage> _stream;

        private IDisposable _streamingDisposableObject;

        #region Constructor

        public ColumnModel(ColumnSetting column, AccountSetting account, AccountModel accountModel)
        {
            _resourceLoader = new ResourceLoader();

            Tweets = new ObservableCollection<ITweet>();
            _stream = new Subject<StreamingMessage>();

            _selectedIndex = -1;

            Tokens = accountModel.Tokens;

            AccountSetting = account;
            ColumnSetting = column;

            AccountModel = accountModel;

            Name = ColumnSetting.Name;
            Action = ColumnSetting.Action;
            Index = ColumnSetting.Index;
            Parameter = ColumnSetting.Parameter;
        }

        #endregion

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

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

        private AccountModel AccountModel { get; }

        #endregion

        private IObservable<StreamingMessage> Stream => _stream.AsObservable();

        public void Dispose()
        {
            try
            {
                _streamingDisposableObject?.Dispose();
            }
            catch
            {
            }

            try
            {
                Disposable.Dispose();
            }
            catch
            {
            }
        }

        #region Initialize

        public async Task Initialize()
        {
            Stream.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(
                    m =>
                    {
                        switch (m.Type)
                        {
                            case StreamingMessage.MessageType.Create:
                                var status = m.Status;
                                var paramList = new List<string>();

                                // ミュートチェック
                                if (!MuteCheck(status))
                                    break;

                                if (_action == SettingSupport.ColumnTypeEnum.Home)
                                {
                                    paramList.Add("home://");
                                    paramList.Add("filter://");
                                    if (status.Entities.UserMentions != null &&
                                        status.Entities.UserMentions.Any(x => x.Id == AccountSetting.UserId))
                                        paramList.Add("mentions://");
                                    else if (SettingService.Setting.ShowRetweetInMentionColumn &&
                                             status.HasRetweetInformation && status.User.Id == AccountSetting.UserId)
                                        paramList.Add("mentions://");
                                }
                                else if (_action == SettingSupport.ColumnTypeEnum.Search)
                                {
                                    paramList.Add("search://" + _parameter);
                                }
                                else if (_action == SettingSupport.ColumnTypeEnum.List)
                                {
                                    paramList.Add("list://" + _parameter);
                                }
                                else if (_action == SettingSupport.ColumnTypeEnum.Federated)
                                {
                                    paramList.Add("federated://" + _parameter);
                                }
                                else if (_action == SettingSupport.ColumnTypeEnum.Local)
                                {
                                    paramList.Add("local://" + _parameter);
                                }

                                Connecter.Instance.TweetReceive_OnCommandExecute(this,
                                    new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance,
                                        paramList, true));

                                if (status.HasRetweetInformation && status.User.Id == AccountSetting.UserId &&
                                    AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                                        new TweetEventArgs(new EventMessage(status), AccountSetting.UserId,
                                            AccountSetting.Instance,
                                            new List<string> {"events://"}, true));

                                break;
                            case StreamingMessage.MessageType.DirectMesssage:
                                var directMessage = m.DirectMessage;
                                Connecter.Instance.TweetReceive_OnCommandExecute(this,
                                    new TweetEventArgs(directMessage, AccountSetting.UserId, AccountSetting.Instance,
                                        new List<string> {"directmessages://"}, true));
                                break;
                            case StreamingMessage.MessageType.Event:
                                var eventMessage = m.EventMessage;
                                Connecter.Instance.TweetReceive_OnCommandExecute(this,
                                    new TweetEventArgs(eventMessage, AccountSetting.UserId, AccountSetting.Instance,
                                        new List<string> {"events://"},
                                        true));

                                if (eventMessage.Type == "Favorite" && eventMessage.TargetStatus != null &&
                                    eventMessage.Source.Id == AccountSetting.UserId && AccountSetting.Platform ==
                                    SettingSupport.PlatformEnum.Twitter)
                                {
                                    eventMessage.TargetStatus.IsFavorited = true;
                                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                                        new TweetEventArgs(eventMessage.TargetStatus, AccountSetting.UserId,
                                            AccountSetting.Instance,
                                            new List<string> {"favorites://"}, true));
                                }

                                break;
                            case StreamingMessage.MessageType.DeleteStatus:
                                var deletedStatusId = m.DeletedStatusId;
                                Connecter.Instance.TweetDelete_OnCommandExecute(this,
                                    new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.Status, deletedStatusId,
                                        AccountSetting.UserId, AccountSetting.Instance));
                                break;
                            case StreamingMessage.MessageType.DeleteDirectMessage:
                                var deletedDirectMessageId = m.DeletedDirectMessageId;
                                Connecter.Instance.TweetDelete_OnCommandExecute(this,
                                    new TweetDeleteEventArgs(TweetDeleteEventArgs.TypeEnum.DirectMessage,
                                        deletedDirectMessageId, AccountSetting.UserId, AccountSetting.Instance));
                                break;
                        }
                    }
                )
                .AddTo(Disposable);

            Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                    h => (sender, e) => h(e),
                    h => Connecter.Instance.TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance]
                        .TweetReceiveCommandExecute += h,
                    h => Connecter.Instance.TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance]
                        .TweetReceiveCommandExecute -= h)
                .Select(e => (object) e)
                .Merge(Observable.FromEvent<EventHandler<TweetDeleteEventArgs>, TweetDeleteEventArgs>(
                        h => (sender, e) => h(e),
                        h => Connecter.Instance.TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance]
                            .TweetDeleteCommandExecute += h,
                        h => Connecter.Instance.TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance]
                            .TweetDeleteCommandExecute -= h)
                    .Select(e => (object) e))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(e =>
                {
                    if (e is TweetEventArgs tweetEventArgs)
                    {
                        if (tweetEventArgs.Streaming == false && Action != SettingSupport.ColumnTypeEnum.Filter)
                            return;

                        switch (tweetEventArgs.Type)
                        {
                            case TweetEventArgs.TypeEnum.Status:
                                if (!Check(tweetEventArgs.Status, tweetEventArgs.Parameter))
                                    return;

                                if (Action == SettingSupport.ColumnTypeEnum.Favorites)
                                    Add(tweetEventArgs.Status, false);
                                else
                                    Add(tweetEventArgs.Status, tweetEventArgs.Streaming);

                                break;
                            case TweetEventArgs.TypeEnum.DirectMessage:
                                if (Action != SettingSupport.ColumnTypeEnum.DirectMessages)
                                    return;

                                Add(tweetEventArgs.DirectMessage, tweetEventArgs.Streaming);
                                break;
                            case TweetEventArgs.TypeEnum.EventMessage:
                                if (Action != SettingSupport.ColumnTypeEnum.Events ||
                                    tweetEventArgs.EventMessage.Source.Id == AccountSetting.UserId)
                                    return;

                                Add(tweetEventArgs.EventMessage, tweetEventArgs.Streaming);
                                break;
                        }
                    }
                    else if (e is TweetDeleteEventArgs tweetDeleteEventArgs)
                    {
                        Delete(tweetDeleteEventArgs.Id);
                    }
                })
                .AddTo(Disposable);

            SettingService.Setting.ObserveProperty(x => x.MuteFilter).Subscribe(x => MuteFilter = x).AddTo(Disposable);
            ColumnSetting.ObserveProperty(x => x.Filter).Subscribe(x => Filter = x).AddTo(Disposable);
            ColumnSetting.ObserveProperty(x => x.Name).Subscribe(x => Name = x).AddTo(Disposable);
            AccountSetting.ObserveProperty(x => x.ScreenName).Subscribe(x => ScreenName = x).AddTo(Disposable);
            AccountSetting.ObserveProperty(x => x.Instance).Subscribe(x => Instance = x).AddTo(Disposable);
            AccountSetting.ObserveProperty(x => x.ProfileImageUrl)
                .Subscribe(x => ProfileImageUrl = x)
                .AddTo(Disposable);

            if (SettingService.Setting.RestoreTimelineOnStartup && SettingService.Setting.EnableDatabase)
                switch (Action)
                {
                    case SettingSupport.ColumnTypeEnum.Collection:
                        foreach (var collection in Database.Instance.GetCollectionEntryFromParam(AccountSetting.UserId,
                            AccountSetting.Instance))
                            Add(collection);
                        break;
                    case SettingSupport.ColumnTypeEnum.DirectMessages:
                        foreach (var dm in Database.Instance.GetDirectMessagesFromParam(AccountSetting.UserId,
                            AccountSetting.Instance))
                            Add(dm);
                        break;
                    case SettingSupport.ColumnTypeEnum.Events:
                        foreach (var ev in Database.Instance.GetEventMessagesFromParam(AccountSetting.UserId,
                            AccountSetting.Instance))
                            Add(ev);
                        break;
                    default:
                        foreach (var status in Database.Instance.GetStatusesFromParam(
                            Action.ToString("F").ToLower() + "://" + _parameter, AccountSetting.UserId,
                            AccountSetting.Instance))
                        {
                            if (!Check(status))
                                continue;

                            Add(status);
                        }

                        break;
                }

            if (!ColumnSetting.DisableStartupRefresh && !SettingService.Setting.DisableStartupTimelineUpdate)
                await Update();

            if (!SettingService.Setting.StopStreamingOnStartup)
                Streaming = ColumnSetting.Streaming;
        }

        #endregion

        private async void StartStreaming()
        {
            if (_streamingDisposableObject != null)
                return;

            try
            {
                IObservable<StreamingMessage> iObservable;

                if (_action == SettingSupport.ColumnTypeEnum.Home)
                {
                    var param = new Dictionary<string, object>();
                    if (AccountSetting.IncludeFollowingsActivity)
                        param.Add("include_followings_activity", true);
                    iObservable = Tokens.Streaming.UserAsObservable(param);
                }
                else if (_action == SettingSupport.ColumnTypeEnum.Search)
                {
                    if (string.IsNullOrWhiteSpace(ColumnSetting.Parameter))
                        return;

                    if (AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                        AccountModel.DisconnectAllFilterStreaming(this);

                    var param = new Dictionary<string, object>();
                    param.Add("track", ColumnSetting.Parameter.ToLower());
                    iObservable = Tokens.Streaming.FilterAsObservable(param);
                }
                else if (_action == SettingSupport.ColumnTypeEnum.List)
                {
                    if (string.IsNullOrWhiteSpace(ColumnSetting.Parameter))
                        return;

                    if (AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                    {
                        AccountModel.DisconnectAllFilterStreaming(this);

                        var userList =
                            await Tokens.Lists.Members.ListAsync(list_id => long.Parse(Parameter), count => 4999);
                        _listStreamUserIdList = userList.Select(x => x.Id).ToList();

                        var param = new Dictionary<string, object>();
                        param.Add("follow", string.Join(",", _listStreamUserIdList));
                        iObservable = Tokens.Streaming.FilterAsObservable(param);
                    }
                    else
                    {
                        iObservable = Tokens.Streaming.ListAsObservable(list => long.Parse(ColumnSetting.Parameter));
                    }
                }
                else if (_action == SettingSupport.ColumnTypeEnum.Federated)
                {
                    if (AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                        return;

                        var param = new Dictionary<string, object>();
                    iObservable = Tokens.Streaming.PublicAsObservable(param);
                }
                else if (_action == SettingSupport.ColumnTypeEnum.Local)
                {
                    if (AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                        return;

                    var param = new Dictionary<string, object> {{"local", true}};
                    iObservable = Tokens.Streaming.PublicAsObservable(param);
                }
                else
                {
                    Streaming = false;
                    return;
                }

                _streamingDisposableObject = iObservable
                    .SubscribeOn(NewThreadScheduler.Default)
                    .Catch(
                        (Exception ex) => iObservable.DelaySubscription(TimeSpan.FromSeconds(15)).Retry())
                    .Repeat()
                    .Subscribe(x => _stream.OnNext(x), ex => _stream.OnError(ex), () => _stream.OnCompleted());
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                Streaming = false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                Streaming = false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                Streaming = false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                Streaming = false;
            }
        }

        private void StopStreaming()
        {
            if (_streamingDisposableObject == null)
                return;

            try
            {
                _streamingDisposableObject.Dispose();
            }
            catch
            {
            }
            finally
            {
                _streamingDisposableObject = null;
            }
        }

        public void ReconnectStreaming()
        {
            StopStreaming();
            StartStreaming();

            _streaming = true;
            RaisePropertyChanged("Streaming");
        }

        public async Task Update(long maxid = 0, long sinceid = 0)
        {
            if (Action == SettingSupport.ColumnTypeEnum.Filter)
                return;

            if (Updating)
                return;

            Updating = true;

            switch (Action)
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
                case SettingSupport.ColumnTypeEnum.Federated:
                    await UpdatePublicTimeline(false, maxid, sinceid);
                    break;
                case SettingSupport.ColumnTypeEnum.Local:
                    await UpdatePublicTimeline(true, maxid, sinceid);
                    break;
            }

            Updating = false;
        }

        private async Task UpdateHome(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var home = await Tokens.Statuses.HomeTimelineAsync(param);
                var lastId = home.Count > 0
                    ? home.Select(x => x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id)
                        .OrderByDescending(x => x).Last()
                    : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in home)
                {
                    if (Check(status))
                        Add(status);

                    var paramList = new List<string> {"home://", "filter://"};
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance, paramList, false));
                }

                if (gapCheck)
                    Add(new Gap(0, lastId - 1, DateTime.Now));
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        private async Task UpdateMentions(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"tweet_mode", CoreTweet.TweetMode.Extended},
                    {"exclude_types", new List<string> {"follow", "favourite", "reblog"}}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var mentions = await Tokens.Statuses.MentionsTimelineAsync(param);
                var lastId = mentions.Count > 0
                    ? mentions.Select(x => x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id)
                        .OrderByDescending(x => x).Last()
                    : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in mentions)
                {
                    if (Check(status))
                        Add(status);

                    var paramList = new List<string> {"mentions://"};
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance, paramList, false));
                }

                if (gapCheck)
                    Add(new Gap(0, lastId - 1, DateTime.Now));
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        private async Task UpdateDirectMessages(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"full_text", true}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                IEnumerable<DirectMessage> directMessages = await Tokens.DirectMessages.ReceivedAsync(param);
                if (AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                    directMessages = directMessages.Concat(await Tokens.DirectMessages.SentAsync(param));
                directMessages = directMessages.OrderByDescending(x => x.Id);

                foreach (var directMessage in directMessages)
                {
                    Add(directMessage);

                    var paramList = new List<string> {"directmessages://"};
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(directMessage, AccountSetting.UserId, AccountSetting.Instance, paramList,
                            false));
                }
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        private async Task UpdateFavorites(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var favorites = await Tokens.Favorites.ListAsync(param);
                var lastId = favorites.Count > 0
                    ? favorites.Select(x => x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id)
                        .OrderByDescending(x => x).Last()
                    : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in favorites)
                {
                    if (Check(status))
                        Add(status);

                    var paramList = new List<string> {"favorites://"};
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance, paramList, false));
                }

                if (gapCheck)
                    Add(new Gap(0, lastId - 1, DateTime.Now));
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        private async Task UpdateList(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"list_id", long.Parse(_parameter)},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var lists = await Tokens.Lists.StatusesAsync(param);
                var lastId = lists.Count > 0
                    ? lists.Select(x => x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id)
                        .OrderByDescending(x => x).Last()
                    : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in lists)
                {
                    if (Check(status))
                        Add(status);

                    var paramList = new List<string> {"list://" + _parameter};
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance, paramList, false));
                }

                if (gapCheck)
                    Add(new Gap(0, lastId - 1, DateTime.Now));
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        private async Task UpdateSearch(long maxid = 0, long sinceid = 0)
        {
            try
            {
                IEnumerable<Status> search;

                if (SettingService.Setting.UseOfficialApi &&
                    TwitterConnectionHelper.OfficialConsumerKeyList.Contains(AccountSetting.ConsumerKey))
                {
                    var param = new Dictionary<string, object>
                    {
                        {"q", _parameter},
                        {"count", ColumnSetting.FetchingNumberOfTweet},
                        {"result_type", "recent"},
                        {"modules", "status"},
                        {"tweet_mode", CoreTweet.TweetMode.Extended}
                    };
                    if (maxid != 0)
                        param["q"] = param["q"] + " max_id:" + maxid;
                    if (sinceid != 0)
                        param["q"] = param["q"] + " since_id:" + sinceid;

                    var res = await Tokens.TwitterTokens.SendRequestAsync(CoreTweet.MethodType.Get,
                        "https://api.twitter.com/1.1/search/universal.json", param);
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(json);
                    var modules = jsonObject["modules"].Children<JObject>();

                    var tweets = new List<CoreTweet.Status>();
                    foreach (var status in modules)
                    foreach (var prop in status.Properties())
                        if (prop.Name == "status")
                            tweets.Add(CoreTweet.Core.CoreBase.Convert<CoreTweet.Status>(
                                JsonConvert.SerializeObject(status["status"]["data"])));

                    search = tweets.Select(x => new Status(x));
                }
                else
                {
                    var param = new Dictionary<string, object>
                    {
                        {"count", ColumnSetting.FetchingNumberOfTweet},
                        {"include_entities", true},
                        {"q", _parameter},
                        {"tweet_mode", CoreTweet.TweetMode.Extended}
                    };
                    if (maxid != 0)
                        param.Add("max_id", maxid);
                    if (sinceid != 0)
                        param.Add("since_id", sinceid);

                    search = await Tokens.Search.TweetsAsync(param);
                }

                var lastId = search.Any()
                    ? search.Select(x => x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id)
                        .OrderByDescending(x => x).Last()
                    : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in search)
                {
                    if (Check(status))
                        Add(status);

                    var paramList = new List<string> {"search://" + _parameter};
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance, paramList, false));
                }

                if (gapCheck)
                    Add(new Gap(0, lastId - 1, DateTime.Now));
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        private async Task UpdateUserTimeline(long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", ColumnSetting.FetchingNumberOfTweet},
                    {"include_entities", true},
                    {"user_id", long.Parse(_parameter)},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var userTimeline = await Tokens.Statuses.UserTimelineAsync(param);
                var lastId = userTimeline.Count > 0
                    ? userTimeline.Select(x => x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id)
                        .OrderByDescending(x => x).Last()
                    : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in userTimeline)
                {
                    if (Check(status))
                        Add(status);

                    var paramList = new List<string> {"usertimeline://" + _parameter};
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance, paramList, false));
                }

                if (gapCheck)
                    Add(new Gap(0, lastId - 1, DateTime.Now));
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        private async Task UpdateEvents(long maxid = 0, long sinceid = 0)
        {
            try
            {
                if (AccountSetting.Platform == SettingSupport.PlatformEnum.Mastodon)
                {
                    var param = new Dictionary<string, object>
                    {
                        {"count", ColumnSetting.FetchingNumberOfTweet},
                        {"include_entities", true},
                        {"tweet_mode", CoreTweet.TweetMode.Extended}
                    };
                    if (maxid != 0)
                        param.Add("max_id", maxid);
                    if (sinceid != 0)
                        param.Add("since_id", sinceid);

                    var events = await Tokens.Activity.AboutMeAsync(param);
                    var lastId = events.Count > 0 ? events.Select(x => x.Id).OrderByDescending(x => x).Last() : -1;
                    var gapCheck = GapCheck(lastId);
                    foreach (var ev in events)
                    {
                        var evObject = ev;
                        Add(evObject);

                        var paramList = new List<string> {"events://"};
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(evObject, AccountSetting.UserId, AccountSetting.Instance, paramList,
                                false));
                    }

                    if (gapCheck)
                        Add(new Gap(0, lastId - 1, DateTime.Now));
                }
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
            }
            catch (NotImplementedException e)
            {
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
            }
        }

        private async Task UpdateCollection(long maxid = 0, long sinceid = 0)
        {
            if (AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
            {
                var maxposition = Tweets.Where(x => x is CollectionEntry)
                    .Cast<CollectionEntry>()
                    .FirstOrDefault(x => x.Status?.Id == maxid)
                    ?.SortIndex;
                var minposition = Tweets.Where(x => x is CollectionEntry)
                    .Cast<CollectionEntry>()
                    .FirstOrDefault(x => x.Status?.Id == sinceid)
                    ?.SortIndex;

                try
                {
                    var param = new Dictionary<string, object>
                    {
                        {"count", ColumnSetting.FetchingNumberOfTweet},
                        {"include_entities", true},
                        {"tweet_mode", CoreTweet.TweetMode.Extended},
                        {"id", Parameter}
                    };
                    if (maxposition != 0)
                        param.Add("max_position", maxposition);
                    if (minposition != 0)
                        param.Add("min_position", minposition);

                    var entriesResult = await Tokens.Collections.EntriesAsync(param);
                    foreach (var collection in entriesResult)
                    {
                        Add(collection);

                        var paramList = new List<string> {"collection://" + _parameter};
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(collection, AccountSetting.UserId, AccountSetting.Instance, paramList,
                                false));
                    }
                }
                catch (CoreTweet.TwitterException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                }
                catch (TootNet.Exception.MastodonException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                }
                catch (NotImplementedException e)
                {
                }
                catch (Exception e)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                        e.ToString());
                }
            }
        }

        private async Task UpdatePublicTimeline(bool local = false, long maxid = 0, long sinceid = 0)
        {
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", ColumnSetting.FetchingNumberOfTweet},
                };
                if (local)
                    param.Add("local", true);
                if (maxid != 0)
                    param.Add("max_id", maxid);
                if (sinceid != 0)
                    param.Add("since_id", sinceid);

                var publicTimeline = await Tokens.Statuses.PublicTimelineAsync(param);
                var lastId = publicTimeline.Count > 0
                    ? publicTimeline.Select(x => x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id)
                        .OrderByDescending(x => x).Last()
                    : -1;
                var gapCheck = GapCheck(lastId);

                foreach (var status in publicTimeline)
                {
                    if (Check(status))
                        Add(status);

                    var paramList = new List<string>();
                    if (local)
                        paramList.Add("local://" + _parameter);
                    else
                        paramList.Add("federated://" + _parameter);
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, AccountSetting.UserId, AccountSetting.Instance, paramList, false));
                }

                if (gapCheck)
                    Add(new Gap(0, lastId - 1, DateTime.Now));
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
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
                    e.ToString());
            }
        }

        // Streaming用ツイート受信時チェック
        private bool MuteCheck(Status status)
        {
            lock (Connecter.Instance.TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance].MuteIdsLock)
            {
                if (Connecter.Instance.TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance].MuteIds
                    .Contains(status.User.Id))
                    return false;

                if (status.HasRetweetInformation && Connecter.Instance
                        .TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance]
                        .MuteIds.Contains(status.RetweetInformation.User.Id))
                    return false;

                if (status.HasRetweetInformation && Connecter.Instance
                        .TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance]
                        .NoRetweetIds.Contains(status.RetweetInformation.User.Id))
                    return false;

                if (Connecter.Instance.TweetCollecter[AccountSetting.UserId + ":" + AccountSetting.Instance].BlockIds
                    .Contains(status.User.Id))
                    return false;
            }

            if (_action == SettingSupport.ColumnTypeEnum.List)
            {
                if (_listStreamUserIdList == null)
                    return false;

                if (!_listStreamUserIdList.Contains(status.HasRetweetInformation
                    ? status.RetweetInformation.User.Id
                    : status.User.Id))
                    return false;

                if (!status.HasRetweetInformation && status.InReplyToUserId != 0 &&
                    !_listStreamUserIdList.Contains(status.InReplyToUserId))
                    return false;
            }

            return true;
        }

        // 通常ツイート追加時チェック
        private bool Check(Status status)
        {
            if (Action != SettingSupport.ColumnTypeEnum.Mentions && Action != SettingSupport.ColumnTypeEnum.Favorites)
            {
                if (AdvancedSettingService.AdvancedSetting.MuteClients != null)
                    if (AdvancedSettingService.AdvancedSetting.MuteClients.Contains(status.Source))
                        return false;
                if (AdvancedSettingService.AdvancedSetting.MuteUsers != null)
                    if (AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.User.ScreenName))
                        return false;
                    else if (status.HasRetweetInformation &&
                             AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.RetweetInformation.User
                                 .ScreenName))
                        return false;
                if (AdvancedSettingService.AdvancedSetting.MuteWords != null)
                    if (AdvancedSettingService.AdvancedSetting.MuteWords.Any(x => status.Text.Contains(x)))
                        return false;

                if (MuteFilterDelegate != null)
                    try
                    {
                        if ((bool) MuteFilterDelegate.DynamicInvoke(status))
                            return false;
                    }
                    catch
                    {
                    }
            }

            if (FilterDelegate != null)
                try
                {
                    if (!(bool) FilterDelegate.DynamicInvoke(status))
                        return false;
                }
                catch
                {
                    return false;
                }

            return true;
        }

        // Streaming用ツイート追加時チェック
        private bool Check(Status status, List<string> param)
        {
            if (!param.Contains(Action.ToString("F").ToLower() + "://" + _parameter))
            {
                // リストのストリームをホームから補完するためのチェック

                if (!SettingService.Setting.ComplementListStream)
                    return false;

                if (Action != SettingSupport.ColumnTypeEnum.List)
                {
                    if (!Streaming)
                        return false;

                    if (!param.Contains("home://") || !status.User.IsProtected)
                        return false;

                    if (_listStreamUserIdList == null)
                        return false;

                    if (!_listStreamUserIdList.Contains(status.HasRetweetInformation
                        ? status.RetweetInformation.User.Id
                        : status.User.Id))
                        return false;

                    if (!status.HasRetweetInformation && status.InReplyToUserId != 0 &&
                        !_listStreamUserIdList.Contains(status.InReplyToUserId))
                        return false;
                }
                else
                {
                    return false;
                }
            }

            if (Action == SettingSupport.ColumnTypeEnum.Mentions && !SettingService.Setting.ShowRetweetInMentionColumn)
                if (status.HasRetweetInformation)
                    return false;

            if (Action != SettingSupport.ColumnTypeEnum.Mentions && Action != SettingSupport.ColumnTypeEnum.Favorites)
            {
                if (AdvancedSettingService.AdvancedSetting.MuteClients != null)
                    if (AdvancedSettingService.AdvancedSetting.MuteClients.Contains(status.Source))
                        return false;
                if (AdvancedSettingService.AdvancedSetting.MuteUsers != null)
                    if (AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.User.ScreenName))
                        return false;
                    else if (status.HasRetweetInformation &&
                             AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(status.RetweetInformation.User
                                 .ScreenName))
                        return false;
                if (AdvancedSettingService.AdvancedSetting.MuteWords != null)
                    if (AdvancedSettingService.AdvancedSetting.MuteWords.Any(x => status.Text.Contains(x)))
                        return false;

                if (MuteFilterDelegate != null)
                    try
                    {
                        if ((bool) MuteFilterDelegate.DynamicInvoke(status))
                            return false;
                    }
                    catch
                    {
                    }
            }

            if (FilterDelegate != null)
                try
                {
                    if (!(bool) FilterDelegate.DynamicInvoke(status))
                        return false;
                }
                catch
                {
                    return false;
                }

            return true;
        }

        private void Add(Status status, bool streaming = false)
        {
            if (streaming)
            {
                if (SettingService.Setting.RemoveRetweetOfMyTweet && status.HasRetweetInformation &&
                    status.User.Id == AccountSetting.UserId &&
                    !(Action == SettingSupport.ColumnTypeEnum.Mentions &&
                      SettingService.Setting.ShowRetweetInMentionColumn))
                    return;

                var retindex = Tweets.IndexOf(Tweets.FirstOrDefault(x => x?.Id == status.Id));
                if (retindex != -1 && SettingService.Setting.RemoveRetweetAlreadyReceive &&
                    status.HasRetweetInformation && status.RetweetInformation.User.Id != AccountSetting.UserId)
                    return;

                // 重複確認(ストリーミングでも中断時の更新によっては重複する可能性あり)
                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = Tweets.IndexOf(
                    Tweets.FirstOrDefault(x => x is Status &&
                                               (((Status) x).HasRetweetInformation
                                                   ? ((Status) x).RetweetInformation.Id
                                                   : ((Status) x).Id) == id));
                if (index != -1)
                    return;

                Tweets.Insert(0, status);
            }
            else
            {
                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = Tweets.IndexOf(
                    Tweets.FirstOrDefault(x => x is Status &&
                                               (((Status) x).HasRetweetInformation
                                                   ? ((Status) x).RetweetInformation.Id
                                                   : ((Status) x).Id) == id));
                if (index == -1)
                {
                    index = Tweets.IndexOf(
                        Tweets.FirstOrDefault(x => x is Status &&
                                                   (((Status) x).HasRetweetInformation
                                                       ? ((Status) x).RetweetInformation.Id
                                                       : ((Status) x).Id) < id));
                    if (index == -1)
                        Tweets.Add(status);
                    else
                        Tweets.Insert(index, status);
                }
            }
        }

        private void Add(DirectMessage directMessage, bool streaming = false)
        {
            if (streaming)
            {
                var index = Tweets.IndexOf(
                    Tweets.FirstOrDefault(x => x is DirectMessage && ((DirectMessage) x).Id == directMessage.Id));
                if (index != -1)
                    return;

                Tweets.Insert(0, directMessage);
            }
            else
            {
                var index = Tweets.IndexOf(
                    Tweets.FirstOrDefault(x => x is DirectMessage && ((DirectMessage) x).Id == directMessage.Id));
                if (index == -1)
                {
                    index = Tweets.IndexOf(
                        Tweets.FirstOrDefault(x => x is DirectMessage && ((DirectMessage) x).Id < directMessage.Id));
                    if (index == -1)
                        Tweets.Add(directMessage);
                    else
                        Tweets.Insert(index, directMessage);
                }
            }
        }

        private void Add(EventMessage eventMessage, bool streaming = false)
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
                var index = Tweets.IndexOf(
                    Tweets.FirstOrDefault(x => x is EventMessage && ((EventMessage) x).Id == eventMessage.Id));
                if (index != -1)
                    return;

                Tweets.Insert(0, eventMessage);
            }
            else
            {
                var index = Tweets.IndexOf(
                    Tweets.FirstOrDefault(x => x is EventMessage && ((EventMessage) x).Id == eventMessage.Id));
                if (index == -1)
                {
                    index = Tweets.IndexOf(
                        Tweets.FirstOrDefault(x => x is EventMessage && ((EventMessage) x).Id < eventMessage.Id));
                    if (index == -1)
                        Tweets.Add(eventMessage);
                    else
                        Tweets.Insert(index, eventMessage);
                }
            }
        }

        private void Add(CollectionEntry collection)
        {
            if (collection.Status.HasRetweetInformation)
                return;

            var index = Tweets.IndexOf(
                Tweets.FirstOrDefault(x => x is CollectionEntry && ((CollectionEntry) x).Id == collection.Id));
            if (index == -1)
            {
                index = Tweets.IndexOf(
                    Tweets.FirstOrDefault(x => x is CollectionEntry &&
                                               ((CollectionEntry) x).SortIndex < collection.SortIndex));
                if (index == -1)
                    Tweets.Add(collection);
                else
                    Tweets.Insert(index, collection);
            }
        }

        private void Add(Gap gap)
        {
            var index = Tweets.IndexOf(Tweets.FirstOrDefault(x =>
            {
                if (x is Gap)
                    return false;

                var tid = x.Id;
                if (x is Status)
                    tid = ((Status) x).HasRetweetInformation ? ((Status) x).RetweetInformation.Id : x.Id;

                return tid <= gap.MaxId;
            }));

            if (index == -1)
                return;

            Tweets.Insert(index, gap);
        }

        public void Delete(long id)
        {
            var index = Tweets.IndexOf(Tweets.FirstOrDefault(x =>
            {
                if (x is Status)
                {
                    var status = x as Status;
                    return (status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id) == id;
                }

                return x.Id == id;
            }));

            if (index != -1)
                Tweets.RemoveAt(index);
        }

        public void Delete(Gap gap)
        {
            if (!Tweets.Contains(gap))
                return;

            Tweets.Remove(gap);
        }

        public bool GapCheck(long id)
        {
            var index = Tweets.IndexOf(Tweets.FirstOrDefault(x =>
            {
                var tid = x.Id;
                if (x is Status)
                    tid = ((Status) x).HasRetweetInformation ? ((Status) x).RetweetInformation.Id : x.Id;

                return tid == id;
            }));
            return index == -1 && index != Tweets.Count - 1;
        }

        public void ClearColumn()
        {
            Tweets.Clear();

            if (SettingService.Setting.EnableDatabase)
                Database.Instance.ClearTweet(AccountSetting.UserId, AccountSetting.Instance,
                    Action.ToString("F").ToLower() + "://" + _parameter);
        }

        #region Action変更通知プロパティ

        private SettingSupport.ColumnTypeEnum _action;

        public SettingSupport.ColumnTypeEnum Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        #endregion

        #region Filter変更通知プロパティ

        private Delegate MuteFilterDelegate { get; set; }
        private Delegate FilterDelegate { get; set; }

        private string _filter;

        public string Filter
        {
            get { return _filter; }
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    RaisePropertyChanged();

                    try
                    {
                        FilterDelegate = Compiler.Compile(_filter, false);
                    }
                    catch
                    {
                        FilterDelegate = null;
                    }
                }
            }
        }

        private string _muteFilter;

        public string MuteFilter
        {
            get { return _muteFilter; }
            set
            {
                if (_muteFilter != value)
                {
                    _muteFilter = value;
                    RaisePropertyChanged();

                    try
                    {
                        MuteFilterDelegate = Compiler.Compile(_muteFilter, true);
                    }
                    catch
                    {
                        MuteFilterDelegate = null;
                    }
                }
            }
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

        #region Index変更通知プロパティ

        private int _index;

        public int Index
        {
            get => _index;
            set
            {
                SetProperty(ref _index, value);
                ColumnSetting.Index = value;
            }
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

        #region Instance変更通知プロパティ

        private string _instance;

        public string Instance
        {
            get => _instance;
            set => SetProperty(ref _instance, value);
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

        #region Parameter変更通知プロパティ

        private string _parameter;

        public string Parameter
        {
            get => _parameter;
            set => SetProperty(ref _parameter, value);
        }

        #endregion

        #region Streaming変更通知プロパティ

        private bool _streaming;

        public bool Streaming
        {
            get { return _streaming; }
            set
            {
                if (_streaming != value)
                {
                    _streaming = value;

                    if (value)
                        StartStreaming();
                    else
                        StopStreaming();

                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region SelectedIndex変更通知プロパティ

        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        #endregion

        #region Updating変更通知プロパティ

        private bool _updating;

        public bool Updating
        {
            get => _updating;
            set => SetProperty(ref _updating, value);
        }

        #endregion

        #region UnreadCount変更通知プロパティ

        private int _unreadCount;

        public int UnreadCount
        {
            get => _unreadCount;
            set => SetProperty(ref _unreadCount, value);
        }

        #endregion

        #region IsScrollLockEnabled変更通知プロパティ

        private bool _isScrollLockEnabled;

        public bool IsScrollLockEnabled
        {
            get => _isScrollLockEnabled;
            set => SetProperty(ref _isScrollLockEnabled, value);
        }

        #endregion


        #region Columns

        public ObservableCollection<ITweet> Tweets { get; }

        #endregion
    }
}