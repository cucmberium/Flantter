using System.Collections.Generic;
using System.Globalization;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class DirectMessageViewModel : ExtendedBindableBase, ITweetViewModel
    {
        public DirectMessageViewModel(DirectMessage directMessage, long userId)
        {
            Model = directMessage;

            BackgroundBrush = "Default";
            if (directMessage.Recipient.Id == userId)
                BackgroundBrush = "Mention";
            else if (directMessage.Sender.Id == userId)
                BackgroundBrush = "MyTweet";

            CreatedAt = directMessage.CreatedAt.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            Text = directMessage.Text;
            ScreenName = directMessage.Sender.ScreenName;
            Name = directMessage.Sender.Name;
            ProfileImageUrl = string.IsNullOrWhiteSpace(directMessage.Sender.ProfileImageUrl)
                ? "http://localhost/"
                : directMessage.Sender.ProfileImageUrl;
            Id = directMessage.Id;
            Entities = directMessage.Entities;

            MediaVisibility = directMessage.Entities.Media.Count != 0;
            MediaEntities = new List<MediaEntityViewModel>();
            foreach (var mediaEntity in directMessage.Entities.Media)
                MediaEntities.Add(new MediaEntityViewModel(mediaEntity, directMessage.PossiblySensitive));

            EntitiesList = new List<EntityViewModel>();
            if (directMessage.Entities.Urls != null && directMessage.Entities.HashTags != null &&
                directMessage.Entities.UserMentions != null)
            {
                foreach (var urlEntity in directMessage.Entities.Urls)
                    EntitiesList.Add(new EntityViewModel(urlEntity));
                foreach (var hashTagEntity in directMessage.Entities.HashTags)
                    EntitiesList.Add(new EntityViewModel(hashTagEntity));
                foreach (var userMentionEntity in directMessage.Entities.UserMentions)
                    EntitiesList.Add(new EntityViewModel(userMentionEntity));
            }

            RecipientName = directMessage.Recipient.Name;
            RecipientProfileImageUrl = string.IsNullOrWhiteSpace(directMessage.Recipient.ProfileImageUrl)
                ? "http://localhost/"
                : directMessage.Recipient.ProfileImageUrl;
            RecipientScreenName = directMessage.Recipient.ScreenName;

            IsMyTweet = directMessage.Sender.Id == userId;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public DirectMessageViewModel(DirectMessage directMessage)
        {
            Model = directMessage;

            BackgroundBrush = "Default";

            CreatedAt = directMessage.CreatedAt.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            Text = directMessage.Text;
            ScreenName = directMessage.Sender.ScreenName;
            Name = directMessage.Sender.Name;
            ProfileImageUrl = string.IsNullOrWhiteSpace(directMessage.Sender.ProfileImageUrl)
                ? "http://localhost/"
                : directMessage.Sender.ProfileImageUrl;
            Id = directMessage.Id;
            Entities = directMessage.Entities;

            MediaVisibility = directMessage.Entities.Media.Count != 0;
            MediaEntities = new List<MediaEntityViewModel>();
            foreach (var mediaEntity in directMessage.Entities.Media)
                MediaEntities.Add(new MediaEntityViewModel(mediaEntity, directMessage.PossiblySensitive));

            EntitiesList = new List<EntityViewModel>();
            foreach (var urlEntity in directMessage.Entities.Urls)
                EntitiesList.Add(new EntityViewModel(urlEntity));
            foreach (var hashTagEntity in directMessage.Entities.HashTags)
                EntitiesList.Add(new EntityViewModel(hashTagEntity));
            foreach (var userMentionEntity in directMessage.Entities.UserMentions)
                EntitiesList.Add(new EntityViewModel(userMentionEntity));

            RecipientName = directMessage.Recipient.Name;
            RecipientProfileImageUrl = string.IsNullOrWhiteSpace(directMessage.Recipient.ProfileImageUrl)
                ? "http://localhost/"
                : directMessage.Recipient.ProfileImageUrl;
            RecipientScreenName = directMessage.Recipient.ScreenName;

            IsMyTweet = false;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public DirectMessage Model { get; }

        public string BackgroundBrush { get; set; }

        public string CreatedAt { get; set; }

        public string Text { get; set; }

        public string ScreenName { get; set; }

        public string Name { get; set; }

        public string ProfileImageUrl { get; set; }

        public Entities Entities { get; set; }

        public bool MediaVisibility { get; set; }

        public List<MediaEntityViewModel> MediaEntities { get; }

        public List<EntityViewModel> EntitiesList { get; }

        public string RecipientProfileImageUrl { get; set; }

        public string RecipientScreenName { get; set; }

        public string RecipientName { get; set; }

        public bool IsMyTweet { get; set; }

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }

        public long Id { get; set; }
    }
}