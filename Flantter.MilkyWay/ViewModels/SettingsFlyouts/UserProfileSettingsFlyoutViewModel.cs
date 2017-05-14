using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserProfileSettingsFlyoutViewModel
    {
        private readonly ResourceLoader _resourceLoader = new ResourceLoader();

        public UserProfileSettingsFlyoutViewModel()
        {
            Model = new UserProfileSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");
            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);
            ScreenName = Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);

            PivotSelectedIndex = new ReactiveProperty<int>(0);
            PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    switch (x)
                    {
                        case 1:
                            if (!Model.OpenFollowing)
                            {
                                await Model.UpdateFollowing();
                                Model.OpenFollowing = true;
                            }
                            break;
                        case 2:
                            if (!Model.OpenFollowers)
                            {
                                await Model.UpdateFollowers();
                                Model.OpenFollowers = true;
                            }
                            break;
                        case 3:
                            if (!Model.OpenFavorite)
                            {
                                await Model.UpdateFavorites();
                                Model.OpenFavorite = true;
                            }
                            break;
                    }
                });

            UrlEntities = Model.ObserveProperty(x => x.UrlEntities).ToReactiveProperty();
            DescriptionEntities = Model.ObserveProperty(x => x.DescriptionEntities).ToReactiveProperty();
            Description = Model.ObserveProperty(x => x.Description).ToReactiveProperty();
            FavouritesCount = Model.ObserveProperty(x => x.FavouritesCount).ToReactiveProperty();
            FollowersCount = Model.ObserveProperty(x => x.FollowersCount).ToReactiveProperty();
            FriendsCount = Model.ObserveProperty(x => x.FriendsCount).ToReactiveProperty();
            ListedCount = Model.ObserveProperty(x => x.ListedCount).ToReactiveProperty();
            IsMuting = Model.ObserveProperty(x => x.IsMuting).Select(x => x ? "" : "").ToReactiveProperty();
            IsProtected = Model.ObserveProperty(x => x.IsProtected).Select(x => x ? "🔒" : "").ToReactiveProperty();
            IsVerified = Model.ObserveProperty(x => x.IsVerified).Select(x => x ? "" : "").ToReactiveProperty();
            Location = Model.ObserveProperty(x => x.Location).ToReactiveProperty();
            ProfileBackgroundColor = Model.ObserveProperty(x => x.ProfileBackgroundColor)
                .Select(x => string.IsNullOrWhiteSpace(x) ? "#C0DEED" : "#" + x)
                .ToReactiveProperty();
            ProfileBannerUrl = Model.ObserveProperty(x => x.ProfileBannerUrl)
                .Select(x => string.IsNullOrWhiteSpace(x) ? "http://localhost/" : x + "/1500x500")
                .ToReactiveProperty();
            ProfileImageUrl = Model.ObserveProperty(x => x.ProfileImageUrl)
                .Select(x => string.IsNullOrWhiteSpace(x) ? "http://localhost/" : x.Replace("_normal", ""))
                .ToReactiveProperty();
            StatusesCount = Model.ObserveProperty(x => x.StatusesCount).ToReactiveProperty();
            Url = Model.ObserveProperty(x => x.Url).ToReactiveProperty();
            Name = Model.ObserveProperty(x => x.Name).ToReactiveProperty();

            UpdatingStatuses = Model.ObserveProperty(x => x.UpdatingStatuses).ToReactiveProperty();
            UpdatingFavorites = Model.ObserveProperty(x => x.UpdatingFavorites).ToReactiveProperty();
            UpdatingFollowers = Model.ObserveProperty(x => x.UpdatingFollowers).ToReactiveProperty();
            UpdationFollowing = Model.ObserveProperty(x => x.UpdatingFollowing).ToReactiveProperty();

            IsMyUserProfile = Model.ObserveProperty(x => x.ScreenName)
                .CombineLatest(Tokens,
                    (screenName, tokens) =>
                    {
                        if (tokens == null)
                            return false;

                        return screenName == tokens.ScreenName;
                    })
                .ToReactiveProperty();

            MuteMenuEnabled = IsMyUserProfile.CombineLatest(Model.ObserveProperty(x => x.IsMuting),
                    (isMyProfile, isMuting) => !isMyProfile && !isMuting)
                .ToReactiveProperty();
            UnmuteMenuEnabled = IsMyUserProfile.CombineLatest(Model.ObserveProperty(x => x.IsMuting),
                    (isMyProfile, isMuting) => !isMyProfile && isMuting)
                .ToReactiveProperty();
            BlockMenuEnabled = IsMyUserProfile.CombineLatest(Model.ObserveProperty(x => x.IsBlocking),
                    (isMyProfile, isBlocking) => !isMyProfile && !isBlocking)
                .ToReactiveProperty();
            UnblockMenuEnabled = IsMyUserProfile.CombineLatest(Model.ObserveProperty(x => x.IsBlocking),
                    (isMyProfile, isBlocking) => !isMyProfile && isBlocking)
                .ToReactiveProperty();

            OpenUserListEnabled = Tokens
                .Select(x => x?.Platform == Models.Twitter.Wrapper.Tokens.PlatformEnum.Twitter)
                .ToReactiveProperty();
            OpenUserCollectionEnabled = Tokens
                .Select(x => x?.Platform == Models.Twitter.Wrapper.Tokens.PlatformEnum.Twitter)
                .ToReactiveProperty();

            FollowButtonText = Model.ObserveProperty(x => x.IsBlocking)
                .CombineLatest(Model.ObserveProperty(x => x.IsFollowing),
                    Model.ObserveProperty(x => x.IsFollowRequestSent),
                    (isBlocking, isFollowing, isFollowRequestSent) =>
                    {
                        if (isBlocking)
                            return "Blocking";
                        if (isFollowing)
                            return "Following";
                        if (isFollowRequestSent)
                            return "Reqest Sent";
                        return "Follow";
                    })
                .ToReactiveProperty();
            FollowButtonPointerOverText = Model.ObserveProperty(x => x.IsBlocking)
                .CombineLatest(Model.ObserveProperty(x => x.IsFollowing),
                    Model.ObserveProperty(x => x.IsFollowRequestSent),
                    (isBlocking, isFollowing, isFollowRequestSent) =>
                    {
                        if (isBlocking)
                            return "Unblock";
                        if (isFollowing)
                            return "Unfollow";
                        if (isFollowRequestSent)
                            return "Cancel Request";
                        return "Follow";
                    })
                .ToReactiveProperty();

            FollowedByText = Model.ObserveProperty(x => x.IsFollowedBy)
                .CombineLatest(IsMyUserProfile,
                    (isFollowedBy, isMyUserProfile) =>
                    {
                        if (isFollowedBy)
                            return _resourceLoader.GetString("SettingsFlyout_UserProfile_FollowBacked");
                        if (isMyUserProfile)
                            return _resourceLoader.GetString("SettingsFlyout_UserProfile_ThatsYou");
                        return "";
                    })
                .ToReactiveProperty();

            ClearCommand = new ReactiveCommand();
            ClearCommand.Subscribe(x =>
            {
                PivotSelectedIndex.Value = 0;

                Model.UserId = 0;
                Model.ScreenName = "";

                Model.OpenFavorite = false;
                Model.OpenFollowers = false;
                Model.OpenFollowing = false;

                Model.Statuses.Clear();
                Model.Favorites.Clear();
                Model.Following.Clear();
                Model.Followers.Clear();

                Model.UrlEntities = null;
                Model.DescriptionEntities = null;
                Model.Description = "";
                Model.FavouritesCount = 0;
                Model.FollowersCount = 0;
                Model.FriendsCount = 0;
                Model.ListedCount = 0;
                Model.IsMuting = false;
                Model.IsProtected = false;
                Model.IsVerified = false;
                Model.Location = "";
                Model.ProfileBackgroundColor = "";
                Model.ProfileBannerUrl = "";
                Model.ProfileImageUrl = "";
                Model.StatusesCount = 0;
                Model.Url = "";
                Model.Name = "";
                Model.IsFollowRequestSent = false;
                Model.IsFollowing = false;
                Model.IsFollowedBy = false;
                Model.IsBlocking = false;
            });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.Subscribe(async x =>
            {
                await Model.UpdateUserInfomation();
                await Model.UpdateRelationShip();
                await Model.UpdateStatuses();
            });

            FollowCommand = new ReactiveCommand();
            FollowCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => { await Model.Follow(); });

            BlockUserCommand = new ReactiveCommand();
            BlockUserCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_Block"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.CreateBlock();
                });

            UnblockUserCommand = new ReactiveCommand();
            UnblockUserCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.DestroyBlock(); });

            MuteUserCommand = new ReactiveCommand();
            MuteUserCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_Mute"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (msgNotification.Result)
                        await Model.CreateMute();

                    msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_MuteInFlantter"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    if (!AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(Model.ScreenName))
                    {
                        AdvancedSettingService.AdvancedSetting.MuteUsers.Add(Model.ScreenName);
                        await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                    }
                });

            UnmuteUserCommand = new ReactiveCommand();
            UnmuteUserCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.DestroyMute(); });

            StatusesIncrementalLoadCommand = new ReactiveCommand();
            StatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.Statuses.Count <= 0)
                        return;

                    var id = Model.Statuses.Last().Id;
                    var status = Model.Statuses.Last();
                    if (status.HasRetweetInformation)
                        id = status.RetweetInformation.Id;

                    await Model.UpdateStatuses(id);
                });

            FavoritesIncrementalLoadCommand = new ReactiveCommand();
            FavoritesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.Favorites.Count <= 0)
                        return;

                    var id = Model.Favorites.Last().Id;
                    var status = Model.Favorites.Last();
                    if (status.HasRetweetInformation)
                        id = status.RetweetInformation.Id;

                    await Model.UpdateFavorites(id);
                });

            FollowersIncrementalLoadCommand = new ReactiveCommand();
            FollowersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => await Model.UpdateFollowers(true));

            FollowingIncrementalLoadCommand = new ReactiveCommand();
            FollowingIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => await Model.UpdateFollowing(true));

            OpenUserProfileInWebCommand = new ReactiveCommand();
            OpenUserProfileInWebCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.Tokens.Platform == Models.Twitter.Wrapper.Tokens.PlatformEnum.Twitter)
                        await Launcher.LaunchUriAsync(new Uri("https://twitter.com/" + Model.ScreenName));
                    else
                        await Launcher.LaunchUriAsync(new Uri("https://" + Model.Tokens.Instance + "/@" + Model.ScreenName));
                });

            AddColumnCommand = new ReactiveCommand();
            AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (string.IsNullOrWhiteSpace(Model.ScreenName))
                        return;

                    var columnSetting = new ColumnSetting
                    {
                        Action = SettingSupport.ColumnTypeEnum.UserTimeline,
                        AutoRefresh = false,
                        AutoRefreshTimerInterval = 180.0,
                        Filter = "()",
                        Name = "User : " + Model.ScreenName,
                        Parameter = Model.UserId.ToString(),
                        Streaming = false,
                        Index = -1,
                        DisableStartupRefresh = false,
                        FetchingNumberOfTweet = 40
                    };
                    Notice.Instance.AddColumnCommand.Execute(columnSetting);
                });

            Statuses = Model.Statuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));
            Favorites = Model.Favorites.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));
            Followers = Model.Followers.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            Following = Model.Following.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            Notice = Notice.Instance;
        }

        public ReactiveProperty<bool> MuteMenuEnabled { get; set; }

        public ReactiveProperty<bool> UnmuteMenuEnabled { get; set; }

        public ReactiveProperty<bool> BlockMenuEnabled { get; set; }

        public ReactiveProperty<bool> UnblockMenuEnabled { get; set; }

        public UserProfileSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }


        public ReadOnlyReactiveCollection<StatusViewModel> Statuses { get; }

        public ReadOnlyReactiveCollection<StatusViewModel> Favorites { get; }

        public ReadOnlyReactiveCollection<UserViewModel> Following { get; }

        public ReadOnlyReactiveCollection<UserViewModel> Followers { get; }


        public ReactiveProperty<bool> IsMyUserProfile { get; set; }

        public ReactiveProperty<bool> OpenUserListEnabled { get; set; }

        public ReactiveProperty<bool> OpenUserCollectionEnabled { get; set; }

        public ReactiveProperty<bool> UpdatingStatuses { get; set; }
        public ReactiveProperty<bool> UpdatingFavorites { get; set; }
        public ReactiveProperty<bool> UpdatingFollowers { get; set; }
        public ReactiveProperty<bool> UpdationFollowing { get; set; }

        public ReactiveProperty<string> FollowedByText { get; set; }

        public ReactiveProperty<string> FollowButtonText { get; set; }

        public ReactiveProperty<string> FollowButtonPointerOverText { get; set; }


        public ReactiveProperty<long> UserId { get; set; }

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

        public ReactiveCommand FollowCommand { get; set; }

        public ReactiveCommand MuteUserCommand { get; set; }

        public ReactiveCommand UnmuteUserCommand { get; set; }

        public ReactiveCommand BlockUserCommand { get; set; }

        public ReactiveCommand UnblockUserCommand { get; set; }

        public ReactiveCommand StatusesIncrementalLoadCommand { get; set; }

        public ReactiveCommand FavoritesIncrementalLoadCommand { get; set; }

        public ReactiveCommand FollowersIncrementalLoadCommand { get; set; }

        public ReactiveCommand FollowingIncrementalLoadCommand { get; set; }

        public ReactiveCommand OpenUserProfileInWebCommand { get; set; }

        public ReactiveCommand AddColumnCommand { get; set; }

        public Notice Notice { get; set; }
    }
}