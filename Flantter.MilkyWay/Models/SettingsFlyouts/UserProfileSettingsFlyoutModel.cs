using CoreTweet;
using CoreTweet.Core;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserProfileSettingsFlyoutModel : BindableBase
    {
        public UserProfileSettingsFlyoutModel()
        {
            this.Statuses = new ObservableCollection<Twitter.Objects.Status>();
            this.Favorites = new ObservableCollection<Twitter.Objects.Status>();
            this.Followers = new ObservableCollection<Twitter.Objects.User>();
            this.Following = new ObservableCollection<Twitter.Objects.User>();
        }

        #region Tokens変更通知プロパティ
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
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

        public bool OpenFollowing { get; set; }
        public bool OpenFollowers { get; set; }
        public bool OpenFavorite { get; set; }

        public ObservableCollection<Twitter.Objects.Status> Statuses { get; set; }
        public ObservableCollection<Twitter.Objects.Status> Favorites { get; set; }
        public ObservableCollection<Twitter.Objects.User> Followers { get; set; }
        public ObservableCollection<Twitter.Objects.User> Following { get; set; }


        #region DescriptionEntities変更通知プロパティ
        private Twitter.Objects.Entities _DescriptionEntities;
        public Twitter.Objects.Entities DescriptionEntities
        {
            get { return this._DescriptionEntities; }
            set { this.SetProperty(ref this._DescriptionEntities, value); }
        }
        #endregion

        #region UrlEntities変更通知プロパティ
        private Twitter.Objects.Entities _UrlEntities;
        public Twitter.Objects.Entities UrlEntities
        {
            get { return this._UrlEntities; }
            set { this.SetProperty(ref this._UrlEntities, value); }
        }
        #endregion

        #region Description変更通知プロパティ
        private string _Description;
        public string Description
        {
            get { return this._Description; }
            set { this.SetProperty(ref this._Description, value); }
        }
        #endregion

        #region FavouritesCount変更通知プロパティ
        private int _FavouritesCount;
        public int FavouritesCount
        {
            get { return this._FavouritesCount; }
            set { this.SetProperty(ref this._FavouritesCount, value); }
        }
        #endregion

        #region FollowersCount変更通知プロパティ
        private int _FollowersCount;
        public int FollowersCount
        {
            get { return this._FollowersCount; }
            set { this.SetProperty(ref this._FollowersCount, value); }
        }
        #endregion

        #region FriendsCount変更通知プロパティ
        private int _FriendsCount;
        public int FriendsCount
        {
            get { return this._FriendsCount; }
            set { this.SetProperty(ref this._FriendsCount, value); }
        }
        #endregion

        #region ListedCount変更通知プロパティ
        private int _ListedCount;
        public int ListedCount
        {
            get { return this._ListedCount; }
            set { this.SetProperty(ref this._ListedCount, value); }
        }
        #endregion

        #region IsMuting変更通知プロパティ
        private bool _IsMuting;
        public bool IsMuting
        {
            get { return this._IsMuting; }
            set { this.SetProperty(ref this._IsMuting, value); }
        }
        #endregion

        #region IsProtected変更通知プロパティ
        private bool _IsProtected;
        public bool IsProtected
        {
            get { return this._IsProtected; }
            set { this.SetProperty(ref this._IsProtected, value); }
        }
        #endregion

        #region IsVerified変更通知プロパティ
        private bool _IsVerified;
        public bool IsVerified
        {
            get { return this._IsVerified; }
            set { this.SetProperty(ref this._IsVerified, value); }
        }
        #endregion

        #region Location変更通知プロパティ
        private string _Location;
        public string Location
        {
            get { return this._Location; }
            set { this.SetProperty(ref this._Location, value); }
        }
        #endregion

        #region ProfileBackgroundColor変更通知プロパティ
        private string _ProfileBackgroundColor;
        public string ProfileBackgroundColor
        {
            get { return this._ProfileBackgroundColor; }
            set { this.SetProperty(ref this._ProfileBackgroundColor, value); }
        }
        #endregion

        #region ProfileBannerUrl変更通知プロパティ
        private string _ProfileBannerUrl;
        public string ProfileBannerUrl
        {
            get { return this._ProfileBannerUrl; }
            set { this.SetProperty(ref this._ProfileBannerUrl, value); }
        }
        #endregion

        #region ProfileImageUrl変更通知プロパティ
        private string _ProfileImageUrl;
        public string ProfileImageUrl
        {
            get { return this._ProfileImageUrl; }
            set { this.SetProperty(ref this._ProfileImageUrl, value); }
        }
        #endregion

        #region StatusesCount変更通知プロパティ
        private int _StatusesCount;
        public int StatusesCount
        {
            get { return this._StatusesCount; }
            set { this.SetProperty(ref this._StatusesCount, value); }
        }
        #endregion

        #region Url変更通知プロパティ
        private string _Url;
        public string Url
        {
            get { return this._Url; }
            set { this.SetProperty(ref this._Url, value); }
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

        #region IsFollowRequestSent変更通知プロパティ
        private bool _IsFollowRequestSent;
        public bool IsFollowRequestSent
        {
            get { return this._IsFollowRequestSent; }
            set { this.SetProperty(ref this._IsFollowRequestSent, value); }
        }
        #endregion

        #region IsFollowing変更通知プロパティ
        private bool _IsFollowing;
        public bool IsFollowing
        {
            get { return this._IsFollowing; }
            set { this.SetProperty(ref this._IsFollowing, value); }
        }
        #endregion

        #region IsFollowedBy変更通知プロパティ
        private bool _IsFollowedBy;
        public bool IsFollowedBy
        {
            get { return this._IsFollowedBy; }
            set { this.SetProperty(ref this._IsFollowedBy, value); }
        }
        #endregion

        #region IsBlocking変更通知プロパティ
        private bool _IsBlocking;
        public bool IsBlocking
        {
            get { return this._IsBlocking; }
            set { this.SetProperty(ref this._IsBlocking, value); }
        }
        #endregion


        #region UpdatingUserInformation変更通知プロパティ
        private bool _UpdatingUserInformation;
        public bool UpdatingUserInformation
        {
            get { return this._UpdatingUserInformation; }
            set { this.SetProperty(ref this._UpdatingUserInformation, value); }
        }
        #endregion

        #region UpdatingRelationShip変更通知プロパティ
        private bool _UpdatingRelationShip;
        public bool UpdatingRelationShip
        {
            get { return this._UpdatingRelationShip; }
            set { this.SetProperty(ref this._UpdatingRelationShip, value); }
        }
        #endregion

        #region UpdatingStatuses変更通知プロパティ
        private bool _UpdatingStatuses;
        public bool UpdatingStatuses
        {
            get { return this._UpdatingStatuses; }
            set { this.SetProperty(ref this._UpdatingStatuses, value); }
        }
        #endregion

        #region UpdatingFavorites変更通知プロパティ
        private bool _UpdatingFavorites;
        public bool UpdatingFavorites
        {
            get { return this._UpdatingFavorites; }
            set { this.SetProperty(ref this._UpdatingFavorites, value); }
        }
        #endregion

        #region UpdatingFollowers変更通知プロパティ
        private bool _UpdatingFollowers;
        public bool UpdatingFollowers
        {
            get { return this._UpdatingFollowers; }
            set { this.SetProperty(ref this._UpdatingFollowers, value); }
        }
        #endregion

        #region UpdatingFollowing変更通知プロパティ
        private bool _UpdatingFollowing;
        public bool UpdatingFollowing
        {
            get { return this._UpdatingFollowing; }
            set { this.SetProperty(ref this._UpdatingFollowing, value); }
        }
        #endregion

        public async Task UpdateUserInfomation()
        {
            if (this.UpdatingUserInformation)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingUserInformation = true;

            UserResponse user;
            try
            {
                user = await Tokens.Users.ShowAsync(screen_name => this._ScreenName, include_entities => true);
            }
            catch
            {
                this.UpdatingUserInformation = false;
                return;
            }

            var userObj = new Twitter.Objects.User(user);

            this.UrlEntities = userObj.Entities.Url;
            this.DescriptionEntities = userObj.Entities.Description;
            this.Description = userObj.Description;
            this.FavouritesCount = userObj.FavouritesCount;
            this.FollowersCount = userObj.FollowersCount;
            this.FriendsCount = userObj.FriendsCount;
            this.ListedCount = userObj.ListedCount;
            this.IsMuting = userObj.IsMuting;
            this.IsProtected = userObj.IsProtected;
            this.IsVerified = userObj.IsVerified;
            this.Location = userObj.Location;
            this.ProfileBackgroundColor = userObj.ProfileBackgroundColor;
            this.ProfileBannerUrl = userObj.ProfileBannerUrl;
            this.ProfileImageUrl = userObj.ProfileImageUrl;
            this.StatusesCount = userObj.StatusesCount;
            this.Url = userObj.Url;
            this.Name = userObj.Name;
            this.IsFollowRequestSent = userObj.IsFollowRequestSent;
            
            this.UpdatingUserInformation = false;
        }

        public async Task UpdateRelationShip()
        {
            if (this.UpdatingRelationShip)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingRelationShip = true;

            Relationship relationShip;
            try
            {
                relationShip = await Tokens.Friendships.ShowAsync(source_screen_name => Tokens.ScreenName, target_screen_name => this._ScreenName);
            }
            catch
            {
                this.UpdatingRelationShip = false;
                return;
            }

            this.IsFollowing = relationShip.Source.IsFollowing;
            this.IsFollowedBy = relationShip.Source.IsFollowedBy;
            this.IsBlocking = relationShip.Source.IsBlocking.HasValue ? relationShip.Source.IsBlocking.Value : false;
            this.IsMuting = relationShip.Source.IsMuting.HasValue ? relationShip.Source.IsMuting.Value : false;
            this.IsFollowRequestSent = relationShip.Source.IsFollowingRequested.HasValue ? relationShip.Source.IsFollowingRequested.Value : false;

            this.UpdatingRelationShip = false;
        }

        public async Task UpdateStatuses(long maxid = 0)
        {
            if (this.UpdatingStatuses)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingStatuses = true;

            ListedResponse<CoreTweet.Status> userTweets;
            try
            {
                if (maxid == 0)
                    userTweets = await Tokens.Statuses.UserTimelineAsync(screen_name => this._ScreenName, count => 20);
                else
                    userTweets = await Tokens.Statuses.UserTimelineAsync(screen_name => this._ScreenName, count => 20, max_id => maxid);
            }
            catch
            {
                if (maxid == 0)
                    this.Statuses.Clear();

                this.UpdatingStatuses = false;
                return;
            }

            if (maxid == 0)
                this.Statuses.Clear();

            foreach (var item in userTweets)
            {
                var status = new Twitter.Objects.Status(item);

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this.Statuses.IndexOf(this.Statuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this.Statuses.IndexOf(this.Statuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this.Statuses.Add(status);
                    else
                        this.Statuses.Insert(index, status);
                }
            }

            // Todo : 受信したツイートをデータベースに登録

            this.UpdatingStatuses = false;
        }

        public async Task UpdateFavorites(long maxid = 0)
        {
            if (this.UpdatingFavorites)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingFavorites = true;

            ListedResponse<CoreTweet.Status> favorites;
            try
            {
                if (maxid == 0)
                    favorites = await Tokens.Favorites.ListAsync(screen_name => this._ScreenName, count => 20);
                else
                    favorites = await Tokens.Favorites.ListAsync(screen_name => this._ScreenName, count => 20, max_id => maxid);
            }
            catch
            {
                if (maxid == 0)
                    this.Favorites.Clear();

                this.UpdatingFavorites = false;
                return;
            }

            if (maxid == 0)
                this.Favorites.Clear();

            foreach (var item in favorites)
            {
                var status = new Twitter.Objects.Status(item);

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this.Favorites.IndexOf(this.Favorites.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this.Favorites.IndexOf(this.Favorites.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this.Favorites.Add(status);
                    else
                        this.Favorites.Insert(index, status);
                }
            }

            // Todo : 受信したツイートをデータベースに登録

            this.UpdatingFavorites = false;
        }

        private long followersCursor = 0;
        public async Task UpdateFollowers(bool useCursor = false)
        {
            if (this.UpdatingFollowers)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            if (useCursor && followersCursor == 0)
                return;

            this.UpdatingFollowers = true;

            Cursored<CoreTweet.User> follower;
            try
            {
                if (useCursor && followersCursor != 0)
                    follower = await Tokens.Followers.ListAsync(screen_name => this._ScreenName, count => 20, cursor => followersCursor);
                else
                    follower = await Tokens.Followers.ListAsync(screen_name => this._ScreenName, count => 20);
            }
            catch
            {
                if (!useCursor || followersCursor == 0)
                    this.Followers.Clear();

                this.UpdatingFollowers = false;
                return;
            }

            if (!useCursor || followersCursor == 0)
                this.Followers.Clear();

            foreach (var item in follower)
            {
                var user = new Twitter.Objects.User(item);
                this.Followers.Add(user);
            }

            followersCursor = follower.NextCursor;
            this.UpdatingFollowers = false;
        }

        private long followingCursor = 0;
        public async Task UpdateFollowing(bool useCursor = false)
        {
            if (this.UpdatingFollowing)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            if (useCursor && followingCursor == 0)
                return;

            this.UpdatingFollowing = true;

            Cursored<CoreTweet.User> following;
            try
            {
                if (useCursor && followingCursor != 0)
                    following = await Tokens.Friends.ListAsync(screen_name => this._ScreenName, count => 20, cursor => followingCursor);
                else
                    following = await Tokens.Friends.ListAsync(screen_name => this._ScreenName, count => 20);
            }
            catch
            {
                if (!useCursor || followingCursor == 0)
                    this.Following.Clear();

                this.UpdatingFollowing = false;
                return;
            }

            if (!useCursor || followingCursor == 0)
                this.Following.Clear();

            foreach (var item in following)
            {
                var user = new Twitter.Objects.User(item);
                this.Following.Add(user);
            }

            followingCursor = following.NextCursor;
            this.UpdatingFollowing = false;
        }

        public async Task Follow()
        {
            if (this.IsBlocking)
            {
                await this.DestroyBlock();
            }
            else if (this.IsFollowing)
            {
                await this.DestroyFollow();
            }
            else if (this.IsFollowRequestSent)
            {
                await this.DestroyFollow();
            }
            else
            {
                await this.CreateFollow();
            }
        }

        public async Task CreateFollow()
        {
            UserResponse user = null;
            try
            {
                user = await this.Tokens.Friendships.CreateAsync(screen_name => this._ScreenName);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            if (user.IsProtected)
                this.IsFollowRequestSent = true;
            else
                this.IsFollowing = true;
        }

        public async Task DestroyFollow()
        {
            UserResponse user = null;
            try
            {
                user = await this.Tokens.Friendships.DestroyAsync(screen_name => this._ScreenName);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }
            
            this.IsFollowing = false;
        }

        public async Task CreateBlock()
        {
            try
            {
                await this.Tokens.Blocks.CreateAsync(screen_name => this._ScreenName);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            this.IsFollowing = false;
            this.IsBlocking = true;
        }

        public async Task DestroyBlock()
        {
            try
            {
                await this.Tokens.Blocks.DestroyAsync(screen_name => this._ScreenName);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            this.IsBlocking = false;
        }

        public async Task CreateMute()
        {
            try
            {
                await this.Tokens.Mutes.Users.CreateAsync(screen_name => this._ScreenName);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }
            
            this.IsMuting = true;
        }

        public async Task DestroyMute()
        {
            try
            {
                await this.Tokens.Mutes.Users.DestroyAsync(screen_name => this._ScreenName);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            this.IsMuting = false;
        }
    }
}
