using System;
using System.Collections.Generic;

namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class EventMessage : ITweet
    {
        private readonly Dictionary<string, string> _mastodonTypeReplaceDictionary = new Dictionary<string, string>
        {
            {"mention", "Mention"},
            {"reblog", "Retweet"},
            {"favourite", "Favorite"},
            {"follow", "Follow"}
        };

        public EventMessage(CoreTweet.Streaming.EventMessage cEventMessage)
        {
            CreatedAt = cEventMessage.CreatedAt.DateTime;
            Id = CreatedAt.ToBinary();
            Source = new User(cEventMessage.Source);
            Target = new User(cEventMessage.Target);
            TargetStatus = cEventMessage.TargetStatus != null ? new Status(cEventMessage.TargetStatus) : null;
            Type = cEventMessage.Event.ToString();
        }

        public EventMessage(TootNet.Objects.Notification cNotification)
        {
            CreatedAt = cNotification.CreatedAt;
            Id = cNotification.Id;
            Source = new User(cNotification.Account);
            Target = null;
            TargetStatus = cNotification.Status != null ? new Status(cNotification.Status) : null;
            Type = _mastodonTypeReplaceDictionary.ContainsKey(cNotification.Type.ToLower())
                ? _mastodonTypeReplaceDictionary[cNotification.Type.ToLower()]
                : cNotification.Type.ToLower();
        }

        public EventMessage(Status cStatus)
        {
            if (!cStatus.HasRetweetInformation)
                return;

            CreatedAt = cStatus.CreatedAt;
            Id = CreatedAt.ToBinary();
            Source = cStatus.RetweetInformation.User;
            Target = cStatus.User;
            TargetStatus = cStatus;
            Type = "Retweet";
        }

        public EventMessage()
        {
        }

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

        #region CreatedAt変更通知プロパティ

        public DateTime CreatedAt { get; set; }

        #endregion

        #region Id変更通知プロパティ

        public long Id { get; set; }

        #endregion
    }
}