using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class User : BindableBase
    {
        public User(CoreTweet.User cUser)
        {
            this.CreateAt = cUser.CreatedAt.DateTime;
            this.Description = cUser.Description;
            this.Entities = new UserEntities(cUser.Entities);
            this.FavouritesCount = cUser.FavouritesCount;
            this.FollowersCount = cUser.FollowersCount;
            this.FriendsCount = cUser.FriendsCount;
            this.Id = cUser.Id.HasValue ? cUser.Id.Value : 0;
            this.IsFollowRequestSent = cUser.IsFollowRequestSent.HasValue ? cUser.IsFollowRequestSent.Value : false;
            this.IsMuting = cUser.IsMuting.HasValue ? cUser.IsMuting.Value : false;
            this.IsProtected = cUser.IsProtected;
            this.IsVerified = cUser.IsVerified;
            this.Language = cUser.Language;
            this.ListedCount = cUser.ListedCount.HasValue ? cUser.ListedCount.Value : 0;
            this.Location = cUser.Location;
            this.Name = cUser.Name;
            this.ProfileBackgroundImageUrl = cUser.ProfileBackgroundImageUrl.AbsoluteUri;
            this.ProfileBannerUrl = cUser.ProfileBannerUrl != null ? cUser.ProfileBannerUrl.AbsoluteUri : null;
            this.ProfileImageUrl = cUser.ProfileImageUrl != null ? cUser.ProfileImageUrl.AbsoluteUri : null;
            this.ScreenName = cUser.ScreenName;
            this.StatusesCount = cUser.StatusesCount;
            this.TimeZone = cUser.TimeZone;
            this.Url = cUser.Url;
        }

        #region CreateAt変更通知プロパティ
        private DateTime _CreateAt;
        public DateTime CreateAt { get { return this._CreateAt; } set { this.SetProperty(ref this._CreateAt, value); } }
        #endregion

        #region Description変更通知プロパティ
        private string _Description;
        public string Description { get { return this._Description; } set { this.SetProperty(ref this._Description, value); } }
        #endregion

        #region Entities変更通知プロパティ
        private UserEntities _Entities;
        public UserEntities Entities { get { return this._Entities; } set { this.SetProperty(ref this._Entities, value); } }
        #endregion

        #region FavouritesCount変更通知プロパティ
        private int _FavouritesCount;
        public int FavouritesCount { get { return this._FavouritesCount; } set { this.SetProperty(ref this._FavouritesCount, value); } }
        #endregion

        #region FollowersCount変更通知プロパティ
        private int _FollowersCount;
        public int FollowersCount { get { return this._FollowersCount; } set { this.SetProperty(ref this._FollowersCount, value); } }
        #endregion

        #region FriendsCount変更通知プロパティ
        private int _FriendsCount;
        public int FriendsCount { get { return this._FriendsCount; } set { this.SetProperty(ref this._FriendsCount, value); } }
        #endregion

        #region Id変更通知プロパティ
        private long _Id;
        public long Id { get { return this._Id; } set { this.SetProperty(ref this._Id, value); } }
        #endregion

        #region IsFollowRequestSent変更通知プロパティ
        private bool _IsFollowRequestSent;
        public bool IsFollowRequestSent { get { return this._IsFollowRequestSent; } set { this.SetProperty(ref this._IsFollowRequestSent, value); } }
        #endregion

        #region IsMuting変更通知プロパティ
        private bool _IsMuting;
        public bool IsMuting { get { return this._IsMuting; } set { this.SetProperty(ref this._IsMuting, value); } }
        #endregion

        #region IsProtected変更通知プロパティ
        private bool _IsProtected;
        public bool IsProtected { get { return this._IsProtected; } set { this.SetProperty(ref this._IsProtected, value); } }
        #endregion

        #region IsVerified変更通知プロパティ
        private bool _IsVerified;
        public bool IsVerified { get { return this._IsVerified; } set { this.SetProperty(ref this._IsVerified, value); } }
        #endregion

        #region Language変更通知プロパティ
        private string _Language;
        public string Language { get { return this._Language; } set { this.SetProperty(ref this._Language, value); } }
        #endregion

        #region ListedCount変更通知プロパティ
        private int _ListedCount;
        public int ListedCount { get { return this._ListedCount; } set { this.SetProperty(ref this._ListedCount, value); } }
        #endregion

        #region Location変更通知プロパティ
        private string _Location;
        public string Location { get { return this._Location; } set { this.SetProperty(ref this._Location, value); } }
        #endregion

        #region Name変更通知プロパティ
        private string _Name;
        public string Name { get { return this._Name; } set { this.SetProperty(ref this._Name, value); } }
        #endregion

        #region ProfileBackgroundImageUrl変更通知プロパティ
        private string _ProfileBackgroundImageUrl;
        public string ProfileBackgroundImageUrl { get { return this._ProfileBackgroundImageUrl; } set { this.SetProperty(ref this._ProfileBackgroundImageUrl, value); } }
        #endregion

        #region ProfileBannerUrl変更通知プロパティ
        private string _ProfileBannerUrl;
        public string ProfileBannerUrl { get { return this._ProfileBannerUrl; } set { this.SetProperty(ref this._ProfileBannerUrl, value); } }
        #endregion

        #region ProfileImageUrl変更通知プロパティ
        private string _ProfileImageUrl;
        public string ProfileImageUrl { get { return this._ProfileImageUrl; } set { this.SetProperty(ref this._ProfileImageUrl, value); } }
        #endregion

        #region ScreenName変更通知プロパティ
        private string _ScreenName;
        public string ScreenName { get { return this._ScreenName; } set { this.SetProperty(ref this._ScreenName, value); } }
        #endregion

        #region StatusesCount変更通知プロパティ
        private int _StatusesCount;
        public int StatusesCount { get { return this._StatusesCount; } set { this.SetProperty(ref this._StatusesCount, value); } }
        #endregion

        #region TimeZone変更通知プロパティ
        private string _TimeZone;
        public string TimeZone { get { return this._TimeZone; } set { this.SetProperty(ref this._TimeZone, value); } }
        #endregion

        #region Url変更通知プロパティ
        private string _Url;
        public string Url { get { return this._Url; } set { this.SetProperty(ref this._Url, value); } }
        #endregion
    }

    public class UserEntities : BindableBase
    {
        public UserEntities(CoreTweet.UserEntities cUserEntities)
        {
            if (cUserEntities == null)
                return;

            this.Description = new Entities(cUserEntities.Description);
            this.Url = new Entities(cUserEntities.Url);
        }

        #region Description変更通知プロパティ
        private Entities _Description;
        public Entities Description { get { return this._Description; } set { this.SetProperty(ref this._Description, value); } }
        #endregion

        #region Url変更通知プロパティ
        private Entities _Url;
        public Entities Url { get { return this._Url; } set { this.SetProperty(ref this._Url, value); } }
        #endregion
    }
}
