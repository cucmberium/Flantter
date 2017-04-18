using System;
using System.Text.RegularExpressions;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Status : ITweet
    {
        private static readonly Regex SourceRegex =
            new Regex(@"^<a href="".+"" rel=""nofollow"">(.+)</a>$", RegexOptions.Compiled);

        private static readonly Regex ContentRegex =
            new Regex(@"<(""[^""]*""|'[^']*'|[^'"">])*>", RegexOptions.Compiled);

        private static readonly Regex LinkRegex = 
            new Regex(@"\s*<a href=\""(.*?)\"".*?>(.*?)</a>\s*", RegexOptions.Compiled);

        public Status(CoreTweet.Status cOrigStatus)
        {
            var cStatus = cOrigStatus;
            if (cStatus.RetweetedStatus != null)
                cStatus = cOrigStatus.RetweetedStatus;

            CreatedAt = cStatus.CreatedAt.DateTime;
            Entities = new Entities(cStatus.ExtendedTweet?.Entities ?? cStatus.Entities,
                cStatus.ExtendedTweet?.ExtendedEntities ?? cStatus.ExtendedEntities);
            FavoriteCount = cStatus.FavoriteCount.HasValue ? cStatus.FavoriteCount.Value : 0;
            RetweetCount = cStatus.RetweetCount.HasValue ? cStatus.RetweetCount.Value : 0;
            InReplyToStatusId = cStatus.InReplyToStatusId.HasValue ? cStatus.InReplyToStatusId.Value : 0;
            InReplyToScreenName = cStatus.InReplyToScreenName;
            InReplyToUserId = cStatus.InReplyToUserId.HasValue ? cStatus.InReplyToUserId.Value : 0;
            Id = cStatus.Id;
            Text = cStatus.ExtendedTweet?.FullText ?? cStatus.FullText ?? cStatus.Text;
            User = cStatus.User != null ? new User(cStatus.User) : null;
            IsFavorited = cStatus.IsFavorited.HasValue ? cStatus.IsFavorited.Value : false;
            IsRetweeted = cStatus.IsRetweeted.HasValue ? cStatus.IsRetweeted.Value : false;
            RetweetInformation = cOrigStatus.RetweetedStatus != null ? new RetweetInformation(cOrigStatus) : null;
            HasRetweetInformation = cOrigStatus.RetweetedStatus != null;
            MentionStatus = null;
            QuotedStatus = cStatus.QuotedStatus != null && cStatus.QuotedStatus.User != null
                ? new Status(cStatus.QuotedStatus)
                : null;
            QuotedStatusId = cStatus.QuotedStatusId.HasValue && QuotedStatus != null ? cStatus.QuotedStatusId.Value : 0;

            var sourceMatch = SourceRegex.Match(cStatus.Source);
            if (sourceMatch.Success)
                Source = sourceMatch.Groups[1].Value;
            else
                Source = cStatus.Source;
        }

        public Status(Mastonet.Entities.Status cOrigStatus)
        {
            var cStatus = cOrigStatus;
            if (cStatus.Reblog != null)
                cStatus = cStatus.Reblog;

            CreatedAt = cStatus.CreatedAt;
            Entities = new Entities(cStatus.MediaAttachments, cStatus.Mentions, cStatus.Tags, cStatus.Content);
            FavoriteCount = cStatus.FavouritesCount;
            RetweetCount = cStatus.ReblogCount;
            InReplyToStatusId = cStatus.InReplyToId.HasValue ? cStatus.InReplyToId.Value : 0;
            InReplyToScreenName = "";
            InReplyToUserId = cStatus.InReplyToAccountId.HasValue ? cStatus.InReplyToAccountId.Value : 0;
            Id = cStatus.Id;
            Text = ContentRegex.Replace(LinkRegex.Replace(cStatus.Content.Replace("<br />", "\n"), x => " " + x.Groups[2].Value + " "), "").Trim();
            User = cStatus.Account != null ? new User(cStatus.Account) : null;
            IsFavorited = cStatus.Favourited.HasValue ? cStatus.Favourited.Value : false;
            IsRetweeted = cStatus.Reblogged.HasValue ? cStatus.Reblogged.Value : false;
            RetweetInformation = cOrigStatus.Reblog != null ? new RetweetInformation(cOrigStatus) : null;
            HasRetweetInformation = cOrigStatus.Reblog != null;
            MentionStatus = null;
            QuotedStatus = null;
            QuotedStatusId = 0;

            Source = cStatus.Application != null ? cStatus.Application.Name : "Web";
        }

        public Status()
        {
        }

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

        #region CreatedAt変更通知プロパティ

        public DateTime CreatedAt { get; set; }

        #endregion

        #region Id変更通知プロパティ

        public long Id { get; set; }

        #endregion
    }

    public class RetweetInformation
    {
        public RetweetInformation(CoreTweet.Status cOrigStatus)
        {
            if (cOrigStatus.RetweetedStatus == null)
                return;

            User = new User(cOrigStatus.User);
            Id = cOrigStatus.Id;
            CreatedAt = cOrigStatus.CreatedAt.DateTime;
        }

        public RetweetInformation(Mastonet.Entities.Status cOrigStatus)
        {
            if (cOrigStatus.Reblog == null)
                return;

            User = new User(cOrigStatus.Account);
            Id = cOrigStatus.Id;
            CreatedAt = cOrigStatus.CreatedAt;
        }

        public RetweetInformation()
        {
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