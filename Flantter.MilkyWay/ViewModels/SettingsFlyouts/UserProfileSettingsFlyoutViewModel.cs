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
        private readonly ResourceLoader resourceLoader = new ResourceLoader();
        public UserProfileSettingsFlyoutViewModel()
        {
            this.Model = new UserProfileSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.ScreenName = this.Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.PivotSelectedIndex = new ReactiveProperty<int>(0);
            this.PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                switch (x)
                {
                    case 1:
                        if (!this.Model.OpenFollowing)
                        {
                            await this.Model.UpdateFollowing();
                            this.Model.OpenFollowing = true;
                        }
                        break;
                    case 2:
                        if (!this.Model.OpenFollowers)
                        {
                            await this.Model.UpdateFollowers();
                            this.Model.OpenFollowers = true;
                        }
                        break;
                    case 3:
                        if (!this.Model.OpenFavorite)
                        {
                            await this.Model.UpdateFavorites();
                            this.Model.OpenFavorite = true;
                        }
                        break;
                }
            });

            this.UrlEntities = this.Model.ObserveProperty(x => x.UrlEntities).ToReactiveProperty();
            this.DescriptionEntities = this.Model.ObserveProperty(x => x.DescriptionEntities).ToReactiveProperty();
            this.Description = this.Model.ObserveProperty(x => x.Description).ToReactiveProperty();
            this.FavouritesCount = this.Model.ObserveProperty(x => x.FavouritesCount).ToReactiveProperty();
            this.FollowersCount = this.Model.ObserveProperty(x => x.FollowersCount).ToReactiveProperty();
            this.FriendsCount = this.Model.ObserveProperty(x => x.FriendsCount).ToReactiveProperty();
            this.ListedCount = this.Model.ObserveProperty(x => x.ListedCount).ToReactiveProperty();
            this.IsMuting = this.Model.ObserveProperty(x => x.IsMuting).Select(x => x ? "" : "").ToReactiveProperty();
            this.IsProtected = this.Model.ObserveProperty(x => x.IsProtected).Select(x => x ? "🔒" : "").ToReactiveProperty();
            this.IsVerified = this.Model.ObserveProperty(x => x.IsVerified).Select(x => x ? "" : "").ToReactiveProperty();
            this.Location = this.Model.ObserveProperty(x => x.Location).ToReactiveProperty();
            this.ProfileBackgroundColor = this.Model.ObserveProperty(x => x.ProfileBackgroundColor).Select(x => string.IsNullOrWhiteSpace(x) ? "#C0DEED" : "#" + x).ToReactiveProperty();
            this.ProfileBannerUrl = this.Model.ObserveProperty(x => x.ProfileBannerUrl).Select(x => string.IsNullOrWhiteSpace(x) ? "http://localhost/" : x + "/1500x500").ToReactiveProperty();
            this.ProfileImageUrl = this.Model.ObserveProperty(x => x.ProfileImageUrl).Select(x => string.IsNullOrWhiteSpace(x) ? "http://localhost/" : x.Replace("_normal", "")).ToReactiveProperty();
            this.StatusesCount = this.Model.ObserveProperty(x => x.StatusesCount).ToReactiveProperty();
            this.Url = this.Model.ObserveProperty(x => x.Url).ToReactiveProperty();
            this.Name = this.Model.ObserveProperty(x => x.Name).ToReactiveProperty();

            this.IsMyUserProfile = Observable.CombineLatest(
                                this.Model.ObserveProperty(x => x.ScreenName),
                                this.Tokens,
                                (screenName, tokens) =>
                                {
                                    if (tokens == null)
                                        return false;

                                    return screenName == tokens.ScreenName;
                                }).ToReactiveProperty();

            this.MuteMenuEnabled = Observable.CombineLatest(
                                this.IsMyUserProfile,
                                this.Model.ObserveProperty(x => x.IsMuting),
                                (isMyProfile, isMuting) =>
                                {
                                    return (!isMyProfile && !isMuting);
                                }).ToReactiveProperty();
            this.UnmuteMenuEnabled = Observable.CombineLatest(
                                this.IsMyUserProfile,
                                this.Model.ObserveProperty(x => x.IsMuting),
                                (isMyProfile, isMuting) =>
                                {
                                    return (!isMyProfile && isMuting);
                                }).ToReactiveProperty();
            this.BlockMenuEnabled = Observable.CombineLatest(
                                this.IsMyUserProfile,
                                this.Model.ObserveProperty(x => x.IsBlocking),
                                (isMyProfile, isBlocking) =>
                                {
                                    return (!isMyProfile && !isBlocking);
                                }).ToReactiveProperty();
            this.UnblockMenuEnabled = Observable.CombineLatest(
                                this.IsMyUserProfile,
                                this.Model.ObserveProperty(x => x.IsBlocking),
                                (isMyProfile, isBlocking) =>
                                {
                                    return (!isMyProfile && isBlocking);
                                }).ToReactiveProperty();

            this.FollowButtonText = Observable.CombineLatest(
                                this.Model.ObserveProperty(x => x.IsBlocking),
                                this.Model.ObserveProperty(x => x.IsFollowing),
                                this.Model.ObserveProperty(x => x.IsFollowRequestSent),
                                (isBlocking, isFollowing, isFollowRequestSent) =>
                                {
                                    if (isBlocking)
                                        return "Blocking";
                                    else if (isFollowing)
                                        return "Following";
                                    else if (isFollowRequestSent)
                                        return "Reqest Sent";
                                    else
                                        return "Follow";
                                }).ToReactiveProperty();
            this.FollowButtonPointerOverText = Observable.CombineLatest(
                                this.Model.ObserveProperty(x => x.IsBlocking),
                                this.Model.ObserveProperty(x => x.IsFollowing),
                                this.Model.ObserveProperty(x => x.IsFollowRequestSent),
                                (isBlocking, isFollowing, isFollowRequestSent) =>
                                {
                                    if (isBlocking)
                                        return "Unblock";
                                    else if (isFollowing)
                                        return "Unfollow";
                                    else if (isFollowRequestSent)
                                        return "Cancel Request";
                                    else
                                        return "Follow";
                                }).ToReactiveProperty();

            this.FollowedByText = Observable.CombineLatest(
                                this.Model.ObserveProperty(x => x.IsFollowedBy),
                                this.IsMyUserProfile,
                                (isFollowedBy, isMyUserProfile) =>
                                {
                                    if (isFollowedBy)
                                        return resourceLoader.GetString("SettingsFlyout_UserProfile_FollowBacked");
                                    else if (isMyUserProfile)
                                        return resourceLoader.GetString("SettingsFlyout_UserProfile_ThatsYou");
                                    else
                                        return "";
                                }).ToReactiveProperty();

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.Subscribe(x => 
            {
                this.PivotSelectedIndex.Value = 0;

                this.Model.OpenFavorite = false;
                this.Model.OpenFollowers = false;
                this.Model.OpenFollowing = false;

                this.Model.Statuses.Clear();
                this.Model.Favorites.Clear();
                this.Model.Following.Clear();
                this.Model.Followers.Clear();

                this.Model.UrlEntities = null;
                this.Model.DescriptionEntities = null;
                this.Model.Description = "";
                this.Model.FavouritesCount = 0;
                this.Model.FollowersCount = 0;
                this.Model.FriendsCount = 0;
                this.Model.ListedCount = 0;
                this.Model.IsMuting = false;
                this.Model.IsProtected = false;
                this.Model.IsVerified = false;
                this.Model.Location = "";
                this.Model.ProfileBackgroundColor = "";
                this.Model.ProfileBannerUrl = "";
                this.Model.ProfileImageUrl = "";
                this.Model.StatusesCount = 0;
                this.Model.Url = "";
                this.Model.Name = "";
                this.Model.IsFollowRequestSent = false;
                this.Model.IsFollowing = false;
                this.Model.IsFollowedBy = false;
                this.Model.IsBlocking = false;
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.Subscribe(async x => 
            {
                await this.Model.UpdateUserInfomation();
                await this.Model.UpdateRelationShip();
                await this.Model.UpdateStatuses();
            });

            this.BlockUserCommand = new ReactiveCommand();
            this.BlockUserCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                await this.Model.CreateBlock();
            });

            this.UnblockUserCommand = new ReactiveCommand();
            this.UnblockUserCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.DestroyBlock();
            });

            // Todo : 実装

            this.MuteUserCommand = new ReactiveCommand();

            this.UnmuteUserCommand = new ReactiveCommand();

            this.StatusesIncrementalLoadCommand = new ReactiveCommand();
            this.StatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.Statuses.Count <= 0)
                    return;

                var id = this.Model.Statuses.Last().Id;
                var status = this.Model.Statuses.Last();
                if (status.HasRetweetInformation)
                    id = status.RetweetInformation.Id;

                await this.Model.UpdateStatuses(id);
            });

            this.FavoritesIncrementalLoadCommand = new ReactiveCommand();
            this.FavoritesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.Favorites.Count <= 0)
                    return;

                var id = this.Model.Favorites.Last().Id;
                var status = this.Model.Favorites.Last();
                if (status.HasRetweetInformation)
                    id = status.RetweetInformation.Id;

                await this.Model.UpdateFavorites(id);
            });

            this.FollowersIncrementalLoadCommand = new ReactiveCommand();
            this.FollowersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => await this.Model.UpdateFollowers(true));

            this.FollowingIncrementalLoadCommand = new ReactiveCommand();
            this.FollowingIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => await this.Model.UpdateFollowing(true));

            this.OpenUserProfileInWebCommand = new ReactiveCommand();
            this.OpenUserProfileInWebCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await Launcher.LaunchUriAsync(new Uri("http://twitter.com/" + this.Model.ScreenName));
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

        public ReactiveProperty<bool> MuteMenuEnabled { get; set; }

        public ReactiveProperty<bool> UnmuteMenuEnabled { get; set; }

        public ReactiveProperty<bool> BlockMenuEnabled { get; set; }

        public ReactiveProperty<bool> UnblockMenuEnabled { get; set; }

        public UserProfileSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }
        

        public ReadOnlyReactiveCollection<StatusViewModel> Statuses { get; private set; }
        
        public ReadOnlyReactiveCollection<StatusViewModel> Favorites { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Following { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Followers { get; private set; }


        public ReactiveProperty<bool> IsMyUserProfile { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<string> FollowedByText { get; set; }

        public ReactiveProperty<string> FollowButtonText { get; set; }

        public ReactiveProperty<string> FollowButtonPointerOverText { get; set; }
        
        
        public ReactiveProperty<Entities> UrlEntities { get; set; }

        public ReactiveProperty<Entities> DescriptionEntities { get; set; }

        public ReactiveProperty<string> Description { get; set; }

        public ReactiveProperty<int> FavouritesCount { get; set; }

        public ReactiveProperty<int> FollowersCount { get; set; }

        public ReactiveProperty<int> FriendsCount { get; set; }

        public ReactiveProperty<int> ListedCount { get; set; }

        public ReactiveProperty<string> IsMuting { get; set; }

        public ReactiveProperty<string> IsProtected { get; set; }

        public ReactiveProperty<string> IsVerified { get; set; }

        public ReactiveProperty<string> Location { get; set; }

        public ReactiveProperty<string> Name { get; set; }

        public ReactiveProperty<string> ProfileBackgroundColor { get; set; }

        public ReactiveProperty<string> ProfileBannerUrl { get; set; }

        public ReactiveProperty<string> ProfileImageUrl { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }

        public ReactiveProperty<int> StatusesCount { get; set; }

        public ReactiveProperty<string> Url { get; set; }


        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand MuteUserCommand { get; set; }

        public ReactiveCommand UnmuteUserCommand { get; set; }

        public ReactiveCommand BlockUserCommand { get; set; }

        public ReactiveCommand UnblockUserCommand { get; set; }

        public ReactiveCommand StatusesIncrementalLoadCommand { get; set; }

        public ReactiveCommand FavoritesIncrementalLoadCommand { get; set; }

        public ReactiveCommand FollowersIncrementalLoadCommand { get; set; }

        public ReactiveCommand FollowingIncrementalLoadCommand { get; set; }

        public ReactiveCommand OpenUserProfileInWebCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
