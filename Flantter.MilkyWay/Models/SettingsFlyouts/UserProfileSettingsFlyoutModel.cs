using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
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
        private Tokens _Tokens;
        public Tokens Tokens
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

        #region UserId変更通知プロパティ
        private long _UserId;
        public long UserId
        {
            get { return this._UserId; }
            set { this.SetProperty(ref this._UserId, value); }
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
            
            try
            {
                var user = await Tokens.Users.ShowAsync(screen_name => this._ScreenName, include_entities => true);

                this.UrlEntities = user.Entities.Url;
                this.DescriptionEntities = user.Entities.Description;
                this.Description = user.Description;
                this.FavouritesCount = user.FavouritesCount;
                this.FollowersCount = user.FollowersCount;
                this.FriendsCount = user.FriendsCount;
                this.ListedCount = user.ListedCount;
                this.IsMuting = user.IsMuting;
                this.IsProtected = user.IsProtected;
                this.IsVerified = user.IsVerified;
                this.Location = user.Location;
                this.ProfileBackgroundColor = user.ProfileBackgroundColor;
                this.ProfileBannerUrl = user.ProfileBannerUrl;
                this.ProfileImageUrl = user.ProfileImageUrl;
                this.StatusesCount = user.StatusesCount;
                this.Url = user.Url;
                this.Name = user.Name;
                this.IsFollowRequestSent = user.IsFollowRequestSent;
                this.UserId = user.Id;
            }
            catch
            {
                this.UpdatingUserInformation = false;
                return;
            }
            
            this.UpdatingUserInformation = false;
        }

        public async Task UpdateRelationShip()
        {
            if (this.UpdatingRelationShip)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingRelationShip = true;
            
            try
            {
                var relationShip = await Tokens.Friendships.ShowAsync(source_screen_name => Tokens.ScreenName, target_screen_name => this._ScreenName); this.IsFollowing = relationShip.Source.IsFollowing;
                this.IsFollowedBy = relationShip.Source.IsFollowedBy;
                this.IsBlocking = relationShip.Source.IsBlocking;
                this.IsMuting = relationShip.Source.IsMuting;
                this.IsFollowRequestSent = relationShip.Source.IsFollowingRequested;
            }
            catch
            {
                this.UpdatingRelationShip = false;
                return;
            }

            this.UpdatingRelationShip = false;
        }

        public async Task UpdateStatuses(long maxid = 0)
        {
            if (this.UpdatingStatuses)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingStatuses = true;
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"screen_name", this._ScreenName},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var userTweets = await Tokens.Statuses.UserTimelineAsync(param);
                if (maxid == 0)
                    this.Statuses.Clear();

                foreach (var status in userTweets)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));

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
            }
            catch
            {
                if (maxid == 0)
                    this.Statuses.Clear();

                this.UpdatingStatuses = false;
                return;
            }

            this.UpdatingStatuses = false;
        }

        public async Task UpdateFavorites(long maxid = 0)
        {
            if (this.UpdatingFavorites)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingFavorites = true;
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"screen_name", this._ScreenName},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var favorites = await Tokens.Favorites.ListAsync(param);

                if (maxid == 0)
                    this.Favorites.Clear();

                foreach (var status in favorites)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));

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
            }
            catch
            {
                if (maxid == 0)
                    this.Favorites.Clear();

                this.UpdatingFavorites = false;
                return;
            }

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
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"screen_name", this._ScreenName},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (useCursor && followersCursor != 0)
                    param.Add("cursor", followersCursor);

                var follower = await Tokens.Followers.ListAsync(param);
                if (!useCursor || followersCursor == 0)
                    this.Followers.Clear();

                foreach (var user in follower)
                {
                    this.Followers.Add(user);
                }

                followersCursor = follower.NextCursor;
            }
            catch
            {
                if (!useCursor || followersCursor == 0)
                    this.Followers.Clear();

                this.UpdatingFollowers = false;
                return;
            }

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
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"screen_name", this._ScreenName},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (useCursor && followingCursor != 0)
                    param.Add("cursor", followingCursor);

                var following = await Tokens.Friends.ListAsync(param);
                if (!useCursor || followingCursor == 0)
                    this.Following.Clear();

                foreach (var user in following)
                {
                    this.Following.Add(user);
                }

                followingCursor = following.NextCursor;
            }
            catch
            {
                if (!useCursor || followingCursor == 0)
                    this.Following.Clear();

                this.UpdatingFollowing = false;
                return;
            }

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
            try
            {
                var user = await this.Tokens.Friendships.CreateAsync(screen_name => this._ScreenName);
                if (user.IsProtected)
                    this.IsFollowRequestSent = true;
                else
                    this.IsFollowing = true;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyFollow()
        {
            try
            {
                await this.Tokens.Friendships.DestroyAsync(screen_name => this._ScreenName);
                this.IsFollowing = false;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task CreateBlock()
        {
            try
            {
                await this.Tokens.Blocks.CreateAsync(screen_name => this._ScreenName);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
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
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
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
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
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
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            this.IsMuting = false;
        }
    }
}
