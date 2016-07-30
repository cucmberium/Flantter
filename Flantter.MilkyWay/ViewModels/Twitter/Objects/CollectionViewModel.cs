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
    public class CollectionViewModel : ExtendedBindableBase
    {
        public CollectionViewModel(Collection collection)
        {
            this.Model = collection;

            this.BackgroundBrush = "Default";

            this.Description = collection.Description;
            this.Name = collection.Name;
            this.ScreenName = collection.User.ScreenName;
            this.ProfileImageUrl = collection.User.ProfileImageUrl;

            this.Notice = Services.Notice.Instance;
            this.Setting = SettingService.Setting;
        }

        public Collection Model { get; private set; }
        
        public string BackgroundBrush { get; set; }

        public string Description { get; set; }
        
        public string Name { get; set; }

        public string ScreenName { get; set; }

        public string ProfileImageUrl { get; set; }

        public Services.Notice Notice { get; set; }

        public Setting.SettingService Setting { get; set; }
    }
}
