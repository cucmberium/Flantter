using CoreTweet.Streaming;
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
using WinRTXamlToolkit.Async;

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
        #endregion

        public enum TypeEnum
        {
            Status,
            DirectMessage,
            EventMessage
        }

        public bool Streaming { get; private set; }

        public long UserId { get; private set; }

        public Status Status { get; private set; }
        public DirectMessage DirectMessage { get; private set; }
        public Twitter.Objects.EventMessage EventMessage { get; private set; }

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
            Databases.Instance.Initialize();
        }

        public void AddAccount(AccountSetting account)
        {
            Databases.Instance.CreateUserTable(account);
            if (!this.TweetCollecter.ContainsKey(account.UserId))
                this.TweetCollecter[account.UserId] = new TweetCollecterService(account.UserId);
        }

        public void RemoveAccount(AccountSetting account)
        {
            Databases.Instance.RemoveUserTable(account);

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

            private IDisposable tweetReceiveDisposableObject = null;
            private IDisposable tweetDeleteDisposableObject = null;
            public TweetCollecterService(long userId)
            {
                this.UserId = userId;
                
                tweetDeleteDisposableObject = Observable.FromEvent<EventHandler<TweetDeleteEventArgs>, TweetDeleteEventArgs>(
                    h => (sender, e) => h(e),
                    h => Connecter.Instance.TweetDelete_CommandExecute += h,
                    h => Connecter.Instance.TweetDelete_CommandExecute -= h).Subscribe(
                    e =>
                    {
                        if (e.UserId != this.UserId)
                            return;

                        if (this.TweetDelete_CommandExecute != null)
                            this.TweetDelete_CommandExecute(this, e);

                        if (SettingService.Setting.EnableDatabase)
                            Databases.Instance.RemoveTweet(e);
                    },
                    ex => Debug.WriteLine(ex.ToString() + "\nMessage:" + ex.Message),
                    () => Debug.WriteLine("Flantter.MilkyWay.Models.Services.Connecter.TweetCollecterService.OnCompleted"));


                tweetReceiveDisposableObject = Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                    h => (sender, e) => h(e),
                    h => Connecter.Instance.TweetReceive_CommandExecute += h,
                    h => Connecter.Instance.TweetReceive_CommandExecute -= h).Subscribe(
                    e =>
                    {
                        if (e.UserId != this.UserId)
                            return;

                        if (this.TweetReceive_CommandExecute != null)
                            this.TweetReceive_CommandExecute(this, e);

                        if (SettingService.Setting.EnableDatabase)
                            Databases.Instance.StoreTweet(e);
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
