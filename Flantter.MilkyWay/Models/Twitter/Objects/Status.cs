using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Status : BindableBase, ITweet
    {
        private static readonly Regex SourceRegex = new Regex("<(\"[^\"]*\"|'[^']*'|[^'\">])*>", RegexOptions.Compiled);

        public Status(CoreTweet.Status cOrigStatus)
        {
            var cStatus = cOrigStatus;
            if (cStatus.RetweetedStatus != null)
                cStatus = cOrigStatus.RetweetedStatus;

            this.CreatedAt = cStatus.CreatedAt.DateTime;
            this.Entities = new Entities(cStatus.Entities, cStatus.ExtendedEntities);
            this.FavoriteCount = cStatus.FavoriteCount.HasValue ? cStatus.FavoriteCount.Value : 0;
            this.RetweetCount = cStatus.RetweetCount.HasValue ? cStatus.RetweetCount.Value : 0;
            this.InReplyToStatusId = cStatus.InReplyToStatusId.HasValue ? cStatus.InReplyToStatusId.Value : 0;
            this.InReplyToScreenName = cStatus.InReplyToScreenName;
            this.InReplyToUserId = cStatus.InReplyToUserId.HasValue ? cStatus.InReplyToUserId.Value : 0;
            this.Id = cStatus.Id;
            this.Source = SourceRegex.Replace(cStatus.Source, "");
            this.Text = cStatus.Text;
            this.User = new User(cStatus.User);
            this.IsFavorited = cStatus.IsFavorited.HasValue ? cStatus.IsFavorited.Value : false;
            this.IsRetweeted = cStatus.IsRetweeted.HasValue ? cStatus.IsRetweeted.Value : false;
            this.RetweetInformation = (cOrigStatus.RetweetedStatus != null) ? new RetweetInformation(cOrigStatus) : null;
            this.HasRetweetInformation = (cOrigStatus.RetweetedStatus != null);
            this.MentionStatus = null;
            this.QuotedStatus = cStatus.QuotedStatus != null ? new Status(cStatus.QuotedStatus) : null;
            this.QuotedStatusId = cStatus.QuotedStatusId.HasValue ? cStatus.QuotedStatusId.Value : 0;
        }

        #region CreatedAt変更通知プロパティ
        private DateTime _CreatedAt;
        public DateTime CreatedAt { get { return this._CreatedAt; } set { this.SetProperty(ref this._CreatedAt, value); } }
        #endregion

        #region Entities変更通知プロパティ
        private Entities _Entities;
        public Entities Entities { get { return this._Entities; } set { this.SetProperty(ref this._Entities, value); } }
        #endregion

        #region FavoriteCount変更通知プロパティ
        private int _FavoriteCount;
        public int FavoriteCount { get { return this._FavoriteCount; } set { this.SetProperty(ref this._FavoriteCount, value); } }
        #endregion

        #region RetweetCount変更通知プロパティ
        private int _RetweetCount;
        public int RetweetCount { get { return this._RetweetCount; } set { this.SetProperty(ref this._RetweetCount, value); } }
        #endregion

        #region InReplyToStatusId変更通知プロパティ
        private long _InReplyToStatusId;
        public long InReplyToStatusId { get { return this._InReplyToStatusId; } set { this.SetProperty(ref this._InReplyToStatusId, value); } }
        #endregion

        #region InReplyToScreenName変更通知プロパティ
        private string _InReplyToScreenName;
        public string InReplyToScreenName { get { return this._InReplyToScreenName; } set { this.SetProperty(ref this._InReplyToScreenName, value); } }
        #endregion

        #region InReplyToUserId変更通知プロパティ
        private long _InReplyToUserId;
        public long InReplyToUserId { get { return this._InReplyToUserId; } set { this.SetProperty(ref this._InReplyToUserId, value); } }
        #endregion

        #region Id変更通知プロパティ
        private long _Id;
        public long Id { get { return this._Id; } set { this.SetProperty(ref this._Id, value); } }
        #endregion

        #region Source変更通知プロパティ
        private string _Source;
        public string Source { get { return this._Source; } set { this.SetProperty(ref this._Source, value); } }
        #endregion

        #region Text変更通知プロパティ
        private string _Text;
        public string Text { get { return this._Text; } set { this.SetProperty(ref this._Text, value); } }
        #endregion

        #region User変更通知プロパティ
        private User _User;
        public User User { get { return this._User; } set { this.SetProperty(ref this._User, value); } }
        #endregion

        #region IsFavorited変更通知プロパティ
        private bool _IsFavorited;
        public bool IsFavorited { get { return this._IsFavorited; } set { this.SetProperty(ref this._IsFavorited, value); } }
        #endregion

        #region IsRetweeted変更通知プロパティ
        private bool _IsRetweeted;
        public bool IsRetweeted { get { return this._IsRetweeted; } set { this.SetProperty(ref this._IsRetweeted, value); } }
        #endregion

        #region RetweetInformation変更通知プロパティ
        private RetweetInformation _RetweetInformation;
        public RetweetInformation RetweetInformation { get { return this._RetweetInformation; } set { this.SetProperty(ref this._RetweetInformation, value); } }
        #endregion

        #region HasRetweetInformation変更通知プロパティ
        private bool _HasRetweetInformation;
        public bool HasRetweetInformation { get { return this._HasRetweetInformation; } set { this.SetProperty(ref this._HasRetweetInformation, value); } }
        #endregion

        #region MentionStatus変更通知プロパティ
        private Status _MentionStatus;
        public Status MentionStatus { get { return this._MentionStatus; } set { this.SetProperty(ref this._MentionStatus, value); } }
        #endregion

        #region QuotedStatus変更通知プロパティ
        private Status _QuotedStatus;
        public Status QuotedStatus { get { return this._QuotedStatus; } set { this.SetProperty(ref this._QuotedStatus, value); } }
        #endregion

        #region QuotedStatusId変更通知プロパティ
        private long _QuotedStatusId;
        public long QuotedStatusId { get { return this._QuotedStatusId; } set { this.SetProperty(ref this._QuotedStatusId, value); } }
        #endregion
    }

    public class RetweetInformation : BindableBase
    {
        public RetweetInformation(CoreTweet.Status cOrigStatus)
        {
            if (cOrigStatus.RetweetedStatus == null)
                return;

            this.User = new User(cOrigStatus.User);
            this.Id = cOrigStatus.Id;
            this.CreatedAt = cOrigStatus.CreatedAt.DateTime;
        }
        
        #region User変更通知プロパティ
        private User _User;
        public User User
        {
            get { return this._User; }
            set { this.SetProperty(ref this._User, value); }
        }
        #endregion
        #region Id変更通知プロパティ
        private long _Id;
        public long Id
        {
            get { return this._Id; }
            set { this.SetProperty(ref this._Id, value); }
        }
        #endregion
        #region CreatedAt変更通知プロパティ
        private DateTime _CreatedAt;
        public DateTime CreatedAt
        {
            get { return this._CreatedAt; }
            set { this.SetProperty(ref this._CreatedAt, value); }
        }
        #endregion
    }
}
