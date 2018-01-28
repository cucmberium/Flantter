using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;

namespace Flantter.MilkyWay.Models.Services
{
    public class TweetDeleteEventArgs : EventArgs
    {
        public enum TypeEnum
        {
            Status,
            DirectMessage
        }

        #region Constructor

        public TweetDeleteEventArgs(TypeEnum type, long id, long userId, string instance)
        {
            Type = type;
            UserId = userId;
            Instance = instance;
            Id = id;
        }

        #endregion

        public long UserId { get; }
        public string Instance { get; }
        public long Id { get; }

        public TypeEnum Type { get; }
    }

    public class TweetEventArgs : EventArgs
    {
        public enum TypeEnum
        {
            Status,
            DirectMessage,
            EventMessage,
            CollectionEntry
        }

        #region Constructor

        public TweetEventArgs(Status status, long userId, string instance, List<string> parameter, bool streaming = false)
        {
            Type = TypeEnum.Status;
            Status = status;
            UserId = userId;
            Instance = instance;
            Parameter = parameter;
            Streaming = streaming;
        }

        public TweetEventArgs(DirectMessage directMessage, long userId, string instance, List<string> parameter, bool streaming = false)
        {
            Type = TypeEnum.DirectMessage;
            DirectMessage = directMessage;
            UserId = userId;
            Instance = instance;
            Parameter = parameter;
            Streaming = streaming;
        }

        public TweetEventArgs(EventMessage eventMessage, long userId, string instance, List<string> parameter, bool streaming = true)
        {
            Type = TypeEnum.EventMessage;
            EventMessage = eventMessage;
            UserId = userId;
            Instance = instance;
            Parameter = parameter;
            Streaming = streaming;
        }

        public TweetEventArgs(CollectionEntry collectionEntry, long userId, string instance, List<string> parameter,
            bool streaming = false)
        {
            Type = TypeEnum.CollectionEntry;
            CollectionEntry = collectionEntry;
            UserId = userId;
            Instance = instance;
            Parameter = parameter;
            Streaming = streaming;
        }

        #endregion

        public bool Streaming { get; }

        public long UserId { get; }
        public string Instance { get; }

        public Status Status { get; }
        public DirectMessage DirectMessage { get; }
        public EventMessage EventMessage { get; }
        public CollectionEntry CollectionEntry { get; }

        public TypeEnum Type { get; }

        public List<string> Parameter { get; }
    }

    public class Connecter
    {
        private Connecter()
        {
        }

        public static Connecter Instance { get; } = new Connecter();

        public Dictionary<string, TweetCollecterService> TweetCollecter { get; set; }

        public event EventHandler<TweetEventArgs> TweetReceiveCommandExecute;

        public void TweetReceive_OnCommandExecute(object sender, TweetEventArgs e)
        {
            TweetReceiveCommandExecute?.Invoke(sender, e);
        }

        public event EventHandler<TweetDeleteEventArgs> TweetDeleteCommandExecute;

        public void TweetDelete_OnCommandExecute(object sender, TweetDeleteEventArgs e)
        {
            TweetDeleteCommandExecute?.Invoke(sender, e);
        }

        public void Initialize()
        {
            TweetCollecter = new Dictionary<string, TweetCollecterService>();
            if (SettingService.Setting.EnableDatabase)
                Database.Database.Instance.Initialize();
        }

        public void Free()
        {
            Database.Database.Instance.Free();
        }

        public void AddAccount(AccountSetting account)
        {
            if (!TweetCollecter.ContainsKey(account.UserId + ":" + account.Instance))
                TweetCollecter[account.UserId + ":" + account.Instance] = new TweetCollecterService(account.UserId, account.Instance);
        }

        public void RemoveAccount(AccountSetting account)
        {
            if (TweetCollecter.ContainsKey(account.UserId + ":" + account.Instance))
            {
                TweetCollecter[account.UserId + ":" + account.Instance].Remove();
                TweetCollecter.Remove(account.UserId + ":" + account.Instance);
            }
        }

        public class TweetCollecterService
        {
            private readonly IDisposable _tweetDeleteDisposableObject;
            private readonly IDisposable _tweetReceiveDisposableObject;

            public TweetCollecterService(long userId, string instance)
            {
                UserId = userId;
                Instance = instance;

                NoRetweetIds = new List<long>();
                MuteIds = new List<long>();
                BlockIds = new List<long>();

                ScreenNameObjects = new List<KeyValuePair<string, long>>();
                HashTagObjects = new List<string>();
                UserObjects = new List<User>();

                _tweetDeleteDisposableObject = Observable
                    .FromEvent<EventHandler<TweetDeleteEventArgs>, TweetDeleteEventArgs>(
                        h => (sender, e) => h(e),
                        h => Connecter.Instance.TweetDeleteCommandExecute += h,
                        h => Connecter.Instance.TweetDeleteCommandExecute -= h)
                    .Where(x => x.UserId == UserId && x.Instance == Instance)
                    .SubscribeOn(NewThreadScheduler.Default)
                    .Subscribe(e => { TweetDeleteCommandExecute?.Invoke(this, e); });


                _tweetReceiveDisposableObject = Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                        h => (sender, e) => h(e),
                        h => Connecter.Instance.TweetReceiveCommandExecute += h,
                        h => Connecter.Instance.TweetReceiveCommandExecute -= h)
                    .Where(x => x.UserId == UserId && x.Instance == Instance)
                    .SubscribeOn(NewThreadScheduler.Default)
                    .Subscribe(
                        e =>
                        {
                            TweetReceiveCommandExecute?.Invoke(this, e);

                            if (SettingService.Setting.EnableDatabase)
                                switch (e.Type)
                                {
                                    case TweetEventArgs.TypeEnum.Status:
                                        Database.Database.Instance.InsertTweet(e.Status, e.Parameter, e.UserId, e.Instance);
                                        break;
                                    case TweetEventArgs.TypeEnum.DirectMessage:
                                        Database.Database.Instance.InsertTweet(e.DirectMessage, e.Parameter, e.UserId, e.Instance);
                                        break;
                                    case TweetEventArgs.TypeEnum.EventMessage:
                                        Database.Database.Instance.InsertTweet(e.EventMessage, e.Parameter, e.UserId, e.Instance);
                                        break;
                                    case TweetEventArgs.TypeEnum.CollectionEntry:
                                        Database.Database.Instance.InsertTweet(e.CollectionEntry, e.Parameter,
                                            e.UserId, e.Instance);
                                        break;
                                }

                            // Todo : 起動時の軽量化必須？
                            if (e.Type == TweetEventArgs.TypeEnum.Status)
                                lock (EntitiesObjectsLock)
                                {
                                    if (ScreenNameObjects.All(x => x.Key != e.Status.User.ScreenName))
                                        ScreenNameObjects.Add(new KeyValuePair<string, long>(e.Status.User.ScreenName, e.Status.User.Id));

                                    if (UserObjects.All(x => x.ScreenName != e.Status.User.ScreenName))
                                        UserObjects.Add(e.Status.User);

                                    if (e.Status.Entities.UserMentions != null)
                                        foreach (var screenName in e.Status.Entities.UserMentions)
                                            if (ScreenNameObjects.All(x => x.Key != screenName.ScreenName))
                                                ScreenNameObjects.Add(new KeyValuePair<string, long>(screenName.ScreenName, screenName.Id));
                                    if (e.Status.Entities.HashTags != null)
                                        foreach (var hashTag in e.Status.Entities.HashTags)
                                            if (!HashTagObjects.Contains(hashTag.Tag))
                                                HashTagObjects.Add(hashTag.Tag);
                                }
                        });
            }

            public long UserId { get; set; }
            public string Instance { get; set; }

            public object EntitiesObjectsLock { get; set; } = new object();
            public object MuteIdsLock { get; set; } = new object();

            public List<long> MuteIds { get; set; }
            public List<long> NoRetweetIds { get; set; }
            public List<long> BlockIds { get; set; }
            public List<KeyValuePair<string, long>> ScreenNameObjects { get; set; }
            public List<string> HashTagObjects { get; set; }
            public List<User> UserObjects { get; set; }
            public event EventHandler<TweetDeleteEventArgs> TweetDeleteCommandExecute;
            public event EventHandler<TweetEventArgs> TweetReceiveCommandExecute;

            public void Remove()
            {
                _tweetReceiveDisposableObject?.Dispose();
                _tweetDeleteDisposableObject?.Dispose();
            }
        }
    }
}