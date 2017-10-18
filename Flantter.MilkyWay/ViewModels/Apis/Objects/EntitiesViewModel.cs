﻿using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Apis.Objects
{
    public class MediaEntityViewModel
    {
        public MediaEntityViewModel(MediaEntity mediaEntity, bool possiblySensitive = false)
        {
            Model = mediaEntity;

            MediaThumbnailUrl = SettingService.Setting.ShowHighQualityImageResolution &&
                                !string.IsNullOrWhiteSpace(mediaEntity.MediaUrl)
                ? mediaEntity.MediaUrl
                : mediaEntity.MediaThumbnailUrl;

            PossibilySensitiveBlur = SettingService.Setting.EnableNsfwFilter && possiblySensitive ? 4.0 : 0.0;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public MediaEntity Model { get; }

        public string MediaThumbnailUrl { get; set; }

        public double PossibilySensitiveBlur { get; set; }

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }
    }

    public class EntityViewModel
    {
        public EntityViewModel(UrlEntity urlEntity)
        {
            Model = urlEntity;

            ExpandedUrl = urlEntity.ExpandedUrl;
            DisplayUrl = urlEntity.DisplayUrl;

            Notice = Notice.Instance;
        }

        public EntityViewModel(HashtagEntity hashtagEntity)
        {
            Model = hashtagEntity;

            ExpandedUrl = "#" + hashtagEntity.Tag;
            DisplayUrl = "#" + hashtagEntity.Tag;

            Notice = Notice.Instance;
        }

        public EntityViewModel(UserMentionEntity userMentionEntity)
        {
            Model = userMentionEntity;

            ExpandedUrl = "@" + userMentionEntity.ScreenName;
            DisplayUrl = "@" + userMentionEntity.ScreenName;

            Notice = Notice.Instance;
        }

        public object Model { get; }

        public string ExpandedUrl { get; set; }

        public string DisplayUrl { get; set; }

        public Notice Notice { get; set; }
    }
}