using CoreTweet.Streaming;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Services
{
    public class TweetDeleteEventArgs : EventArgs
    {
        #region Constructor
        public TweetDeleteEventArgs(TypeEnum type, long Id, long UserId)
        {
            this.Type = type;
            this.UserId = UserId;
            this.Id = Id;
        }
        #endregion

        public enum TypeEnum
        {
            Status,
            DirectMessage,
        }

        public long UserId { get; private set; }
        public long Id { get; private set; }

        public TypeEnum Type { get; private set; }
    }

    public class TweetEventArgs : EventArgs
    {
        #region Constructor
        public TweetEventArgs(Status status, long userId, List<string> parameter, bool streaming = false)
        {
            this.Type = TypeEnum.Status;
            this.Status = status;
            this.UserId = userId;
            this.Parameter = parameter;
            this.Streaming = streaming;
        }
        public TweetEventArgs(DirectMessage directMessage, long userId, List<string> parameter, bool streaming = false)
        {
            this.Type = TypeEnum.DirectMessage;
            this.DirectMessage = directMessage;
            this.UserId = userId;
            this.Parameter = parameter;
            this.Streaming = streaming;
        }
        public TweetEventArgs(Twitter.Objects.EventMessage eventMessage, long userId, List<string> parameter, bool streaming = true)
        {
            this.Type = TypeEnum.EventMessage;
            this.EventMessage = eventMessage;
            this.UserId = userId;
            this.Parameter = parameter;
            this.Streaming = true;
        }
        public TweetEventArgs(Twitter.Objects.CollectionEntry collectionEntry, long userId, List<string> parameter, bool streaming = true)
        {
            this.Type = TypeEnum.CollectionEntry;
            this.CollectionEntry = collectionEntry;
            this.UserId = userId;
            this.Parameter = parameter;
            this.Streaming = false;
        }
        #endregion

        public enum TypeEnum
        {
            Status,
            DirectMessage,
            EventMessage,
            CollectionEntry,
        }

        public bool Streaming { get; private set; }

        public long UserId { get; private set; }

        public Status Status { get; private set; }
        public DirectMessage DirectMessage { get; private set; }
        public Twitter.Objects.EventMessage EventMessage { get; private set; }
        public Twitter.Objects.CollectionEntry CollectionEntry { get; private set; }

        public TypeEnum Type { get; private set; }

        public List<string> Parameter { get; private set; }
    }

    public class Connecter
    {
        private static Connecter _Instance = new Connecter();
        private Connecter() { }

        public static Connecter Instance
        {
            get { return _Instance; }
        }

        public event EventHandler<TweetEventArgs> TweetReceive_CommandExecute;
        public void TweetReceive_OnCommandExecute(object sender, TweetEventArgs e)
        {
            this.TweetReceive_CommandExecute(sender, e);
        }

        public event EventHandler<TweetDeleteEventArgs> TweetDelete_CommandExecute;
        public void TweetDelete_OnCommandExecute(object sender, TweetDeleteEventArgs e)
        {
            this.TweetDelete_CommandExecute(sender, e);
        }

        public void Initialize()
        {
            this.TweetCollecter = new Dictionary<long, TweetCollecterService>();
            if (SettingService.Setting.EnableDatabase)
                Database.Database.Instance.Initialize();
        }

        public void Free()
        {
            Database.Database.Instance.Free();
        }

        public void AddAccount(AccountSetting account)
        {
            if (!this.TweetCollecter.ContainsKey(account.UserId))
                this.TweetCollecter[account.UserId] = new TweetCollecterService(account.UserId);
        }

        public void RemoveAccount(AccountSetting account)
        {
            if (this.TweetCollecter.ContainsKey(account.UserId))
            {
                this.TweetCollecter[account.UserId].Remove();
                this.TweetCollecter.Remove(account.UserId);
            }
        }
        
        public class TweetCollecterService
        {
            public event EventHandler<TweetDeleteEventArgs> TweetDelete_CommandExecute;
            public event EventHandler<TweetEventArgs> TweetReceive_CommandExecute;

            public long UserId { get; set; }
            
            public object MuteIdsLock { get; set; } = new object();

            public List<long> MuteIds { get; set; }
            public List<long> NoRetweetIds { get; set; }
            public List<long> BlockIds { get; set; }

            public object EntitiesObjectsLock = new object();
            public List<string> ScreenNameObjects { get; set; }
            public List<string> HashTagObjects { get; set; }
            public List<User> UserObjects { get; set; }

            private IDisposable tweetReceiveDisposableObject = null;
            private IDisposable tweetDeleteDisposableObject = null;
            public TweetCollecterService(long userId)
            {
                this.UserId = userId;

                this.NoRetweetIds = new List<long>();
                this.MuteIds = new List<long>();
                this.BlockIds = new List<long>();

                this.ScreenNameObjects = new List<string>();
                this.HashTagObjects = new List<string>();
                this.UserObjects = new List<User>();

                tweetDeleteDisposableObject = Observable.FromEvent<EventHandler<TweetDeleteEventArgs>, TweetDeleteEventArgs>(
                    h => (sender, e) => h(e),
                    h => Connecter.Instance.TweetDelete_CommandExecute += h,
                    h => Connecter.Instance.TweetDelete_CommandExecute -= h).Where(x => x.UserId == this.UserId).Subscribe(
                    e =>
                    {
                        this.TweetDelete_CommandExecute?.Invoke(this, e);
                    },
                    ex => Debug.WriteLine(ex.ToString() + "\nMessage:" + ex.Message),
                    () => Debug.WriteLine("Flantter.MilkyWay.Models.Services.Connecter.TweetCollecterService.OnCompleted"));


                tweetReceiveDisposableObject = Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                    h => (sender, e) => h(e),
                    h => Connecter.Instance.TweetReceive_CommandExecute += h,
                    h => Connecter.Instance.TweetReceive_CommandExecute -= h).Where(x => x.UserId == this.UserId).Subscribe(
                    e =>
                    {
                        if (this.TweetReceive_CommandExecute != null)
                            this.TweetReceive_CommandExecute(this, e);

                        if (SettingService.Setting.EnableDatabase)
                        {
                            switch (e.Type)
                            {
                                case TweetEventArgs.TypeEnum.Status:
                                    Database.Database.Instance.InsertTweet(e.Status, e.Parameter, e.UserId);
                                    break;
                                case TweetEventArgs.TypeEnum.DirectMessage:
                                    Database.Database.Instance.InsertTweet(e.DirectMessage, e.Parameter, e.UserId);
                                    break;
                                case TweetEventArgs.TypeEnum.EventMessage:
                                    Database.Database.Instance.InsertTweet(e.EventMessage, e.Parameter, e.UserId);
                                    break;
                                case TweetEventArgs.TypeEnum.CollectionEntry:
                                    Database.Database.Instance.InsertTweet(e.CollectionEntry, e.Parameter, e.UserId);
                                    break;
                            }
                        }

                        // Todo : 起動時の軽量化必須？

                        if (e.Type == TweetEventArgs.TypeEnum.Status)
                        {
                            lock (this.EntitiesObjectsLock)
                            {
                                if (!this.ScreenNameObjects.Contains(e.Status.User.ScreenName))
                                    this.ScreenNameObjects.Add(e.Status.User.ScreenName);

                                if (!this.UserObjects.Any(x => x.ScreenName == e.Status.User.ScreenName))
                                    this.UserObjects.Add(e.Status.User);

                                if (e.Status.Entities.UserMentions != null)
                                {
                                    foreach (var screenName in e.Status.Entities.UserMentions)
                                    {
                                        if (!this.ScreenNameObjects.Contains(screenName.ScreenName))
                                            this.ScreenNameObjects.Add(screenName.ScreenName);
                                    }
                                }
                                if (e.Status.Entities.HashTags != null)
                                {
                                    foreach (var hashTag in e.Status.Entities.HashTags)
                                    {
                                        if (!this.HashTagObjects.Contains(hashTag.Tag))
                                            this.HashTagObjects.Add(hashTag.Tag);
                                    }
                                }
                            }
                        }                        
                    },
                    ex => Debug.WriteLine(ex.ToString() + "\nMessage:" + ex.Message),
                    () => Debug.WriteLine("TweetServiceProvider.TweetCollecterService.OnCompleted"));
            }

            public void Remove()
            {
                if (tweetReceiveDisposableObject != null)
                    tweetReceiveDisposableObject.Dispose();

                if (tweetDeleteDisposableObject != null)
                    tweetDeleteDisposableObject.Dispose();
            }
        }
        public Dictionary<long, TweetCollecterService> TweetCollecter { get; set; }
    }
}
