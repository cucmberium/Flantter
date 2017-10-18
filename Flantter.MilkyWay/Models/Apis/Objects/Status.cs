using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class Status : ITweet
    {
        public static readonly Regex SourceRegex =
            new Regex(@"^<a href="".+"" rel=""nofollow"">(.+)</a>$", RegexOptions.Compiled);

        public static readonly Regex ContentRegex =
            new Regex(@"<(""[^""]*""|'[^']*'|[^'"">])*>", RegexOptions.Compiled);

        public static readonly Regex LinkRegex =
            new Regex(@"\s*(@?)<a href=\""(.*?)\"".*?>(.*?)</a>\s*", RegexOptions.Compiled);

        public Status(CoreTweet.Status cOrigStatus)
        {
            var cStatus = cOrigStatus;
            if (cStatus.RetweetedStatus != null)
                cStatus = cOrigStatus.RetweetedStatus;

            CreatedAt = cStatus.CreatedAt.DateTime;
            Entities = new Entities(cStatus.ExtendedTweet?.Entities ?? cStatus.Entities,
                cStatus.ExtendedTweet?.ExtendedEntities ?? cStatus.ExtendedEntities);
            FavoriteCount = cStatus.FavoriteCount ?? 0;
            RetweetCount = cStatus.RetweetCount ?? 0;
            InReplyToStatusId = cStatus.InReplyToStatusId ?? 0;
            InReplyToScreenName = cStatus.InReplyToScreenName;
            InReplyToUserId = cStatus.InReplyToUserId ?? 0;
            Id = cStatus.Id;
            Text = cStatus.ExtendedTweet?.FullText ?? cStatus.FullText ?? cStatus.Text;
            User = cStatus.User != null ? new User(cStatus.User) : null;
            IsFavorited = cStatus.IsFavorited ?? false;
            IsRetweeted = cStatus.IsRetweeted ?? false;
            PossiblySensitive = cStatus.PossiblySensitive ?? false;
            RetweetInformation = cOrigStatus.RetweetedStatus != null ? new RetweetInformation(cOrigStatus) : null;
            HasRetweetInformation = cOrigStatus.RetweetedStatus != null;
            MentionStatus = null;
            QuotedStatus = cStatus.QuotedStatus?.User != null
                ? new Status(cStatus.QuotedStatus)
                : null;
            QuotedStatusId = cStatus.QuotedStatusId.HasValue && QuotedStatus != null ? cStatus.QuotedStatusId.Value : 0;
            Url = "https://twitter.com/" + cStatus.User?.ScreenName + "/status/" + cStatus.Id;

            var sourceMatch = SourceRegex.Match(cStatus.Source);
            Source = sourceMatch.Success ? sourceMatch.Groups[1].Value : cStatus.Source;
        }

        public Status(TootNet.Objects.Status cOrigStatus)
        {
            var cStatus = cOrigStatus;
            if (cStatus.Reblog != null)
                cStatus = cStatus.Reblog;

            CreatedAt = cStatus.CreatedAt;
            FavoriteCount = cStatus.FavouritesCount;
            RetweetCount = cStatus.ReblogsCount;
            InReplyToStatusId = cStatus.InReplyToId ?? 0;
            InReplyToScreenName = "";
            InReplyToUserId = cStatus.InReplyToAccountId ?? 0;
            Id = cStatus.Id;

            var urlEntities = new List<string>();
            var text = cStatus.Content.Replace("<br />", "\n");
            text = LinkRegex.Replace(text, match =>
            {
                var userMention = cStatus.Mentions.Where(x =>
                        x.Url == match.Groups[2].Value || x.Url.Replace("/@", "/users/") == match.Groups[2].Value)
                    .ToArray();
                if (userMention.Length != 0)
                    return " @" + userMention.First().Acct + " ";

                urlEntities.Add(match.Groups[2].Value);
                return " " + match.Groups[1]?.Value + match.Groups[3].Value + " ";
            });
            text = ContentRegex.Replace(text, "").Trim();
            text = EmojiPatterns.LightValidEmoji.Replace(text,
                x => EmojiPatterns.EmojiDictionary.TryGetValue(x.Groups[2].Value, out string val) ? val : x.Value);
            Text = text;

            Entities = new Entities(cStatus.MediaAttachments, cStatus.Mentions, cStatus.Tags, urlEntities, text);

            User = cStatus.Account != null ? new User(cStatus.Account) : null;
            IsFavorited = cStatus.Favourited ?? false;
            IsRetweeted = cStatus.Reblogged ?? false;
            PossiblySensitive = cStatus.Sensitive ?? false;
            RetweetInformation = cOrigStatus.Reblog != null ? new RetweetInformation(cOrigStatus) : null;
            HasRetweetInformation = cOrigStatus.Reblog != null;
            MentionStatus = null;
            QuotedStatus = null;
            QuotedStatusId = 0;
            Url = cStatus.Url;
            Source = cStatus.Application != null ? cStatus.Application.Name : "Web";

            SpoilerText = cStatus.SpoilerText;
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

        #region PossiblySensitive変更通知プロパティ

        public bool PossiblySensitive { get; set; }

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

        #region Url変更通知プロパティ

        public string Url { get; set; }

        #endregion

        #region SpoilerText変更通知プロパティ

        public string SpoilerText { get; set; }

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

        public RetweetInformation(TootNet.Objects.Status cOrigStatus)
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