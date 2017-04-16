using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class CollectionViewModel : ExtendedBindableBase
    {
        public CollectionViewModel(Collection collection)
        {
            Model = collection;

            BackgroundBrush = "Default";

            Description = collection.Description;
            Name = collection.Name;
            ScreenName = collection.User.ScreenName;
            ProfileImageUrl = collection.User.ProfileImageUrl;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public Collection Model { get; }

        public string BackgroundBrush { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string ScreenName { get; set; }

        public string ProfileImageUrl { get; set; }

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }
    }
}