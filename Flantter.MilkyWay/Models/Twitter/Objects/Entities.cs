using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Entities : BindableBase
    {
        public Entities(CoreTweet.Entities cEntities, CoreTweet.Entities cExtendedEntities)
        {
            var mediaList = MediaParser.Parse(cEntities, cExtendedEntities);

            this.HashTags = new ObservableCollection<HashtagEntity>();
            this.Media = new ObservableCollection<MediaEntity>();
            this.Urls = new ObservableCollection<UrlEntity>();
            this.UserMentions = new ObservableCollection<UserMentionEntity>();

            foreach (var cHashTag in cEntities.HashTags)
                this.HashTags.Add(new HashtagEntity(cHashTag));

            foreach (var fMedia in mediaList)
                this.Media.Add(new MediaEntity(fMedia));

            foreach (var cUrl in cEntities.Urls)
                this.Urls.Add(new UrlEntity(cUrl));

            foreach (var cUserMention in cEntities.UserMentions)
                this.UserMentions.Add(new UserMentionEntity(cUserMention));
        }

        public Entities(CoreTweet.Entities cEntities)
        {
            this.HashTags = new ObservableCollection<HashtagEntity>();
            this.Media = new ObservableCollection<MediaEntity>();
            this.Urls = new ObservableCollection<UrlEntity>();
            this.UserMentions = new ObservableCollection<UserMentionEntity>();

            foreach (var cHashTag in cEntities.HashTags)
                this.HashTags.Add(new HashtagEntity(cHashTag));

            foreach (var cUrl in cEntities.Urls)
                this.Urls.Add(new UrlEntity(cUrl));

            foreach (var cUserMention in cEntities.UserMentions)
                this.UserMentions.Add(new UserMentionEntity(cUserMention));
        }

        #region HashTags変更通知プロパティ
        private ObservableCollection<HashtagEntity> _HashTags;
        public ObservableCollection<HashtagEntity> HashTags { get { return this._HashTags; } set { this.SetProperty(ref this._HashTags, value); } }
        #endregion

        #region Media変更通知プロパティ
        private ObservableCollection<MediaEntity> _Media;
        public ObservableCollection<MediaEntity> Media { get { return this._Media; } set { this.SetProperty(ref this._Media, value); } }
        #endregion

        #region Urls変更通知プロパティ
        private ObservableCollection<UrlEntity> _Urls;
        public ObservableCollection<UrlEntity> Urls { get { return this._Urls; } set { this.SetProperty(ref this._Urls, value); } }
        #endregion

        #region UserMentions変更通知プロパティ
        private ObservableCollection<UserMentionEntity> _UserMentions;
        public ObservableCollection<UserMentionEntity> UserMentions { get { return this._UserMentions; } set { this.SetProperty(ref this._UserMentions, value); } }
        #endregion
    }

    public class HashtagEntity : BindableBase
    {
        public HashtagEntity(CoreTweet.HashtagEntity cHashTag)
        {
            this.Tag = cHashTag.Text;
            this.Start = cHashTag.Indices.First();
            this.End = cHashTag.Indices.Last();   
        }

        #region Tag変更通知プロパティ
        private string _Tag;
        public string Tag { get { return this._Tag; } set { this.SetProperty(ref this._Tag, value); } }
        #endregion

        #region Start変更通知プロパティ
        private int _Start;
        public int Start { get { return this._Start; } set { this.SetProperty(ref this._Start, value); } }
        #endregion

        #region End変更通知プロパティ
        private int _End;
        public int End { get { return this._End; } set { this.SetProperty(ref this._End, value); } }
        #endregion
    }

    public class MediaEntity : BindableBase
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

        #region MediaUrl変更通知プロパティ
        private string _MediaUrl;
        public string MediaUrl { get { return this._MediaUrl; } set { this.SetProperty(ref this._MediaUrl, value); } }
        #endregion

        #region MediaThumbnailUrl変更通知プロパティ
        private string _MediaThumbnailUrl;
        public string MediaThumbnailUrl { get { return this._MediaThumbnailUrl; } set { this.SetProperty(ref this._MediaThumbnailUrl, value); } }
        #endregion

        #region DisplayUrl変更通知プロパティ
        private string _DisplayUrl;
        public string DisplayUrl { get { return this._DisplayUrl; } set { this.SetProperty(ref this._DisplayUrl, value); } }
        #endregion

        #region ExpandedUrl変更通知プロパティ
        private string _ExpandedUrl;
        public string ExpandedUrl { get { return this._ExpandedUrl; } set { this.SetProperty(ref this._ExpandedUrl, value); } }
        #endregion

        #region Type変更通知プロパティ
        private string _Type;
        public string Type { get { return this._Type; } set { this.SetProperty(ref this._Type, value); } }
        #endregion

        #region VideoInfo変更通知プロパティ
        private VideoInfo _VideoInfo;
        public VideoInfo VideoInfo { get { return this._VideoInfo; } set { this.SetProperty(ref this._VideoInfo, value); } }
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
    }

    public class UrlEntity : BindableBase
    {
        public UrlEntity(CoreTweet.UrlEntity cUrlEntity)
        {
            this.Url = cUrlEntity.Url.AbsoluteUri;
            this.DisplayUrl = cUrlEntity.DisplayUrl;
            this.ExpandedUrl = cUrlEntity.ExpandedUrl;
            this.Start = cUrlEntity.Indices.First();
            this.End = cUrlEntity.Indices.Last();
        }

        #region Url変更通知プロパティ
        private string _Url;
        public string Url { get { return this._Url; } set { this.SetProperty(ref this._Url, value); } }
        #endregion

        #region DisplayUrl変更通知プロパティ
        private string _DisplayUrl;
        public string DisplayUrl
        {
            get { return this._DisplayUrl; }
            set { this.SetProperty(ref this._DisplayUrl, value); }
        }
        #endregion

        #region ExpandedUrl変更通知プロパティ
        private string _ExpandedUrl;
        public string ExpandedUrl
        {
            get { return this._ExpandedUrl; }
            set { this.SetProperty(ref this._ExpandedUrl, value); }
        }
        #endregion

        #region Start変更通知プロパティ
        private int _Start;
        public int Start
        {
            get { return this._Start; }
            set { this.SetProperty(ref this._Start, value); }
        }
        #endregion

        #region End変更通知プロパティ
        private int _End;
        public int End
        {
            get { return this._End; }
            set { this.SetProperty(ref this._End, value); }
        }
        #endregion
    }

    public class UserMentionEntity : BindableBase
    {
        public UserMentionEntity(CoreTweet.UserMentionEntity cUrlEntity)
        {
            this.Id = cUrlEntity.Id;
            this.Name = cUrlEntity.Name;
            this.ScreenName = cUrlEntity.ScreenName;
            this.Start = cUrlEntity.Indices.First();
            this.End = cUrlEntity.Indices.Last();
        }

        #region Id変更通知プロパティ
        private long _Id;
        public long Id
        {
            get { return this._Id; }
            set { this.SetProperty(ref this._Id, value); }
        }
        #endregion

        #region Name変更通知プロパティ
        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set { this.SetProperty(ref this._Name, value); }
        }
        #endregion

        #region ScreenName変更通知プロパティ
        private string _ScreenName;
        public string ScreenName
        {
            get { return this._ScreenName; }
            set { this.SetProperty(ref this._ScreenName, value); }
        }
        #endregion

        #region Start変更通知プロパティ
        private int _Start;
        public int Start
        {
            get { return this._Start; }
            set { this.SetProperty(ref this._Start, value); }
        }
        #endregion

        #region End変更通知プロパティ
        private int _End;
        public int End
        {
            get { return this._End; }
            set { this.SetProperty(ref this._End, value); }
        }
        #endregion
    }
}
