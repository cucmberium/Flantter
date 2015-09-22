using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Status : ITweet
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
        public DateTime CreatedAt { get; set; }
        #endregion

        #region Entities変更通知プロパティ
        public Entities Entities { get; set; }
        #endregion

        #region FavoriteCount変更通知プロパティ
        public int FavoriteCount { get; set; }
        #endregion

        #region RetweetCount変更通知プロパティ
        public int RetweetCount { get; set; }
        #endregion

        #region InReplyToStatusId変更通知プロパティ
        public long InReplyToStatusId { get; set; }
        #endregion

        #region InReplyToScreenName変更通知プロパティ
        public string InReplyToScreenName { get; set; }
        #endregion

        #region InReplyToUserId変更通知プロパティ
        public long InReplyToUserId { get; set; }
        #endregion

        #region Id変更通知プロパティ
        public long Id { get; set; }
        #endregion

        #region Source変更通知プロパティ
        public string Source { get; set; }
        #endregion

        #region Text変更通知プロパティ
        public string Text { get; set; }
        #endregion

        #region User変更通知プロパティ
        public User User { get; set; }
        #endregion

        #region IsFavorited変更通知プロパティ
        public bool IsFavorited { get; set; }
        #endregion

        #region IsRetweeted変更通知プロパティ
        public bool IsRetweeted { get; set; }
        #endregion

        #region RetweetInformation変更通知プロパティ
        public RetweetInformation RetweetInformation { get; set; }
        #endregion

        #region HasRetweetInformation変更通知プロパティ
        public bool HasRetweetInformation { get; set; }
        #endregion

        #region MentionStatus変更通知プロパティ
        public Status MentionStatus { get; set; }
        #endregion

        #region QuotedStatus変更通知プロパティ
        public Status QuotedStatus { get; set; }
        #endregion

        #region QuotedStatusId変更通知プロパティ
        public long QuotedStatusId { get; set; }
        #endregion
    }

    public class RetweetInformation
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
        public User User { get; set; }
        #endregion
        #region Id変更通知プロパティ
        public long Id { get; set; }
        #endregion
        #region CreatedAt変更通知プロパティ
        public DateTime CreatedAt { get; set; }
        #endregion
    }
}
