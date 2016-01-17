using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class ListViewModel : ExtendedBindableBase
    {
        public ListViewModel(List list)
        {
            this.Model = list;

            this.BackgroundBrush = "Default";

            this.Description = list.Description;
            this.Name = list.Name;
            this.FullName = list.FullName;
            this.SubscriberCount = list.SubscriberCount;
            this.MemberCount = list.MemberCount;
            this.Id = list.Id;
            this.ScreenName = list.User.ScreenName;
            this.ProfileImageUrl = list.User.ProfileImageUrl;

            this.Notice = Services.Notice.Instance;
            this.Setting = SettingService.Setting;
        }

        public List Model { get; private set; }
        
        public string BackgroundBrush { get; set; }

        public string Description { get; set; }
        
        public string Name { get; set; }

        public string FullName { get; set; }

        public int SubscriberCount { get; set; }
        
        public int MemberCount { get; set; }
        
        public long Id { get; set; }

        public string ScreenName { get; set; }

        public string ProfileImageUrl { get; set; }

        public Services.Notice Notice { get; set; }

        public Setting.SettingService Setting { get; set; }
    }
}
