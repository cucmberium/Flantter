﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Apis.Objects
{
    public class StatusViewModel : ExtendedBindableBase
    {
        public StatusViewModel()
        {
            Model = null;

            // BackgroundBrush
            BackgroundBrush = "Default";
            CreatedAt = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            Source = "";
            Text = "";
            ScreenName = "";
            Name = "";
            ProfileImageUrl = "http://localhost/";
            Entities = null;
            ProtectedText = "";

            RetweetInformationVisibility = false;
            MediaVisibility = false;
            MediaEntities = new List<MediaEntityViewModel>();
            EntitiesList = new List<EntityViewModel>();

            IsFavorited = false;
            IsRetweeted = false;

            // RetweetInformation
            RetweetInformationText = "";
            RetweetInformationProfileImageUrl = "http://localhost/";

            // RetweetCounter
            RetweetCounterVisibility = false;
            RetweetCounterText = "";
            FavoriteCounterVisibility = false;
            FavoriteCounterText = "";

            QuotedStatusVisibility = false;
            QuotedStatusProfileImageUrl = "http://localhost/";

            MentionStatusProfileImageUrl = "http://localhost/";

            MentionStatusVisibility = false;
            IsMentionStatusLoading = false;

            RetweetCount = 0;

            IsMyTweet = false;
            IsMyRetweet = false;
            IsUserProtected = false;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public StatusViewModel(Status status)
        {
            Model = status;

            // BackgroundBrush
            BackgroundBrush = "Default";
            if (status.HasRetweetInformation)
                BackgroundBrush = "Retweet";
            else if (status.IsFavorited)
                BackgroundBrush = "Favorite";

            CreatedAt = status.CreatedAt.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            Source = status.Source;
            Text = status.Text;
            ScreenName = status.User.ScreenName;
            Name = status.User.Name;
            if (SettingService.Setting.ShowGifProfileImage)
            {
                ProfileImageUrl = string.IsNullOrWhiteSpace(status.User.ProfileGifImageUrl)
                    ? "http://localhost/"
                    : status.User.ProfileGifImageUrl;
            }
            else
            {
                ProfileImageUrl = string.IsNullOrWhiteSpace(status.User.ProfileImageUrl)
                    ? "http://localhost/"
                    : status.User.ProfileImageUrl;
            }
            
            Entities = status.Entities;
            ProtectedText = status.User.IsProtected ? "🔒 " : "";

            RetweetInformationVisibility = status.HasRetweetInformation;
            MediaVisibility = status.Entities.Media.Count != 0;
            MediaEntities = new List<MediaEntityViewModel>();
            foreach (var mediaEntity in status.Entities.Media)
                MediaEntities.Add(new MediaEntityViewModel(mediaEntity, status.PossiblySensitive));

            EntitiesList = new List<EntityViewModel>();
            if (status.Entities.Urls != null && status.Entities.HashTags != null &&
                status.Entities.UserMentions != null)
            {
                foreach (var urlEntity in status.Entities.Urls)
                    EntitiesList.Add(new EntityViewModel(urlEntity));
                foreach (var hashTagEntity in status.Entities.HashTags)
                    EntitiesList.Add(new EntityViewModel(hashTagEntity));
                foreach (var userMentionEntity in status.Entities.UserMentions)
                    EntitiesList.Add(new EntityViewModel(userMentionEntity));
            }

            IsFavorited = status.IsFavorited;
            IsRetweeted = status.IsRetweeted;

            // RetweetInformation
            if (status.RetweetInformation != null)
            {
                if (status.RetweetCount >= 2)
                    RetweetInformationText = "Retweet by " + status.RetweetInformation.User.ScreenName + " ( " +
                                             status.RetweetInformation.User.Name + " ) and " + status.RetweetCount +
                                             " others";
                else
                    RetweetInformationText = "Retweet by " + status.RetweetInformation.User.ScreenName + " ( " +
                                             status.RetweetInformation.User.Name + " )";

                RetweetInformationProfileImageUrl =
                    string.IsNullOrWhiteSpace(status.RetweetInformation.User.ProfileImageUrl)
                        ? "http://localhost/"
                        : status.RetweetInformation.User.ProfileImageUrl;
                RetweetInformationScreenName = status.RetweetInformation.User.ScreenName;
            }
            else
            {
                RetweetInformationText = "";
                RetweetInformationProfileImageUrl = "http://localhost/";
            }

            CounterVisibility = !status.HasRetweetInformation;
            // RetweetCounter
            if (!status.HasRetweetInformation && status.RetweetCount > 0)
            {
                RetweetCounterVisibility = true;
                RetweetCounterText = "Retweeted " + status.RetweetCount + " time";

                if (status.RetweetCount > 1)
                    RetweetCounterText += "s";
            }
            else
            {
                RetweetCounterVisibility = false;
                RetweetCounterText = "";
            }
            // FavoriteCounter
            if (status.FavoriteCount > 0)
            {
                FavoriteCounterVisibility = true;
                FavoriteCounterText = "Favorited " + status.FavoriteCount + " time";

                if (status.FavoriteCount > 1)
                    FavoriteCounterText += "s";
            }
            else
            {
                FavoriteCounterVisibility = false;
                FavoriteCounterText = "";
            }

            // TriangleIcon
            if (!status.IsRetweeted && status.IsFavorited)
                FavoriteTriangleIconVisibility = true;
            else
                FavoriteTriangleIconVisibility = false;
            if (status.IsRetweeted && !status.IsFavorited)
                RetweetTriangleIconVisibility = true;
            else
                RetweetTriangleIconVisibility = false;
            if (status.IsRetweeted && status.IsFavorited)
                RetweetFavoriteTriangleIconVisibility = true;
            else
                RetweetFavoriteTriangleIconVisibility = false;

            QuotedStatusVisibility = status.QuotedStatusId != 0 && status.QuotedStatus != null;
            if (status.QuotedStatus != null)
            {
                QuotedStatusName = status.QuotedStatus.User.Name;
                QuotedStatusScreenName = status.QuotedStatus.User.ScreenName;
                QuotedStatusText = status.QuotedStatus.Text;
                QuotedStatusEntities = status.QuotedStatus.Entities;
                QuotedStatusProfileImageUrl = string.IsNullOrWhiteSpace(status.QuotedStatus.User.ProfileImageUrl)
                    ? "http://localhost/"
                    : status.QuotedStatus.User.ProfileImageUrl;
            }
            else
            {
                QuotedStatusProfileImageUrl = "http://localhost/";
            }

            MentionStatusProfileImageUrl = "http://localhost/";

            MentionStatusVisibility = status.InReplyToStatusId != 0;
            IsMentionStatusLoaded = status.MentionStatus != null;
            IsMentionStatusLoading = false;

            RetweetCount = status.RetweetCount;

            IsMyTweet = false;
            IsMyRetweet = status.IsRetweeted;
            IsUserProtected = status.User.IsProtected;

            IsContentWarning = !string.IsNullOrWhiteSpace(status.SpoilerText);
            SpoilerText = status.SpoilerText;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public StatusViewModel(Status status, long userId) : this(status)
        {
            Model = status;

            // BackgroundBrush
            BackgroundBrush = "Default";
            if (status.HasRetweetInformation)
                BackgroundBrush = "Retweet";
            else if (status.InReplyToUserId == userId)
                BackgroundBrush = "Mention";
            else if (status.IsFavorited)
                BackgroundBrush = "Favorite";
            else if (status.User.Id == userId)
                BackgroundBrush = "MyTweet";

            IsMyTweet = status.User.Id == userId;
            IsMyRetweet = status.RetweetInformation != null && status.RetweetInformation.User.Id == userId ||
                          status.IsRetweeted;
            IsUserProtected = status.User.IsProtected && status.User.Id != userId;
        }

        public StatusViewModel(Status status, long userId, string collectionParameter) : this(status, userId)
        {
            IsCollectionStatus = true;
            CollectionParameter = collectionParameter;
        }

        public Status Model { get; }

        public string BackgroundBrush { get; set; }

        public string CreatedAt { get; set; }

        public string Source { get; set; }

        public string Text { get; set; }

        public string ScreenName { get; set; }

        public string Name { get; set; }

        public string ProtectedText { get; set; }

        public string ProfileImageUrl { get; set; }

        public Entities Entities { get; set; }

        public bool IsFavorited { get; set; }

        public bool IsRetweeted { get; set; }

        public bool RetweetInformationVisibility { get; set; }

        public bool MediaVisibility { get; set; }

        public List<MediaEntityViewModel> MediaEntities { get; }

        public List<EntityViewModel> EntitiesList { get; }

        public string RetweetInformationText { get; set; }

        public string RetweetInformationProfileImageUrl { get; set; }

        public string RetweetInformationScreenName { get; set; }


        public bool CounterVisibility { get; set; }

        public bool RetweetCounterVisibility { get; set; }

        public string RetweetCounterText { get; set; }

        public bool FavoriteCounterVisibility { get; set; }

        public string FavoriteCounterText { get; set; }

        public bool RetweetTriangleIconVisibility { get; set; }

        public bool FavoriteTriangleIconVisibility { get; set; }

        public bool RetweetFavoriteTriangleIconVisibility { get; set; }


        public bool QuotedStatusVisibility { get; set; }

        public string QuotedStatusScreenName { get; set; }

        public string QuotedStatusName { get; set; }

        public string QuotedStatusText { get; set; }

        public string QuotedStatusProfileImageUrl { get; set; }

        public bool QuotedStatusMediaVisibility { get; set; }

        public Entities QuotedStatusEntities { get; set; }

        public List<MediaEntityViewModel> QuotedStatusMediaEntities { get; }


        public bool MentionStatusVisibility { get; set; }

        public bool IsMentionStatusLoaded { get; set; }

        public bool IsMentionStatusLoading { get; set; }

        public string MentionStatusScreenName { get; set; }

        public string MentionStatusName { get; set; }

        public string MentionStatusText { get; set; }

        public string MentionStatusProfileImageUrl { get; set; }

        public Entities MentionStatusEntities { get; set; }

        public int RetweetCount { get; set; }

        public bool IsMyTweet { get; set; }

        public bool IsMyRetweet { get; set; }

        public bool IsUserProtected { get; set; }

        public bool IsCollectionStatus { get; set; }

        public string CollectionParameter { get; set; }

        public string SpoilerText { get; set; }

        public bool IsContentWarning { get; set; }

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }
    }
}