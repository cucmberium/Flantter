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

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserProfileSettingsFlyoutModel : BindableBase
    {
        public UserProfileSettingsFlyoutModel()
        {
            this.Statuses = new ObservableCollection<Twitter.Objects.Status>();
            this.Favorites = new ObservableCollection<Twitter.Objects.Status>();
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

        public long UserFollowingCursor { get; set; }
        public long UserFollowerCursor { get; set; }

        public bool OpenFollowing { get; set; }
        public bool OpenFollower { get; set; }
        public bool OpenFavorite { get; set; }

        #region SelectedTab変更通知プロパティ
        private int _SelectedTab;
        public int SelectedTab
        {
            get { return this._SelectedTab; }
            set
            { 
                if (this._SelectedTab != value)
                {
                    this._SelectedTab = value;
                    this.OnPropertyChanged();

                    switch (this._SelectedTab)
                    {
                        case 0:
                            break;
                        case 1:
                            if (!this.OpenFollowing)
                            {
                                //this.UpdateFollowing();
                                this.OpenFollowing = true;
                            }
                            break;
                        case 2:
                            if (!this.OpenFollower)
                            {
                                //this.UpdateFollower();
                                this.OpenFollower = true;
                            }
                            break;
                        case 3:
                            if (!this.OpenFavorite)
                            {
                                this.UpdateFavorites();
                                this.OpenFavorite = true;
                            }
                            break;
                    }

                }
            }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.Status> Statuses { get; set; }
        public ObservableCollection<Twitter.Objects.Status> Favorites { get; set; }

        #region UserInformation変更通知プロパティ
        private Twitter.Objects.User _UserInformation;
        public Twitter.Objects.User UserInformation
        {
            get { return this._UserInformation; }
            set { this.SetProperty(ref this._UserInformation, value); }
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

        #region IsFollowing変更通知プロパティ
        private bool _IsFollowedBy;
        public bool IsFollowedBy
        {
            get { return this._IsFollowedBy; }
            set { this.SetProperty(ref this._IsFollowedBy, value); }
        }
        #endregion

        #region IsFollowing変更通知プロパティ
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

            this.UserInformation = new Twitter.Objects.User(user);

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

            this.UpdatingRelationShip = false;
        }

        public async Task UpdateStatuses(long maxid = 0)
        {
            if (this.UpdatingStatuses)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

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

            this.UpdatingStatuses = false;
        }

        public async Task UpdateFavorites(long maxid = 0)
        {
            if (this.UpdatingFavorites)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

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

            this.UpdatingFavorites = false;
        }
    }
}
