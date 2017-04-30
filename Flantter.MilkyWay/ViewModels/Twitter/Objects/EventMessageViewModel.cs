using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class EventMessageViewModel : ExtendedBindableBase, ITweetViewModel
    {
        public EventMessageViewModel(EventMessage eventMessage, long userId)
        {
            Model = eventMessage;

            BackgroundBrush = "Default";
            if (eventMessage.Source.Id == userId)
                BackgroundBrush = "MyTweet";
            else if (eventMessage.Target != null && eventMessage.Target.Id == userId)
                BackgroundBrush = "Mention";
            else if (eventMessage.Type == "Retweet" || eventMessage.Type == "RetweetedRetweet")
                BackgroundBrush = "Retweet";
            else if (eventMessage.Type == "Favorite" || eventMessage.Type == "FavoritedRetweet")
                BackgroundBrush = "Favorite";

            CreatedAt = eventMessage.CreatedAt.ToLocalTime().ToString(CultureInfo.InvariantCulture);
            ScreenName = eventMessage.Source.ScreenName;
            Name = eventMessage.Source.Name;
            ProfileImageUrl = string.IsNullOrWhiteSpace(eventMessage.Source.ProfileImageUrl)
                ? "http://localhost/"
                : eventMessage.Source.ProfileImageUrl;
            Id = 0;

            var resourceLoader = new ResourceLoader();
            var sourceUser = "@" + eventMessage.Source.ScreenName + " (" + eventMessage.Source.Name + ") ";
            string targetUser;
            if (eventMessage.Target != null)
                targetUser = "@" + eventMessage.Target.ScreenName + " (" + eventMessage.Target.Name + ") ";
            else
                targetUser = resourceLoader.GetString("Event_Me");

            switch (eventMessage.Type)
            {
                case "Favorite":
                    Text = string.Format(resourceLoader.GetString("Event_Favorite"), sourceUser, targetUser);
                    break;
                case "Follow":
                    Text = string.Format(resourceLoader.GetString("Event_Follow"), sourceUser, targetUser);
                    break;
                case "Unfavorite":
                    Text = string.Format(resourceLoader.GetString("Event_Unfavorite"), sourceUser, targetUser);
                    break;
                case "UserUpdate":
                    Text = string.Format(resourceLoader.GetString("Event_UserUpdate"), sourceUser);
                    break;
                case "FavoritedRetweet":
                    Text = string.Format(resourceLoader.GetString("Event_FavoritedRetweet"), sourceUser, targetUser);
                    break;
                case "RetweetedRetweet":
                    Text = string.Format(resourceLoader.GetString("Event_RetweetedRetweet"), sourceUser, targetUser);
                    break;
                case "QuotedTweet":
                    Text = string.Format(resourceLoader.GetString("Event_QuotedTweet"), sourceUser, targetUser);
                    break;
                case "Retweet":
                    Text = string.Format(resourceLoader.GetString("Event_RetweetTweet"), sourceUser, targetUser);
                    break;
                case "Mention":
                    Text = string.Format(resourceLoader.GetString("Event_Mention"), sourceUser, targetUser);
                    break;
            }

            if (eventMessage.TargetStatus != null)
            {
                TargetStatusVisibility = true;
                TargetStatusId = eventMessage.TargetStatus.Id;
                TargetStatusName = eventMessage.TargetStatus.User.Name;
                TargetStatusScreenName = eventMessage.TargetStatus.User.ScreenName;
                TargetStatusText = eventMessage.TargetStatus.Text;
                TargetStatusEntities = eventMessage.TargetStatus.Entities;
                TargetStatusProfileImageUrl = string.IsNullOrWhiteSpace(eventMessage.TargetStatus.User.ProfileImageUrl)
                    ? "http://localhost/"
                    : eventMessage.TargetStatus.User.ProfileImageUrl;

                TargetStatusMediaVisibility = (eventMessage.TargetStatus.Entities.Media.Count != 0) &&
                                              SettingService.Setting.ShowQuotedStatusMedia;

                TargetStatusMediaEntities = new List<MediaEntityViewModel>();
                foreach (var mediaEntity in eventMessage.TargetStatus.Entities.Media)
                    TargetStatusMediaEntities.Add(new MediaEntityViewModel(mediaEntity));
            }
            else
            {
                TargetStatusVisibility = false;
                TargetStatusProfileImageUrl = "http://localhost/";
            }

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public EventMessage Model { get; }

        public string BackgroundBrush { get; set; }

        public string CreatedAt { get; set; }

        public string Text { get; set; }

        public string ScreenName { get; set; }

        public string Name { get; set; }

        public string ProfileImageUrl { get; set; }

        public bool TargetStatusVisibility { get; set; }

        public long TargetStatusId { get; set; }

        public string TargetStatusText { get; set; }

        public string TargetStatusScreenName { get; set; }

        public string TargetStatusName { get; set; }

        public string TargetStatusProfileImageUrl { get; set; }

        public bool TargetStatusMediaVisibility { get; set; }

        public Entities TargetStatusEntities { get; set; }

        public List<MediaEntityViewModel> TargetStatusMediaEntities { get; }

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }

        public long Id { get; set; }
    }
}