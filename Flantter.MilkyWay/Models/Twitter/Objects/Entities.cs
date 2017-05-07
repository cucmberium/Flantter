using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Entities
    {
        public Entities(CoreTweet.Entities cEntities, CoreTweet.Entities cExtendedEntities)
        {
            var mediaList = MediaParser.Parse(cEntities, cExtendedEntities);

            HashTags = new List<HashtagEntity>();
            Media = new List<MediaEntity>();
            Urls = new List<UrlEntity>();
            UserMentions = new List<UserMentionEntity>();

            if (cEntities == null)
                return;

            foreach (var fMedia in mediaList)
                Media.Add(new MediaEntity(fMedia, this));

            if (cEntities.HashTags != null)
                foreach (var cHashTag in cEntities.HashTags)
                    HashTags.Add(new HashtagEntity(cHashTag));

            if (cEntities.Urls != null)
                foreach (var cUrl in cEntities.Urls)
                    Urls.Add(new UrlEntity(cUrl));

            if (cEntities.UserMentions != null)
                foreach (var cUserMention in cEntities.UserMentions)
                    UserMentions.Add(new UserMentionEntity(cUserMention));

            if (cEntities.Media != null)
                Urls.Add(new UrlEntity(cEntities.Media.First()));
            else if (cExtendedEntities?.Media != null)
                Urls.Add(new UrlEntity(cExtendedEntities.Media.First()));
        }

        public Entities(CoreTweet.Entities cEntities)
        {
            var mediaList = MediaParser.Parse(cEntities);

            HashTags = new List<HashtagEntity>();
            Media = new List<MediaEntity>();
            Urls = new List<UrlEntity>();
            UserMentions = new List<UserMentionEntity>();

            if (cEntities == null)
                return;

            foreach (var fMedia in mediaList)
                Media.Add(new MediaEntity(fMedia, this));

            if (cEntities.HashTags != null)
                foreach (var cHashTag in cEntities.HashTags)
                    HashTags.Add(new HashtagEntity(cHashTag));

            if (cEntities.Urls != null)
                foreach (var cUrl in cEntities.Urls)
                    Urls.Add(new UrlEntity(cUrl));

            if (cEntities.UserMentions != null)
                foreach (var cUserMention in cEntities.UserMentions)
                    UserMentions.Add(new UserMentionEntity(cUserMention));

            if (cEntities.Media != null)
                foreach (var cMedia in cEntities.Media)
                    Urls.Add(new UrlEntity(cMedia));
        }

        public Entities(IEnumerable<Mastonet.Entities.Attachment> cAttachments, IEnumerable<Mastonet.Entities.Mention> cMentions, IEnumerable<Mastonet.Entities.Tag> cTags,
            IEnumerable<string> cUrls, string cContent)
        {
            var mediaList = MediaParser.Parse(cAttachments, cContent);

            HashTags = new List<HashtagEntity>();
            Media = new List<MediaEntity>();
            Urls = new List<UrlEntity>();
            UserMentions = new List<UserMentionEntity>();

            foreach (var fMedia in mediaList)
                Media.Add(new MediaEntity(fMedia, this));

            if (cTags != null)
                foreach (var cHashTag in cTags)
                    HashTags.Add(new HashtagEntity(cHashTag));

            if (cUrls != null)
                foreach (var cUrl in cUrls)
                    Urls.Add(new UrlEntity(cUrl));

            if (cMentions != null)
                foreach (var cUserMention in cMentions)
                    UserMentions.Add(new UserMentionEntity(cUserMention));

            DeficientEntity = true;
        }

        public Entities()
        {
        }

        #region HashTags変更通知プロパティ

        public List<HashtagEntity> HashTags { get; set; }

        #endregion

        #region Media変更通知プロパティ

        public List<MediaEntity> Media { get; set; }

        #endregion

        #region Urls変更通知プロパティ

        public List<UrlEntity> Urls { get; set; }

        #endregion

        #region UserMentions変更通知プロパティ

        public List<UserMentionEntity> UserMentions { get; set; }

        #endregion

        #region DeficientEntity変更通知プロパティ

        public bool DeficientEntity { get; set; }

        #endregion
    }

    public class HashtagEntity
    {
        public HashtagEntity(CoreTweet.HashtagEntity cHashTag)
        {
            Tag = cHashTag.Text;
            Start = cHashTag.Indices.First();
            End = cHashTag.Indices.Last();
        }

        public HashtagEntity(Mastonet.Entities.Tag cHashTag)
        {
            Tag = cHashTag.Name;
            Start = 0;
            End = 0;
        }

        public HashtagEntity()
        {
        }

        #region Tag変更通知プロパティ

        public string Tag { get; set; }

        #endregion

        #region Start変更通知プロパティ

        public int Start { get; set; }

        #endregion

        #region End変更通知プロパティ

        public int End { get; set; }

        #endregion
    }

    public class MediaEntity
    {
        public MediaEntity(Media fMedia)
        {
            MediaUrl = fMedia.MediaUrl;
            MediaThumbnailUrl = fMedia.MediaThumbnailUrl;
            DisplayUrl = fMedia.DisplayUrl;
            ExpandedUrl = fMedia.ExpandedUrl;
            Type = fMedia.Type;
            VideoInfo = new VideoInfo(fMedia);
        }

        public MediaEntity(Media fMedia, Entities parentEntities)
        {
            MediaUrl = fMedia.MediaUrl;
            MediaThumbnailUrl = fMedia.MediaThumbnailUrl;
            DisplayUrl = fMedia.DisplayUrl;
            ExpandedUrl = fMedia.ExpandedUrl;
            Type = fMedia.Type;
            VideoInfo = new VideoInfo(fMedia);
            ParentEntities = parentEntities;
        }

        public MediaEntity()
        {
        }

        #region MediaUrl変更通知プロパティ

        public string MediaUrl { get; set; }

        #endregion

        #region MediaThumbnailUrl変更通知プロパティ

        public string MediaThumbnailUrl { get; set; }

        #endregion

        #region DisplayUrl変更通知プロパティ

        public string DisplayUrl { get; set; }

        #endregion

        #region ExpandedUrl変更通知プロパティ

        public string ExpandedUrl { get; set; }

        #endregion

        #region Type変更通知プロパティ

        public string Type { get; set; }

        #endregion

        #region VideoInfo変更通知プロパティ

        public VideoInfo VideoInfo { get; set; }

        #endregion

        #region ParentEntities変更通知プロパティ

        [JsonIgnore]
        public Entities ParentEntities { get; set; }

        #endregion
    }

    public class VideoInfo
    {
        public VideoInfo(Media fMedia)
        {
            if (fMedia.VideoInfo == null)
                return;

            VideoId = fMedia.VideoInfo.VideoId;
            VideoType = fMedia.VideoInfo.VideoType;
        }

        public VideoInfo()
        {
        }

        public string VideoType { get; set; }
        public string VideoId { get; set; }
        public string VideoContentType { get; set; }
    }

    public class UrlEntity
    {
        public UrlEntity(CoreTweet.UrlEntity cUrlEntity)
        {
            Url = cUrlEntity.Url;
            DisplayUrl = cUrlEntity.DisplayUrl;
            ExpandedUrl = cUrlEntity.ExpandedUrl;
            Start = cUrlEntity.Indices.First();
            End = cUrlEntity.Indices.Last();
        }

        public UrlEntity(string cUrl)
        {
            var displayUrl = cUrl.Replace("http://", "")
                .Replace("https://", "")
                .Replace("www.", "");
            if (displayUrl.Length >= 31)
                displayUrl = displayUrl.Substring(0, 30) + "...";

            Url = cUrl;
            DisplayUrl = displayUrl;
            ExpandedUrl = cUrl;
            Start = 0;
            End = 0;
        }

        public UrlEntity()
        {
        }

        #region Url変更通知プロパティ

        public string Url { get; set; }

        #endregion

        #region DisplayUrl変更通知プロパティ

        public string DisplayUrl { get; set; }

        #endregion

        #region ExpandedUrl変更通知プロパティ

        public string ExpandedUrl { get; set; }

        #endregion

        #region Start変更通知プロパティ

        public int Start { get; set; }

        #endregion

        #region End変更通知プロパティ

        public int End { get; set; }

        #endregion
    }

    public class UserMentionEntity
    {
        public UserMentionEntity(CoreTweet.UserMentionEntity cUrlEntity)
        {
            Id = cUrlEntity.Id ?? 0;
            Name = cUrlEntity.Name;
            ScreenName = cUrlEntity.ScreenName;
            Start = cUrlEntity.Indices.First();
            End = cUrlEntity.Indices.Last();
        }

        public UserMentionEntity(Mastonet.Entities.Mention cMention)
        {
            Id = cMention.Id;
            Name = cMention.UserName;
            ScreenName = cMention.AccountName;
            Start = 0;
            End = 0;
        }

        public UserMentionEntity()
        {
        }

        #region Id変更通知プロパティ

        public long Id { get; set; }

        #endregion

        #region Name変更通知プロパティ

        public string Name { get; set; }

        #endregion

        #region ScreenName変更通知プロパティ

        public string ScreenName { get; set; }

        #endregion

        #region Start変更通知プロパティ

        public int Start { get; set; }

        #endregion

        #region End変更通知プロパティ

        public int End { get; set; }

        #endregion
    }
}