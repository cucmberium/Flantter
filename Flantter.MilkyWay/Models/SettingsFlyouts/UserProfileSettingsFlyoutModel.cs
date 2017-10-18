using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserProfileSettingsFlyoutModel : BindableBase
    {
        private ResourceLoader _resourceLoader;

        private long _followersCursor;

        private long _followingCursor;

        public UserProfileSettingsFlyoutModel()
        {
            _resourceLoader = new ResourceLoader();

            Statuses = new ObservableCollection<Status>();
            Favorites = new ObservableCollection<Status>();
            Followers = new ObservableCollection<User>();
            Following = new ObservableCollection<User>();
        }

        public bool OpenFollowing { get; set; }
        public bool OpenFollowers { get; set; }
        public bool OpenFavorite { get; set; }

        public ObservableCollection<Status> Statuses { get; set; }
        public ObservableCollection<Status> Favorites { get; set; }
        public ObservableCollection<User> Followers { get; set; }
        public ObservableCollection<User> Following { get; set; }

        public async Task UpdateUserInfomation()
        {
            if (UpdatingUserInformation)
                return;

            if ((_userId == 0 && string.IsNullOrWhiteSpace(_screenName)) || Tokens == null)
                return;

            UpdatingUserInformation = true;

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"include_entities", true},
                };
                if (_userId != 0)
                    param.Add("user_id", _userId);
                else
                    param.Add("screen_name", _screenName);

                var user = await Tokens.Users.ShowAsync(param);

                UserId = user.Id;
                ScreenName = user.ScreenName;
                UrlEntities = user.Entities.Url;
                DescriptionEntities = user.Entities.Description;
                Description = user.Description;
                FavouritesCount = user.FavouritesCount;
                FollowersCount = user.FollowersCount;
                FriendsCount = user.FriendsCount;
                ListedCount = user.ListedCount;
                IsMuting = user.IsMuting;
                IsProtected = user.IsProtected;
                IsVerified = user.IsVerified;
                Location = user.Location;
                ProfileBackgroundColor = user.ProfileBackgroundColor;
                ProfileBannerUrl = user.ProfileBannerUrl;
                ProfileImageUrl = user.ProfileImageUrl;
                StatusesCount = user.StatusesCount;
                Url = user.Url;
                Name = user.Name;
                IsFollowRequestSent = user.IsFollowRequestSent;
            }
            catch
            {
                UpdatingUserInformation = false;
                return;
            }

            UpdatingUserInformation = false;
        }

        public async Task UpdateRelationShip()
        {
            if (UpdatingRelationShip)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            UpdatingRelationShip = true;

            try
            {
                var relationShip = await Tokens.Friendships.ShowAsync(source_id => Tokens.UserId,
                    target_id => _userId);
                IsFollowing = relationShip.Source.IsFollowing;
                IsFollowedBy = relationShip.Source.IsFollowedBy;
                IsBlocking = relationShip.Source.IsBlocking;
                IsMuting = relationShip.Source.IsMuting;
                IsFollowRequestSent = relationShip.Source.IsFollowingRequested;
            }
            catch
            {
                UpdatingRelationShip = false;
                return;
            }

            UpdatingRelationShip = false;
        }

        public async Task UpdateStatuses(long maxid = 0)
        {
            if (UpdatingStatuses)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            UpdatingStatuses = true;

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"user_id", _userId},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var userTweets = await Tokens.Statuses.UserTimelineAsync(param);
                if (maxid == 0)
                    Statuses.Clear();

                foreach (var status in userTweets)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = Statuses.IndexOf(
                        Statuses.FirstOrDefault(x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                    if (index == -1)
                    {
                        index = Statuses.IndexOf(
                            Statuses.FirstOrDefault(
                                x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            Statuses.Add(status);
                        else
                            Statuses.Insert(index, status);
                    }
                }
            }
            catch
            {
                if (maxid == 0)
                    Statuses.Clear();

                UpdatingStatuses = false;
                return;
            }

            UpdatingStatuses = false;
        }

        public async Task UpdateFavorites(long maxid = 0)
        {
            if (UpdatingFavorites)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            UpdatingFavorites = true;

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"id", _userId},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var favorites = await Tokens.Favorites.ListAsync(param);

                if (maxid == 0)
                    Favorites.Clear();

                foreach (var status in favorites)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = Favorites.IndexOf(
                        Favorites.FirstOrDefault(x =>
                            (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                    if (index == -1)
                    {
                        index = Favorites.IndexOf(
                            Favorites.FirstOrDefault(x =>
                                (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            Favorites.Add(status);
                        else
                            Favorites.Insert(index, status);
                    }
                }
            }
            catch
            {
                if (maxid == 0)
                    Favorites.Clear();

                UpdatingFavorites = false;
                return;
            }

            UpdatingFavorites = false;
        }

        public async Task UpdateFollowers(bool useCursor = false)
        {
            if (UpdatingFollowers)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            if (useCursor && _followersCursor == 0)
                return;

            UpdatingFollowers = true;

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"user_id", _userId},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (useCursor && _followersCursor != 0)
                    param.Add("cursor", _followersCursor);

                var follower = await Tokens.Followers.ListAsync(param);
                if (!useCursor || _followersCursor == 0)
                    Followers.Clear();

                foreach (var user in follower)
                    Followers.Add(user);

                _followersCursor = follower.NextCursor;
            }
            catch
            {
                if (!useCursor || _followersCursor == 0)
                    Followers.Clear();

                UpdatingFollowers = false;
                return;
            }

            UpdatingFollowers = false;
        }

        public async Task UpdateFollowing(bool useCursor = false)
        {
            if (UpdatingFollowing)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            if (useCursor && _followingCursor == 0)
                return;

            UpdatingFollowing = true;

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20},
                    {"include_entities", true},
                    {"user_id", _userId},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (useCursor && _followingCursor != 0)
                    param.Add("cursor", _followingCursor);

                var following = await Tokens.Friends.ListAsync(param);
                if (!useCursor || _followingCursor == 0)
                    Following.Clear();

                foreach (var user in following)
                    Following.Add(user);

                _followingCursor = following.NextCursor;
            }
            catch
            {
                if (!useCursor || _followingCursor == 0)
                    Following.Clear();

                UpdatingFollowing = false;
                return;
            }

            UpdatingFollowing = false;
        }

        public async Task Follow()
        {
            if (IsBlocking)
                await DestroyBlock();
            else if (IsFollowing)
                await DestroyFollow();
            else if (IsFollowRequestSent)
                await DestroyFollow();
            else
                await CreateFollow();
        }

        public async Task CreateFollow()
        {
            try
            {
                var user = await Tokens.Friendships.CreateAsync(user_id => _userId);
                if (user.IsProtected)
                    IsFollowRequestSent = true;
                else
                    IsFollowing = true;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task DestroyFollow()
        {
            try
            {
                await Tokens.Friendships.DestroyAsync(user_id => _userId);
                IsFollowing = false;
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
            }
        }

        public async Task CreateBlock()
        {
            try
            {
                await Tokens.Blocks.CreateAsync(user_id => _userId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
                return;
            }

            IsFollowing = false;
            IsBlocking = true;
        }

        public async Task DestroyBlock()
        {
            try
            {
                await Tokens.Blocks.DestroyAsync(user_id => _userId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
                return;
            }

            IsBlocking = false;
        }

        public async Task CreateMute()
        {
            try
            {
                await Tokens.Mutes.Users.CreateAsync(user_id => _userId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
                return;
            }

            IsMuting = true;
        }

        public async Task DestroyMute()
        {
            try
            {
                await Tokens.Mutes.Users.DestroyAsync(user_id => _userId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                return;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
                return;
            }

            IsMuting = false;
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region UserId変更通知プロパティ

        private long _userId;

        public long UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        #endregion


        #region ScreenName変更通知プロパティ

        private string _screenName;

        public string ScreenName
        {
            get => _screenName;
            set => SetProperty(ref _screenName, value);
        }

        #endregion

        #region DescriptionEntities変更通知プロパティ

        private Entities _descriptionEntities;

        public Entities DescriptionEntities
        {
            get => _descriptionEntities;
            set => SetProperty(ref _descriptionEntities, value);
        }

        #endregion

        #region UrlEntities変更通知プロパティ

        private Entities _urlEntities;

        public Entities UrlEntities
        {
            get => _urlEntities;
            set => SetProperty(ref _urlEntities, value);
        }

        #endregion

        #region Description変更通知プロパティ

        private string _description;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        #endregion

        #region FavouritesCount変更通知プロパティ

        private int _favouritesCount;

        public int FavouritesCount
        {
            get => _favouritesCount;
            set => SetProperty(ref _favouritesCount, value);
        }

        #endregion

        #region FollowersCount変更通知プロパティ

        private int _followersCount;

        public int FollowersCount
        {
            get => _followersCount;
            set => SetProperty(ref _followersCount, value);
        }

        #endregion

        #region FriendsCount変更通知プロパティ

        private int _friendsCount;

        public int FriendsCount
        {
            get => _friendsCount;
            set => SetProperty(ref _friendsCount, value);
        }

        #endregion

        #region ListedCount変更通知プロパティ

        private int _listedCount;

        public int ListedCount
        {
            get => _listedCount;
            set => SetProperty(ref _listedCount, value);
        }

        #endregion

        #region IsMuting変更通知プロパティ

        private bool _isMuting;

        public bool IsMuting
        {
            get => _isMuting;
            set => SetProperty(ref _isMuting, value);
        }

        #endregion

        #region IsProtected変更通知プロパティ

        private bool _isProtected;

        public bool IsProtected
        {
            get => _isProtected;
            set => SetProperty(ref _isProtected, value);
        }

        #endregion

        #region IsVerified変更通知プロパティ

        private bool _isVerified;

        public bool IsVerified
        {
            get => _isVerified;
            set => SetProperty(ref _isVerified, value);
        }

        #endregion

        #region Location変更通知プロパティ

        private string _location;

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        #endregion

        #region ProfileBackgroundColor変更通知プロパティ

        private string _profileBackgroundColor;

        public string ProfileBackgroundColor
        {
            get => _profileBackgroundColor;
            set => SetProperty(ref _profileBackgroundColor, value);
        }

        #endregion

        #region ProfileBannerUrl変更通知プロパティ

        private string _profileBannerUrl;

        public string ProfileBannerUrl
        {
            get => _profileBannerUrl;
            set => SetProperty(ref _profileBannerUrl, value);
        }

        #endregion

        #region ProfileImageUrl変更通知プロパティ

        private string _profileImageUrl;

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => SetProperty(ref _profileImageUrl, value);
        }

        #endregion

        #region StatusesCount変更通知プロパティ

        private int _statusesCount;

        public int StatusesCount
        {
            get => _statusesCount;
            set => SetProperty(ref _statusesCount, value);
        }

        #endregion

        #region Url変更通知プロパティ

        private string _url;

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        #endregion

        #region Name変更通知プロパティ

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        #endregion

        #region IsFollowRequestSent変更通知プロパティ

        private bool _isFollowRequestSent;

        public bool IsFollowRequestSent
        {
            get => _isFollowRequestSent;
            set => SetProperty(ref _isFollowRequestSent, value);
        }

        #endregion

        #region IsFollowing変更通知プロパティ

        private bool _isFollowing;

        public bool IsFollowing
        {
            get => _isFollowing;
            set => SetProperty(ref _isFollowing, value);
        }

        #endregion

        #region IsFollowedBy変更通知プロパティ

        private bool _isFollowedBy;

        public bool IsFollowedBy
        {
            get => _isFollowedBy;
            set => SetProperty(ref _isFollowedBy, value);
        }

        #endregion

        #region IsBlocking変更通知プロパティ

        private bool _isBlocking;

        public bool IsBlocking
        {
            get => _isBlocking;
            set => SetProperty(ref _isBlocking, value);
        }

        #endregion


        #region UpdatingUserInformation変更通知プロパティ

        private bool _updatingUserInformation;

        public bool UpdatingUserInformation
        {
            get => _updatingUserInformation;
            set => SetProperty(ref _updatingUserInformation, value);
        }

        #endregion

        #region UpdatingRelationShip変更通知プロパティ

        private bool _updatingRelationShip;

        public bool UpdatingRelationShip
        {
            get => _updatingRelationShip;
            set => SetProperty(ref _updatingRelationShip, value);
        }

        #endregion

        #region UpdatingStatuses変更通知プロパティ

        private bool _updatingStatuses;

        public bool UpdatingStatuses
        {
            get => _updatingStatuses;
            set => SetProperty(ref _updatingStatuses, value);
        }

        #endregion

        #region UpdatingFavorites変更通知プロパティ

        private bool _updatingFavorites;

        public bool UpdatingFavorites
        {
            get => _updatingFavorites;
            set => SetProperty(ref _updatingFavorites, value);
        }

        #endregion

        #region UpdatingFollowers変更通知プロパティ

        private bool _updatingFollowers;

        public bool UpdatingFollowers
        {
            get => _updatingFollowers;
            set => SetProperty(ref _updatingFollowers, value);
        }

        #endregion

        #region UpdatingFollowing変更通知プロパティ

        private bool _updatingFollowing;

        public bool UpdatingFollowing
        {
            get => _updatingFollowing;
            set => SetProperty(ref _updatingFollowing, value);
        }

        #endregion
    }
}