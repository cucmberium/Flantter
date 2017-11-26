using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Apis.Objects
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
            if (SettingService.Setting.ShowGifProfileImage)
            {
                ProfileImageUrl = string.IsNullOrWhiteSpace(collection.User.ProfileGifImageUrl)
                    ? "http://localhost/"
                    : collection.User.ProfileGifImageUrl;
            }
            else
            {
                ProfileImageUrl = string.IsNullOrWhiteSpace(collection.User.ProfileImageUrl)
                    ? "http://localhost/"
                    : collection.User.ProfileImageUrl;
            }

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