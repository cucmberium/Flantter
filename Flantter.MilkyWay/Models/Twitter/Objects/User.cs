using System;
using System.Text.RegularExpressions;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class User
    {
        private static readonly Regex ContentRegex =
            new Regex(@"<(""[^""]*""|'[^']*'|[^'"">])*>", RegexOptions.Compiled);

        private static readonly Regex LinkRegex =
            new Regex(@"\s*<a href=\""(.*?)\"".*?>(.*?)</a>\s*", RegexOptions.Compiled);

        public User(CoreTweet.User cUser)
        {
            CreateAt = cUser.CreatedAt.DateTime;
            Description = cUser.Description;
            Entities = new UserEntities(cUser.Entities);
            FavouritesCount = cUser.FavouritesCount;
            FollowersCount = cUser.FollowersCount;
            FriendsCount = cUser.FriendsCount;
            Id = cUser.Id ?? 0;
            IsFollowRequestSent = cUser.IsFollowRequestSent ?? false;
            IsMuting = cUser.IsMuting ?? false;
            IsProtected = cUser.IsProtected;
            IsVerified = cUser.IsVerified;
            Language = cUser.Language;
            ListedCount = cUser.ListedCount ?? 0;
            Location = cUser.Location;
            Name = cUser.Name;
            ProfileBackgroundColor = cUser.ProfileBackgroundColor;
            ProfileBackgroundImageUrl = cUser.ProfileBackgroundImageUrl;
            ProfileBannerUrl = cUser.ProfileBannerUrl;
            ProfileImageUrl = cUser.ProfileImageUrl;
            ScreenName = cUser.ScreenName;
            StatusesCount = cUser.StatusesCount;
            TimeZone = cUser.TimeZone;
            Url = cUser.Url;
        }

        public User(Mastonet.Entities.Account cUser)
        {
            CreateAt = cUser.CreatedAt;
            Description = ContentRegex.Replace(LinkRegex.Replace(cUser.Note.Replace("<br>", "\n"), x => " " + x.Groups[1].Value + " "), "").Trim();
            Entities = new UserEntities();
            FavouritesCount = 0;
            FollowersCount = cUser.FollowersCount;
            FriendsCount = cUser.FollowingCount;
            Id = cUser.Id;
            IsFollowRequestSent = false;
            IsMuting = false;
            IsProtected = cUser.Locked;
            IsVerified = false;
            Language = "en";
            ListedCount = 0;
            Location = "";
            var name = string.IsNullOrWhiteSpace(cUser.DisplayName) ? cUser.UserName : cUser.DisplayName;
            name = EmojiPatterns.LightValidEmoji.Replace(name,
                x => EmojiPatterns.EmojiDictionary.TryGetValue(x.Groups[2].Value, out string val) ? val : x.Value);
            Name = name;
            ProfileBackgroundColor = "C0DEED";
            ProfileBackgroundImageUrl = "http://localhost/";
            ProfileBannerUrl = cUser.HeaderUrl.StartsWith("http") ? cUser.HeaderUrl : "http://localhost/";
            ProfileImageUrl = cUser.AvatarUrl.StartsWith("http") ? cUser.AvatarUrl : "http://localhost/";
            ScreenName = cUser.AccountName;
            StatusesCount = cUser.StatusesCount;
            TimeZone = null;
            Url = cUser.ProfileUrl;
        }

        public User()
        {
        }

        #region CreateAt変更通知プロパティ

        public DateTime CreateAt { get; set; }

        #endregion

        #region Description変更通知プロパティ

        public string Description { get; set; }

        #endregion

        #region Entities変更通知プロパティ

        public UserEntities Entities { get; set; }

        #endregion

        #region FavouritesCount変更通知プロパティ

        public int FavouritesCount { get; set; }

        #endregion

        #region FollowersCount変更通知プロパティ

        public int FollowersCount { get; set; }

        #endregion

        #region FriendsCount変更通知プロパティ

        public int FriendsCount { get; set; }

        #endregion

        #region Id変更通知プロパティ

        public long Id { get; set; }

        #endregion

        #region IsFollowRequestSent変更通知プロパティ

        public bool IsFollowRequestSent { get; set; }

        #endregion

        #region IsMuting変更通知プロパティ

        public bool IsMuting { get; set; }

        #endregion

        #region IsProtected変更通知プロパティ

        public bool IsProtected { get; set; }

        #endregion

        #region IsVerified変更通知プロパティ

        public bool IsVerified { get; set; }

        #endregion

        #region Language変更通知プロパティ

        public string Language { get; set; }

        #endregion

        #region ListedCount変更通知プロパティ

        public int ListedCount { get; set; }

        #endregion

        #region Location変更通知プロパティ

        public string Location { get; set; }

        #endregion

        #region Name変更通知プロパティ

        public string Name { get; set; }

        #endregion

        #region ProfileBackgroundImageUrl変更通知プロパティ

        public string ProfileBackgroundImageUrl { get; set; }

        #endregion

        #region ProfileBannerUrl変更通知プロパティ

        public string ProfileBannerUrl { get; set; }

        #endregion

        #region ProfileImageUrl変更通知プロパティ

        public string ProfileImageUrl { get; set; }

        #endregion

        #region ProfileBackgroundColor変更通知プロパティ

        public string ProfileBackgroundColor { get; set; }

        #endregion

        #region ScreenName変更通知プロパティ

        public string ScreenName { get; set; }

        #endregion

        #region StatusesCount変更通知プロパティ

        public int StatusesCount { get; set; }

        #endregion

        #region TimeZone変更通知プロパティ

        public string TimeZone { get; set; }

        #endregion

        #region Url変更通知プロパティ

        public string Url { get; set; }

        #endregion
    }

    public class UserEntities
    {
        public UserEntities(CoreTweet.UserEntities cUserEntities)
        {
            if (cUserEntities == null)
                return;

            Description = new Entities(cUserEntities.Description);
            Url = new Entities(cUserEntities.Url);
        }

        public UserEntities()
        {
        }

        #region Description変更通知プロパティ

        public Entities Description { get; set; }

        #endregion

        #region Url変更通知プロパティ

        public Entities Url { get; set; }

        #endregion
    }
}