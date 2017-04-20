using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class ListViewModel : ExtendedBindableBase
    {
        public ListViewModel(List list)
        {
            Model = list;

            Description = list.Description;
            Name = list.Name;
            SubscriberCount = list.SubscriberCount;
            MemberCount = list.MemberCount;
            ScreenName = list.User.ScreenName;
            ProfileImageUrl = list.User.ProfileImageUrl;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public List Model { get; }

        public string Description { get; set; }

        public string Name { get; set; }

        public int SubscriberCount { get; set; }

        public int MemberCount { get; set; }

        public string ScreenName { get; set; }

        public string ProfileImageUrl { get; set; }

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }
    }
}