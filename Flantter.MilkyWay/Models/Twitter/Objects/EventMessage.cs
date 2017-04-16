using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class EventMessage : ITweet
    {
        private readonly Dictionary<string, string> MastodonTypeReplaceDictionary = new Dictionary<string, string>()
        {
            { "mention", "Mention" },
            { "reblog", "Retweet" },
            { "favourite", "Favorite" },
            { "follow", "Follow" }
        };

        public EventMessage(CoreTweet.Streaming.EventMessage cEventMessage)
        {
            this.CreatedAt = cEventMessage.CreatedAt.DateTime;
            this.Id = this.CreatedAt.ToBinary();
            this.Source = new User(cEventMessage.Source);
            this.Target = new User(cEventMessage.Target);
            this.TargetStatus = (cEventMessage.TargetStatus != null) ? new Status(cEventMessage.TargetStatus) : null;
            this.Type = cEventMessage.Event.ToString();
        }

        public EventMessage(Mastonet.Entities.Notification cNotification)
        {
            this.CreatedAt = cNotification.CreatedAt;
            this.Id = cNotification.Id;
            this.Source = new User(cNotification.Account);
            this.Target = null;
            this.TargetStatus = (cNotification.Status != null) ? new Status(cNotification.Status) : null;
            this.Type = MastodonTypeReplaceDictionary.ContainsKey(cNotification.Type) ? MastodonTypeReplaceDictionary[cNotification.Type] : cNotification.Type;
        }

        public EventMessage(Twitter.Objects.Status cStatus)
        {
            if (!cStatus.HasRetweetInformation)
                return;

            this.CreatedAt = cStatus.CreatedAt;
            this.Id = this.CreatedAt.ToBinary();
            this.Source = cStatus.RetweetInformation.User;
            this.Target = cStatus.User;
            this.TargetStatus = cStatus;
            this.Type = "Retweet";
        }

        public EventMessage()
        {
        }

        #region CreatedAt変更通知プロパティ
        public DateTime CreatedAt { get; set; }
        #endregion

        #region Source変更通知プロパティ
        public User Source { get; set; }
        #endregion

        #region Target変更通知プロパティ
        public User Target { get; set; }
        #endregion

        #region TargetStatus変更通知プロパティ
        public Status TargetStatus { get; set; }
        #endregion

        #region Type変更通知プロパティ
        public string Type { get; set; }
        #endregion

        #region Id変更通知プロパティ
        public long Id { get; set; }
        #endregion
    }
}
