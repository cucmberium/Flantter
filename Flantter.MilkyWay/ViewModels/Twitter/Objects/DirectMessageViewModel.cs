using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class DirectMessageViewModel : ExtendedBindableBase, ITweetViewModel
    {
        public DirectMessageViewModel(DirectMessage directMessage, ColumnModel column)
        {
            this.Model = directMessage;

            this.BackgroundBrush = "Default";
            if (directMessage.Recipient.Id == column.OwnerUserId)
                this.BackgroundBrush = "Mention";
            else if (directMessage.Sender.Id == column.OwnerUserId)
                this.BackgroundBrush = "MyTweet";

            this.CreatedAt = directMessage.CreatedAt.ToLocalTime().ToString();
            this.Text = directMessage.Text;
            this.ScreenName = directMessage.Sender.ScreenName;
            this.Name = directMessage.Sender.Name;
            this.ProfileImageUrl = string.IsNullOrWhiteSpace(directMessage.Sender.ProfileImageUrl) ? "http://localhost/" : directMessage.Sender.ProfileImageUrl;
            this.Id = directMessage.Id;
            this.Entities = directMessage.Entities;

            this.MediaVisibility = directMessage.Entities.Media.Count == 0 ? false : true;
            this.MediaEntities = new List<MediaEntityViewModel>();
            foreach (var mediaEntity in directMessage.Entities.Media)
                this.MediaEntities.Add(new MediaEntityViewModel(mediaEntity));

            this.UrlEntities = new List<UrlEntityViewModel>();
            foreach (var urlEntity in directMessage.Entities.Urls)
                this.UrlEntities.Add(new UrlEntityViewModel(urlEntity));

            this.RecipientName = directMessage.Recipient.Name;
            this.RecipientProfileImageUrl = string.IsNullOrWhiteSpace(directMessage.Recipient.ProfileImageUrl) ? "http://localhost/" : directMessage.Recipient.ProfileImageUrl;
            this.RecipientScreenName = directMessage.Recipient.ScreenName;

            this.IsMyTweet = (directMessage.Sender.Id == column.OwnerUserId);

            this.Notice = Services.Notice.Instance;
        }

        public DirectMessage Model { get; private set; }

        public string BackgroundBrush { get; set; }

        public string CreatedAt { get; set; }

        public string Text { get; set; }

        public string ScreenName { get; set; }

        public string Name { get; set; }

        public string ProfileImageUrl { get; set; }

        public long Id { get; set; }

        public Entities Entities { get; set; }

        public bool MediaVisibility { get; set; }

        public List<MediaEntityViewModel> MediaEntities { get; private set; }

        public List<UrlEntityViewModel> UrlEntities { get; private set; }

        public string RecipientProfileImageUrl { get; set; }

        public string RecipientScreenName { get; set; }

        public string RecipientName { get; set; }

        public bool IsMyTweet { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
