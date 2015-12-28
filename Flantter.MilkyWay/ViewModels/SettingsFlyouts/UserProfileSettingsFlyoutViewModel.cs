using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserProfileSettingsFlyoutViewModel
    {
        private ResourceLoader resourceLoader = new ResourceLoader();
        public UserProfileSettingsFlyoutViewModel()
        {
            this.Model = new UserProfileSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.ScreenName = this.Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.SelectedTab = this.Model.ToReactivePropertyAsSynchronized(x => x.SelectedTab);

            this.Description = new ReactiveProperty<string>();
            this.UrlEntities = new ReactiveProperty<Entities>();
            this.DescriptionEntities = new ReactiveProperty<Entities>();
            this.FavouritesCount = new ReactiveProperty<int>();
            this.FollowersCount = new ReactiveProperty<int>();
            this.FriendsCount = new ReactiveProperty<int>();
            this.ListedCount = new ReactiveProperty<int>();
            this.IsMuting = new ReactiveProperty<string>();
            this.IsProtected = new ReactiveProperty<string>();
            this.IsVerified = new ReactiveProperty<string>();
            this.Location = new ReactiveProperty<string>();
            this.ProfileBackgroundColor = new ReactiveProperty<string>();
            this.ProfileBannerUrl = new ReactiveProperty<string>("http://localhost/");
            this.ProfileImageUrl = new ReactiveProperty<string>("http://localhost/");
            this.StatusesCount = new ReactiveProperty<int>();
            this.Url = new ReactiveProperty<string>();
            this.Name = new ReactiveProperty<string>();

            this.IsMyUserProfile = new ReactiveProperty<bool>();

            this.FollowButtonText = new ReactiveProperty<string>("Follow");
            this.FollowButtonPointerOverText = new ReactiveProperty<string>("Follow");

            this.FollowedByText = new ReactiveProperty<string>();

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.Subscribe(x => 
            {
                this.SelectedTab.Value = 0;

                this.Model.OpenFavorite = false;
                this.Model.OpenFollowers = false;
                this.Model.OpenFollowing = false;

                this.Model.Statuses.Clear();
                this.Model.Favorites.Clear();
                this.Model.Following.Clear();
                this.Model.Followers.Clear();
                
                this.Model.UserInformation.Description = "";
                this.Model.UserInformation.FavouritesCount = 0;
                this.Model.UserInformation.FollowersCount = 0;
                this.Model.UserInformation.FriendsCount = 0;
                this.Model.UserInformation.ListedCount = 0;
                this.Model.UserInformation.IsMuting = false;
                this.Model.UserInformation.IsProtected = false;
                this.Model.UserInformation.IsVerified = false;
                this.Model.UserInformation.Location = "";
                this.Model.UserInformation.ProfileBackgroundColor = "";
                this.Model.UserInformation.ProfileBackgroundImageUrl = "";
                this.Model.UserInformation.ProfileBannerUrl = "";
                this.Model.UserInformation.ProfileImageUrl = "";
                this.Model.UserInformation.StatusesCount = 0;
                this.Model.UserInformation.Url = "";
                this.Model.UserInformation.Name = "";

                this.DescriptionEntities.Value = null;
                this.UrlEntities.Value = null;
                this.Description.Value = "";
                this.FavouritesCount.Value = 0;
                this.FollowersCount.Value = 0;
                this.FriendsCount.Value = 0;
                this.ListedCount.Value = 0;
                this.IsMuting.Value = "";
                this.IsProtected.Value = "";
                this.IsVerified.Value = "";
                this.Location.Value = "";
                this.ProfileBackgroundColor.Value = "#C0DEED"; //"#55ACEE";
                this.ProfileBannerUrl.Value = "http://localhost/";
                this.ProfileImageUrl.Value = "http://localhost/";
                this.StatusesCount.Value = 0;
                this.Url.Value = "";
                this.Name.Value = "";

                this.IsMyUserProfile.Value = false;
                this.FollowedByText.Value = "";

                this.FollowButtonText.Value = "Follow";
                this.FollowButtonPointerOverText.Value = "Follow";
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.Subscribe(async x => 
            {
                await this.Model.UpdateUserInfomation();
                
                this.UrlEntities.Value = this.Model.UserInformation.Entities != null ? this.Model.UserInformation.Entities.Url : null;
                this.DescriptionEntities.Value = this.Model.UserInformation.Entities != null ? this.Model.UserInformation.Entities.Description : null;
                this.Description.Value = this.Model.UserInformation.Description;
                this.FavouritesCount.Value = this.Model.UserInformation.FavouritesCount;
                this.FollowersCount.Value = this.Model.UserInformation.FollowersCount;
                this.FriendsCount.Value = this.Model.UserInformation.FriendsCount;
                this.ListedCount.Value = this.Model.UserInformation.ListedCount;
                this.IsMuting.Value = this.Model.UserInformation.IsMuting ? "" : "";
                this.IsProtected.Value = this.Model.UserInformation.IsProtected ? "🔒" : "";
                this.IsVerified.Value = this.Model.UserInformation.IsVerified ? "" : "";
                this.Location.Value = this.Model.UserInformation.Location;
                this.ProfileBackgroundColor.Value = "#" + this.Model.UserInformation.ProfileBackgroundColor;
                this.ProfileBannerUrl.Value = string.IsNullOrWhiteSpace(this.Model.UserInformation.ProfileBannerUrl) ? "http://localhost/" : this.Model.UserInformation.ProfileBannerUrl + "/1500x500";
                this.ProfileImageUrl.Value = string.IsNullOrWhiteSpace(this.Model.UserInformation.ProfileImageUrl) ? "http://localhost/" : this.Model.UserInformation.ProfileImageUrl.Replace("_normal", "");
                this.StatusesCount.Value = this.Model.UserInformation.StatusesCount;
                this.Url.Value = this.Model.UserInformation.Url;
                this.Name.Value = this.Model.UserInformation.Name;

                await this.Model.UpdateRelationShip();

                this.IsMyUserProfile.Value = this.ScreenName.Value == this.Tokens.Value.ScreenName;
                this.FollowedByText.Value = this.Model.IsFollowedBy ? resourceLoader.GetString("SettingsFlyout_UserProfile_FollowBacked") : (this.IsMyUserProfile.Value ? resourceLoader.GetString("SettingsFlyout_UserProfile_ThatsYou") : "");
                if (this.Model.IsBlocking)
                {
                    this.FollowButtonText.Value = "Blocking";
                    this.FollowButtonPointerOverText.Value = "Unblock";
                }
                else if (this.Model.IsFollowing)
                {
                    this.FollowButtonText.Value = "Following";
                    this.FollowButtonPointerOverText.Value = "Unfollow";
                }
                else if (this.Model.UserInformation.IsFollowRequestSent)
                {
                    this.FollowButtonText.Value = "Reqest Sent";
                    this.FollowButtonPointerOverText.Value = "Cancel Request";
                }
                else
                {
                    this.FollowButtonText.Value = "Follow";
                    this.FollowButtonPointerOverText.Value = "Follow";
                }

                await this.Model.UpdateStatuses();
            });

            this.BlockUserCommand = new ReactiveCommand();
            this.BlockUserCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                await this.Model.CreateBlock();
                if (this.Model.IsBlocking)
                {
                    this.FollowButtonText.Value = "Blocking";
                    this.FollowButtonPointerOverText.Value = "Unblock";
                }
                else if (this.Model.IsFollowing)
                {
                    this.FollowButtonText.Value = "Following";
                    this.FollowButtonPointerOverText.Value = "Unfollow";
                }
                else if (this.Model.UserInformation.IsFollowRequestSent)
                {
                    this.FollowButtonText.Value = "Reqest Sent";
                    this.FollowButtonPointerOverText.Value = "Cancel Request";
                }
                else
                {
                    this.FollowButtonText.Value = "Follow";
                    this.FollowButtonPointerOverText.Value = "Follow";
                }
            });

            this.MuteUserCommand = new ReactiveCommand();

            this.StatusesIncrementalLoadCommand = new ReactiveCommand();
            this.StatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.Statuses.Count > 0)
                    await this.Model.UpdateStatuses(this.Model.Statuses.LastOrDefault().Id);
            });

            this.FavoritesIncrementalLoadCommand = new ReactiveCommand();
            this.FavoritesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.Favorites.Count > 0)
                    await this.Model.UpdateFavorites(this.Model.Favorites.LastOrDefault().Id);
            });

            this.FollowersIncrementalLoadCommand = new ReactiveCommand();
            this.FollowersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => await this.Model.UpdateFollowers(true));

            this.FollowingIncrementalLoadCommand = new ReactiveCommand();
            this.FollowingIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => await this.Model.UpdateFollowing(true));

            this.ScrollViewerIncrementalLoadCommand = new ReactiveCommand();
            this.ScrollViewerIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                switch (this.SelectedTab.Value)
                {
                    case 0:
                        if (this.Model.Statuses.Count != 0)
                            await this.Model.UpdateStatuses(this.Model.Statuses.LastOrDefault().Id);
                        break;
                    case 1:
                        await this.Model.UpdateFollowing(true);
                        break;
                    case 2:
                        await this.Model.UpdateFollowers(true);
                        break;
                    case 3:
                        if (this.Model.Favorites.Count != 0)
                            await this.Model.UpdateFavorites(this.Model.Favorites.LastOrDefault().Id);
                        break;
                }
            });

            this.OpenUserProfileInWebCommand = new ReactiveCommand();
            this.OpenUserProfileInWebCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var url = "http://twitter.com/" + this.Model.ScreenName;
                await Launcher.LaunchUriAsync(new Uri(url));
            });

            this.Statuses = this.Model.Statuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, this.Tokens.Value.UserId));
            this.Favorites = this.Model.Favorites.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, this.Tokens.Value.UserId));
            this.Followers = this.Model.Followers.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            this.Following = this.Model.Following.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            this.Updating = Observable.CombineLatest(
                                this.Model.ObserveProperty(x => x.UpdatingFavorites),
                                this.Model.ObserveProperty(x => x.UpdatingFollowers),
                                this.Model.ObserveProperty(x => x.UpdatingFavorites),
                                this.Model.ObserveProperty(x => x.UpdatingStatuses),
                                this.Model.ObserveProperty(x => x.UpdatingUserInformation),
                                this.Model.ObserveProperty(x => x.UpdatingRelationShip),
                                (updatingFavorite, updatingFollowers, updatingFavorites, updatingStatuses, updatingUserInformation, updatingRelationShip) =>
                                {
                                    return (updatingFavorite || updatingFollowers || updatingFavorites || updatingStatuses || updatingUserInformation || updatingRelationShip);
                                }).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public UserProfileSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> SelectedTab { get; set; }
        
        public ReadOnlyReactiveCollection<StatusViewModel> Statuses { get; private set; }
        
        public ReadOnlyReactiveCollection<StatusViewModel> Favorites { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Following { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Followers { get; private set; }

        public ReactiveProperty<bool> IsMyUserProfile { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<string> FollowedByText { get; set; }

        public ReactiveProperty<string> FollowButtonText { get; set; }

        public ReactiveProperty<string> FollowButtonPointerOverText { get; set; }

        #region Description変更通知プロパティ
        public ReactiveProperty<string> Description { get; set; }
        #endregion

        #region Entities変更通知プロパティ
        public ReactiveProperty<Entities> UrlEntities { get; set; }
        public ReactiveProperty<Entities> DescriptionEntities { get; set; }
        #endregion

        #region FavouritesCount変更通知プロパティ
        public ReactiveProperty<int> FavouritesCount { get; set; }
        #endregion

        #region FollowersCount変更通知プロパティ
        public ReactiveProperty<int> FollowersCount { get; set; }
        #endregion

        #region FriendsCount変更通知プロパティ
        public ReactiveProperty<int> FriendsCount { get; set; }
        #endregion

        #region ListedCount変更通知プロパティ
        public ReactiveProperty<int> ListedCount { get; set; }
        #endregion

        #region IsMuting変更通知プロパティ
        public ReactiveProperty<string> IsMuting { get; set; }
        #endregion

        #region IsProtected変更通知プロパティ
        public ReactiveProperty<string> IsProtected { get; set; }
        #endregion

        #region IsVerified変更通知プロパティ
        public ReactiveProperty<string> IsVerified { get; set; }
        #endregion

        #region Location変更通知プロパティ
        public ReactiveProperty<string> Location { get; set; }
        #endregion

        #region Name変更通知プロパティ
        public ReactiveProperty<string> Name { get; set; }
        #endregion

        #region ProfileBackgroundColor変更通知プロパティ
        public ReactiveProperty<string> ProfileBackgroundColor { get; set; }
        #endregion

        #region ProfileBannerUrl変更通知プロパティ
        public ReactiveProperty<string> ProfileBannerUrl { get; set; }
        #endregion

        #region ProfileImageUrl変更通知プロパティ
        public ReactiveProperty<string> ProfileImageUrl { get; set; }
        #endregion

        #region ScreenName変更通知プロパティ
        public ReactiveProperty<string> ScreenName { get; set; }
        #endregion

        #region StatusesCount変更通知プロパティ
        public ReactiveProperty<int> StatusesCount { get; set; }
        #endregion

        #region Url変更通知プロパティ
        public ReactiveProperty<string> Url { get; set; }
        #endregion

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand MuteUserCommand { get; set; }

        public ReactiveCommand BlockUserCommand { get; set; }

        public ReactiveCommand StatusesIncrementalLoadCommand { get; set; }
        public ReactiveCommand FavoritesIncrementalLoadCommand { get; set; }
        public ReactiveCommand FollowersIncrementalLoadCommand { get; set; }
        public ReactiveCommand FollowingIncrementalLoadCommand { get; set; }
        public ReactiveCommand ScrollViewerIncrementalLoadCommand { get; set; }
        public ReactiveCommand OpenUserProfileInWebCommand { get; set; }
        public Services.Notice Notice { get; set; }
    }
}
