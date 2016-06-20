using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Entities
    {
        public Entities(CoreTweet.Entities cEntities, CoreTweet.Entities cExtendedEntities)
        {
            var mediaList = MediaParser.Parse(cEntities, cExtendedEntities);

            this.HashTags = new List<HashtagEntity>();
            this.Media = new List<MediaEntity>();
            this.Urls = new List<UrlEntity>();
            this.UserMentions = new List<UserMentionEntity>();

            if (cEntities == null)
                return;

            foreach (var fMedia in mediaList)
                this.Media.Add(new MediaEntity(fMedia, this));

            if (cEntities.HashTags != null)
            {
                foreach (var cHashTag in cEntities.HashTags)
                    this.HashTags.Add(new HashtagEntity(cHashTag));
            }

            if (cEntities.Urls != null)
            {
                foreach (var cUrl in cEntities.Urls)
                    this.Urls.Add(new UrlEntity(cUrl));
            }

            if (cEntities.UserMentions != null)
            {
                foreach (var cUserMention in cEntities.UserMentions)
                    this.UserMentions.Add(new UserMentionEntity(cUserMention));
            }

            if (cEntities != null && cEntities.Media != null)
            {
                this.Urls.Add(new UrlEntity(cEntities.Media.First()));
            }
            else if (cExtendedEntities != null && cExtendedEntities.Media != null)
            {
                this.Urls.Add(new UrlEntity(cExtendedEntities.Media.First()));
            }
            
            
        }

        public Entities(CoreTweet.Entities cEntities)
        {
            var mediaList = MediaParser.Parse(cEntities);

            this.HashTags = new List<HashtagEntity>();
            this.Media = new List<MediaEntity>();
            this.Urls = new List<UrlEntity>();
            this.UserMentions = new List<UserMentionEntity>();

            if (cEntities == null)
                return;

            if (cEntities.HashTags != null)
            {
                foreach (var cHashTag in cEntities.HashTags)
                    this.HashTags.Add(new HashtagEntity(cHashTag));
            }

            if (cEntities.Urls != null)
            {
                foreach (var cUrl in cEntities.Urls)
                    this.Urls.Add(new UrlEntity(cUrl));
            }

            if (cEntities.UserMentions != null)
            {
                foreach (var cUserMention in cEntities.UserMentions)
                    this.UserMentions.Add(new UserMentionEntity(cUserMention));
            }

            if (cEntities != null && cEntities.Media != null)
            {
                foreach (var cMedia in cEntities.Media)
                    this.Urls.Add(new UrlEntity(cMedia));
            }
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
    }

    public class HashtagEntity
    {
        public HashtagEntity(CoreTweet.HashtagEntity cHashTag)
        {
            this.Tag = cHashTag.Text;
            this.Start = cHashTag.Indices.First();
            this.End = cHashTag.Indices.Last();   
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
            this.MediaUrl = fMedia.MediaUrl;
            this.MediaThumbnailUrl = fMedia.MediaThumbnailUrl;
            this.DisplayUrl = fMedia.DisplayUrl;
            this.ExpandedUrl = fMedia.ExpandedUrl;
            this.Type = fMedia.Type;
            this.VideoInfo = new VideoInfo(fMedia);
        }

        public MediaEntity(Media fMedia, Entities parentEntities)
        {
            this.MediaUrl = fMedia.MediaUrl;
            this.MediaThumbnailUrl = fMedia.MediaThumbnailUrl;
            this.DisplayUrl = fMedia.DisplayUrl;
            this.ExpandedUrl = fMedia.ExpandedUrl;
            this.Type = fMedia.Type;
            this.VideoInfo = new VideoInfo(fMedia);
            this.ParentEntities = parentEntities;
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
        public Entities ParentEntities { get; set; }
        #endregion
    }

    public class VideoInfo
    {
        public VideoInfo(Media fMedia)
        {
            if (fMedia.VideoInfo == null)
                return;

            this.VideoId = fMedia.VideoInfo.VideoId;
            this.VideoType = fMedia.VideoInfo.VideoType;
        }

        public string VideoType { get; set; }
        public string VideoId { get; set; }
        public string VideoContentType { get; set; }
    }

    public class UrlEntity
    {
        public UrlEntity(CoreTweet.UrlEntity cUrlEntity)
        {
            this.Url = cUrlEntity.Url;
            this.DisplayUrl = cUrlEntity.DisplayUrl;
            this.ExpandedUrl = cUrlEntity.ExpandedUrl;
            this.Start = cUrlEntity.Indices.First();
            this.End = cUrlEntity.Indices.Last();
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
            this.Id = cUrlEntity.Id.HasValue ? cUrlEntity.Id.Value : 0;
            this.Name = cUrlEntity.Name;
            this.ScreenName = cUrlEntity.ScreenName;
            this.Start = cUrlEntity.Indices.First();
            this.End = cUrlEntity.Indices.Last();
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
