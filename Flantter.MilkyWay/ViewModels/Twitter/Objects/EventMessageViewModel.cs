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
    public class EventMessageViewModel : ExtendedBindableBase
    {
        public EventMessageViewModel(EventMessage eventMessage, ColumnModel column)
        {
            this.Model = eventMessage;

            this.BackgroundBrush = "Default";
            if (eventMessage.Source.Id == column.OwnerUserId)
                this.BackgroundBrush = "MyTweet";
            else if (eventMessage.Target != null && eventMessage.Target.Id == column.OwnerUserId)
                this.BackgroundBrush = "Mention";

            this.CreatedAt = eventMessage.CreatedAt.ToLocalTime().ToString();
            this.ScreenName = eventMessage.Source.ScreenName;
            this.Name = eventMessage.Source.Name;
            this.ProfileImageUrl = string.IsNullOrWhiteSpace(eventMessage.Source.ProfileImageUrl) ? "http://localhost/" : eventMessage.Source.ProfileImageUrl;
            
            if (eventMessage.TargetStatus != null)
            {
                this.TargetStatusVisibility = true;
                this.TargetStatusId = eventMessage.TargetStatus.Id;
                this.TargetStatusName = eventMessage.TargetStatus.User.Name;
                this.TargetStatusScreenName = eventMessage.TargetStatus.User.ScreenName;
                this.TargetStatusText = eventMessage.TargetStatus.Text;
                this.TargetStatusEntities = eventMessage.TargetStatus.Entities;
                this.TargetStatusProfileImageUrl = string.IsNullOrWhiteSpace(eventMessage.TargetStatus.User.ProfileImageUrl) ? "http://localhost/" : eventMessage.TargetStatus.User.ProfileImageUrl;
            }
            else
            {
                this.TargetStatusVisibility = false;
                this.TargetStatusProfileImageUrl = "http://localhost/";
            }

            this.Notice = Service.Notice.Instance;
        }

        public EventMessage Model { get; private set; }

        public string BackgroundBrush { get; set; }

        public string CreatedAt { get; set; }

        public string Text { get; set; }

        public string ScreenName { get; set; }

        public string Name { get; set; }

        public string ProfileImageUrl { get; set; }
        
        public bool TargetStatusVisibility { get; set;}

        public long TargetStatusId { get; set; }

        public string TargetStatusCreatedAt { get; set; }

        public string TargetStatusText { get; set; }

        public string TargetStatusScreenName { get; set; }

        public string TargetStatusName { get; set; }

        public string TargetStatusProfileImageUrl { get; set; }
        
        public Entities TargetStatusEntities { get; set; }

        public Service.Notice Notice { get; set; }
    }
}
